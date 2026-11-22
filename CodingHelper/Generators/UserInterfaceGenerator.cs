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
 *  UserInterfaceGenerator.cs: help to generate user interface for plugin.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
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
public class UserInterfaceGenerator : IIncrementalGenerator {
    internal const string ConcernedAttribute = "Muggle.TsExtensions.CodingHelper.Generators.UserInterfaceAttribute";

    private const string PluginAttribute = "Tekla.Structures.Plugins.PluginAttribute";
    private const string StructuresFieldAttribute = "Tekla.Structures.Plugins.StructuresFieldAttribute";
    private const string SecondaryTypeAttribute = "Tekla.Structures.Plugins.SecondaryTypeAttribute";
    private const string SeamInputTypeAttribute = "Tekla.Structures.Plugins.SeamInputTypeAttribute";
    private const string PluginUserInterfaceAttribute = "Tekla.Structures.Plugins.PluginUserInterfaceAttribute";
    private const string SecondaryZero = "Tekla.Structures.Plugins.ConnectionBase.SecondaryType.SECONDARYTYPE_ZERO";

    private static readonly string[] BaseTypes = [
        "Tekla.Structures.Plugins.ConnectionBase",
        "Tekla.Structures.Plugins.CustomPartBase",
        "Tekla.Structures.Plugins.PluginBase",
        "Tekla.Structures.Plugins.DrawingPluginBase",
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => {
            ctx.AddSource("UserInterfaceType.g.cs",
                SourceText.From(GetResourceAsString("UserInterfaceType.cs"), Encoding.UTF8));
            ctx.AddSource("UserInterfaceAttribute.g.cs",
                SourceText.From(GetResourceAsString("UserInterfaceAttribute.cs"), Encoding.UTF8));
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

        var projectDirProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => {
                provider.GlobalOptions.TryGetValue("build_property.projectdir", out var dir);
                return dir;
            });

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(ConcernedAttribute, static (_, _) => true, Transform);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var userInterfaceInfo = provider.Select(static (x, _) => x.Value).Where(x => x != default);
        context.RegisterSourceOutput(userInterfaceInfo.Combine(versionProvider.Combine(projectDirProvider)), GenerateUiClass);
        context.RegisterSourceOutput(userInterfaceInfo, GeneratePluginClass);
    }

    private static GatheredInfo<UserInterfaceInfo> Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        var attData = context.Attributes.Single();
        var attSyntax = (AttributeSyntax)attData.ApplicationSyntaxReference!.GetSyntax(token);

        var uiType = UserInterfaceType.WPF;
        var viewModelQualifiedName = string.Empty;
        Location viewModelArgLocation = null;
        for (int i = 0; i < attSyntax.ArgumentList!.Arguments.Count; i++) {
            var argument = attSyntax.ArgumentList.Arguments[i];
            if (argument.NameColon is null && i == 0 ||
                argument.NameColon?.Name.Identifier.ValueText == "uiType") {
                uiType = argument.Expression switch {
                    CastExpressionSyntax castExpressionSyntax =>
                        (UserInterfaceType)Enum.Parse(typeof(UserInterfaceType),
                            castExpressionSyntax.Expression.ToString(), false),
                    _ => (UserInterfaceType)Enum.Parse(typeof(UserInterfaceType),
                        GetUnqualifiedName(argument.Expression.ToString()), false)
                };
            } else if (argument.NameColon is null && i == 1 ||
                       argument.NameColon?.Name.Identifier.ValueText == "viewModelQualifiedName") {
                viewModelArgLocation = argument.GetLocation();
                viewModelQualifiedName = argument.Expression switch {
                    LiteralExpressionSyntax literalExpressionSyntax => literalExpressionSyntax.Token.ValueText,
                    InvocationExpressionSyntax {
                        Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" }
                    } invocationExpressionSyntax => semanticModel
                        .GetSymbolInfo(invocationExpressionSyntax.ArgumentList.Arguments[0].Expression, token)
                        .Symbol?.ToDisplayString() ?? string.Empty,
                    _ => string.Empty
                };
            }
        }

