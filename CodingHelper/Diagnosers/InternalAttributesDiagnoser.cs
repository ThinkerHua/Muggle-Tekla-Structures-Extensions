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

    #region Descriptors

    internal static readonly DiagnosticDescriptor NotPartial = new(
        "MTSECH001",
        "Target class must be partial",
        "Cannot generate members for '{0}' because it is not partial",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NotImplementINotifyPropertyChanged = new(
        "MTSECH002",
        "INotifyPropertyChanged not implemented",
        "A class that applied '{0}' must implement 'INotifyPropertyChanged' interface. " +
        "Simplified inherit it from 'ConnectionViewModel' or 'DetailViewModel' or 'NotificationObject' " +
        "within 'Muggle.TsExtensions.CodingHelper.Generators' namespace, " +
        "or directly implement 'INotifyPropertyChanged' interface with an " +
        "'OnPropertyChanged(string propertyName)' method.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor LengthExceedLimitation = new(
        "MTSECH003",
        "Name or number too long",
        "Due to the limitation of Tekla Structures, " +
        "the name length of plugin data variables must not exceed 19. " +
        "Considering the prefix and suffix (such as {1}), " +
        "the argument for '{0}' must be 1 to {2} characters.",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsSpecialCharacters = new(
        "MTSECH004",
        "Name contains special characters",
        "Name must not contain special characters",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ContainsUnsuggestedCharacters = new(
        "MTSECH005",
        "Name contains unsuggested characters",
        "It is not recommended to use characters other than \"_\", \"A-Z\", \"a-z\", \"0-9\"",
        Category,
        DiagnosticSeverity.Info,
        true);

    internal static readonly DiagnosticDescriptor FieldsFromAttributeNotApplied = new(
        "MTSECH010",
        "\"FieldsFromAttribute\" not applied",
        "Must also apply \"FieldsFromAttribute\" when applied \"{0}\" on class",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor AppliedOnOverOnePlace = new(
        "MTSECH011",
        "Applied on over one place",
        "These attributes should only be applied on one place (Class, Field or Property): " +
        "\"PartFieldDefaultValuesAttribute\", \"PlateFieldDefaultValuesAttribute\", " +
        "\"WeldFieldDefaultValuesAttribute\", \"BoltFieldDefaultValuesAttribute\", " +
        "\"BoltCircleFieldDefaultValuesAttribute\"",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor DataTypeDoesNotContainTheseFields = new(
        "MTSECH012",
        "Target data type doesn't contain these fields",
        "Ensure that target data type contain these fields",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor SetDefaultValueMultiTimes = new(
        "MTSECH013",
        "Set default value multi times",
        "Should not set default value multi times",
        Category,
        DiagnosticSeverity.Error,
        true);

#if CompatibleWithViewModelPropertiesGenerator
    internal static readonly DiagnosticDescriptor AlreadyBeGeneratedByOldGenerator = new(
        "MTSECH014",
        "Already be generated by old generator",
        "'{0}' has already registered default values for the same model object, so that '{1}' will not work",
        Category,
        DiagnosticSeverity.Warning,
        true);
#endif

    internal static readonly DiagnosticDescriptor NameStartsWithNumber = new(
        "MTSECH015",
        "Field or property name starts with number",
        "Field or property name should not starts with number",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ArgumentsMustBePassedInPairs = new(
        "MTSECH016",
        "Arguments must be passed in pairs",
        "Must pass name and value pairs, such as [\"param1\", 12, \"param2\", 8.5, \"param3\", \"value\"]",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NotSupportedDataType = new(
        "MTSECH017",
        "Not supported data type",
        "Only support {0} types {1}",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NotExpectedDataType = new(
        "MTSECH018",
        "Not expected data type",
        "Expect '{0}' type value",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor RegisterFieldOrPropertyMultiTimes = new(
        "MTSECH019",
        "Registering field or property multi times",
        "Registering field or property of '{0}' multi times",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor NotPassedInPairs = new(
        "MTSECH020",
        "Not passed in pairs",
        "Should pass arguments in pairs",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor ShouldNotUseBooleanType = new(
        "MTSECH021",
        "Should not use 'Boolean' type",
        "Should not use 'Boolean' type, use 'Integer' type instead",
        Category,
        DiagnosticSeverity.Error,
        true);

    #endregion

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        NotPartial, NotImplementINotifyPropertyChanged, LengthExceedLimitation,
        ContainsSpecialCharacters, ContainsUnsuggestedCharacters, FieldsFromAttributeNotApplied, AppliedOnOverOnePlace,
        DataTypeDoesNotContainTheseFields, SetDefaultValueMultiTimes,
#if CompatibleWithViewModelPropertiesGenerator
        AlreadyBeGeneratedByOldGenerator,
#endif
        NameStartsWithNumber, ArgumentsMustBePassedInPairs, NotSupportedDataType, NotExpectedDataType,
        RegisterFieldOrPropertyMultiTimes, NotPassedInPairs, ShouldNotUseBooleanType
    ];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIfIsPartial, SyntaxKind.ClassDeclaration, SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInterface, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePluginDataFields, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePluginFieldDefaultValuesAttributeAppliedPlaces,
            SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePluginFields, SyntaxKind.ClassDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeViewModelProperties, SyntaxKind.ClassDeclaration);
    }

    internal static void AnalyzeIfIsPartial(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;

        IEnumerable<AttributeSyntax> matchedAttributes;
        string className;
        switch (context.Node) {
        case ClassDeclarationSyntax classDeclarationSyntax:
            matchedAttributes = classDeclarationSyntax.AttributeLists.SelectMany(list => list.Attributes)
                .Where(att => {
                    var attQualifiedName = GetAttributeQualifiedName(att, semanticModel);
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
                    var attQualifiedName = GetAttributeQualifiedName(att, semanticModel);
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

            context.ReportDiagnostic(Diagnostic.Create(NotPartial, location, className));
        }
    }

    internal static bool AnalyzeNameCharacters(string attributeName, LiteralExpressionSyntax nameSyntax,
        SyntaxNodeAnalysisContext context) {

        var passed = true;

        var name = nameSyntax.Token.ValueText;
        var maxlength = MaxLengthOfArgument(attributeName);
        if (name.Length == 0 || name.Length > maxlength) {
            context.ReportDiagnostic(Diagnostic.Create(LengthExceedLimitation, nameSyntax.GetLocation(),
                attributeName, PrefixSuffixExample(attributeName), maxlength));
            passed = false;
        }

        if (attributeName == GetUnqualifiedName(PluginDataFieldsGenerator.ConcernedAttributes.Last()) ||
            attributeName ==
            GetUnqualifiedName(ViewModelPropertiesWithDefaultValuesGenerator.ConcernedAttributes.Last())) {
            if (Regex.IsMatch(name, "^[0-9]")) {
                context.ReportDiagnostic(Diagnostic.Create(NameStartsWithNumber, nameSyntax.GetLocation()));
                passed = false;
            }
        }

        if (Regex.IsMatch(name, SpecialCharacterPattern)) {
            context.ReportDiagnostic(Diagnostic.Create(ContainsSpecialCharacters, nameSyntax.GetLocation()));
            passed = false;
        }

        if (Regex.IsMatch(name, UnsuggestedCharacterPattern)) {
            context.ReportDiagnostic(Diagnostic.Create(
                ContainsUnsuggestedCharacters, nameSyntax.GetLocation()));
        }

        return passed;
    }

    internal static string PrefixSuffixExample(string attributeName) {
        if (attributeName == null) return string.Empty;
        if (attributeName.StartsWith("Part")) return "PT<nameOrNumber>MATL";
        if (attributeName.StartsWith("Plate")) return "PL<nameOrNumber>MATL";
        if (attributeName.StartsWith("Weld")) return "W<nameOrNumber>SIZEA";
        if (attributeName.StartsWith("BoltCircle")) return "BC<nameOrNumber>PLAIN";
        if (attributeName.StartsWith("Bolt")) return "B<nameOrNumber>DISTX";
        return "TheLongestAttribute";
    }

    /// <summary>
    ///     Return the character length of the arguments passed to the constructor of specified attribute.
    /// </summary>
    /// <remarks>
    ///     Only supports the attributes within the "Muggle.TsExtensions.CodingHelper" namespace.
    ///     The return value for unsported attributes is 19.
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
        return 19;
    }

    internal static void AnalyzeInterface(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var attributeSyntaxes = classDeclarationSyntax.AttributeLists.SelectMany(als => als.Attributes).ToArray();
        var appliedAttributes = attributeSyntaxes
            .Select(attSyntax => GetAttributeQualifiedName(attSyntax, semanticModel))
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
                NotImplementINotifyPropertyChanged,
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
                matchedAttributeSyntaxes.AddRange(attributeSyntaxes);

                return true;
            default:
                return false;
            }
        }).ToArray();

        if (placeCnt <= 1) return;

        foreach (var attributeSyntax in matchedAttributeSyntaxes) {
            context.ReportDiagnostic(Diagnostic.Create(AppliedOnOverOnePlace, attributeSyntax.GetLocation()));
        }
    }

    internal static void AnalyzePluginDataFields(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        AttributeSyntax[] attSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                PluginDataFieldsGenerator.ConcernedAttributes, ref attSyntaxes))
            return;

        //  key - attribute name, value - name set
        var dict = new Dictionary<string, HashSet<string>>();

        foreach (var attSyntax in attSyntaxes) {
            if (attSyntax.ArgumentList is null) continue;

            var attName = GetAttributeName(attSyntax, semanticModel);

            if (!dict.TryGetValue(attName, out var nameSet)) {
                nameSet = [];
                dict.Add(attName, nameSet);
            }

            if (attName is "GeneralFieldsAttribute") {
                var supportedType = SupportedDataTypes(attName);

                var fieldTypeSyntax = attSyntax.ArgumentList
                    .DescendantNodes().OfType<TypeOfExpressionSyntax>().FirstOrDefault()?.Type;
                if (fieldTypeSyntax is null) continue;

                var fieldType = fieldTypeSyntax.ToString();
                if (!supportedType.Contains(fieldType)) {
                    context.ReportDiagnostic(Diagnostic.Create(
                        NotSupportedDataType, fieldTypeSyntax.GetLocation(),
                        string.Join(", ", supportedType.Select(s => '\'' + s + '\'')), ""));
                }
            }

            var exprSyntaxes = attSyntax.ArgumentList.DescendantNodes().OfType<LiteralExpressionSyntax>();
            foreach (var exprSyntax in exprSyntaxes) {
                AnalyzeNameCharacters(attName, exprSyntax, context);

                var value = exprSyntax.Token.ValueText;
                if (!nameSet.Add(value)) {
                    context.ReportDiagnostic(Diagnostic.Create(
                        RegisterFieldOrPropertyMultiTimes, exprSyntax.GetLocation(), value));
                }
            }
        }
    }

    internal static HashSet<string> SupportedDataTypes(string attributeName) => attributeName switch {
        "GeneralFieldsAttribute" => ["int", "double", "string"],
        "GeneralPropertiesWithDefaultValuesAttribute" =>
            ["Integer", "Double", "Boolean", "Distance", "DistanceList", "String"],
        _ => []
    };

    internal static void AnalyzePluginFields(SyntaxNodeAnalysisContext context) {
        var semanticModel = context.SemanticModel;

        //  get plugin data type and default value attributes
        AttributeSyntax[] attributeSyntaxes = null;
        ITypeSymbol dataType;
        switch (context.Node) {
        //  analyze if apply FieldsFromAttribute
        case ClassDeclarationSyntax classDeclarationSyntax:
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            AttributeSyntax[] fieldsFromAttSyntax = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    [PluginFieldsGenerator.ConcernedAttribute], ref fieldsFromAttSyntax)) {
                foreach (var attributeSyntax in attributeSyntaxes) {
                    context.ReportDiagnostic(Diagnostic.Create(
                        FieldsFromAttributeNotApplied, attributeSyntax.GetLocation(), attributeSyntax.Name.ToString()));
                }

                return;
            }

            var expression = fieldsFromAttSyntax.Single().DescendantNodes().OfType<TypeOfExpressionSyntax>().Single();
            dataType = semanticModel.GetTypeInfo(expression.Type).Type;
            break;
        case FieldDeclarationSyntax fieldDeclarationSyntax:
            if (!TryGetMatchedAttributes(fieldDeclarationSyntax.AttributeLists,
                    semanticModel, PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            dataType = semanticModel.GetTypeInfo(fieldDeclarationSyntax.Declaration.Type).Type;
            break;
        case PropertyDeclarationSyntax propertyDeclarationSyntax:
            if (!TryGetMatchedAttributes(propertyDeclarationSyntax.AttributeLists,
                    semanticModel, PluginFieldDefaultValuesGenerator.ConcernedAttributes, ref attributeSyntaxes))
                return;

            dataType = semanticModel.GetTypeInfo(propertyDeclarationSyntax.Type).Type;
            break;
        default:
            return;
        }

        //  for GeneralFieldsAttribute, category is 'int' or 'double' or 'string',
        //  for other Attributes, category is attribute name
        var pluginDataFields = dataType!.GetAttributes()
            .Where(a => PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass!.ToDisplayString()))
            .Select(a => {
                var attName = a.AttributeClass!.ToDisplayString();
                attName = attName.Substring(attName.LastIndexOf('.') + 1);

                string category;
                if (attName is "GeneralFieldsAttribute") {
                    category = a.ConstructorArguments
                        .FirstOrDefault(typedConstant => typedConstant.Kind == TypedConstantKind.Type).Value?
                        .ToString();
                } else {
                    category = attName;
                }

                var args = a.ConstructorArguments
                    .Where(typedConstant => typedConstant.Kind == TypedConstantKind.Array)
                    .SelectMany(typedConstant => typedConstant.Values.Select(constant => constant.Value!.ToString()));

                return (category, args);
            });

        //  key - category
        var pluginDataFieldsDict = new Dictionary<string, HashSet<string>>();
        foreach (var (category, args) in pluginDataFields) {
            if (!pluginDataFieldsDict.TryGetValue(category, out var hashSet)) {
                hashSet = [];
                pluginDataFieldsDict.Add(category, hashSet);
            }

            foreach (var arg in args) {
                hashSet.Add(arg);
            }
        }

        var pluginDataGeneralFieldNames = pluginDataFieldsDict.Where(kvp => kvp.Key is "int" or "double" or "string")
            .SelectMany(kvp => kvp.Value).ToArray();

        //  key - attribute name
        var pluginFieldDefaultValuesDict = new Dictionary<string, HashSet<string>>();

        foreach (var attributeSyntax in attributeSyntaxes) {
            if (attributeSyntax.ArgumentList is null) continue;

            var attQualifiedName = GetAttributeQualifiedName(attributeSyntax, semanticModel) ?? string.Empty;
            //  *DefaultValuesAttribute => *sAttribute
            var attName = attQualifiedName.Substring(attQualifiedName.LastIndexOf('.') + 1);
            attName = attName.Replace("DefaultValue", "");

            if (attName is "GeneralFieldsAttribute" && pluginDataGeneralFieldNames.Length == 0
                || attName is not "GeneralFieldsAttribute" && !pluginDataFieldsDict.ContainsKey(attName)) {
                context.ReportDiagnostic(Diagnostic.Create(
                    DataTypeDoesNotContainTheseFields, attributeSyntax.GetLocation()));
            }

            if (!pluginFieldDefaultValuesDict.TryGetValue(attName, out var nameOrNumberSet)) {
                nameOrNumberSet = [];
                pluginFieldDefaultValuesDict.Add(attName, nameOrNumberSet);
            }

            if (attName is "GeneralFieldsAttribute") {
                var argSyntaxes = attributeSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();

                if (argSyntaxes.Length % 2 != 0) {
                    context.ReportDiagnostic(Diagnostic.Create(
                        ArgumentsMustBePassedInPairs, attributeSyntax.ArgumentList.GetLocation()));
                }

                for (int i = 0; i < argSyntaxes.Length / 2 * 2; i += 2) {
                    var paramSyntax = argSyntaxes[i];
                    var valueSyntax = argSyntaxes[i + 1];
                    var param = paramSyntax.Token.ValueText;
                    var value = valueSyntax.Token.ValueText;

                    if (!pluginDataGeneralFieldNames.Contains(param)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DataTypeDoesNotContainTheseFields, paramSyntax.GetLocation()));
                    } else {
                        var expectedDataType = pluginDataFieldsDict.Single(kvp => kvp.Value.Contains(param)).Key;
                        var passedDataType = "UnexpectedDatatype";
                        switch (expectedDataType) {
                        case "int":
                            if (valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) &&
                                int.TryParse(value, out _))
                                passedDataType = expectedDataType;
                            break;
                        case "double":
                            if (valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) &&
                                double.TryParse(value, out _))
                                passedDataType = expectedDataType;
                            break;
                        case "string":
                            if (valueSyntax.IsKind(SyntaxKind.StringLiteralExpression))
                                passedDataType = expectedDataType;
                            break;
                        }

                        if (expectedDataType != passedDataType) {
                            context.ReportDiagnostic(Diagnostic.Create(
                                NotExpectedDataType, valueSyntax.GetLocation(), expectedDataType));
                        }
                    }

                    if (!pluginFieldDefaultValuesDict[attName].Add(param))
                        context.ReportDiagnostic(Diagnostic.Create(
                            SetDefaultValueMultiTimes, paramSyntax.GetLocation()));
                }
            } else {
                var index = -1;
                foreach (var argSyntax in attributeSyntax.ArgumentList.Arguments) {
                    index++;

                    var param = argSyntax.NameColon?.Name.Identifier.ValueText;
                    var value = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

                    if (param == null && index != 0 ||
                        param != null &&
                        !Regex.IsMatch(param, "(part|plate|weld|bolt|boltCircle)N(ame|umber)"))
                        continue;

                    if (!pluginDataFieldsDict[attName].Contains(value)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DataTypeDoesNotContainTheseFields, argSyntax.Expression.GetLocation()));
                    }

                    if (!pluginFieldDefaultValuesDict[attName].Add(value))
                        context.ReportDiagnostic(Diagnostic.Create(
                            SetDefaultValueMultiTimes, argSyntax.Expression.GetLocation()));
                }
            }
        }
    }

    internal static void AnalyzeViewModelProperties(SyntaxNodeAnalysisContext context) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

