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
 *  GeneratorAnalyzer.cs: code analyzer for generators within "Muggle.TsExtensions.CodingHelper.Analyzers" namespace.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Muggle.TsExtensions.CodingHelper.Analyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class GeneratorAnalyzer : DiagnosticAnalyzer {
        private const string IdNotPartial = "MTSECH001";
        private const string TitleNotPartial = "Target class must be partial";

        private const string MessageFormatNotPartial =
            "Cannot generate fields or properties for '{0}' because it is not partial.";

        private static DiagnosticDescriptor NotPartialDescriptor => new DiagnosticDescriptor(
            IdNotPartial, TitleNotPartial, MessageFormatNotPartial, Category, DiagnosticSeverity.Error, true);

        private const string IdNotImplementINotifyPropertyChanged = "MTSECH002";
        private const string TitleNotImplementINotifyPropertyChanged = "INotifyPropertyChanged not implemented";

        private const string MessageFormatNotImplementINotifyPropertyChanged =
            "A class that applied '{0}' must implement 'INotifyPropertyChanged' interface. " +
            "Simplified inherit it from 'ConnectionViewModel' or 'DetailViewModel' or 'NotificationObject' " +
            "within 'Muggle.TsExtensions.CodingHelper.Generators' namespace, " +
            "or directly implement 'INotifyPropertyChanged' interface.";

        private static DiagnosticDescriptor NotImplementINotifyPropertyChanged => new DiagnosticDescriptor(
            IdNotImplementINotifyPropertyChanged, TitleNotImplementINotifyPropertyChanged,
            MessageFormatNotImplementINotifyPropertyChanged, Category, DiagnosticSeverity.Error, true);

        private const string IdLengthExceedLimitation = "MTSECH003";
        private const string TitleLengthExceedLimitation = "Name or number too long";

        private const string MessageFormatLengthExceedLimitation =
            "Due to the limitation of Tekla Structures, " +
            "the name length of plugin data variables must not exceed 19. " +
            "Considering the prefix and suffix (such as {1}), " +
            "the argument for '{0}' must be 1 to {2} characters.";

        private static DiagnosticDescriptor LengthExceedLimitationDescriptor => new DiagnosticDescriptor(
            IdLengthExceedLimitation, TitleLengthExceedLimitation, MessageFormatLengthExceedLimitation, Category,
            DiagnosticSeverity.Error, true);

        private const string Category = "Muggle.TsExtensions.CodingHelper";

        private readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PartPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlatePropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltPropertiesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCirclePropertiesAttribute"
        ];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
            NotPartialDescriptor, NotImplementINotifyPropertyChanged, LengthExceedLimitationDescriptor
        ];

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeIfIsPartial, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeArgumentLength, SyntaxKind.Attribute);
        }

        private void AnalyzeIfIsPartial(SyntaxNodeAnalysisContext context) {
            var semanticModel = context.SemanticModel;
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var attributes = classDeclarationSyntax.AttributeLists.SelectMany(list => list.Attributes).ToArray();
            foreach (var attribute in attributes) {
                var typeInfo = semanticModel.GetTypeInfo(attribute);
                var attDisplayName = typeInfo.Type.ToDisplayString();
            }

            var matchedAttributes = attributes.Where(att => {
                var attTypeInfo = semanticModel.GetTypeInfo(att);
                var attDisplayName = attTypeInfo.Type?.ToDisplayString();
                if (ConcernedAttributes.Contains(attDisplayName)) return true;
                return false;
            }).ToArray();

            if (!matchedAttributes.Any()) return;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var partialDeclaration = classSymbol?.DeclaringSyntaxReferences
                .Select(sr => sr.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault(tds => tds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

            if (partialDeclaration != null) return;

            foreach (var attribute in matchedAttributes) {
                var attSymbol = semanticModel.GetSymbolInfo(attribute);
                var name = attribute.Name;
                var location = attribute.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.GetLocation();

                context.ReportDiagnostic(Diagnostic.Create(NotPartialDescriptor, location,
                    classDeclarationSyntax.Identifier.ValueText));
            }
        }

        private void AnalyzeArgumentLength(SyntaxNodeAnalysisContext context) {
            var semanticModel = context.SemanticModel;
            var attributeSyntax = (AttributeSyntax)context.Node;
            var typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            var attributeQualifiedName = typeInfo.Type?.ToDisplayString();
            if (!ConcernedAttributes.Contains(attributeQualifiedName)) return;

            var expressionSyntaxes = attributeSyntax.ArgumentList?.Arguments
                .SelectMany(aas => aas.ChildNodes().OfType<ExpressionSyntax>().SelectMany(es => {
                    if (es is CollectionExpressionSyntax collectionExpressionSyntax) {
                        return collectionExpressionSyntax.Elements.SelectMany(ces =>
                            ces.ChildNodes().OfType<LiteralExpressionSyntax>());
                    } else if (es is LiteralExpressionSyntax literalExpressionSyntax) {
                        return [literalExpressionSyntax];
                    }

                    return [];
                })).ToArray();
            if (expressionSyntaxes == null || !expressionSyntaxes.Any()) return;

            var attributeName = attributeQualifiedName?.Substring(attributeQualifiedName.LastIndexOf('.') + 1);
            var example = attributeName switch {
                "PartFieldsAttribute" or "PartPropertiesAttribute" => "PT<nameOrNumber>MATL",
                "PlateFieldsAttribute" or "PlatePropertiesAttribute" => "PL<nameOrNumber>MATL",
                "WeldFieldsAttribute" or "WeldPropertiesAttribute" => "W<nameOrNumber>SIZEA",
                "BoltFieldsAttribute" or "BoltPropertiesAttribute" => "B<nameOrNumber>DISTX",
                "BoltCircleFieldsAttribute" or "BoltCirclePropertiesAttribute" => "BC<nameOrNumber>PLAIN",
                _ => string.Empty
            };
            var maxLength = MaxLengthOfArgument(attributeName);
            foreach (var expressionSyntax in expressionSyntaxes) {
                var argument = expressionSyntax.Token.ValueText;
                if (argument.Length > 0 && argument.Length <= maxLength) continue;

                context.ReportDiagnostic(Diagnostic.Create(
                    LengthExceedLimitationDescriptor,
                    expressionSyntax.GetLocation(),
                    [attributeName, example, maxLength]));
            }
        }

        /// <summary>
        ///     Return the character length of the arguments passed to the constructor of specified attribute.
        /// </summary>
        /// <remarks>
        ///     Only supports the attributes within the "Muggle.TsExtensions.CodingHelper" namespace.
        ///     The return value for unsported attributes is <see cref="int.MinValue" />.
        /// </remarks>
        /// <param name="attributeName">The name of the specified attribute.</param>
        /// <returns>The character lenght of the arguments.</returns>
        internal static int MaxLengthOfArgument(string attributeName) {
            return attributeName switch {
                "PartFieldsAttribute" or "PartPropertiesAttribute" => 13,
                "PlateFieldsAttribute" or "PlatePropertiesAttribute" => 13,
                "WeldFieldsAttribute" or "WeldPropertiesAttribute" => 13,
                "BoltFieldsAttribute" or "BoltPropertiesAttribute" => 13,
                "BoltCircleFieldsAttribute" or "BoltCirclePropertiesAttribute" => 12,
                _ => int.MinValue
            };
        }

        private void AnalyzeInterface(SyntaxNodeAnalysisContext context) {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            var attributeSyntaxes = classDeclarationSyntax.AttributeLists.SelectMany(als => als.Attributes).ToArray();
            var concernedAttributes = ConcernedAttributes.Skip(5);
            var appliedAttributes = attributeSyntaxes
                .Select(attSyntax => semanticModel.GetTypeInfo(attSyntax).Type?.ToDisplayString())
                .Where(name => concernedAttributes.Contains(name)).ToArray();
            if (!appliedAttributes.Any()) return;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (classSymbol == null || classSymbol.AllInterfaces.Any(i =>
                    i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged"))
                return;

            foreach (var attribute in appliedAttributes) {
                context.ReportDiagnostic(Diagnostic.Create(
                    NotImplementINotifyPropertyChanged,
                    classDeclarationSyntax.Identifier.GetLocation(),
                    attribute.Substring(attribute.LastIndexOf('.') + 1)
                ));
            }
        }
    }
}