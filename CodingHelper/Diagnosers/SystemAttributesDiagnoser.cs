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
 *  SystemAttributesDiagnoser.cs: diagnoser for attributes provided by Tekla Structures.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

#pragma warning disable RS2008

using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Diagnosers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class SystemAttributesDiagnoser : DiagnosticAnalyzer {
    internal static readonly string[] ConcernedAttributes = [
        "Tekla.Structures.Dialog.StructuresDialogAttribute",
        "Tekla.Structures.Plugins.StructuresFieldAttribute"
    ];

    internal static readonly DiagnosticDescriptor LengthExceedLimitationDescriptor = new(
        "MTSECH006",
        "Attribute name too long",
        "The attribute name must be 1 to 19 characters",
        InternalAttributesDiagnoser.Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsSpecialCharacters = new(
        "MTSECH007",
        "Attribute name contains special characters",
        "Attribute name must not contain special characters",
        InternalAttributesDiagnoser.Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsUnsuggestedCharacters = new(
        "MTSECH008",
        "Attribute name contains unsuggested characters",
        "It is not recommended to use characters other than \"_\", \"A-Z\", \"a-z\", \"0-9\"",
        InternalAttributesDiagnoser.Category,
        DiagnosticSeverity.Info,
        true);

    internal static readonly DiagnosticDescriptor UseMathematicalConstant = new(
        "MTSECH009",
        "Attribute name use a mathematical constant",
        "Attribute name cannot use a mathematical constants, such as PI or e",
        InternalAttributesDiagnoser.Category,
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        LengthExceedLimitationDescriptor, ContainsSpecialCharacters, ContainsUnsuggestedCharacters,
        UseMathematicalConstant
    ];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.Attribute);
    }

    internal static void AnalyzeArgument(SyntaxNodeAnalysisContext context) {
        var attributeSyntax = (AttributeSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        if (!ConcernedAttributes.Contains(GetAttributeQualifiedName(attributeSyntax, semanticModel))) return;

        var argumentSyntax = attributeSyntax.ArgumentList?.Arguments[0];
        if (argumentSyntax == null) return;

        var argumentLes = argumentSyntax.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault();
        var argument = argumentLes?.Token.ValueText;
        if (argument == null) return;

        var location = argumentSyntax.GetLocation();

        if (argument.Length is 0 or > 19) {
            context.ReportDiagnostic(Diagnostic.Create(LengthExceedLimitationDescriptor, location));
        }

        if (argument is "PI" or "e") {
            context.ReportDiagnostic(Diagnostic.Create(UseMathematicalConstant, location));
        }

        if (Regex.IsMatch(argument, InternalAttributesDiagnoser.SpecialCharacterPattern)) {
            context.ReportDiagnostic(Diagnostic.Create(ContainsSpecialCharacters, location));
        }

        if (Regex.IsMatch(argument, InternalAttributesDiagnoser.UnsuggestedCharacterPattern)) {
            context.ReportDiagnostic(Diagnostic.Create(ContainsUnsuggestedCharacters, location));
        }
    }
}