#if CompatibleWithViewModelPropertiesGenerator
        AttributeSyntax[] oldAttributeSyntaxes = null;
        var oldAttDict = new Dictionary<string, HashSet<string>>();

        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ViewModelPropertiesGenerator.ConcernedAttributes, ref oldAttributeSyntaxes))
            goto NoOldAttributes;

        foreach (var attributeSyntax in oldAttributeSyntaxes) {
            var attName = GetAttributeName(attributeSyntax, semanticModel);

            if (!oldAttDict.TryGetValue(attName, out var nameOrNumberSet)) {
                nameOrNumberSet = [];
                oldAttDict.Add(attName, nameOrNumberSet);
            }

            if (attributeSyntax.ArgumentList == null) continue;
            var argSyntaxes = attributeSyntax.ArgumentList.Arguments
                .SelectMany(argumentSyntax => argumentSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>());

            foreach (var argSyntax in argSyntaxes) {
                AnalyzeNameCharacters(attName, argSyntax, context);

                var arg = argSyntax.Token.ValueText;
                nameOrNumberSet.Add(arg);
            }
        }

        NoOldAttributes: ;
#endif

        AttributeSyntax[] attSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ViewModelPropertiesWithDefaultValuesGenerator.ConcernedAttributes, ref attSyntaxes))
            return;

        var attDict = new Dictionary<string, HashSet<string>>();
        var generalPropertySet = new HashSet<string>();

        foreach (var attSyntax in attSyntaxes) {
            var attName = GetAttributeName(attSyntax, semanticModel) ?? string.Empty;
#if CompatibleWithViewModelPropertiesGenerator
            var oldAttName = attName.Replace("WithDefaultValues", "");
#endif

            if (!attDict.TryGetValue(attName, out var nameOrNumberSet)) {
                nameOrNumberSet = [];
                attDict.Add(attName, nameOrNumberSet);
            }

            if (attSyntax.ArgumentList == null) continue;

            if (attName == "GeneralPropertiesWithDefaultValuesAttribute") {
                var supportedDataTypes = SupportedDataTypes(attName);

                var dataTypeSyntax = attSyntax.ArgumentList
                    .DescendantNodes().OfType<TypeOfExpressionSyntax>().FirstOrDefault().Type;
                var dataType = GetUnqualifiedName(dataTypeSyntax.ToString());

                if (!supportedDataTypes.Contains(dataType)) {
                    context.ReportDiagnostic(Diagnostic.Create(NotSupportedDataType, dataTypeSyntax.GetLocation(),
                        string.Join(", ", supportedDataTypes.Select(s => $"'{s}'")),
                        "within 'Tekla.Structures.Datatype'"));
                    continue;
                }

                var argSyntaxes = attSyntax.ArgumentList.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();

                if (dataType is "Boolean") {
                    //  should not use 'Boolean' type, use 'Integer' instead
                    /*foreach (var argSyntax in argSyntaxes) {
                        if (!argSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                            context.ReportDiagnostic(Diagnostic.Create(NotExpectedDataType, argSyntax.GetLocation(),
                                "string"));
                            continue;
                        }

                        if (AnalyzeNameCharacters(attName, argSyntax, context) &&
                            !generalPropertySet.Add(argSyntax.Token.ValueText)) {
                            context.ReportDiagnostic(Diagnostic.Create(RegisterFieldOrPropertyMultiTimes,
                                argSyntax.GetLocation(), argSyntax.Token.ValueText));
                        }
                    }*/
                    context.ReportDiagnostic(Diagnostic.Create(ShouldNotUseBooleanType, dataTypeSyntax.GetLocation()));
                } else {
                    if (argSyntaxes.Length % 2 != 0) {
                        context.ReportDiagnostic(Diagnostic.Create(NotPassedInPairs, argSyntaxes.Last().GetLocation()));
                    }

                    for (int i = 0; i < argSyntaxes.Length / 2 * 2; i += 2) {
                        var paramSyntax = argSyntaxes[i];
                        var argSyntax = argSyntaxes[i + 1];

                        var arg = argSyntax.Token.ValueText;

                        if (!paramSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                            context.ReportDiagnostic(Diagnostic.Create(NotExpectedDataType, paramSyntax.GetLocation(),
                                "string"));
                        } else {
                            if (AnalyzeNameCharacters(attName, paramSyntax, context) &&
                                !generalPropertySet.Add(paramSyntax.Token.ValueText)) {
                                context.ReportDiagnostic(Diagnostic.Create(RegisterFieldOrPropertyMultiTimes,
                                    paramSyntax.GetLocation(), paramSyntax.Token.ValueText));
                            }
                        }

                        switch (dataType) {
                        case "Integer":
                            if (!argSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                                !int.TryParse(arg, out _)) {
                                context.ReportDiagnostic(Diagnostic.Create(NotExpectedDataType, argSyntax.GetLocation(),
                                    "int"));
                            }

                            break;
                        case "Double":
                        case "Distance":
                            if (!argSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                                !double.TryParse(arg, out _)) {
                                context.ReportDiagnostic(Diagnostic.Create(NotExpectedDataType, argSyntax.GetLocation(),
                                    "double"));
                            }

                            break;
                        case "DistanceList":
                        case "String":
                            if (!argSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                                context.ReportDiagnostic(Diagnostic.Create(NotExpectedDataType, argSyntax.GetLocation(),
                                    "string"));
                            }

                            break;
                        }
                    }
                }
            } else {
                var index = -1;
                foreach (var argSyntax in attSyntax.ArgumentList.Arguments) {
                    index++;

                    var param = argSyntax.NameColon?.Name.Identifier.ValueText;
                    var arg = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

                    if (index != 0 && param == null ||
                        param != null && !Regex.IsMatch(param, "(part|plate|weld|bolt|boltCircle)N(ame|umber)")) {
                        continue;
                    }

                    AnalyzeNameCharacters(attName, argSyntax.Expression as LiteralExpressionSyntax, context);

                    if (!nameOrNumberSet.Add(arg)) {
                        context.ReportDiagnostic(Diagnostic.Create(SetDefaultValueMultiTimes, argSyntax.GetLocation()));
                    }

#if CompatibleWithViewModelPropertiesGenerator
                    if (oldAttDict.ContainsKey(oldAttName) && oldAttDict[oldAttName].Contains(arg)) {
                        context.ReportDiagnostic(Diagnostic.Create(
                            AlreadyBeGeneratedByOldGenerator, argSyntax.GetLocation(), oldAttName, attName));
                    }
#endif
                }
            }
        }
    }
}