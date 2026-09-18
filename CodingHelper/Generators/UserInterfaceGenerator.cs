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
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
public class UserInterfaceGenerator : IIncrementalGenerator {
    internal const string ConcernedAttribute = "Muggle.TsExtensions.CodingHelper.Generators.UserInterfaceAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => {
            ctx.AddSource("UserInterfaceType.g.cs",
                SourceText.From(GetResourceAsString("UserInterfaceType.cs"), Encoding.UTF8));
            ctx.AddSource("UserInterfaceAttribute.g.cs",
                SourceText.From(GetResourceAsString("UserInterfaceAttribute.cs"), Encoding.UTF8));
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

        var userInterfaceInfo = provider
            .Select(static (x, _) => x.Value)
            .Where(x => x != default)
            .Combine(projectDirProvider);
        context.RegisterSourceOutput(userInterfaceInfo, Generate);
    }

    private static GatheredInfo<UserInterfaceInfo> Transform(
        GeneratorAttributeSyntaxContext context,
        CancellationToken token) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var semanticModel = context.SemanticModel;

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
                        Expression: IdentifierNameSyntax {
                            Identifier.ValueText: "nameof"
                        }
                    } invocationExpressionSyntax => semanticModel
                        .GetSymbolInfo(invocationExpressionSyntax.ArgumentList.Arguments[0].Expression, token)
                        .Symbol?.ToDisplayString() ?? string.Empty,
                    _ => string.Empty
                };
            }
        }

        var builder = ImmutableArray.CreateBuilder<DiagnosticInfo>();
        if (uiType is UserInterfaceType.WPF && string.IsNullOrEmpty(viewModelQualifiedName)) {
            builder.Add(new DiagnosticInfo(NeedViewModelQualifiedName, viewModelArgLocation, []));
            return new GatheredInfo<UserInterfaceInfo>(
                new UserInterfaceInfo(uiType, viewModelQualifiedName, default),
                builder.ToImmutableArray());
        }

        var pluginDataInfo = PluginDataFieldsGenerator.Transform(classDeclarationSyntax, semanticModel, token);
        builder.AddRange(pluginDataInfo.Item2);

        return new GatheredInfo<UserInterfaceInfo>(
            new UserInterfaceInfo(uiType, viewModelQualifiedName, pluginDataInfo.Item1),
            builder.ToImmutableArray());
    }

    private static void Generate(SourceProductionContext context, (UserInterfaceInfo uiInfo, string projectDir) info) {
        IUserInterfaceGenerator generator = info.uiInfo.UiType switch {
            /*UserInterfaceType.INP => InpUiGenerator.Instance,
            UserInterfaceType.WinForm => WinFormUiGenerator.Instance,*/
            UserInterfaceType.WPF => WpfUiGenerator.Instance,
            _ => throw new ArgumentOutOfRangeException()
        };
        generator.Generate(context, info.uiInfo, info.projectDir);
    }

}