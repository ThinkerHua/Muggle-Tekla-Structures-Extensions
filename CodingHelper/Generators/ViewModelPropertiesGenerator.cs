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
 *  ViewModelPropertiesGenerator.cs: help to generate properties (with "StructuresDialogAttribute" applied)
 *  for view model which used by plugin WPF UI.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Diagnosers;
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;
using PropertyInfo = (string Name, string AttributeName, string Type, string Predicate);

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
internal class ViewModelPropertiesGenerator : IIncrementalGenerator {
    private const string TsDatatype = "global::Tekla.Structures.Datatype";
    private const string TsDialog = "global::Tekla.Structures.Dialog";

    internal static readonly string[] ConcernedAttributes = [
        "Muggle.TsExtensions.CodingHelper.Generators.PartPropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.PlatePropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.WeldPropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltPropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltCirclePropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.ChamferPropertiesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.GeneralPropertiesAttribute"
    ];

    internal static readonly PropertyInfo[] PartPropertyInfos = [
        ("Profile", "PRF", "String", "string.IsNullOrEmpty(value)"),
        ("Material", "MATL", "String", "string.IsNullOrEmpty(value)"),
        ("Name", "NAME", "String", "string.IsNullOrEmpty(value)"),
        ("Finish", "FNSH", "String", "string.IsNullOrEmpty(value)"),
        ("Class", "CLS", "Integer", "value == int.MinValue"),
        ("AssemblyPrefix", "ASMP", "String", "string.IsNullOrEmpty(value)"),
        ("AssemblyStartNumber", "ASMN", "Integer", "value == int.MinValue"),
        ("PartPrefix", "PTP", "String", "string.IsNullOrEmpty(value)"),
        ("PartStartNumber", "PTN", "Integer", "value == int.MinValue"),
    ];

    internal static readonly PropertyInfo[] PlatePropertyInfos = [
        ("Thickness", "T", "Double", "value == int.MinValue"),
        ("Breadth", "B", "Double", "value == int.MinValue"),
        ("Height", "H", "Double", "value == int.MinValue"),
        ("Material", "MATL", "String", "string.IsNullOrEmpty(value)"),
        ("Name", "NAME", "String", "string.IsNullOrEmpty(value)"),
        ("Finish", "FNSH", "String", "string.IsNullOrEmpty(value)"),
        ("Class", "CLS", "Integer", "value == int.MinValue"),
        ("AssemblyPrefix", "ASMP", "String", "string.IsNullOrEmpty(value)"),
        ("AssemblyStartNumber", "ASMN", "Integer", "value == int.MinValue"),
        ("PartPrefix", "PTP", "String", "string.IsNullOrEmpty(value)"),
        ("PartStartNumber", "PTN", "Integer", "value == int.MinValue"),
    ];

    internal static readonly PropertyInfo[] WeldPropertyInfos = [
        ("TypeAbove", "TYPEA", "Integer", "value < 0 || value > 26"),
        ("TypeBelow", "TYPEB", "Integer", "value < 0 || value > 26"),
        ("SizeAbove", "SIZEA", "Double", "value == int.MinValue"),
        ("SizeBelow", "SIZEB", "Double", "value == int.MinValue"),
        ("AngleAbove", "ANGA", "Double", "value == int.MinValue"),
        ("AngleBelow", "ANGB", "Double", "value == int.MinValue"),
        ("ContourAbove", "CTRA", "Integer", "value < 0 || value > 3"),
        ("ContourBelow", "CTRB", "Integer", "value < 0 || value > 3"),
        ("FinishAbove", "FNSHA", "Integer", "value < 0 || value > 5"),
        ("FinishBelow", "FNSHB", "Integer", "value < 0 || value > 5"),
        ("RootFaceAbove", "FACEA", "Double", "value == int.MinValue"),
        ("RootFaceBelow", "FACEB", "Double", "value == int.MinValue"),
        ("EffectiveThroatAbove", "THROA", "Double", "value == int.MinValue"),
        ("EffectiveThroatBelow", "THROB", "Double", "value == int.MinValue"),
        ("RootOpeningAbove", "OPNGA", "Double", "value == int.MinValue"),
        ("RootOpeningBelow", "OPNGB", "Double", "value == int.MinValue"),
        ("IncrementAmountAbove", "INCRA", "Integer", "value == int.MinValue"),
        ("IncrementAmountBelow", "INCRB", "Integer", "value == int.MinValue"),
        ("LengthAbove", "LENA", "Double", "value == int.MinValue"),
        ("LengthBelow", "LENB", "Double", "value == int.MinValue"),
        ("PitchAbove", "PITA", "Double", "value == int.MinValue"),
        ("PitchBelow", "PITB", "Double", "value == int.MinValue"),
        ("Around", "ARND", "Integer", "value < 0 || value > 1"),
        ("Shop", "SHOP", "Integer", "value < 0 || value > 1"),
        ("Placement", "PLACE", "Integer", "value < 0 || value > 2"),
        ("Preparation", "PREP", "Integer", "value < 0 || value > 3"),
        ("Intermittent", "INTMI", "Integer", "value < 0 || value > 2"),
        ("ReferenceText", "TEXT", "String", "string.IsNullOrEmpty(value)"),
    ];

