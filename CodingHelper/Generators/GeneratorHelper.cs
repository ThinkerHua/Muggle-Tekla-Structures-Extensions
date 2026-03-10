/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  GeneratorHelper.cs: help generators to gather information.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    internal static class GeneratorHelper {
        internal static AppliedClassInfo? GetClassInfo(GeneratorSyntaxContext syntaxContext, CancellationToken token,
            IReadOnlyCollection<string> matchTheseAttributes) {
            if (syntaxContext.Node is not ClassDeclarationSyntax classDeclarationSyntax) return null;

            var attributesArguments = new Dictionary<string, HashSet<string>>();

            foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists) {
                foreach (var attributeSyntax in attributeListSyntax.Attributes) {
                    if (token.IsCancellationRequested) continue;

                    var attributeTypeInfo = syntaxContext.SemanticModel.GetTypeInfo(attributeSyntax, token);
                    var attributeDisplayName = attributeTypeInfo.Type?.ToDisplayString();
                    if (!matchTheseAttributes.Contains(attributeDisplayName)) continue;
                    var attributeName = attributeDisplayName!.Substring(attributeDisplayName!.LastIndexOf('.') + 1);

                    if (!attributesArguments.TryGetValue(attributeName!, out HashSet<string> argumentSet)) {
                        argumentSet = new HashSet<string>();
                        attributesArguments.Add(attributeName, argumentSet);
                    }

                    if (attributeSyntax.ArgumentList == null) continue;
                    var arguments = attributeSyntax.ArgumentList.Arguments.SelectMany(argumentSyntax =>
                        argumentSyntax.ChildNodes().OfType<ExpressionSyntax>().SelectMany(es => {
                            if (es is CollectionExpressionSyntax collectionExpressionSyntax) {
                                return collectionExpressionSyntax.Elements.SelectMany(ces =>
                                    ces.ChildNodes().OfType<LiteralExpressionSyntax>());
                            } else if (es is LiteralExpressionSyntax literalExpressionSyntax) {
                                return [literalExpressionSyntax];
                            }

                            return [];
                        }).Select(expressionSyntax => expressionSyntax.Token.ValueText));

                    foreach (var argument in arguments) {
                        argumentSet.Add(argument);
                    }
                }
            }

            if (attributesArguments.Count == 0 || attributesArguments.All(kvp => kvp.Value.Count == 0)) return null;

            var classSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var isPartial = classSymbol!.DeclaringSyntaxReferences
                .Select(sr => sr.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault(tds => tds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) != null;
            if (!isPartial) return null;

            return new AppliedClassInfo {
                Name = classSymbol.Name,
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol!.IsRecord,
                AttributesInfo = attributesArguments
            };
        }
    }
}