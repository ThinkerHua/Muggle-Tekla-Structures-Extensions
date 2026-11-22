/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  PluginFieldsGenerator.cs: help to generate code for plugin class.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
public class PluginFieldsGenerator : IIncrementalGenerator {
    private const string ConcernedAttribute = "Muggle.TsExtensions.CodingHelper.Generators.FieldsFromDataAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => {
            var shortName = ConcernedAttribute.Substring(ConcernedAttribute.LastIndexOf('.') + 1);
            ctx.AddSource($"{shortName}.g.cs",
                SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
        });

        var versionProvider = context.CompilationProvider.Select(static (compilation, _) => {
            foreach (var reference in compilation.References) {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol ||
                    assemblySymbol.Name != "Tekla.Structures.Model" &&
                    assemblySymbol.Identity.Name != "Tekla.Structures.Model") {
                    continue;
                }

                return assemblySymbol.Identity.Version;
            }

            return null;
        });

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(ConcernedAttribute, Predicate, Transform)
            .Where(x => x != default);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics,
            static (spc, info) => {
                spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
            });

        var pluginFieldsInfo = provider
            .Select(static (x, _) => x.Value)
            .Combine(versionProvider)
            .Where(pair => pair.Left != default && pair.Right != null);
        context.RegisterSourceOutput(pluginFieldsInfo, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        return node is ClassDeclarationSyntax;
    }

    private static GatheredInfo<PluginFieldsInfo> Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        var result = new GatheredInfo<PluginFieldsInfo>(default, []);
        var builder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        if (!classDeclarationSyntax!.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) {
            builder.Add(new DiagnosticInfo(NotPartial, classDeclarationSyntax.Identifier.GetLocation(),
                [classDeclarationSyntax.Identifier.ValueText]));
            result.DiagnosticInfos = builder.ToImmutableArray();
            return result;
        }

        var attSyntax = classDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes)
            .First(a => GetAttributeQualifiedName(a, semanticModel) == ConcernedAttribute);

        ConstructorDeclarationSyntax constructorSyntax;
        try {
            constructorSyntax = classDeclarationSyntax.ChildNodes().OfType<ConstructorDeclarationSyntax>().Single();
        } catch (InvalidOperationException) {
            builder.Add(new DiagnosticInfo(UniqueConstructorNotFound, attSyntax.GetLocation(), []));
            result.DiagnosticInfos = builder.ToImmutableArray();
            return result;
        }

        ParameterSyntax parameterSyntax;
        try {
            parameterSyntax = constructorSyntax.ParameterList.Parameters.Single();
        } catch (InvalidOperationException) {
            builder.Add(new DiagnosticInfo(UniqueParameterNotFound, attSyntax.GetLocation(), []));
            result.DiagnosticInfos = builder.ToImmutableArray();
            return result;
        }

        var pluginDataType = semanticModel.GetTypeInfo(parameterSyntax.Type!).Type;
        if (pluginDataType is null) return result;

        var argDict = AnalyzePluginDataRegisteredIds(pluginDataType);
        if (argDict.Count == 0 || argDict.All(kvp => kvp.Value.Count == 0)) return result;

        token.ThrowIfCancellationRequested();

        result.DiagnosticInfos = builder.ToImmutableArray();
        result.Value = new PluginFieldsInfo {
            ClassInfo = new ClassInfo {
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
                Name = classDeclarationSyntax.Identifier.ValueText,
            },
            DataType = pluginDataType,
            Arguments = argDict
        };

        return result;
    }

    internal static ArgumentsDictionary<IdSet> AnalyzePluginDataRegisteredIds(ITypeSymbol pluginDataType) {
        //  Key - for 'GeneralAttribute' is 'int' or 'double' or 'string'
        //        for other attributes is attribute name
        //  Value - id hash set
        var argDict = new ArgumentsDictionary<IdSet>();

        // 直接使用 pluginDataType.GetAttributes() 获取不到在源生成阶段才有的特性（BoltAttribute）
        var attSyntaxes = pluginDataType.DeclaringSyntaxReferences
            .Select(dsr => dsr.GetSyntax() as TypeDeclarationSyntax)
            .Where(tds => tds is not null)
            .SelectMany(tds => tds.AttributeLists.SelectMany(a => a.Attributes));

        foreach (var attSyntax in attSyntaxes) {
            // 当 plugin data class 的定义与 plugin class 的定义不在同一个文件时, 此方法无效
            // var attName = GetAttributeName(dataAttSyntax, semanticModel);
            var attName = attSyntax!.Name.ToString();
            if (!attName.EndsWith("Attribute")) attName += "Attribute";
            if (!PluginDataFieldsGenerator.ConcernedAttributes.Any(a => GetUnqualifiedName(a).Equals(attName)))
                continue;

            if (attName is "GeneralAttribute") {
                AnalyzeGeneralIds(attSyntax, ref argDict);
            } else {
                if (!argDict.TryGetValue(attName, out var idSet)) {
                    idSet = [];
                    argDict.Add(attName, idSet);
                }

                AnalyzeSeriesIds([.. attSyntax.ArgumentList!.Arguments], ref idSet);
            }
        }

        return argDict;
    }

    private static void AnalyzeGeneralIds(AttributeSyntax attSyntax, ref ArgumentsDictionary<IdSet> argDict) {
        if (attSyntax.ArgumentList is null) return;
        var dataType = attSyntax.ArgumentList.DescendantNodes().OfType<TypeOfExpressionSyntax>()
            .FirstOrDefault()?.Type.ToString();
        if (string.IsNullOrEmpty(dataType)) return;
        if (!argDict.TryGetValue(dataType, out var idSet)) {
            idSet = [];
            argDict.Add(dataType, idSet);
        }

        var exprSyntaxes = attSyntax.ArgumentList.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();
        if (exprSyntaxes.Length == 0) return;

        //  take two at once
        for (int i = 0; i < exprSyntaxes.Length / 2 * 2; i += 2) {
            var nameSyntax = exprSyntaxes[i];
            if (!nameSyntax.IsKind(SyntaxKind.StringLiteralExpression)) continue;
            var name = nameSyntax.Token.ValueText;
            idSet.Add(name);
        }
    }

    private static void AnalyzeSeriesIds(ICollection<AttributeArgumentSyntax> argSyntaxes, ref IdSet idSet) {
        var index = -1;
        foreach (var argSyntax in argSyntaxes) {
            index++;

            //  style of (param: value)
            var paramName = argSyntax.NameColon?.Name.Identifier.ValueText;
            var paramValue = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

            if (paramName is null && index == 0 || paramName is "id") {
                idSet.Add(paramValue);
            }
        }
    }

    private static void Generate(SourceProductionContext context, (PluginFieldsInfo, Version) pair) {
        var fieldsInfo = pair.Item1;
        var tsmVersion = pair.Item2;

        var fieldsBuilder = GenerateFields(fieldsInfo, tsmVersion);
        var methodBuilder = GenerateGetFieldValuesFromMethod(fieldsInfo, tsmVersion);

        var classInfo = fieldsInfo.ClassInfo;
        var generatedSourceText =
#if DEBUG
            $"//  <auto-generated/> at {DateTime.Now}\r\n" +
#else
            $"//  <auto-generated/>\r\n" +
#endif
            $"\r\n" +
            $"namespace {classInfo.NameSpace} {{\r\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name} {{\r\n" +
            $"{fieldsBuilder}\r\n" +
            $"{methodBuilder}\r\n" +
            $"    }}\r\n" +
            $"}}";

        context.AddSource($"{fieldsInfo.ClassInfo.Name}.g.cs", SourceText.From(generatedSourceText, Encoding.UTF8));
    }

    private static StringBuilder GenerateFields(PluginFieldsInfo info, Version tsmVersion) {
        var builder = new StringBuilder();
        var singleFieldsBuilder = new StringBuilder();
        var seriesFieldsBuilder = new StringBuilder();

        var fieldSymbols = info.DataType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public);

        foreach (var fieldSymbol in fieldSymbols) {
            var fieldName = ToPrivateFieldNameStyle(fieldSymbol.Name);
            builder.AppendLine($"        private {fieldSymbol.Type} {fieldName};");
        }

        foreach (var kvp in info.Arguments) {
            if (kvp.Key.EndsWith("Attribute")) {
                GenerateSeriesFields(kvp, tsmVersion, ref seriesFieldsBuilder);
            } else {
                GenerateSingleFields(kvp, ref singleFieldsBuilder);
            }
        }

        builder.Append(singleFieldsBuilder).Append(seriesFieldsBuilder);
        return builder;
    }

    private static void GenerateSeriesFields(KeyValuePair<string, IdSet> kvp, Version tsmVersion,
        ref StringBuilder builder) {

        var attName = kvp.Key;
        var fieldPrefix = ToPrivateFieldNameStyle(attName.Substring(0, attName.Length - 9));
        var fieldInfos = attName switch {
            "PartAttribute" => PluginDataFieldsGenerator.PartFieldInfos(tsmVersion),
            "PlateAttribute" => PluginDataFieldsGenerator.PlateFieldInfos(tsmVersion),
            "BoltAttribute" => PluginDataFieldsGenerator.BoltFieldInfos(tsmVersion),
            "StudAttribute" => PluginDataFieldsGenerator.StudFieldInfos(tsmVersion),
            "WeldAttribute" => PluginDataFieldsGenerator.WeldFieldInfos(tsmVersion),
            "ChamferAttribute" => PluginDataFieldsGenerator.ChamferFieldInfos(tsmVersion),
            _ => []
        };

        foreach (var id in kvp.Value) {
            foreach (var info in fieldInfos) {
                builder.AppendLine(
                    $"        private {info.DataType} {fieldPrefix}{ToPropertyNameStyle(id)}{info.PropertyName};");
            }
        }
    }

    private static void GenerateSingleFields(KeyValuePair<string, IdSet> kvp, ref StringBuilder builder) {
        var dataType = kvp.Key;
        if (dataType != "int" && dataType != "double" && dataType != "string") return;

        foreach (var id in kvp.Value) {
            builder.AppendLine($"        private {dataType} {ToPrivateFieldNameStyle(id)};");
        }
    }

    private static StringBuilder GenerateGetFieldValuesFromMethod(PluginFieldsInfo fieldsInfo, Version tsmVersion) {
        var builder = new StringBuilder();
        builder.AppendLine(
            $"        /// <summary>\r\n" +
            $"        /// Copy the values of each field in <paramref name=\"data\" /> to the fields of this plugin.\r\n" +
            $"        /// </summary>\r\n" +
            $"        /// <param name=\"data\">The given data provider.</param>\r\n" +
            $"        private void GetFieldValuesFrom({fieldsInfo.DataType} data) {{");

        var fieldSymbols = fieldsInfo.DataType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public);
        foreach (var fieldSymbol in fieldSymbols) {
            var privateFieldName = ToPrivateFieldNameStyle(fieldSymbol.Name);
            builder.AppendLine($"            {privateFieldName} = data.{fieldSymbol.Name};");
        }

        foreach (var kvp in fieldsInfo.Arguments) {
            var key = kvp.Key;

            if (key is "int" or "double" or "string") {
                foreach (var id in kvp.Value) {
                    builder.AppendLine(
                        $"            {ToPrivateFieldNameStyle(id)} = data.{ToPropertyNameStyle(id)};");
                }
            } else {
                var fieldInfos = key switch {
                    "PartAttribute" => PluginDataFieldsGenerator.PartFieldInfos(tsmVersion),
                    "PlateAttribute" => PluginDataFieldsGenerator.PlateFieldInfos(tsmVersion),
                    "BoltAttribute" => PluginDataFieldsGenerator.BoltFieldInfos(tsmVersion),
                    "StudAttribute" => PluginDataFieldsGenerator.StudFieldInfos(tsmVersion),
                    "WeldAttribute" => PluginDataFieldsGenerator.WeldFieldInfos(tsmVersion),
                    "ChamferAttribute" => PluginDataFieldsGenerator.ChamferFieldInfos(tsmVersion),
                    _ => []
                };

                var modelObjectType = key.Substring(0, key.Length - 9);

                foreach (var id in kvp.Value) {
                    foreach (var fieldInfo in fieldInfos) {
                        var fieldName = $"{modelObjectType}{ToPropertyNameStyle(id)}{fieldInfo.PropertyName}";
                        builder.AppendLine(
                            $"            {ToPrivateFieldNameStyle(fieldName)} = data.{fieldName};");
                    }
                }
            }
        }

        builder.AppendLine($"        }}");
        return builder;
    }
}