    internal static readonly PropertyInfo[] BoltPropertyInfos = [
        ("Size", "SIZE", "Distance", "value.Millimeters == int.MinValue || value.Millimeters == 0.0"),
        ("Standard", "STD", "String", "string.IsNullOrEmpty(value)"),
        ("DistX", "DISTX", "DistanceList", "value.Count == 0"),
        ("DistY", "DISTY", "DistanceList", "value.Count == 0"),
        ("Type", "TYPE", "Integer", "value < 0 || value > 1"),
        ("ThreadInMaterial", "THRD", "Integer", "value < 0 || value > 1"),
        ("Length", "LEN", "Double", "value == int.MinValue"),
        ("CutLength", "CLEN", "Double", "value == int.MinValue"),
        ("ExtraLength", "XLEN", "Double", "value == int.MinValue"),
        ("Tolerance", "TOL", "Double", "value == int.MinValue"),
        ("PlainType", "PLAIN", "Integer", "value < 0 || value > 1"),
        ("BlindHoleDepth", "DEPTH", "Double", "value == int.MinValue"),
        ("Hole1", "HOLE1", "Integer", "value < 0 || value > 1"),
        ("Hole2", "HOLE2", "Integer", "value < 0 || value > 1"),
        ("Hole3", "HOLE3", "Integer", "value < 0 || value > 1"),
        ("Hole4", "HOLE4", "Integer", "value < 0 || value > 1"),
        ("Hole5", "HOLE5", "Integer", "value < 0 || value > 1"),
        ("HoleType", "HOLTY", "Integer", "value < 0 || value > 2"),
        ("SlottedHoleX", "SLOTX", "Double", "value == int.MinValue"),
        ("SlottedHoleY", "SLOTY", "Double", "value == int.MinValue"),
        ("RotateSlots", "RSLOT", "Integer", "value < 0 || value > 2"),
        ("IsBolt", "ISBOT", "Integer", "value < 0 || value > 1"),
        ("UseNut1", "NUT1", "Integer", "value < 0 || value > 1"),
        ("UseNut2", "NUT2", "Integer", "value < 0 || value > 1"),
        ("UseWasher1", "WSHR1", "Integer", "value < 0 || value > 1"),
        ("UseWasher2", "WSHR2", "Integer", "value < 0 || value > 1"),
        ("UseWasher3", "WSHR3", "Integer", "value < 0 || value > 1"),
    ];

