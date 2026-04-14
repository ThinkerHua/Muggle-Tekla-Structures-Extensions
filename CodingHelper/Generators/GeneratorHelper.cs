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
 *  GeneratorHelper.cs: help generators to gather information.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Muggle.TsExtensions.CodingHelper.Generators.Information;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    internal static class GeneratorHelper {
        internal static PluginDataFieldsInfo GetClassInfo(GeneratorSyntaxContext syntaxContext, CancellationToken token,
            IReadOnlyCollection<string> matchTheseAttributes) {
            if (syntaxContext.Node is not ClassDeclarationSyntax classDeclarationSyntax) return default;

            var attributesArguments = new ArgumentsDictionary<NameOrNumberSet>();

            foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists) {
                foreach (var attributeSyntax in attributeListSyntax.Attributes) {
                    if (token.IsCancellationRequested) continue;

                    var attributeTypeInfo = syntaxContext.SemanticModel.GetTypeInfo(attributeSyntax, token);
                    var attributeDisplayName = attributeTypeInfo.Type?.ToDisplayString();
                    if (!matchTheseAttributes.Contains(attributeDisplayName)) continue;
                    var attributeName = attributeDisplayName!.Substring(attributeDisplayName!.LastIndexOf('.') + 1);

                    if (!attributesArguments.TryGetValue(attributeName!, out NameOrNumberSet argumentSet)) {
                        argumentSet = [];
                        attributesArguments.Add(attributeName, argumentSet);
                    }

                    if (attributeSyntax.ArgumentList == null) continue;
                    var arguments = attributeSyntax.ArgumentList.Arguments
                        .SelectMany(argumentSyntax =>
                            argumentSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>())
                        .Select(expressionSyntax => expressionSyntax.Token.ValueText);

                    foreach (var argument in arguments) {
                        argumentSet.Add(argument);
                    }
                }
            }

            if (attributesArguments.Count == 0 || attributesArguments.All(kvp => kvp.Value.Count == 0)) return default;

            var classSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;

            return new PluginDataFieldsInfo {
                ClassInfo = new ClassInfo {
                    Name = classSymbol.Name,
                    NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                    Accessibility = classSymbol.DeclaredAccessibility,
                    IsRecord = classSymbol!.IsRecord,
                },
                Arguments = attributesArguments
            };
        }

        internal static string ToPrivateFieldNameStyle(string name) {
            if (Regex.Match(name, "^_[a-z]").Success) return name;

            while (name.StartsWith("_")) { name = name.Substring(1); }

            var match = Regex.Match(name, "^[A-Z]");
            if (match.Success) { name = "_" + match.Groups[0].Value.ToLower() + name.Substring(1); } else {
                name = "_" + name;
            }

            return name;
        }

        internal static bool TryGetSpecificAttributes(
            IEnumerable<AttributeListSyntax> attributeLists,
            SemanticModel semanticModel,
            IEnumerable<string> matchAttributes,
            ref IEnumerable<AttributeSyntax> attributeSyntaxes) {

            if (matchAttributes == null || !matchAttributes.Any()) return false;

            var query = attributeLists
                .SelectMany(attListSyntax => attListSyntax.Attributes)
                .Where(attSyntax => {
                    var attTypeInfo = semanticModel.GetTypeInfo(attSyntax);
                    var attQualifiedName = attTypeInfo.Type?.ToDisplayString();
                    return matchAttributes.Contains(attQualifiedName);
                }).ToArray();

            if (query.Any()) {
                attributeSyntaxes = query;
                return true;
            }

            return false;
        }
    }
}