        var gatheredInfo = new GatheredInfo<UserInterfaceInfo>(default, default);
        var builder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) {
            builder.Add(new DiagnosticInfo(NotPartial, classDeclarationSyntax.Identifier.GetLocation(),
                [classDeclarationSyntax.Identifier.ValueText]));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        var baseTypeSyntax = classDeclarationSyntax.BaseList?.ChildNodes().OfType<SimpleBaseTypeSyntax>()
            .FirstOrDefault();
        if (baseTypeSyntax is null || !BaseTypes.Any(baseType => baseType.Contains(baseTypeSyntax.Type.ToString()))) {
            builder.Add(new DiagnosticInfo(BaseTypeNotFound, classDeclarationSyntax.Identifier.GetLocation(), []));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                [PluginUserInterfaceAttribute], out var pluginUserInterfaceAttSyntaxes)) {
            builder.Add(new DiagnosticInfo(ShouldRemovePluginUserInterfaceAttribute,
                pluginUserInterfaceAttSyntaxes.First().GetLocation(), []));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        if (uiType is UserInterfaceType.WPF && string.IsNullOrEmpty(viewModelQualifiedName)) {
            builder.Add(new DiagnosticInfo(NeedViewModelQualifiedName, viewModelArgLocation, []));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        ConstructorDeclarationSyntax constructorSyntax;
        try {
            constructorSyntax = classDeclarationSyntax.ChildNodes().OfType<ConstructorDeclarationSyntax>().Single();
        } catch (InvalidOperationException) {
            builder.Add(new DiagnosticInfo(UniqueConstructorNotFound, attSyntax.GetLocation(), []));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        ParameterSyntax parameterSyntax;
        try {
            parameterSyntax = constructorSyntax.ParameterList.Parameters.Single();
        } catch (InvalidOperationException) {
            builder.Add(new DiagnosticInfo(UniqueParameterNotFound, attSyntax.GetLocation(), []));
            gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();
            return gatheredInfo;
        }

        var pluginType = PluginTypes.Connection;
        switch (GetUnqualifiedName(baseTypeSyntax.Type.ToString())) {
        case "ConnectionBase":
            if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, [SeamInputTypeAttribute],
                    out _)) {
                pluginType = PluginTypes.Seam;
                break;
            }

            if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, [SecondaryTypeAttribute],
                    out var distinction)) {
                var expression = distinction.First().ArgumentList?.Arguments.First().Expression.ToString();
                if (expression == "0" || !string.IsNullOrEmpty(expression) && SecondaryZero.Contains(expression))
                    pluginType = PluginTypes.Detail;
            }

            break;
        case "CustomPartBase":
            pluginType = PluginTypes.CustomPart;
            break;
        case "PluginBase":
            pluginType = PluginTypes.Component;
            break;
        case "DrawingPluginBase":
            pluginType = PluginTypes.DrawingPlugin;
            break;
        }

        var pluginName = string.Empty;
        if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, [PluginAttribute],
                out var pluginAttSyntaxes)) {
            pluginName = pluginAttSyntaxes.FirstOrDefault()?.DescendantNodes().OfType<LiteralExpressionSyntax>()
                .FirstOrDefault()?.Token.ValueText;
        }

        if (string.IsNullOrEmpty(pluginName)) pluginName = classSymbol.Name;

        var dataType = semanticModel.GetTypeInfo(parameterSyntax.Type!).ConvertedType!;
        var dataClassRegisteredIds = PluginFieldsGenerator.AnalyzePluginDataRegisteredIds(dataType);

        var dataClassManualFields = (from fieldSymbol in dataType.GetMembers().OfType<IFieldSymbol>()
                                     from attribute in fieldSymbol.GetAttributes()
                                     where StructuresFieldAttribute.Contains(attribute.AttributeClass!.MetadataName)
                                     let attributeName = attribute.ConstructorArguments.First().Value!.ToString()
                                     select (type: fieldSymbol.Type.ToDisplayString(), attributeName))
            .ToArray();
        foreach (var field in dataClassManualFields) {
            if (!dataClassRegisteredIds.TryGetValue(field.type, out var idSet)) {
                idSet = [];
                dataClassRegisteredIds.Add(field.type, idSet);
            }

            idSet.Add(field.attributeName);
        }

        var userInterfaceInfo = new UserInterfaceInfo {
            UiType = uiType,
            ViewModelQualifiedName = viewModelQualifiedName,
            ClassInfo = new ClassInfo {
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
                Name = classSymbol.Name,
            },
            PluginType = pluginType,
            PluginName = pluginName,
            Arguments = dataClassRegisteredIds,
        };

        gatheredInfo.Value = userInterfaceInfo;
        gatheredInfo.DiagnosticInfos = builder.ToImmutableArray();

        return gatheredInfo;
    }

    private static void GenerateUiClass(SourceProductionContext context,
        (UserInterfaceInfo uiInfo, (Version tsmVersion, string projectDir)) info) {
        IUserInterfaceGenerator generator = info.uiInfo.UiType switch {
            UserInterfaceType.INP => InpUiGenerator.Instance,
            // UserInterfaceType.WinForm => WinFormUiGenerator.Instance,
            UserInterfaceType.WPF => WpfUiGenerator.Instance,
            _ => throw new ArgumentOutOfRangeException()
        };
        generator.Generate(context, info.uiInfo, info.Item2.tsmVersion, info.Item2.projectDir);
    }

    private static void GeneratePluginClass(SourceProductionContext context, UserInterfaceInfo info) {
        var classInfo = info.ClassInfo;
        var builder = new StringBuilder();

        var uiName = info.UiType switch {
            UserInterfaceType.INP => $"{classInfo.NameSpace}.InpUi.Definition",
            // UserInterfaceType.WinForm => $"\"{classInfo.NameSpace}.WinFormUi\"",
            UserInterfaceType.WPF => $"\"{classInfo.NameSpace}.WpfUi\"",
            _ => throw new ArgumentOutOfRangeException()
        };

        builder.AppendLine(
            $"namespace {classInfo.NameSpace} {{\r\n" +
            $"    [Tekla.Structures.Plugins.PluginUserInterfaceAttribute({uiName})]\r\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name};\r\n" +
            $"}}");

        context.AddSource($"{classInfo.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

}