    internal static readonly PropertyInfo[] BoltCircleProperties = [
        ("Size", "SIZE", "Distance", "value.Millimeters == int.MinValue || value.Millimeters == 0.0"),
        ("Standard", "STD", "String", "string.IsNullOrEmpty(value)"),
        ("NumberOfBolts", "NUM", "Integer", "value == int.MinValue"),
        ("Diameter", "DIAM", "Double", "value == int.MinValue"),
        ("Type", "TYPE", "Integer", "value < 0 || value > 1"),
        ("ThreadInMaterial", "THRD", "Integer", "value < 0 || value > 1"),
        ("Length", "LEN", "Double", "value == int.MinValue"),
        ("CutLength", "CLEN", "Double", "value == int.MinValue"),
        ("ExtraLength", "XLEN", "Double", "value == int.MinValue"),
        ("Tolerance", "TOL", "Double", "value == int.MinValue"),
        ("PlainType", "PLAIN", "Integer", "value < 0 || value > 1"),
        ("BlindHoleDepth", "DEPTH", "Double", "value == int.MinValue"),
        ("Hole1", "HOLE1", "Integer", "value < 0 || value > 1"),
        ("Hole2", "HOLE2", "Integer", "value < 0 || value > 1"),
        ("Hole3", "HOLE3", "Integer", "value < 0 || value > 1"),
        ("Hole4", "HOLE4", "Integer", "value < 0 || value > 1"),
        ("Hole5", "HOLE5", "Integer", "value < 0 || value > 1"),
        ("HoleType", "HOLTY", "Integer", "value < 0 || value > 2"),
        ("SlottedHoleX", "SLOTX", "Double", "value == int.MinValue"),
        ("SlottedHoleY", "SLOTY", "Double", "value == int.MinValue"),
        ("RotateSlots", "RSLOT", "Integer", "value < 0 || value > 2"),
        ("IsBolt", "ISBOT", "Integer", "value < 0 || value > 1"),
        ("UseNut1", "NUT1", "Integer", "value < 0 || value > 1"),
        ("UseNut2", "NUT2", "Integer", "value < 0 || value > 1"),
        ("UseWasher1", "WSHR1", "Integer", "value < 0 || value > 1"),
        ("UseWasher2", "WSHR2", "Integer", "value < 0 || value > 1"),
        ("UseWasher3", "WSHR3", "Integer", "value < 0 || value > 1"),
    ];

    internal static readonly PropertyInfo[] ChamferPropertyInfos = [
        ("Type", "TYPE", "Integer", "value < 0 || value > 7"),
        ("X", "X", "Double", "value == int.MinValue"),
        ("Y", "Y", "Double", "value == int.MinValue"),
        ("Dz1", "DZ1", "Double", "value == int.MinValue"),
        ("Dz2", "DZ2", "Double", "value == int.MinValue"),
    ];

    /// <summary>
    /// Dictionary of preset values.
    /// <list type="bullet">
    ///     <item>Key - attribute short name, such as "PartPropertiesAttribute".</item>
    ///     <item>Value - dictionary of property name and preset value.
    ///         <list type="bullet">
    ///             <item>Key - property name, such as "profile".</item>
    ///             <item>Value - preset value.</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </summary>
    private ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> PresetValues { get; }

