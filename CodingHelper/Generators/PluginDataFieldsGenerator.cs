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
 *  PluginDataFieldsGenerator.cs: help to generate fields (with "StructuresFieldAttribute") for plugin data.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;
using FieldInfo = (string Name, string AttributeName, string Type);

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
internal class PluginDataFieldsGenerator : IIncrementalGenerator {
    internal static readonly string[] ConcernedAttributes = [
        "Muggle.TsExtensions.CodingHelper.Generators.PartFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.ChamferFieldsAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.GeneralFieldsAttribute"
    ];

    internal static readonly FieldInfo[] PartFieldInfos = [
        ("Name", "NAME", "string"),
        ("Profile", "PRF", "string"),
        ("Material", "MATL", "string"),
        ("Finish", "FNSH", "string"),
        ("Class", "CLS", "int"),
        ("AssemblyPrefix", "ASMP", "string"),
        ("AssemblyStartNumber", "ASMN", "int"),
        ("PartPrefix", "PTP", "string"),
        ("PartStartNumber", "PTN", "int")
    ];

    internal static readonly FieldInfo[] PlateFieldInfos = [
        ("Name", "NAME", "string"),
        ("Thickness", "T", "double"),
        ("Breadth", "B", "double"),
        ("Height", "H", "double"),
        ("Material", "MATL", "string"),
        ("Finish", "FNSH", "string"),
        ("Class", "CLS", "int"),
        ("AssemblyPrefix", "ASMP", "string"),
        ("AssemblyStartNumber", "ASMN", "int"),
        ("PartPrefix", "PTP", "string"),
        ("PartStartNumber", "PTN", "int")
    ];

    internal static readonly FieldInfo[] WeldFieldInfos = [
        ("SizeAbove", "SIZEA", "double"),
        ("SizeBelow", "SIZEB", "double"),
        ("TypeAbove", "TYPEA", "int"),
        ("TypeBelow", "TYPEB", "int"),
        ("AngleAbove", "ANGA", "double"),
        ("AngleBelow", "ANGB", "double"),
        ("ContourAbove", "CTRA", "int"),
        ("ContourBelow", "CTRB", "int"),
        ("FinishAbove", "FNSHA", "int"),
        ("FinishBelow", "FNSHB", "int"),
        ("RootFaceAbove", "FACEA", "double"),
        ("RootFaceBelow", "FACEB", "double"),
        ("EffectiveThroatAbove", "THROA", "double"),
        ("EffectiveThroatBelow", "THROB", "double"),
        ("RootOpeningAbove", "OPNGA", "double"),
        ("RootOpeningBelow", "OPNGB", "double"),
        ("IncrementAmountAbove", "INCRA", "int"),
        ("IncrementAmountBelow", "INCRB", "int"),
        ("LengthAbove", "LENA", "double"),
        ("LengthBelow", "LENB", "double"),
        ("PitchAbove", "PITA", "double"),
        ("PitchBelow", "PITB", "double"),
        ("Around", "ARND", "int"),
        ("Shop", "SHOP", "int"),
        ("Placement", "PLACE", "int"),
        ("Preparation", "PREP", "int"),
        ("Intermittent", "INTMI", "int"),
        ("ReferenceText", "TEXT", "string")
    ];

    internal static readonly FieldInfo[] BoltFieldInfos = [
        ("Size", "SIZE", "double"),
        ("Standard", "STD", "string"),
        ("DistXText", "DISTX", "string"),
        ("DistYText", "DISTY", "string"),
        ("Type", "TYPE", "int"),
        ("ThreadInMaterial", "THRD", "int"),
        ("Length", "LEN", "double"),
        ("CutLength", "CLEN", "double"),
        ("ExtraLength", "XLEN", "double"),
        ("Tolerance", "TOL", "double"),
        ("PlainType", "PLAIN", "int"),
        ("BlindHoleDepth", "DEPTH", "double"),
        ("Hole1", "HOLE1", "int"),
        ("Hole2", "HOLE2", "int"),
        ("Hole3", "HOLE3", "int"),
        ("Hole4", "HOLE4", "int"),
        ("Hole5", "HOLE5", "int"),
        ("HoleType", "HOLTY", "int"),
        ("SlottedHoleX", "SLOTX", "double"),
        ("SlottedHoleY", "SLOTY", "double"),
        ("RotateSlots", "RSLOT", "int"),
        ("IsBolt", "ISBOT", "int"),
        ("UseNut1", "NUT1", "int"),
        ("UseNut2", "NUT2", "int"),
        ("UseWasher1", "WSHR1", "int"),
        ("UseWasher2", "WSHR2", "int"),
        ("UseWasher3", "WSHR3", "int"),
    ];

