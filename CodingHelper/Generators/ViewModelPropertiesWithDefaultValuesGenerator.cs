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
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;
using PropertyInfo = (string Name, string AttributeName, string Type, string Validation);

namespace Muggle.TsExtensions.CodingHelper.Generators {
    [Generator]
    internal class ViewModelPropertiesWithDefaultValuesGenerator : IIncrementalGenerator {
        internal static readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlatePropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltPropertiesWithDefaultValuesAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCirclePropertiesWithDefaultValuesAttribute"
        ];

        internal static readonly PropertyInfo[] PartPropertyInfos = [
            ("Profile", "PRF", "String", "String.IsNullOrEmpty(value)"),
            ("Material", "MATL", "String", "String.IsNullOrEmpty(value)"),
            ("Name", "NAME", "String", "String.IsNullOrEmpty(value)"),
            ("Finish", "FNSH", "String", "String.IsNullOrEmpty(value)"),
            ("Class", "CLS", "Integer", "value == int.MinValue"),
            ("AssemblyPrefix", "ASMP", "String", "String.IsNullOrEmpty(value)"),
            ("AssemblyStartNumber", "ASMN", "Integer", "value == int.MinValue"),
            ("PartPrefix", "PTP", "String", "String.IsNullOrEmpty(value)"),
            ("PartStartNumber", "PTN", "Integer", "value == int.MinValue"),
        ];

        internal static readonly PropertyInfo[] PlatePropertyInfos = [
            ("Thickness", "T", "Double", "value == int.MinValue"),
            ("Breadth", "B", "Double", "value == int.MinValue"),
            ("Height", "H", "Double", "value == int.MinValue"),
            ("Material", "MATL", "String", "String.IsNullOrEmpty(value)"),
            ("Name", "NAME", "String", "String.IsNullOrEmpty(value)"),
            ("Finish", "FNSH", "String", "String.IsNullOrEmpty(value)"),
            ("Class", "CLS", "Integer", "value == int.MinValue"),
            ("AssemblyPrefix", "ASMP", "String", "String.IsNullOrEmpty(value)"),
            ("AssemblyStartNumber", "ASMN", "Integer", "value == int.MinValue"),
            ("PartPrefix", "PTP", "String", "String.IsNullOrEmpty(value)"),
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
            ("ReferenceText", "TEXT", "String", "String.IsNullOrEmpty(value)"),
        ];

        internal static readonly PropertyInfo[] BoltPropertyInfos = [
            ("Size", "SIZE", "Distance", "value.Millimeters == int.MinValue || value.Millimeters == 0.0"),
            ("Standard", "STD", "String", "string.IsNullOrEmpty(value)"),
            ("DistX", "DISTX", "DistanceList", "value.Count == 0"),
            ("DistY", "DISTY", "DistanceList", "value.Count == 0"),
            ("Type", "TYPE", "Integer", "value < 0 || value > 1"),
            ("ThreadInMaterial", "THRD", "Integer", "value < 0 || value > 1"),
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

            foreach (var text in GetAttributeSourceTexts(ConcernedAttributes)) {
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

            AttributeSyntax[] attributeSyntaxes = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attributeSyntaxes))
                return default;

#if CompatibleWithViewModelPropertiesGenerator
            AttributeSyntax[] oldAttributeSyntaxes = null;
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    ViewModelPropertiesGenerator.ConcernedAttributes, ref oldAttributeSyntaxes))
                goto SkipCompatibleWithViewModelPropertiesGenerator;

            var oldAttributeDict = new Dictionary<string, HashSet<string>>();
            foreach (var attributeSyntax in oldAttributeSyntaxes) {
                var attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax, token);
                var qualifiedName = attributeTypeInfo.Type!.ToDisplayString();
                var attributeName = qualifiedName.Substring(qualifiedName.LastIndexOf('.') + 1);

                if (!oldAttributeDict.TryGetValue(attributeName, out var hashset)) {
                    hashset = new HashSet<string>();
                    oldAttributeDict.Add(attributeName, hashset);
                }

                if (attributeSyntax.ArgumentList == null) continue;
                var arguments = attributeSyntax.ArgumentList.Arguments
                    .SelectMany(argSyntax => argSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>())
                    .Select(literalSyntax => literalSyntax.Token.ValueText);

                foreach (var arg in arguments) {
                    hashset.Add(arg);
                }
            }

            //  remove items that already contained in old attribute

            attributeSyntaxes = attributeSyntaxes.Where(attSyntax => {
                if (attSyntax.ArgumentList == null) return false;

                var nameOrNumber = attSyntax.ArgumentList.Arguments.Select((argSyntax, index) => (argSyntax, index))
                    .First(tuple => {
                        var argSyntax = tuple.argSyntax;
                        var index = tuple.index;
                        if (index == 0 && argSyntax.NameColon == null)
                            return true;

                        var name = argSyntax.NameColon?.Name.Identifier.ValueText;
                        return name != null &&
                               Regex.Match(name, "(part|plate|weld|bolt|boltCircle)N(ame|umber)").Success;
                    })
                    .argSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>().Single().Token.ValueText;

                var attributeTypeInfo = semanticModel.GetTypeInfo(attSyntax, token);
                var qualifiedName = attributeTypeInfo.Type!.ToDisplayString();
                var attributeName = qualifiedName.Substring(qualifiedName.LastIndexOf('.') + 1);

                return !oldAttributeDict.TryGetValue(attributeName.Replace("WithDefaultValues", ""), out var hashset) ||
                       !hashset.Contains(nameOrNumber);

            }).ToArray();

        SkipCompatibleWithViewModelPropertiesGenerator:;
