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

#pragma warning disable RS2008
#define CompatibleWithViewModelPropertiesGenerator

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Muggle.TsExtensions.CodingHelper.Generators;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Diagnosers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class InternalAttributesDiagnoser : DiagnosticAnalyzer {
    internal const string Category = "Muggle.TsExtensions.CodingHelper";
    internal const string SpecialCharacterPattern = """[`~!@#$%\^&\*\(\)\-\+=\[\]\{}\|\\;:'",\.<>/?\s]""";
    internal const string UnsuggestedCharacterPattern = "[^0-9A-Za-z_]";

    internal static readonly DiagnosticDescriptor NotPartialDescriptor = new DiagnosticDescriptor(
        "MTSECH001",
        "Target class must be partial",
        "Cannot generate members for '{0}' because it is not partial",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NotImplementINotifyPropertyChangedDescriptor =
        new DiagnosticDescriptor(
            "MTSECH002",
            "INotifyPropertyChanged not implemented",
            "A class that applied '{0}' must implement 'INotifyPropertyChanged' interface. " +
            "Simplified inherit it from 'ConnectionViewModel' or 'DetailViewModel' or 'NotificationObject' " +
            "within 'Muggle.TsExtensions.CodingHelper.Generators' namespace, " +
            "or directly implement 'INotifyPropertyChanged' interface with an 'OnPropertyChanged(string propertyName)' method.",
            Category,
            DiagnosticSeverity.Error,
            true);

    internal static readonly DiagnosticDescriptor LengthExceedLimitationDescriptor = new DiagnosticDescriptor(
        "MTSECH003",
        "Name or number too long",
        "Due to the limitation of Tekla Structures, " +
        "the name length of plugin data variables must not exceed 19. " +
        "Considering the prefix and suffix (such as {1}), " +
        "the argument for '{0}' must be 1 to {2} characters.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsSpecialCharacters = new DiagnosticDescriptor(
        "MTSECH004",
        "Name contains special characters",
        "Name must not contain special characters",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsUnsuggestedCharacters = new DiagnosticDescriptor(
        "MTSECH005",
        "Name contains unsuggested characters",
        "It is not recommended to use characters other than \"_\", \"A-Z\", \"a-z\", \"0-9\"",
        Category,
        DiagnosticSeverity.Info,
        true);

    internal static readonly DiagnosticDescriptor FieldsFromAttributeNotApplied = new DiagnosticDescriptor(
        "MTSECH010",
        "\"FieldsFromAttribute\" not applied",
        "Must also apply \"FieldsFromAttribute\" when applied \"{0}\" on class",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor AppliedOnOverOnePlace = new DiagnosticDescriptor(
        "MTSECH011",
        "Applied on over one place",
        "These attributes should only be applied on one place (Class, Field or Property): " +
        "\"PartFieldDefaultValuesAttribute\", \"PlateFieldDefaultValuesAttribute\", \"WeldFieldDefaultValuesAttribute\", " +
        "\"BoltFieldDefaultValuesAttribute\", \"BoltCircleFieldDefaultValuesAttribute\"",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor DataTypeDoesNotContainTheseFields = new DiagnosticDescriptor(
        "MTSECH012",
        "Target data type doesn't contain these fields",
        "Ensure that target data type contain these fields",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor SetDefaultValueMultiTimes = new DiagnosticDescriptor(
        "MTSECH013",
        "Set default value multi times",
        "Should not set default value multi times",
        Category,
        DiagnosticSeverity.Error,
        true);

#if CompatibleWithViewModelPropertiesGenerator
    internal static readonly DiagnosticDescriptor AlreadyBeGeneratedByOldGenerator = new DiagnosticDescriptor(
        "MTSECH014",
        "Already be generated by old generator",
        "'{0}' has already registered default values for the same model object, so that '{1}' will not work",
        Category,
        DiagnosticSeverity.Warning,
        true);
#endif

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        NotPartialDescriptor, NotImplementINotifyPropertyChangedDescriptor, LengthExceedLimitationDescriptor,
        ContainsSpecialCharacters, ContainsUnsuggestedCharacters, FieldsFromAttributeNotApplied, AppliedOnOverOnePlace,
        DataTypeDoesNotContainTheseFields, SetDefaultValueMultiTimes,
#if CompatibleWithViewModelPropertiesGenerator
        AlreadyBeGeneratedByOldGenerator
#endif
    ];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIfIsPartial, SyntaxKind.ClassDeclaration, SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeArgumentLength, SyntaxKind.Attribute);
        context.RegisterSyntaxNodeAction(AnalyzePluginFieldDefaultValuesAttributeAppliedPlaces,
            SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeExistingFields, SyntaxKind.ClassDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeExistingProperties, SyntaxKind.ClassDeclaration);
    }

    internal static void AnalyzeIfIsPartial(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;

        IEnumerable<AttributeSyntax> matchedAttributes;
        string className;
        switch (context.Node) {
        case ClassDeclarationSyntax classDeclarationSyntax:
            matchedAttributes = classDeclarationSyntax.AttributeLists.SelectMany(list => list.Attributes)
                .Where(att => {
                    var attTypeInfo = semanticModel.GetTypeInfo(att);
                    var attQualifiedName = attTypeInfo.Type?.ToDisplayString();
                    return PluginDataFieldsGenerator.ConcernedAttributes.Contains(attQualifiedName) ||
#if CompatibleWithViewModelPropertiesGenerator
                           ViewModelPropertiesGenerator.ConcernedAttributes.Contains(attQualifiedName) ||
#endif
                           PluginFieldsGenerator.ConcernedAttribute == attQualifiedName ||
                           PluginFieldDefaultValuesGenerator.ConcernedAttributes.Contains(attQualifiedName) ||
                           ViewModelPropertiesWithDefaultValuesGenerator.ConcernedAttributes.Contains(attQualifiedName);
                }).ToArray();

            if (!matchedAttributes.Any()) return;

            var isPartial = classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
            if (isPartial) return;

            className = classDeclarationSyntax.Identifier.ValueText;
            break;
        case FieldDeclarationSyntax:
        case PropertyDeclarationSyntax:
            var declarationSyntax = context.Node as MemberDeclarationSyntax;
            matchedAttributes = declarationSyntax!.AttributeLists.SelectMany(list => list.Attributes)
                .Where(att => {
                    var attTypeInfo = semanticModel.GetTypeInfo(att);
                    var attQualifiedName = attTypeInfo.Type?.ToDisplayString();
                    return PluginFieldDefaultValuesGenerator.ConcernedAttributes.Contains(attQualifiedName);
                }).ToArray();

            if (!matchedAttributes.Any()) return;

            var parentDeclarationSyntax = (ClassDeclarationSyntax)declarationSyntax.Parent;
            if (parentDeclarationSyntax!.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return;

            className = ((ClassDeclarationSyntax)declarationSyntax.Parent!).Identifier.ValueText;
            break;
        default:
            return;
        }

        foreach (var attribute in matchedAttributes) {
            var location = attribute.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(NotPartialDescriptor, location, className));
        }
    }

    internal static void AnalyzeArgumentLength(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;
        var attributeSyntax = (AttributeSyntax)context.Node;
        var typeInfo = semanticModel.GetTypeInfo(attributeSyntax);
        var attributeQualifiedName = typeInfo.Type?.ToDisplayString();
        if (!PluginDataFieldsGenerator.ConcernedAttributes.Contains(attributeQualifiedName)
#if CompatibleWithViewModelPropertiesGenerator
            && !ViewModelPropertiesGenerator.ConcernedAttributes.Contains(attributeQualifiedName)
#endif
           )
            return;

        var literalExpressionSyntaxes = attributeSyntax.ArgumentList?.Arguments
            .SelectMany(aas => aas.DescendantNodes().OfType<LiteralExpressionSyntax>()).ToArray();
        if (literalExpressionSyntaxes == null || !literalExpressionSyntaxes.Any()) return;

        var attributeName = attributeQualifiedName?.Substring(attributeQualifiedName.LastIndexOf('.') + 1);
        var example = DescriptorExample(attributeName);
        var maxLength = MaxLengthOfArgument(attributeName);
        foreach (var expressionSyntax in literalExpressionSyntaxes) {
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
                attributeName, example, maxLength));
        }
    }

    internal static string DescriptorExample(string attributeName) {
        if (attributeName == null) return string.Empty;
        if (attributeName.StartsWith("Part")) return "PT<nameOrNumber>MATL";
        if (attributeName.StartsWith("Plate")) return "PL<nameOrNumber>MATL";
        if (attributeName.StartsWith("Weld")) return "W<nameOrNumber>SIZEA";
        if (attributeName.StartsWith("BoltCircle")) return "BC<nameOrNumber>PLAIN";
        if (attributeName.StartsWith("Bolt")) return "B<nameOrNumber>DISTX";
        return string.Empty;
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
        if (attributeName == null) return int.MinValue;
        if (attributeName.StartsWith("Part")) return 13;
        if (attributeName.StartsWith("Plate")) return 13;
        if (attributeName.StartsWith("Weld")) return 13;
        if (attributeName.StartsWith("BoltCircle")) return 12;
        if (attributeName.StartsWith("Bolt")) return 13;
        return int.MinValue;
    }

    internal static void AnalyzeInterface(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var attributeSyntaxes = classDeclarationSyntax.AttributeLists.SelectMany(als => als.Attributes).ToArray();
        var appliedAttributes = attributeSyntaxes
            .Select(attSyntax => semanticModel.GetTypeInfo(attSyntax).Type?.ToDisplayString())
            .Where(name =>
#if CompatibleWithViewModelPropertiesGenerator
                ViewModelPropertiesGenerator.ConcernedAttributes.Contains(name) ||
#endif
                ViewModelPropertiesWithDefaultValuesGenerator.ConcernedAttributes.Contains(name))
            .ToArray();
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

    internal static void AnalyzePluginFieldDefaultValuesAttributeAppliedPlaces(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        var placeCnt = 0;
        var matchedAttributeSyntaxes = new List<AttributeSyntax>();
        AttributeSyntax[] attributeSyntaxes = null;
        if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes)) {
            placeCnt++;
            // attributeSyntaxes = attributeSyntaxes.ToArray();
            matchedAttributeSyntaxes.AddRange(attributeSyntaxes);
        }

        _ = classDeclarationSyntax.Members.Where(member => {
            switch (member) {
            case FieldDeclarationSyntax:
            case PropertyDeclarationSyntax:
                if (!TryGetMatchedAttributes(member.AttributeLists, semanticModel,
                        PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes)) {
                    return false;
                }

                placeCnt++;
                // attributeSyntaxes = attributeSyntaxes.ToArray();
                matchedAttributeSyntaxes.AddRange(attributeSyntaxes);

                return true;
            default:
                return false;
            }
        }).ToArray();

        if (placeCnt <= 1) return;

        foreach (var attributeSyntax in matchedAttributeSyntaxes) {
            context.ReportDiagnostic(Diagnostic.Create(
                AppliedOnOverOnePlace, attributeSyntax.GetLocation()));
        }
    }

    internal static void AnalyzeExistingFields(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;

        AttributeSyntax[] attributeSyntaxes = null;
        ITypeSymbol dataType;
        switch (context.Node) {
        case ClassDeclarationSyntax classDeclarationSyntax:
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            attributeSyntaxes = attributeSyntaxes.ToArray();
            AttributeSyntax[] fieldsFromAttSyntax = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    [PluginFieldsGenerator.ConcernedAttribute], ref fieldsFromAttSyntax)) {
                foreach (var attributeSyntax in attributeSyntaxes) {
                    context.ReportDiagnostic(Diagnostic.Create(
                        FieldsFromAttributeNotApplied, attributeSyntax.GetLocation(), attributeSyntax.Name.ToString()));
                }

                return;
            }

            var expression = (TypeOfExpressionSyntax)fieldsFromAttSyntax.Single().ArgumentList!.Arguments[0].Expression;
            dataType = semanticModel.GetTypeInfo(expression.Type).Type;
            break;
        case FieldDeclarationSyntax fieldDeclarationSyntax:
            if (!TryGetMatchedAttributes(fieldDeclarationSyntax.AttributeLists,
                    semanticModel, PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            attributeSyntaxes = attributeSyntaxes.ToArray();
            dataType = semanticModel.GetTypeInfo(fieldDeclarationSyntax.Declaration.Type).Type;
            break;
        case PropertyDeclarationSyntax propertyDeclarationSyntax:
            if (!TryGetMatchedAttributes(propertyDeclarationSyntax.AttributeLists,
                    semanticModel, PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            attributeSyntaxes = attributeSyntaxes.ToArray();
            dataType = semanticModel.GetTypeInfo(propertyDeclarationSyntax.Type).Type;
            break;
        default:
            return;
        }

        var dictPluginDataFields = new Dictionary<string, HashSet<string>>();
        var pluginDataFieldsTuples = dataType!.GetAttributes()
            .Where(a => PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass!.ToDisplayString()))
            .Select(a => {
                var attName = a.AttributeClass!.ToDisplayString();
                attName = attName.Substring(attName.LastIndexOf('.') + 1);

                var hashSet = new HashSet<string>(a.ConstructorArguments
                    .SelectMany(argArray => argArray.Values.Select(arg => arg.Value!.ToString())));

                return (attName, hashSet);
            });
        foreach ((string attName, HashSet<string> hashSet) in pluginDataFieldsTuples) {
            dictPluginDataFields.Add(attName, hashSet);
        }

        var dictPluginFieldDefaultValues = new Dictionary<string, HashSet<string>>();

        foreach (var attributeSyntax in attributeSyntaxes) {
            var attTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            var attQualifiedName = attTypeInfo.Type?.ToDisplayString() ?? string.Empty;
            var attName = attQualifiedName.Substring(attQualifiedName.LastIndexOf('.') + 1);
            attName = attName.Substring(0, attName.Length - 22) + "sAttribute";

            if (!dictPluginDataFields.ContainsKey(attName)) {
                context.ReportDiagnostic(Diagnostic.Create(DataTypeDoesNotContainTheseFields,
                    attributeSyntax.GetLocation()));
            }

            if (!dictPluginFieldDefaultValues.TryGetValue(attName, out HashSet<string> nameOrNumberSet)) {
                nameOrNumberSet = new HashSet<string>();
                dictPluginFieldDefaultValues.Add(attName, nameOrNumberSet);
            }

            var nameOrNumberTuple = attributeSyntax.ArgumentList!.Arguments.Select((a, i) => (a, i)).Select(tuple => {
                var argSyntax = tuple.a;

                var index = tuple.i;
                var paramName = argSyntax.NameColon?.Name.Identifier.ValueText;

                if (paramName == null && index == 0 ||
                    paramName != null &&
                    Regex.Match(paramName, "(part|plate|weld|bolt|boltCircle)N(ame|umber)").Success) {
                    return (value: ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText,
                        location:
                        argSyntax.Expression.GetLocation());
                }

                return (string.Empty, null);
            }).Single(tuple => !string.IsNullOrEmpty(tuple.value));

            if (!dictPluginDataFields[attName].Contains(nameOrNumberTuple.value)) {
                context.ReportDiagnostic(Diagnostic.Create(
                    DataTypeDoesNotContainTheseFields, nameOrNumberTuple.location));
            }

            if (!dictPluginFieldDefaultValues[attName].Contains(nameOrNumberTuple.value)) {
                dictPluginFieldDefaultValues[attName].Add(nameOrNumberTuple.value);
            } else {
                context.ReportDiagnostic(Diagnostic.Create(
                    SetDefaultValueMultiTimes, nameOrNumberTuple.location));
            }
        }
    }

    internal static void AnalyzeExistingProperties(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

#if CompatibleWithViewModelPropertiesGenerator
        AttributeSyntax[] oldAttributeSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ViewModelPropertiesGenerator.ConcernedAttributes, ref oldAttributeSyntaxes))
            return;

        var oldAttDict = new Dictionary<string, HashSet<string>>();

        foreach (var attributeSyntax in oldAttributeSyntaxes) {
            var attTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            var attQualifiedName = attTypeInfo.Type?.ToDisplayString() ?? string.Empty;
            var attName = attQualifiedName.Substring(attQualifiedName.LastIndexOf('.') + 1);

            if (!oldAttDict.TryGetValue(attName, out HashSet<string> nameOrNumberSet)) {
                nameOrNumberSet = new HashSet<string>();
                oldAttDict.Add(attName, nameOrNumberSet);
            }

            if (attributeSyntax.ArgumentList == null) continue;
            var arguments = attributeSyntax.ArgumentList.Arguments
                .SelectMany(argumentSyntax =>
                    argumentSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>())
                .Select(expressionSyntax => expressionSyntax.Token.ValueText);

            foreach (var argument in arguments) {
                nameOrNumberSet.Add(argument);
            }
        }