    internal static readonly FieldInfo[] BoltCircleFieldInfos = [
        ("Size", "SIZE", "double"),
        ("Standard", "STD", "string"),
        ("NumberOfBolts", "NUM", "int"),
        ("Diameter", "DIAM", "double"),
        ("Type", "TYPE", "int"),
        ("ThreadInMaterial", "THRD", "int"),
        ("Length", "LEN", "double"),
        ("CutLength", "CLEN", "double"),
        ("ExtraLength", "XLEN", "double"),
        ("Tolerance", "TOL", "double"),
        ("PlainType", "PLAIN", "int"),
        ("BlindHoleDepth", "DEPTH", "double"),
        ("Hole1", "HOLE1", "int"),
        ("Hole2", "HOLE2", "int"),
        ("Hole3", "HOLE3", "int"),
        ("Hole4", "HOLE4", "int"),
        ("Hole5", "HOLE5", "int"),
        ("HoleType", "HOLTY", "int"),
        ("SlottedHoleX", "SLOTX", "double"),
        ("SlottedHoleY", "SLOTY", "double"),
        ("RotateSlots", "RSLOT", "int"),
        ("IsBolt", "ISBOT", "int"),
        ("UseNut1", "NUT1", "int"),
        ("UseNut2", "NUT2", "int"),
        ("UseWasher1", "WSHR1", "int"),
        ("UseWasher2", "WSHR2", "int"),
        ("UseWasher3", "WSHR3", "int")
    ];

    internal static readonly FieldInfo[] ChamferFieldInfos = [
        ("Type", "TYPE", "int"),
        ("X", "X", "double"),
        ("Y", "Y", "double"),
        ("Dz1", "DZ1", "double"),
        ("Dz2", "DZ2", "double")
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            foreach (var attribute in ConcernedAttributes) {
                var shortName = attribute.Substring(attribute.LastIndexOf('.') + 1);
                ctx.AddSource($"{shortName}.g.cs",
                    SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
            }
        });

        var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Where(x => x != default);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var pluginDataFieldsInfos = provider.Select(static (x, _) => x.Value).Where(x => x != default);
        context.RegisterSourceOutput(pluginDataFieldsInfos, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        return node is ClassDeclarationSyntax classDeclarationSyntax &&
               classDeclarationSyntax.AttributeLists.Count != 0;
    }

