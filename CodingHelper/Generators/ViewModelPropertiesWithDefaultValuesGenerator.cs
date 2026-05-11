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
 *  ViewModelPropertiesWithDefaultValuesGenerator.cs: help to generate properties
 *  (with "StructuresDialogAttribute" applied) for view model which used by plugin WPF UI.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

#define CompatibleWithViewModelPropertiesGenerator
using System;
using System.Collections.Generic;
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
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;
using PropertyInfo = (string Name, string AttributeName, string Type, string Predicate);

namespace Muggle.TsExtensions.CodingHelper.Generators {
    [Generator]
    internal class ViewModelPropertiesWithDefaultValuesGenerator : IIncrementalGenerator {
        private const string TsDatatype = "global::Tekla.Structures.Datatype";
        private const string TsDialog = "global::Tekla.Structures.Dialog";

        internal static readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlatePropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCirclePropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.GeneralPropertiesWithDefaultValuesAttribute"
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

        /// <summary>
        /// Dictionary of preset values.
        /// <list type="bullet">
        ///     <item>Key - attribute short name, such as "PartPropertiesWithDefaultValuesAttribute".</item>
        ///     <item>Value - dictionary of property name and preset value.
        ///         <list type="bullet">
        ///             <item>Key - property name, such as "profile".</item>
        ///             <item>Value - preset value.</item>
        ///         </list>
        ///     </item>
        /// </list>
        /// </summary>
        private ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> PresetValues { get; }

        public ViewModelPropertiesWithDefaultValuesGenerator() {
            var dict = new Dictionary<string, ReadOnlyDictionary<string, string>>();

            //  include 'GeneralPropertyWithDefaultValueAttribute'
            foreach (var text in GetAttributeSourceTexts(ConcernedAttributes.Take(ConcernedAttributes.Length - 1))) {
                var defaultValues =
                    GetDefaultValuesFromSyntaxTree(CSharpSyntaxTree.ParseText(text), out string attributeName);
                dict.Add(attributeName, new ReadOnlyDictionary<string, string>(defaultValues));
            }

            PresetValues = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dict);
        }

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            context.RegisterPostInitializationOutput(ctx => {
                foreach (var attribute in ConcernedAttributes) {
                    var shortName = attribute.Substring(attribute.LastIndexOf('.') + 1);
                    ctx.AddSource($"{shortName}.g.cs",
                        SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
                }
            });

            var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform)
                .Where(x => x != default);

            context.RegisterSourceOutput(provider, Generate);
        }

        private static bool Predicate(SyntaxNode syntaxNode, CancellationToken token) {
            if (token.IsCancellationRequested) return false;

            return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax &&
                   classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }

        private ViewModelPropertiesInfo Transform(GeneratorSyntaxContext context, CancellationToken token) {
            var semanticModel = context.SemanticModel;
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (classSymbol == null || !classSymbol.AllInterfaces.Any(i =>
                    i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged"))
                return default;

            AttributeSyntax[] attSyntaxes = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attSyntaxes))
                return default;

#if CompatibleWithViewModelPropertiesGenerator
            AttributeSyntax[] oldAttributeSyntaxes = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    ViewModelPropertiesGenerator.ConcernedAttributes, ref oldAttributeSyntaxes))
                goto SkipCompatibleWithViewModelPropertiesGenerator;

            var oldAttributeDict = new Dictionary<string, HashSet<string>>();
            foreach (var attributeSyntax in oldAttributeSyntaxes) {
                var attName = GetAttributeName(attributeSyntax, semanticModel);

                if (!oldAttributeDict.TryGetValue(attName, out var hashset)) {
                    hashset = [];
                    oldAttributeDict.Add(attName, hashset);
                }

                if (attributeSyntax.ArgumentList == null) continue;
                var arguments = attributeSyntax.ArgumentList.Arguments
                    .SelectMany(argSyntax => argSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>())
                    .Select(litSyntax => litSyntax.Token.ValueText)
                    .Where(s =>
                        s.Length > 0 &&
                        s.Length <= InternalAttributesDiagnoser.MaxLengthOfArgument(attName) &&
                        !Regex.IsMatch(s, InternalAttributesDiagnoser.SpecialCharacterPattern));

                foreach (var arg in arguments) {
                    hashset.Add(arg);
                }
            }

            //  remove items that already contained in old attribute

            attSyntaxes = attSyntaxes.Where(attSyntax => {
                var qualifiedName = GetAttributeQualifiedName(attSyntax, semanticModel);
                //  all 'GeneralPropertyWithDefaultValueAttribute' are accepted
                if (qualifiedName == ConcernedAttributes.Last()) return true;

                var attName = GetUnqualifiedName(qualifiedName);

                if (attSyntax.ArgumentList == null) return false;

                var nameOrNumber = attSyntax.ArgumentList.Arguments
                    .Select((argSyntax, index) => (argSyntax, index))
                    .FirstOrDefault(tuple => {
                        var argSyntax = tuple.argSyntax;
                        var index = tuple.index;
                        if (index == 0 && argSyntax.NameColon == null)
                            return true;

                        var name = argSyntax.NameColon?.Name.Identifier.ValueText;
                        return name != null &&
                               Regex.IsMatch(name, "(part|plate|weld|bolt|boltCircle)N(ame|umber)");
                    })
                    .argSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>().Single().Token.ValueText;

                if (nameOrNumber.Length == 0 ||
                    nameOrNumber.Length > InternalAttributesDiagnoser.MaxLengthOfArgument(attName) ||
                    Regex.IsMatch(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern))
                    return false;

                return !oldAttributeDict.TryGetValue(attName.Replace("WithDefaultValues", ""), out var hashset) ||
                       !hashset.Contains(nameOrNumber);
            }).ToArray();

            SkipCompatibleWithViewModelPropertiesGenerator: ;