#endif

            var argumentsDict = new ArgumentsDictionary<DefaultValueDictionary>();

            foreach (var attributeSyntax in attributeSyntaxes) {
                if (attributeSyntax.ArgumentList == null) continue;

                var attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax, token);
                var qualifiedName = attributeTypeInfo.Type!.ToDisplayString();
                var attributeName = qualifiedName.Substring(qualifiedName.LastIndexOf('.') + 1);

                if (!argumentsDict.TryGetValue(attributeName, out var arguments)) {
                    arguments = new DefaultValueDictionary();
                    argumentsDict.Add(attributeName, arguments);
                }

                var nameOrNumber = string.Empty;
                var defaultValues = new Dictionary<string, string>();

                var index = -1;
                foreach (var argSyntax in attributeSyntax.ArgumentList.Arguments) {
                    index++;

                    var parameter = argSyntax.NameColon?.Name.Identifier.ValueText;
                    var argument = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;
                    if (index == 0 && parameter == null || parameter != null &&
                        Regex.Match(parameter, "(part|plate|weld|bolt|boltCircle)N(ame|umber)").Success) {
                        nameOrNumber = argument;

                        if (arguments.ContainsKey(nameOrNumber)) goto ContinueAttributeLoop;

                        continue;
                    }

                    parameter ??= PresetValues[attributeName].ElementAt(index - 1).Key;
                    defaultValues.Add(parameter, argument);
                }

                arguments.Add(nameOrNumber, defaultValues);
            ContinueAttributeLoop:;
            }

            return new ViewModelPropertiesInfo {
                ClassInfo = new ClassInfo {
                    Name = classDeclarationSyntax.Identifier.Text,
                    NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                    Accessibility = classSymbol.DeclaredAccessibility,
                    IsRecord = classSymbol.IsRecord,
                },
                Arguments = argumentsDict
            };
        }

        private void Generate(SourceProductionContext context, ViewModelPropertiesInfo info) {
            const string tsDatatype = "Tekla.Structures.Datatype";
            const string tsDialog = "Tekla.Structures.Dialog";

            var fieldsBuilder = new StringBuilder();
            var propertiesBuilder = new StringBuilder();

            foreach (var attDict in info.Arguments) {
                var attributeName = attDict.Key;

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

                foreach (var modelObjDict in attDict.Value) {
                    var nameOrNumber = modelObjDict.Key;
                    var defaultValues = modelObjDict.Value;

                    for (int i = 0; i < presetValues.Count; i++) {
                        var property = presetValues.ElementAt(i).Key;

                        if (!defaultValues.TryGetValue(property, out var value)) {
                            value = presetValues.ElementAt(i).Value;
                        }

                        value = propertyInfos[i].Type switch {
                            "String" => $"\"{value}\"",
                            "Distance" => $"new {tsDatatype}.Distance({value})",
                            "DistanceList" => $"{tsDatatype}.DistanceList.Parse(\"{value}\")",
                            _ => value
                        };

                        var propertyName = propertyPrefix + nameOrNumber + ToPropertyNameStyle(property);
                        var fieldName = ToPrivateFieldNameStyle(propertyName);

                        fieldsBuilder.AppendLine($"        private {tsDatatype}.{propertyInfos[i].Type} {fieldName};");
                        propertiesBuilder.AppendLine(
                            $"        \n" +
                            $"        [{tsDialog}.StructuresDialog(\"{attributePrefix}{nameOrNumber}{propertyInfos[i].AttributeName}\", typeof({tsDatatype}.{propertyInfos[i].Type}))]\n" +
                            $"        public {tsDatatype}.{propertyInfos[i].Type} {propertyName} {{\n" +
                            $"            get {{\n" +
                            $"                return {fieldName};\n" +
                            $"            }}\n" +
                            $"            set {{\n" +
                            $"                {fieldName} = {propertyInfos[i].Validation} ? {value} : value;\n" +
                            $"                OnPropertyChanged();\n" +
                            $"            }}\n" +
                            $"        }}");
                    }
                }
            }

            var generatedSourceText =
                $"//  <auto-generated/>" +
#if DEBUG
                $" at {DateTime.Now}" +
#endif
                $"\n\n" +
                $"using System;\n" +
                //$"using Tekla.Structures.Datatype;\n" +
                //$"using Tekla.Structures.Dialog;\n" +
                $"\n" +
                $"namespace {info.ClassInfo.NameSpace} {{\n" +
                $"    {info.ClassInfo.Accessibility.ToString().ToLower()} partial {(info.ClassInfo.IsRecord ? "record " : "")}class {info.ClassInfo.Name} {{\n" +
                $"{fieldsBuilder}\n" +
                $"{propertiesBuilder}\n" +
                $"    }}\n" +
                $"}}";

            context.AddSource($"{info.ClassInfo.Name}.g.cs", generatedSourceText);
        }
    }
}