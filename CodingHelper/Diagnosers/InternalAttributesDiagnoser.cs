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
 *  InternalAttributesDiagnoser.cs: diagnoser for attributes within "Muggle.TsExtensions.CodingHelper.Generators" namespace.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Muggle.TsExtensions.CodingHelper.Diagnosers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class InternalAttributesDiagnoser : DiagnosticAnalyzer {
    internal const string Category = "Muggle.TsExtensions.CodingHelper";
    internal const string SpecialCharacterPattern = """[`~!@#$%\^&\*\(\)\-\+=\[\]\{}\|\\;:'",\.<>/?\s]""";
    internal const string UnsuggestedCharacterPattern = "[^0-9A-Za-z_]";

    internal static readonly string[] ConcernedAttributes = [
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

    internal static DiagnosticDescriptor NotPartialDescriptor => new DiagnosticDescriptor(
        "MTSECH001",
        "Target class must be partial",
        "Cannot generate fields or properties for '{0}' because it is not partial.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static DiagnosticDescriptor NotImplementINotifyPropertyChangedDescriptor => new DiagnosticDescriptor(
        "MTSECH002",
        "INotifyPropertyChanged not implemented",
        "A class that applied '{0}' must implement 'INotifyPropertyChanged' interface. " +
        "Simplified inherit it from 'ConnectionViewModel' or 'DetailViewModel' or 'NotificationObject' " +
        "within 'Muggle.TsExtensions.CodingHelper.Generators' namespace, " +
        "or directly implement 'INotifyPropertyChanged' interface.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static DiagnosticDescriptor LengthExceedLimitationDescriptor => new DiagnosticDescriptor(
        "MTSECH003",
        "Name or number too long",
        "Due to the limitation of Tekla Structures, " +
        "the name length of plugin data variables must not exceed 19. " +
        "Considering the prefix and suffix (such as {1}), " +
        "the argument for '{0}' must be 1 to {2} characters.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static DiagnosticDescriptor ContainsSpecialCharacters => new DiagnosticDescriptor(
        "MTSECH004",
        "Name contains special characters",
        "Name must not contain special characters.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static DiagnosticDescriptor ContainsUnsuggestedCharacters => new DiagnosticDescriptor(
        "MTSECH005",
        "Name contains unsuggested characters",
        "It is not recommended to use characters other than \"_\", \"A-Z\", \"a-z\", \"0-9\".",
        Category,
        DiagnosticSeverity.Info,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        NotPartialDescriptor, NotImplementINotifyPropertyChangedDescriptor, LengthExceedLimitationDescriptor,
        ContainsSpecialCharacters, ContainsUnsuggestedCharacters
    ];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIfIsPartial, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeArgumentLength, SyntaxKind.Attribute);
    }

    internal static void AnalyzeIfIsPartial(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var attributes = classDeclarationSyntax.AttributeLists.SelectMany(list => list.Attributes).ToArray();

        var matchedAttributes = attributes.Where(att => {
            var attTypeInfo = semanticModel.GetTypeInfo(att);
            var attQualifiedName = attTypeInfo.Type?.ToDisplayString();
            if (ConcernedAttributes.Contains(attQualifiedName)) return true;
            return false;
        }).ToArray();

        if (!matchedAttributes.Any()) return;

        var isPartial = classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (isPartial) return;

        foreach (var attribute in matchedAttributes) {
            var location = attribute.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(NotPartialDescriptor, location,
                classDeclarationSyntax.Identifier.ValueText));
        }
    }

    internal static void AnalyzeArgumentLength(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;
        var attributeSyntax = (AttributeSyntax)context.Node;
        var typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
        var attributeQualifiedName = typeInfo.Type?.ToDisplayString();
        if (!ConcernedAttributes.Contains(attributeQualifiedName)) return;

        var expressionSyntaxes = attributeSyntax.ArgumentList?.Arguments
            .SelectMany(aas => aas.ChildNodes().OfType<ExpressionSyntax>().SelectMany(es => es switch {
                    CollectionExpressionSyntax collectionExpressionSyntax =>
                        collectionExpressionSyntax.Elements.SelectMany(ces =>
                            ces.ChildNodes().OfType<LiteralExpressionSyntax>()),
                    LiteralExpressionSyntax literalExpressionSyntax => [literalExpressionSyntax],
                    _ => []
                }
            )).ToArray();
        if (expressionSyntaxes == null || !expressionSyntaxes.Any()) return;

        var attributeName = attributeQualifiedName?.Substring(attributeQualifiedName.LastIndexOf('.') + 1);
        var example = DescriptorExample(attributeName);
        var maxLength = MaxLengthOfArgument(attributeName);
        foreach (var expressionSyntax in expressionSyntaxes) {
            var argument = expressionSyntax.Token.ValueText;

            var match = Regex.Match(argument, SpecialCharacterPattern);
            if (match.Success) {
                context.ReportDiagnostic(Diagnostic.Create(ContainsSpecialCharacters, expressionSyntax.GetLocation()));
            }

            match = Regex.Match(argument, UnsuggestedCharacterPattern);
            if (match.Success) {
                context.ReportDiagnostic(Diagnostic.Create(ContainsUnsuggestedCharacters,
                    expressionSyntax.GetLocation()));
            }

            if (argument.Length > 0 && argument.Length <= maxLength) continue;

            context.ReportDiagnostic(Diagnostic.Create(
                LengthExceedLimitationDescriptor,
                expressionSyntax.GetLocation(),
                [attributeName, example, maxLength]));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    internal static string DescriptorExample(string attributeName) {
        return attributeName switch {
            "PartFieldsAttribute" or "PartPropertiesAttribute" => "PT<nameOrNumber>MATL",
            "PlateFieldsAttribute" or "PlatePropertiesAttribute" => "PL<nameOrNumber>MATL",
            "WeldFieldsAttribute" or "WeldPropertiesAttribute" => "W<nameOrNumber>SIZEA",
            "BoltFieldsAttribute" or "BoltPropertiesAttribute" => "B<nameOrNumber>DISTX",
            "BoltCircleFieldsAttribute" or "BoltCirclePropertiesAttribute" => "BC<nameOrNumber>PLAIN",
            _ => string.Empty
        };
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

    internal static void AnalyzeInterface(SyntaxNodeAnalysisContext context) {
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
                NotImplementINotifyPropertyChangedDescriptor,
                classDeclarationSyntax.Identifier.GetLocation(),
                attribute.Substring(attribute.LastIndexOf('.') + 1)
            ));
        }
    }
}