    private static GatheredInfo<PluginDataFieldsInfo> Transform(GeneratorSyntaxContext syntaxContext,
        CancellationToken token) {

        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
        var semanticModel = syntaxContext.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        AttributeSyntax[] attSyntaxes = null;
        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                ref attSyntaxes))
            return default;

        var result = new GatheredInfo<PluginDataFieldsInfo>(default, []);
        var diagnosticInfos = result.DiagnosticInfos;

        if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(
                    NotPartial,
                    classDeclarationSyntax.Identifier.GetLocation(),
                    [classDeclarationSyntax.Identifier.ValueText])
            );
            result.DiagnosticInfos = diagnosticInfos;
            return result;
        }

        //  Key - for 'GeneralFieldsAttribute' is 'int' or 'double' or 'string'
        //        for other attribute is attribute name
        //  Value - id set
        var attArguments = new ArgumentsDictionary<IdSet>();

        foreach (var attSyntax in attSyntaxes) {
            if (attSyntax.ArgumentList is null) continue;

            var attName = GetAttributeName(attSyntax, semanticModel);
            var key = attName;

            // if (attName == "GeneralFieldsAttribute") key = "int", "double", "string"
            if (attName == GetUnqualifiedName(ConcernedAttributes.Last())) {
                var datatypeSyntax = attSyntax.ArgumentList.Arguments
                    .SelectMany(argSyntax => argSyntax.DescendantNodes().OfType<TypeOfExpressionSyntax>())
                    .FirstOrDefault()?.Type;
                var dataType = GetUnqualifiedName(datatypeSyntax?.ToString());
                if (string.IsNullOrEmpty(dataType)) continue;
                
                var supportedDatatypes = SupportedDataTypes(attName);
                if (!supportedDatatypes.Contains(dataType)) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(
                            NotSupportedDataType,
                            datatypeSyntax!.GetLocation(),
                            [string.Join(", ", supportedDatatypes), ""])
                    );
                    continue;
                }
                
                key = dataType;
            }

            if (!attArguments.TryGetValue(key, out var idSet)) {
                idSet = [];
                attArguments.Add(key, idSet);
            }

            var argSyntaxes = attSyntax.ArgumentList.Arguments
                .SelectMany(argSyntax => argSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>());

            foreach (var argSyntax in argSyntaxes) {
                var id = argSyntax.Token.ValueText;

                var pass = DiagnoseIdCharacters(id, attName, argSyntax.GetLocation(), key != attName,
                    ref diagnosticInfos);
                if (!pass) continue;

                if (!idSet.Add(id)) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(RegisterFieldOrPropertyMultiTimes, argSyntax.GetLocation(), [id])
                    );
                }
            }
        }

        token.ThrowIfCancellationRequested();

        result.Value = new PluginDataFieldsInfo {
            ClassInfo = new ClassInfo {
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
                Name = classSymbol.Name,
            },
            Arguments = attArguments
        };
        result.DiagnosticInfos = diagnosticInfos;

        return result;
    }

    internal static bool DiagnoseIdCharacters(string id, string attName, Location argSyntaxLocation,
        bool isGeneralFieldsAttribute, ref ImmutableArray<DiagnosticInfo> diagnosticInfos) {

        var pass = true;
        if (id.Length == 0 || id.Length > MaxLengthOfArgument(attName)) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(LengthExceedLimitation, argSyntaxLocation,
                    [attName, PrefixSuffixExample(attName), MaxLengthOfArgument(attName)])
            );
            pass = false;
        }

        if (Regex.IsMatch(id, SpecialCharacterPattern)) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(ContainsSpecialCharacters, argSyntaxLocation, [])
            );
            pass = false;
        }

        if (isGeneralFieldsAttribute && Regex.IsMatch(id, "^[0-9]")) {
            diagnosticInfos = diagnosticInfos.Add(new DiagnosticInfo(NameStartsWithNumber, argSyntaxLocation, []));
            pass = false;
        }

        if (Regex.IsMatch(id, UnsuggestedCharacterPattern)) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(ContainsUnsuggestedCharacters, argSyntaxLocation, [])
            );
        }

        return pass;
    }

    private static void Generate(SourceProductionContext context, PluginDataFieldsInfo fieldsInfo) {
        var singleFieldsBuilder = new StringBuilder();
        var seriesFieldsBuilder = new StringBuilder();

        foreach (var kvp in fieldsInfo.Arguments) {
            if (kvp.Key.EndsWith("FieldsAttribute")) {
                seriesFieldsBuilder.Append(GenerateSeriesFields(kvp));
            } else {
                singleFieldsBuilder.Append(GenerateGeneralFields(kvp));
            }
        }

        var classInfo = fieldsInfo.ClassInfo;
        var generatedSourceText =
            $"//  <auto-generated/>" +
#if DEBUG
            $" at {DateTime.Now}" +
#endif
            $"\n\n" +
            $"namespace {classInfo.NameSpace} {{\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name} {{\n" +
            $"{singleFieldsBuilder}\n" +
            $"{seriesFieldsBuilder}\n" +
            $"    }}\n" +
            $"}}";

        context.AddSource($"{classInfo.Name}.g.cs", SourceText.From(generatedSourceText, Encoding.UTF8));
    }

    private static string GenerateSeriesFields(KeyValuePair<string, IdSet> kvp) {
        var attName = kvp.Key;

        //  PartFieldsAttribute => Part
        var fieldPrefix = attName.Substring(0, kvp.Key.Length - 15);
        var attPrefix = fieldPrefix switch {
            "Part" => "PT",
            "Plate" => "PL",
            "Weld" => "W",
            "Bolt" => "B",
            "BoltCircle" => "BC",
            "Chamfer" => "CF",
            _ => string.Empty
        };

        if (attPrefix == string.Empty) { return string.Empty; }

        var infos = attName switch {
            "PartFieldsAttribute" => PartFieldInfos,
            "PlateFieldsAttribute" => PlateFieldInfos,
            "WeldFieldsAttribute" => WeldFieldInfos,
            "BoltFieldsAttribute" => BoltFieldInfos,
            "BoltCircleFieldsAttribute" => BoltCircleFieldInfos,
            "ChamferFieldsAttribute" => ChamferFieldInfos,
            _ => []
        };

        var fieldsBuilder = new StringBuilder();
        foreach (var id in kvp.Value) {
            foreach (var info in infos) {
                fieldsBuilder.AppendLine(
                    $"        \n" +
                    $"        [global::Tekla.Structures.Plugins.StructuresField(\"{attPrefix}{id}{info.AttributeName}\")]\n" +
                    $"        public {info.Type} {fieldPrefix}{ToPropertyNameStyle(id)}{info.Name};");
            }
        }

        return fieldsBuilder.ToString();
    }

    private static string GenerateGeneralFields(KeyValuePair<string, IdSet> kvp) {
        var dataType = kvp.Key;

        if (dataType != "int" && dataType != "double" && dataType != "string") { return string.Empty; }

        var fieldsBuilder = new StringBuilder();
        foreach (var id in kvp.Value) {
            fieldsBuilder.AppendLine(
                $"        \n" +
                $"        [global::Tekla.Structures.Plugins.StructuresField(\"{id}\")]\n" +
                $"        public {dataType} {ToPropertyNameStyle(id)};");
        }

        return fieldsBuilder.ToString();
    }
}