#endif

        AttributeSyntax[] attributeSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ViewModelPropertiesWithDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
            return;

        var attDict = new Dictionary<string, HashSet<string>>();

        foreach (var attributeSyntax in attributeSyntaxes) {
            var attTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
            var attQualifiedName = attTypeInfo.Type?.ToDisplayString() ?? string.Empty;
            var attName = attQualifiedName.Substring(attQualifiedName.LastIndexOf('.') + 1);
            var oldAttName = attName.Replace("WithDefaultValues", "");

            if (!attDict.TryGetValue(attName, out HashSet<string> nameOrNumberSet)) {
                nameOrNumberSet = new HashSet<string>();
                attDict.Add(attName, nameOrNumberSet);
            }

            if (attributeSyntax.ArgumentList == null) continue;

            var index = -1;
            foreach (var argSyntax in attributeSyntax.ArgumentList.Arguments) {
                index++;

                var parameter = argSyntax.NameColon?.Name.Identifier.ValueText;
                var argument = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

                if (index == 0 && parameter == null || parameter != null &&
                    Regex.Match(parameter, "(part|plate|weld|bolt|boltCircle)N(ame|umber)").Success) {
                    
                    var maxLength = MaxLengthOfArgument(attName);
                    if (argument.Length > maxLength) {
                        var example = DescriptorExample(attName);
                        context.ReportDiagnostic(Diagnostic.Create(
                            LengthExceedLimitationDescriptor, argSyntax.GetLocation(),
                            attName, example, maxLength));
                    }
                    
                    if (Regex.Match(argument, SpecialCharacterPattern).Success) {
                        context.ReportDiagnostic(Diagnostic.Create(ContainsSpecialCharacters, argSyntax.GetLocation()));
                    }

                    if (Regex.Match(argument, UnsuggestedCharacterPattern).Success) {
                        context.ReportDiagnostic(Diagnostic.Create(ContainsUnsuggestedCharacters,
                            argSyntax.GetLocation()));
                    }

                    if (!nameOrNumberSet.Add(argument)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            SetDefaultValueMultiTimes, argSyntax.GetLocation()));
                    }

#if CompatibleWithViewModelPropertiesGenerator
                    if (oldAttDict.ContainsKey(oldAttName) && oldAttDict[oldAttName].Contains(argument)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            AlreadyBeGeneratedByOldGenerator, argSyntax.GetLocation(),
                            oldAttName, attName));
                    }
#endif
                }
            }
        }
    }
}