#endif

            //  Key - attribute name
            //  Value - 
            //      Key - for 'GeneralPropertiesWithDefaultValuesAttribute' is data type,
            //            for other attributes is name or number
            //      Value - 
            //          Key - for 'GeneralPropertiesWithDefaultValuesAttribute' is parameter name,
            //                for other attributes is property name of model object
            //          Value - default value
            var attDict = new ArgumentsDictionary<DefaultValueDictionary>();
            var generalPropertySet = new HashSet<string>();

            foreach (var attSyntax in attSyntaxes) {
                if (attSyntax.ArgumentList == null) continue;

                var attName = GetAttributeName(attSyntax, semanticModel);

                if (!attDict.TryGetValue(attName, out var elementDict)) {
                    elementDict = new DefaultValueDictionary();
                    attDict.Add(attName, elementDict);
                }

                if (attName is "GeneralPropertiesWithDefaultValuesAttribute") {
                    var supportedTypes = InternalAttributesDiagnoser.SupportedDataTypes(attName);

                    var type = GetUnqualifiedName(attSyntax.ArgumentList.DescendantNodes()
                        .OfType<TypeOfExpressionSyntax>().FirstOrDefault()?.Type.ToString());
                    if (type is null || !supportedTypes.Contains(type)) continue;

                    if (!elementDict.TryGetValue(type, out var paramDict)) {
                        paramDict = new Dictionary<string, string>();
                        elementDict.Add(type, paramDict);
                    }

                    var argSyntaxes = attSyntax.ArgumentList
                        .DescendantNodes().OfType<LiteralExpressionSyntax>().ToList();

                    if (type is "Boolean") {
                        //  should not use 'Boolean' type, use 'Integer' instead
                        /*foreach (var argSyntax in argSyntaxes) {
                            if (!argSyntax.IsKind(SyntaxKind.StringLiteralExpression)) continue;

                            var s = argSyntax.Token.ValueText;
                            if (s.Length == 0 ||
                                s.Length >= InternalAttributesDiagnoser.MaxLengthOfArgument(attName) ||
                                Regex.IsMatch(s, InternalAttributesDiagnoser.SpecialCharacterPattern) ||
                                Regex.IsMatch(s, "^[0-9]"))
                                continue;

                            if (generalPropertySet.Add(s))
                                paramDict.Add(s, "");
                        }*/
                    } else {
                        for (int i = 0; i < argSyntaxes.Count / 2 * 2; i += 2) {
                            var paramSyntax = argSyntaxes[i];
                            var valueSyntax = argSyntaxes[i + 1];

                            if (!paramSyntax.IsKind(SyntaxKind.StringLiteralExpression)) continue;

                            var param = paramSyntax.Token.ValueText;
                            var value = valueSyntax.Token.ValueText;

                            switch (type) {
                            case "Integer":
                                if (!valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                                    !int.TryParse(value, out _)) 
                                    continue;
                                break;
                            case "Double":
                            case "Distance":
                                if (!valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                                    !double.TryParse(value, out _)) 
                                    continue;
                                break;
                            case "DistanceList":
                            case "String":
                                if (!valueSyntax.IsKind(SyntaxKind.StringLiteralExpression)) continue;
                                break;
                            }

                            if (generalPropertySet.Add(param))
                                paramDict.Add(param, value);
                        }
                    }
                } else {
                    var nameOrNumber = string.Empty;
                    var defaultValues = new Dictionary<string, string>();

                    var index = -1;
                    foreach (var argSyntax in attSyntax.ArgumentList.Arguments) {
                        index++;

                        var parameter = argSyntax.NameColon?.Name.Identifier.ValueText;
                        var argument = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;
                        if (index == 0 && parameter == null || parameter != null &&
                            Regex.IsMatch(parameter, "(part|plate|weld|bolt|boltCircle)N(ame|umber)")) {
                            nameOrNumber = argument;

                            if (nameOrNumber.Length == 0 ||
                                nameOrNumber.Length > InternalAttributesDiagnoser.MaxLengthOfArgument(attName) ||
                                Regex.IsMatch(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern))
                                goto ContinueForeachAttribute;
                            if (elementDict.ContainsKey(nameOrNumber)) goto ContinueForeachAttribute;

                            continue;
                        }

                        parameter ??= PresetValues[attName].ElementAt(index - 1).Key;
                        defaultValues.Add(parameter, argument);
                    }

                    elementDict.Add(nameOrNumber, defaultValues);
                }

                ContinueForeachAttribute: ;
            }

            return new ViewModelPropertiesInfo {
                ClassInfo = new ClassInfo {
                    Name = classDeclarationSyntax.Identifier.Text,
                    NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                    Accessibility = classSymbol.DeclaredAccessibility,
                    IsRecord = classSymbol.IsRecord,
                },
                Arguments = attDict
            };
        }

        private void Generate(SourceProductionContext context, ViewModelPropertiesInfo info) {

            var fieldsBuilder = new StringBuilder();
            var propertiesBuilder = new StringBuilder();

            foreach (var attDict in info.Arguments) {
                var attName = attDict.Key;
                var g = attName switch {
                    "GeneralPropertiesWithDefaultValuesAttribute" => GenerateGeneralSource(attDict.Value),
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
                        //  should not use 'Boolean' type, use 'Integer' instead
                        // "Boolean" => "value",
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
                "PartPropertiesWithDefaultValuesAttribute" => PartPropertyInfos,
                "PlatePropertiesWithDefaultValuesAttribute" => PlatePropertyInfos,
                "WeldPropertiesWithDefaultValuesAttribute" => WeldPropertyInfos,
                "BoltPropertiesWithDefaultValuesAttribute" => BoltPropertyInfos,
                "BoltCirclePropertiesWithDefaultValuesAttribute" => BoltCircleProperties,
                _ => []
            };

            var propertyPrefix = attributeName.Substring(0, attributeName.Length - 36);
            var attributePrefix = propertyPrefix switch {
                "Part" => "PT",
                "Plate" => "PL",
                "Weld" => "W",
                "Bolt" => "B",
                "BoltCircle" => "BC",
                _ => string.Empty
            };

            var presetValues = PresetValues[attributeName];

            foreach (var modelObjDict in defaultValueDict) {
                var nameOrNumber = modelObjDict.Key;
                var defaultValues = modelObjDict.Value;

                if (nameOrNumber.Length == 0 ||
                    nameOrNumber.Length > InternalAttributesDiagnoser.MaxLengthOfArgument(attributeName))
                    continue;
                if (Regex.IsMatch(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern))
                    continue;

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

                    var propertyName = propertyPrefix + nameOrNumber + ToPropertyNameStyle(property);
                    var fieldName = ToPrivateFieldNameStyle(propertyName);

                    fieldsBuilder.AppendLine($"        private {TsDatatype}.{propertyInfos[i].Type} {fieldName};");
                    propertiesBuilder.AppendLine(
                        $"        \n" +
                        $"        [{TsDialog}.StructuresDialog(\"{attributePrefix}{nameOrNumber}{propertyInfos[i].AttributeName}\", typeof({TsDatatype}.{propertyInfos[i].Type}))]\n" +
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
}