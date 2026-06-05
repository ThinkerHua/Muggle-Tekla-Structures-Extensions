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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    internal static class GeneratorHelper {
        
        internal static string GetAttributeQualifiedName(AttributeSyntax attributeSyntax, SemanticModel semanticModel) {
            var attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            return attributeTypeInfo.Type?.ToDisplayString();
        }

        internal static string GetAttributeName(AttributeSyntax attributeSyntax, SemanticModel semanticModel) {
            var attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            var qualifiedName = attributeTypeInfo.Type?.ToDisplayString();
            return qualifiedName?.Substring(qualifiedName.LastIndexOf('.') + 1);
        }

        internal static string GetUnqualifiedName(string qualifiedName) {
            return qualifiedName?.Substring(qualifiedName.LastIndexOf('.') + 1);
        }

        internal static string ToPrivateFieldNameStyle(string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (Regex.IsMatch(name, "^_[a-z]")) return name;

            while (name.StartsWith("_")) { name = name.Substring(1); }

            var match = Regex.Match(name, "^[A-Z]");
            if (match.Success) {
                name = "_" + match.Groups[0].Value.ToLower() + name.Substring(1);
            } else {
                name = "_" + name;
            }

            return name;
        }

        internal static string ToPropertyNameStyle(string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (Regex.IsMatch(name, "^[A-Z]")) return name;

            while (name.StartsWith("_")) { name = name.Substring(1); }

            name = name.Substring(0, 1).ToUpper() + name.Substring(1);

            return name;
        }

        internal static string ToLocalVariableNameStyle(string name) {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (Regex.IsMatch(name, "^[a-z]")) return name;

            while (name.StartsWith("_")) { name = name.Substring(1); }

            name = name.Substring(0, 1).ToLower() + name.Substring(1);

            return name;
        }

        internal static bool TryGetMatchedAttributes(
            IEnumerable<AttributeListSyntax> attributeLists,
            SemanticModel semanticModel,
            IEnumerable<string> matchAttributes,
            ref AttributeSyntax[] attributeSyntaxes) {
            var query = attributeLists
                .SelectMany(attListSyntax => attListSyntax.Attributes)
                .Where(attSyntax => matchAttributes.Contains(GetAttributeQualifiedName(attSyntax, semanticModel)))
                .ToArray();

            if (!query.Any()) return false;

            attributeSyntaxes = query;
            return true;
        }

        internal static string GetResourceAsString(string resourceName) {
            var assembly = typeof(GeneratorHelper).Assembly;
            var manifestResourceNames = assembly.GetManifestResourceNames();
            resourceName = manifestResourceNames.Single(x =>
                x.Equals($"Muggle.TsExtensions.CodingHelper.Resources.{resourceName}",
                    StringComparison.OrdinalIgnoreCase));

            using var stream = assembly.GetManifestResourceStream(resourceName) ??
                               throw new InvalidOperationException($"Resource '{resourceName}' not found.");
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        internal static IEnumerable<string> GetAttributeSourceTexts(IEnumerable<string> attributeQualifiedNames) =>
            attributeQualifiedNames.Select(name =>
                GetResourceAsString($"{name.Substring(name.LastIndexOf('.') + 1)}.cs"));

        /// <summary>
        /// Get default value from a constructor declaration syntax of attribute.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree which only contains one class declaration syntax of attribute.</param>
        /// <param name="attributeName">The declared attribute name.</param>
        /// <returns>A dictionary of default values,
        /// Key - parameter, which is some kine of property of model object,
        /// Value - argument, which is the value of property.</returns>
        internal static Dictionary<string, string> GetDefaultValuesFromSyntaxTree(
            SyntaxTree syntaxTree, out string attributeName) {

            var attClassDeclarationSyntax = syntaxTree.GetRoot().DescendantNodes()
                .First(node => node is ClassDeclarationSyntax) as ClassDeclarationSyntax;
            attributeName = attClassDeclarationSyntax!.Identifier.ValueText;

            var dict = new Dictionary<string, string>();

            var paramListSyntax = attClassDeclarationSyntax
                .DescendantNodes().OfType<ConstructorDeclarationSyntax>().Last()
                .DescendantNodes().OfType<ParameterListSyntax>().Single();

            for (int i = 1; i < paramListSyntax.Parameters.Count; i++) {
                var parameterSyntax = paramListSyntax.Parameters[i];
                var parameter = parameterSyntax.Identifier.ValueText;
                var argument = parameterSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .Single().Token.ValueText;
                dict.Add(parameter, argument);
            }

            return dict;
        }
    }
}