    public ViewModelPropertiesGenerator() {
        var dict = new Dictionary<string, ReadOnlyDictionary<string, string>>();

        //  include 'GeneralPropertyWithDefaultValueAttribute'
        foreach (var text in GetAttributeSourceTexts(ConcernedAttributes.Take(ConcernedAttributes.Length - 1))) {
            var defaultValues =
                GetDefaultValuesFromSyntaxTree(CSharpSyntaxTree.ParseText(text), out var attributeName);
            dict.Add(attributeName, new ReadOnlyDictionary<string, string>(defaultValues));
        }

        PresetValues = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dict);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("NotificationObject.g.cs",
                SourceText.From(GetResourceAsString("NotificationObject.cs"), Encoding.UTF8));
            ctx.AddSource("ConnectionViewModel.g.cs",
                SourceText.From(GetResourceAsString("ConnectionViewModel.cs"), Encoding.UTF8));
            ctx.AddSource("DetailViewModel.g.cs",
                SourceText.From(GetResourceAsString("DetailViewModel.cs"), Encoding.UTF8));
            ctx.AddSource("CustomPartViewModel.g.cs",
                SourceText.From(GetResourceAsString("CustomPartViewModel.cs"), Encoding.UTF8));
            foreach (var attribute in ConcernedAttributes) {
                var shortName = attribute.Substring(attribute.LastIndexOf('.') + 1);
                ctx.AddSource($"{shortName}.g.cs",
                    SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
            }
        });

        var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform)
            .Where(x => x != default);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var propertiesDefaultValues = provider.Select(static (x, _) => x.Value).Where(x => x != default);
        context.RegisterSourceOutput(propertiesDefaultValues, Generate);
    }

    private static bool Predicate(SyntaxNode syntaxNode, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private GatheredInfo<ViewModelPropertiesInfo>
        Transform(GeneratorSyntaxContext context, CancellationToken token) {

        var semanticModel = context.SemanticModel;
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        AttributeSyntax[] attSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                ref attSyntaxes))
            return default;

        var result = new GatheredInfo<ViewModelPropertiesInfo>(default, []);
        var diagnosticInfos = result.DiagnosticInfos;

        if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(
                    NotPartial,
                    classDeclarationSyntax.Identifier.GetLocation(),
                    [classDeclarationSyntax.Identifier.ValueText])
            );
            result.DiagnosticInfos = diagnosticInfos;
            return result;
        }

        if (!classSymbol.AllInterfaces.Any(i =>
                i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged")) {
            diagnosticInfos = attSyntaxes.Aggregate(diagnosticInfos,
                (current, attSyntax) => current.Add(
                    new DiagnosticInfo(
                        NotImplementINotifyPropertyChanged,
                        attSyntax.Name.GetLocation(),
                        [attSyntax.Name]))
            );
            result.DiagnosticInfos = diagnosticInfos;
            return result;
        }

        //  Key - attribute name
        //  Value - 
        //      Key - for 'GeneralPropertiesAttribute' is data type,
        //            for other attributes is id
        //      Value - 
        //          Key - for 'GeneralPropertiesAttribute' is parameter name,
        //                for other attributes is property name of model object
        //          Value - default value
        var attDict = new ArgumentsDictionary<DefaultValueDictionary>();
        var generalPropertySet = new HashSet<string>();

        foreach (var attSyntax in attSyntaxes) {
            if (attSyntax.ArgumentList is null) continue;

            var attName = GetAttributeName(attSyntax, semanticModel);

            if (!attDict.TryGetValue(attName, out var elementDict)) {
                elementDict = new DefaultValueDictionary();
                attDict.Add(attName, elementDict);
            }

            if (attName is "GeneralPropertiesAttribute") {
                AnalyzeGeneralProperties(attSyntax, attName,
                    ref diagnosticInfos, ref elementDict, ref generalPropertySet);
            } else {
                AnalyzeSeriesProperties(attSyntax, attName, PresetValues, ref diagnosticInfos, ref elementDict);
            }

            result.DiagnosticInfos = diagnosticInfos;
        }

        if (attDict.Count == 0) return result;

        token.ThrowIfCancellationRequested();

        result.Value = new ViewModelPropertiesInfo {
            ClassInfo = new ClassInfo {
                Name = classDeclarationSyntax.Identifier.Text,
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
            },
            Arguments = attDict
        };

        return result;
    }

    private static void AnalyzeGeneralProperties(AttributeSyntax attSyntax, string attName,
        ref ImmutableArray<DiagnosticInfo> diagnosticInfos,
        ref DefaultValueDictionary elementDict,
        ref HashSet<string> generalPropertySet) {

        var typeSyntax = attSyntax.ArgumentList?.DescendantNodes()
            .OfType<TypeOfExpressionSyntax>().FirstOrDefault()?.Type;
        if (typeSyntax is null) return;

        var type = GetUnqualifiedName(typeSyntax.ToString());
        var supportedDatatypes = SupportedDataTypes(attName);

        if (type is "Boolean") {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(ShouldNotUseBooleanType, typeSyntax.GetLocation(), []));
            return;
        }

        if (!supportedDatatypes.Contains(type)) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(
                    NotSupportedDataType,
                    typeSyntax.GetLocation(),
                    [string.Join(", ", supportedDatatypes), "within 'Tekla.Structures.Datatype'"])
            );
            return;
        }

        if (!elementDict.TryGetValue(type, out var paramDict)) {
            paramDict = new Dictionary<string, string>();
            elementDict.Add(type, paramDict);
        }

        var argSyntaxes = attSyntax.ArgumentList
            .DescendantNodes().OfType<LiteralExpressionSyntax>().ToList();
        if (argSyntaxes.Count == 0) return;

        if (argSyntaxes.Count % 2 != 0) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(ArgumentsMustBePassedInPairs, argSyntaxes.Last().GetLocation(), []));
            if (argSyntaxes.Count < 2) return;
        }

        var maxLength = MaxLengthOfArgument(attName);
        for (int i = 0; i < argSyntaxes.Count / 2 * 2; i += 2) {
            var paramSyntax = argSyntaxes[i];
            var valueSyntax = argSyntaxes[i + 1];

            if (!paramSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(NotExpectedDataType, paramSyntax!.GetLocation(), ["string"]));
                continue;
            }

            var param = paramSyntax.Token.ValueText;
            var value = valueSyntax.Token.ValueText;

            if (!PluginDataFieldsGenerator.DiagnoseIdCharacters(param, attName, paramSyntax.GetLocation(), true,
                    ref diagnosticInfos)) {
                continue;
            }

            var isRightType = type switch {
                "Integer" => valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) && int.TryParse(value, out _),
                "Double" or "Distance" => valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) &&
                                          double.TryParse(value, out _),
                "DistanceList" or "String" => valueSyntax.IsKind(SyntaxKind.StringLiteralExpression),
                _ => false
            };
            if (!isRightType) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(
                        NotExpectedDataType,
                        valueSyntax.GetLocation(),
                        [
                            type switch {
                                "Integer" => "int",
                                "Double" or "Distance" => "double",
                                "DistanceList" or "String" => "string",
                                _ => string.Empty
                            }
                        ]));
                continue;
            }

            if (generalPropertySet.Add(param)) {
                paramDict.Add(param, value);
            } else {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(RegisterFieldOrPropertyMultiTimes, paramSyntax.GetLocation(), [param]));
            }
        }
    }

    private static void AnalyzeSeriesProperties(AttributeSyntax attSyntax, string attName,
        ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> presetValues,
        ref ImmutableArray<DiagnosticInfo> diagnosticInfos, ref DefaultValueDictionary elementDict) {

        var argSyntaxes = attSyntax.ArgumentList?.Arguments;
        if (argSyntaxes is null) return;

        var valueDict = new Dictionary<string, string>();

        var id = string.Empty;
        var index = -1;
        foreach (var argSyntax in argSyntaxes) {
            index++;

            //  style of (param: value)
            var paramName = argSyntax.NameColon?.Name.Identifier.ValueText;
            var paramValue = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

            if (paramName is null && index == 0 || paramName is "id") {
                id = paramValue;

                if (!PluginDataFieldsGenerator.DiagnoseIdCharacters(id, attName, argSyntax.GetLocation(), false,
                        ref diagnosticInfos)) {
                    return;
                }

                if (elementDict.ContainsKey(id)) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(RegisterFieldOrPropertyMultiTimes, argSyntax.GetLocation(), [id]));
                    return;
                }

                continue;
            }

            //  style of (Property = value), Property => property
            paramName ??= argSyntax.NameEquals?.Name.Identifier.ValueText;
            if (paramName is not null) paramName = ToLocalVariableNameStyle(paramName);

            //  index-1 is safe here
            paramName ??= presetValues[attName].ElementAt(index - 1).Key;

            valueDict.Add(paramName, paramValue);
        }

        //  id != string.Empty
        elementDict.Add(id, valueDict);
    }

    private void Generate(SourceProductionContext context, ViewModelPropertiesInfo info) {

        var fieldsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();

        foreach (var attDict in info.Arguments) {
            var attName = attDict.Key;
            var g = attName switch {
                "GeneralPropertiesAttribute" => GenerateGeneralSource(attDict.Value),
                _ => GenerateSeriesSource(attName, attDict.Value)
            };

            fieldsBuilder.Append(g.fields);
            propertiesBuilder.Append(g.properties);
        }

        var generatedSourceText =
            $"//  <auto-generated/>" +
#if DEBUG
            $" at {DateTime.Now}" +
#endif
            $"\n\n" +
            $"using System;\n" +
            $"\n" +
            $"namespace {info.ClassInfo.NameSpace} {{\n" +
            $"    {info.ClassInfo.Accessibility.ToString().ToLower()} partial {(info.ClassInfo.IsRecord ? "record " : "")}class {info.ClassInfo.Name} {{\n" +
            $"{fieldsBuilder}\n" +
            $"{propertiesBuilder}\n" +
            $"    }}\n" +
            $"}}";

        context.AddSource($"{info.ClassInfo.Name}.g.cs", generatedSourceText);
    }

    private static (string fields, string properties) GenerateGeneralSource(
        DefaultValueDictionary defaultValueDict) {

        var fieldsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();

        foreach (var kvp in defaultValueDict) {
            var dataType = kvp.Key;

            var predicate = dataType switch {
                "Integer" or "Double" => "value == int.MinValue",
                "Distance" => "value.Millimeters == int.MinValue || value.Millimeters == 0.0",
                "DistanceList" => "value.Count == 0",
                "String" => "string.IsNullOrEmpty(value)",
                _ => string.Empty
            };

            foreach (var kvp2 in kvp.Value) {
                var name = kvp2.Key;
                var fieldName = ToPrivateFieldNameStyle(name);
                var propertyName = ToPropertyNameStyle(name);

                fieldsBuilder.AppendLine(
                    $"        private {TsDatatype}.{dataType} {fieldName};");

                var defaultValue = kvp2.Value;
                var equalsValueClause = dataType switch {
                    "Integer" or "Double" => $"{predicate} ? {defaultValue} : value",
                    "Distance" => $"{predicate} ? new {TsDatatype}.{dataType}({defaultValue}) : value",
                    "DistanceList" => $"{predicate} ? {TsDatatype}.{dataType}.Parse(\"{defaultValue}\") : value",
                    "String" => $"{predicate} ? \"{defaultValue}\" : value",
                    _ => string.Empty
                };

                propertiesBuilder.AppendLine(
                    $"        \n" +
                    $"        [{TsDialog}.StructuresDialog(\"{name}\", typeof({TsDatatype}.{dataType}))]\n" +
                    $"        public {TsDatatype}.{dataType} {propertyName} {{\n" +
                    $"            get {{\n" +
                    $"                return {fieldName};\n" +
                    $"            }}\n" +
                    $"            set {{\n" +
                    $"                {fieldName} = {equalsValueClause};\n" +
                    $"                OnPropertyChanged();\n" +
                    $"            }}\n" +
                    $"        }}");
            }
        }

        return (fieldsBuilder.ToString(), propertiesBuilder.ToString());
    }

    private (string fields, string properties) GenerateSeriesSource(string attributeName,
        DefaultValueDictionary defaultValueDict) {

        var fieldsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();

        var propertyInfos = attributeName switch {
            "PartPropertiesAttribute" => PartPropertyInfos,
            "PlatePropertiesAttribute" => PlatePropertyInfos,
            "WeldPropertiesAttribute" => WeldPropertyInfos,
            "BoltPropertiesAttribute" => BoltPropertyInfos,
            "BoltCirclePropertiesAttribute" => BoltCircleProperties,
            "ChamferPropertiesAttribute" => ChamferPropertyInfos,
            _ => []
        };

        var propertyPrefix = attributeName.Substring(0, attributeName.Length - 19);
        var attributePrefix = propertyPrefix switch {
            "Part" => "PT",
            "Plate" => "PL",
            "Weld" => "W",
            "Bolt" => "B",
            "BoltCircle" => "BC",
            "Chamfer" => "CF",
            _ => string.Empty
        };

        var presetValues = PresetValues[attributeName];

        foreach (var idDict in defaultValueDict) {
            var id = idDict.Key;
            var defaultValues = idDict.Value;

            for (int i = 0; i < presetValues.Count; i++) {
                var property = presetValues.ElementAt(i).Key;

                if (!defaultValues.TryGetValue(property, out var value)) {
                    value = presetValues.ElementAt(i).Value;
                }

                value = propertyInfos[i].Type switch {
                    "String" => $"\"{value}\"",
                    "Distance" => $"new {TsDatatype}.Distance({value})",
                    "DistanceList" => $"{TsDatatype}.DistanceList.Parse(\"{value}\")",
                    _ => value
                };

                var propertyName = propertyPrefix + ToPropertyNameStyle(id) + ToPropertyNameStyle(property);
                var fieldName = ToPrivateFieldNameStyle(propertyName);

                fieldsBuilder.AppendLine($"        private {TsDatatype}.{propertyInfos[i].Type} {fieldName};");
                propertiesBuilder.AppendLine(
                    $"        \n" +
                    $"        [{TsDialog}.StructuresDialog(\"{attributePrefix}{id}{propertyInfos[i].AttributeName}\", typeof({TsDatatype}.{propertyInfos[i].Type}))]\n" +
                    $"        public {TsDatatype}.{propertyInfos[i].Type} {propertyName} {{\n" +
                    $"            get {{\n" +
                    $"                return {fieldName};\n" +
                    $"            }}\n" +
                    $"            set {{\n" +
                    $"                {fieldName} = {propertyInfos[i].Predicate} ? {value} : value;\n" +
                    $"                OnPropertyChanged();\n" +
                    $"            }}\n" +
                    $"        }}");
            }
        }

        return (fieldsBuilder.ToString(), propertiesBuilder.ToString());
    }

}