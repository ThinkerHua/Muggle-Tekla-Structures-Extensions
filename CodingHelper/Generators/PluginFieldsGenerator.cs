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
 *  PluginFieldsGenerator.cs: help to generate fields and get field values method for plugin.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Diagnosers;
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
public class PluginFieldsGenerator : IIncrementalGenerator {
    internal const string ConcernedAttribute = "Muggle.TsExtensions.CodingHelper.Generators.FieldsFromAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            var shortName = ConcernedAttribute.Substring(ConcernedAttribute.LastIndexOf('.') + 1);
            ctx.AddSource($"{shortName}.g.cs",
                SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
        });

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(ConcernedAttribute, Predicate, Transform)
            .Where(x => x != default);

        context.RegisterSourceOutput(provider, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        var classDeclarationSyntax = node as ClassDeclarationSyntax;

        return classDeclarationSyntax!.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    private static PluginFieldsInfo Transform(GeneratorAttributeSyntaxContext context, CancellationToken token) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol == null) return default;

        var attSyntax = classDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes)
            .First(a => GetAttributeQualifiedName(a, semanticModel) == ConcernedAttribute);
        if (attSyntax.ArgumentList == null) return default;

        var dataTypeInfo = semanticModel.GetTypeInfo(attSyntax.ArgumentList.DescendantNodes()
            .OfType<TypeOfExpressionSyntax>().First().Type);
        if (dataTypeInfo.Type == null) return default;

        var dataAttSyntaxes = dataTypeInfo.Type.GetAttributes().Where(a =>
                PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass?.ToDisplayString()))
            .Select(a => a.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax);

        //  Key - for 'GeneralFieldDefaultValuesAttribute' is 'int' or 'double' or 'string'
        //        for other attributes is attribute name
        //  Value - name or number hash set
        var argDict = new ArgumentsDictionary<NameOrNumberSet>();
        var generalFieldNameSet = new NameOrNumberSet();

        foreach (var dataAttSyntax in dataAttSyntaxes) {
            //  when plugin data class not in the same file as plugin class, it doesn't work
            // var attName = GetAttributeName(dataAttSyntax, semanticModel);

            var attName = dataAttSyntax!.Name.ToString();
            if (!attName.EndsWith("Attribute")) attName += "Attribute";

            var generalFieldDataType = dataAttSyntax.ArgumentList!.Arguments
                .SelectMany(attArgSyntax => attArgSyntax.DescendantNodes().OfType<TypeOfExpressionSyntax>())
                .FirstOrDefault()?.Type.ToString();
            if (!string.IsNullOrEmpty(generalFieldDataType)) attName = generalFieldDataType;

            if (!argDict.TryGetValue(attName, out var nameOrNumberSet)) {
                nameOrNumberSet = [];
                argDict.Add(attName, nameOrNumberSet);
            }

            var nameOrNumbers = dataAttSyntax.ArgumentList!.Arguments
                .SelectMany(attArgSyntax => attArgSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>())
                .Select(exprSyntax => exprSyntax.Token.ValueText);

            foreach (var nameOrNumber in nameOrNumbers) {
                if (nameOrNumber.Length == 0 ||
                    nameOrNumber.Length > InternalAttributesDiagnoser.MaxLengthOfArgument(attName) ||
                    Regex.IsMatch(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern))
                    continue;
                if (!string.IsNullOrEmpty(generalFieldDataType) && Regex.IsMatch(nameOrNumber, "^[0-9]")) continue;

                nameOrNumberSet.Add(nameOrNumber);
            }
        }

        if (argDict.Count == 0 || argDict.All(kvp => kvp.Value.Count == 0)) return default;

        token.ThrowIfCancellationRequested();

        return new PluginFieldsInfo {
            ClassInfo = new ClassInfo {
                Name = classDeclarationSyntax.Identifier.ValueText,
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
            },
            DataType = dataTypeInfo.Type,
            Arguments = argDict
        };

    }

    private static void Generate(SourceProductionContext context, PluginFieldsInfo fieldsInfo) {
        var fields = GenerateFields(fieldsInfo);
        var method = GenerateGetFieldValuesFromMethod(fieldsInfo);

        var classInfo = fieldsInfo.ClassInfo;
        var generatedSourceText =
            $"//  <auto-generated/>" +
#if DEBUG
            $" at {DateTime.Now}" +
#endif
            $"\n\n" +
            $"namespace {classInfo.NameSpace} {{\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name} {{\n" +
            $"{fields}\n" +
            $"{method}\n" +
            $"    }}\n" +
            $"}}";

        context.AddSource($"{fieldsInfo.ClassInfo.Name}.g.cs", SourceText.From(generatedSourceText, Encoding.UTF8));
    }

    private static string GenerateFields(PluginFieldsInfo info) {
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
            var attName = kvp.Key;
            var attPrefix = attName.Length > 15 ? attName.Substring(0, 15) : string.Empty;

            if (attPrefix == string.Empty) {
                singleFieldsBuilder.Append(GenerateSingleFields(kvp));
            } else {
                seriesFieldsBuilder.Append(GenerateSeriesFields(kvp));
            }
        }

        return builder.Append(singleFieldsBuilder).Append(seriesFieldsBuilder).ToString();
    }

    private static string GenerateSeriesFields(KeyValuePair<string, NameOrNumberSet> kvp) {
        var attName = kvp.Key;

        var fieldPrefix = ToPrivateFieldNameStyle(attName.Substring(0, attName.Length - 15));

        var fieldInfos = attName switch {
            "PartFieldsAttribute" => PluginDataFieldsGenerator.PartFieldInfos,
            "PlateFieldsAttribute" => PluginDataFieldsGenerator.PlateFieldInfos,
            "WeldFieldsAttribute" => PluginDataFieldsGenerator.WeldFieldInfos,
            "BoltFieldsAttribute" => PluginDataFieldsGenerator.BoltFieldInfos,
            "BoltCircleFieldsAttribute" => PluginDataFieldsGenerator.BoltCircleFieldInfos,
            _ => []
        };

        var fieldsBuilder = new StringBuilder();
        foreach (var nameOrNumber in kvp.Value) {
            foreach (var info in fieldInfos) {
                fieldsBuilder.AppendLine($"        private {info.Type} {fieldPrefix}{nameOrNumber}{info.Name};");
            }
        }

        return fieldsBuilder.ToString();
    }

    private static string GenerateSingleFields(KeyValuePair<string, NameOrNumberSet> kvp) {
        var dataType = kvp.Key;

        if (dataType != "int" && dataType != "double" && dataType != "string") { return string.Empty; }

        var fieldsBuilder = new StringBuilder();
        foreach (var name in kvp.Value) {
            fieldsBuilder.AppendLine($"        private {dataType} {ToPrivateFieldNameStyle(name)};");
        }

        return fieldsBuilder.ToString();
    }

    private static string GenerateGetFieldValuesFromMethod(PluginFieldsInfo fieldsInfo) {

        var builder = new StringBuilder();

        var fieldSymbols = fieldsInfo.DataType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public);

        foreach (var fieldSymbol in fieldSymbols) {
            var privateFieldName = ToPrivateFieldNameStyle(fieldSymbol.Name);
            builder.AppendLine($"            {privateFieldName} = data.{fieldSymbol.Name};");
        }

        foreach (var kvp in fieldsInfo.Arguments) {
            var attName = kvp.Key;

            if (attName is "int" or "double" or "string") {
                foreach (var name in kvp.Value) {
                    builder.AppendLine(
                        $"            {ToPrivateFieldNameStyle(name)} = data.{ToPropertyNameStyle(name)};");
                }
            } else {
                var fieldInfos = attName switch {
                    "PartFieldsAttribute" => PluginDataFieldsGenerator.PartFieldInfos,
                    "PlateFieldsAttribute" => PluginDataFieldsGenerator.PlateFieldInfos,
                    "WeldFieldsAttribute" => PluginDataFieldsGenerator.WeldFieldInfos,
                    "BoltFieldsAttribute" => PluginDataFieldsGenerator.BoltFieldInfos,
                    "BoltCircleFieldsAttribute" => PluginDataFieldsGenerator.BoltCircleFieldInfos,
                    _ => []
                };

                var modelObjectType = attName.Substring(0, attName.Length - 15);

                foreach (var fieldInfo in fieldInfos) {
                    foreach (var nameOrNumber in kvp.Value) {
                        builder.AppendLine(
                            $"            {ToPrivateFieldNameStyle(modelObjectType)}{nameOrNumber}{fieldInfo.Name} = data.{modelObjectType}{nameOrNumber}{fieldInfo.Name};");
                    }
                }
            }
        }

        return $"        private void GetFieldValuesFrom({fieldsInfo.DataType} data) {{\n" +
               $"{builder}" +
               $"        }}";
    }
}