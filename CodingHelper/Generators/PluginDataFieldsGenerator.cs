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
 *  PluginDataFieldsGenerator.cs: help to generate code for plugin data class.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
public class PluginDataFieldsGenerator : IIncrementalGenerator {

    public readonly record struct FieldInfo(
        string PropertyName,
        string DataType,
        string PresetValue,
        string AttributeNamePattern,
        string InpAttributeNamePattern) {

        public string PropertyName { get; } = PropertyName;
        public string DataType { get; } = DataType;
        public string PresetValue { get; } = PresetValue;
        public string AttributeNamePattern { get; } = AttributeNamePattern;
        public string InpAttributeNamePattern { get; } = InpAttributeNamePattern;

    }

    internal static readonly string[] ConcernedAttributes = [
        "Muggle.TsExtensions.CodingHelper.Generators.PartAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.PlateAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.ChamferAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.StudAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.WeldAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.GeneralAttribute",
    ];

    internal static IReadOnlyList<FieldInfo> PartFieldInfos(Version tsmVersion) => [
        new("Name", "string", "", "PT{0}NAME", ""),
        new("Profile", "string", "", "PT{0}PRF", ""),
        new("Material", "string", "", "PT{0}MATL", ""),
        new("Finish", "string", "", "PT{0}FNSH", ""),
        new("Class", "int", "99", "PT{0}CLS", ""),
        new("PartPrefix", "string", "P", "PT{0}PTP", "prefix_pt{0}p"),
        new("PartStartNumber", "int", "1", "PT{0}PTN", "startno_pt{0}p"),
        new("AssemblyPrefix", "string", "A-", "PT{0}ASMP", "prefix_pt{0}a"),
        new("AssemblyStartNumber", "int", "1", "PT{0}ASMN", "startno_pt{0}a"),
    ];

    internal static IReadOnlyList<FieldInfo> PlateFieldInfos(Version tsmVersion) => [
        new("Name", "string", "", "PL{0}NAME", ""),
        new("Thickness", "double", "0.0", "PL{0}T", ""),
        new("Breadth", "double", "0.0", "PL{0}B", ""),
        new("Height", "double", "0.0", "PL{0}H", ""),
        new("Material", "string", "", "PL{0}MATL", ""),
        new("Finish", "string", "", "PL{0}FNSH", ""),
        new("Class", "int", "99", "PL{0}CLS", ""),
        new("PartPrefix", "string", "P", "PL{0}PTP", "prefix_pl{0}p"),
        new("PartStartNumber", "int", "1", "PL{0}PTN", "startno_pl{0}p"),
        new("AssemblyPrefix", "string", "A-", "PL{0}ASMP", "perfix_pl{0}a"),
        new("AssemblyStartNumber", "int", "1", "PL{0}ASMN", "startno_pl{0}a"),
    ];

    internal static IReadOnlyList<FieldInfo> ChamferFieldInfos(Version tsmVersion) => [
        new("Type", "int", "0", "CF{0}TYPE", ""),
        new("X", "double", "0.0", "CF{0}X", ""),
        new("Y", "double", "0.0", "CF{0}Y", ""),
        new("Dz1", "double", "0.0", "CF{0}DZ1", ""),
        new("Dz2", "double", "0.0", "CF{0}DZ2", ""),
    ];

    internal static IReadOnlyList<FieldInfo> BoltFieldInfos(Version tsmVersion) {
        var result = new List<FieldInfo> {
            new("Shape", "int", "0", "B{0}SHAPE", ""),
            new("Standard", "string", "A", "B{0}STD", "b{0}_screwdin"),
            new("Size", "double", "8.0", "B{0}SIZE", "b{0}_diameter"),
            new("DistX", "string", "0.0", "B{0}DISTX", ""),
            new("DistY", "string", "0.0", "B{0}DISTY", ""),
            new("NumberOfBolts", "int", "6", "B{0}NUM", ""),
            new("Diameter", "double", "100.0", "B{0}DIAM", ""),
            new("Type", "int", "0", "B{0}TYPE", ""),
            new("ThreadInMaterial", "int", "1", "B{0}THRD", ""),
            new("Length", "double", "40.0", "B{0}LEN", ""),
            new("CutLength", "double", "100.0", "B{0}CLEN", ""),
            new("ExtraLength", "double", "0.0", "B{0}XLEN", ""),
            new("Tolerance", "double", "2.0", "B{0}TOL", ""),
            new("PlainType", "int", "0", "B{0}PLAIN", ""),
            new("BlindHoleDepth", "double", "0.0", "B{0}DEPTH", ""),
            new("HoleType", "int", "0", "B{0}HOLTY", ""),
            new("SlottedHoleX", "double", "0.0", "B{0}SLOTX", ""),
            new("SlottedHoleY", "double", "0.0", "B{0}SLOTY", ""),
        };

        if (tsmVersion is not null && tsmVersion.Major >= 2023) {
            result.AddRange([
                new("SlotOffsetX", "double", "0.0", "B{0}SOFFX", ""),
                new("SlotOffsetY", "double", "0.0", "B{0}SOFFY", ""),
            ]);
        }

        result.AddRange([
            new("RotateSlots", "int", "2", "B{0}RSLOT", ""),
            new("Hole1", "int", "1", "B{0}HOLE1", ""),
            new("Hole2", "int", "1", "B{0}HOLE2", ""),
            new("Hole3", "int", "0", "B{0}HOLE3", ""),
            new("Hole4", "int", "0", "B{0}HOLE4", ""),
            new("Hole5", "int", "0", "B{0}HOLE5", ""),
            new("IsBolt", "int", "1", "B{0}ISBOT", ""),
            new("UseNut1", "int", "1", "B{0}NUT1", ""),
            new("UseNut2", "int", "0", "B{0}NUT2", ""),
            new("UseWasher1", "int", "0", "B{0}WSHR1", ""),
            new("UseWasher2", "int", "0", "B{0}WSHR2", ""),
            new("UseWasher3", "int", "1", "B{0}WSHR3", ""),
        ]);

        return result;
    }

    internal static IReadOnlyList<FieldInfo> StudFieldInfos(Version tsmVersion) => [
        new("Shape", "int", "0", "S{0}SHAPE", ""),
        new("Standard", "string", "STUD", "S{0}STD", "s{0}_standard"),
        new("Size", "double", "10.0", "S{0}SIZE", "s{0}_size"),
        new("DistX", "string", "0.0", "S{0}DISTX", ""),
        new("DistY", "string", "0.0", "S{0}DISTY", ""),
        new("NumberOfBolts", "int", "6", "S{0}NUM", ""),
        new("Diameter", "double", "100.0", "S{0}DIAM", ""),
        new("Type", "int", "0", "S{0}TYPE", ""),
        new("Length", "double", "40.0", "S{0}LEN", "s{0}_length"),
        new("CutLength", "double", "100.0", "S{0}CLEN", ""),
    ];

    internal static IReadOnlyList<FieldInfo> WeldFieldInfos(Version tsmVersion) => [
        new("PrefixAbove", "string", "", "W{0}PRFXA", "w{0}_prefix"),
        new("PrefixBelow", "string", "", "W{0}PRFXB", "w{0}_prefix2"),
        new("TypeAbove", "int", "0", "W{0}TYPEA", "w{0}_type"),
        new("TypeBelow", "int", "0", "W{0}TYPEB", "w{0}_type2"),
        new("SizeAbove", "double", "0.0", "W{0}SIZEA", "w{0}_size"),
        new("SizeBelow", "double", "0.0", "W{0}SIZEB", "w{0}_size2"),
        new("AdditionalSizeAbove", "double", "0.0", "W{0}ADDSA", "w{0}_other_size"),
        new("AdditionalSizeBelow", "double", "0.0", "W{0}ADDSB", "w{0}_other_size2"),
        new("AngleAbove", "double", "0.0", "W{0}ANGA", "w{0}_angle"),
        new("AngleBelow", "double", "0.0", "W{0}ANGB", "w{0}_angle2"),
        new("ContourAbove", "int", "0", "W{0}CTRA", "w{0}_ftype"),
        new("ContourBelow", "int", "0", "W{0}CTRB", "w{0}_ftype2"),
        new("FinishAbove", "int", "0", "W{0}FNSHA", "w{0}_finish"),
        new("FinishBelow", "int", "0", "W{0}FNSHB", "w{0}_finish2"),
        new("RootFaceAbove", "double", "0.0", "W{0}FACEA", "w{0}_root_face_thick"),
        new("RootFaceBelow", "double", "0.0", "W{0}FACEB", "w{0}_root_face_thck2"),
        new("EffectiveThroatAbove", "double", "0.0", "W{0}THROA", "w{0}_effective_throat"),
        new("EffectiveThroatBelow", "double", "0.0", "W{0}THROB", "w{0}_eff_throat2"),
        new("RootOpeningAbove", "double", "0.0", "W{0}OPNGA", "w{0}_root_opening"),
        new("RootOpeningBelow", "double", "0.0", "W{0}OPNGB", "w{0}_root_opening2"),
        new("Around", "int", "0", "W{0}ARND", "w{0}_around"),
        new("Shop", "int", "0", "W{0}SHOP", "w{0}_wtype"),
        new("Placement", "int", "0", "W{0}PLACE", "w{0}_placement"),
        new("Preparation", "int", "0", "W{0}PREP", "w{0}_preparation"),
        new("Intermittent", "int", "0", "W{0}INTMI", "w{0}_stitch"),
        new("IncrementAmountAbove", "int", "0", "W{0}INCRA", "w{0}_inc_amount"),
        new("IncrementAmountBelow", "int", "0", "W{0}INCRB", "w{0}_inc_amount2"),
        new("LengthAbove", "double", "0.0", "W{0}LENA", "w{0}_length"),
        new("LengthBelow", "double", "0.0", "W{0}LENB", "w{0}_length2"),
        new("PitchAbove", "double", "0.0", "W{0}PITA", "w{0}_cc"),
        new("PitchBelow", "double", "0.0", "W{0}PITB", "w{0}_cc2"),
        new("ReferenceText", "string", "", "W{0}TEXT", "w{0}_text"),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => {
            // BoltAttribute depends on API version
            foreach (var attribute in ConcernedAttributes.Where(a => !a.Contains("Bolt"))) {
                var shortName = GetUnqualifiedName(attribute);
                ctx.AddSource($"{shortName}.g.cs",
                    SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
            }
        });

        var versionProvider = context.CompilationProvider.Select(static (compilation, _) => {
            foreach (var reference in compilation.References) {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol ||
                    assemblySymbol.Name != "Tekla.Structures.Model" &&
                    assemblySymbol.Identity.Name != "Tekla.Structures.Model") {
                    continue;
                }

                return assemblySymbol.Identity.Version;
            }

            return null;
        });

        context.RegisterSourceOutput(versionProvider, (spc, version) => {
            if (version is null) return;

            // BoltAttribute depends on API version
            spc.AddSource("BoltAttribute.g.cs",
                SourceText.From(GetResourceAsString("BoltAttribute.cs", version), Encoding.UTF8));
        });

        var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Where(x => x != default);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var pluginDataFieldsInfos = provider
            .Select(static (x, _) => x.Value)
            .Combine(versionProvider)
            .Where(pair => pair.Left != default && pair.Right is not null);
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
        if (classSymbol is null ||
            classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged"))
            return default; // it is a view model

        var gatheredInfo = Transform(classDeclarationSyntax, semanticModel, token, 0);

        return gatheredInfo == default
            ? default
            : new GatheredInfo<PluginDataFieldsInfo>(gatheredInfo.Item1, gatheredInfo.Item2.ToImmutableArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classDeclarationSyntax"></param>
    /// <param name="semanticModel"></param>
    /// <param name="token"></param>
    /// <param name="classType"><inheritdoc cref="SupportedDataTypes" path="/param[1]"/></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static (PluginDataFieldsInfo, ImmutableArray<DiagnosticInfo>.Builder) Transform(
        ClassDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel,
        CancellationToken token,
        int classType) {
        if (classDeclarationSyntax == null) {
            throw new ArgumentNullException(nameof(classDeclarationSyntax));
        }

        if (semanticModel == null) {
            throw new ArgumentNullException(nameof(semanticModel));
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                out var attSyntaxes))
            return default;

        var builder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) {
            builder.Add(
                new DiagnosticInfo(
                    NotPartial,
                    classDeclarationSyntax.Identifier.GetLocation(),
                    [classDeclarationSyntax.Identifier.ValueText])
            );
            return (default, builder);
        }

        //  Key - for 'GeneralAttribute' is 'int' or 'double' or 'string'
        //        for other attribute is attribute name
        var attArguments = new ArgumentsDictionary<DefaultValueDictionary>();

        foreach (var attSyntax in attSyntaxes) {
            if (attSyntax.ArgumentList is null) continue;

            var attName = GetAttributeName(attSyntax, semanticModel);
            if (!attArguments.TryGetValue(attName, out var elements)) {
                elements = [];
                attArguments.Add(attName, elements);
            }

            if (attName is "GeneralAttribute") {
                AnalyzeGeneralDefaultValues(attSyntax, classType, ref elements, ref builder);
            } else {
                AnalyzeSeriesDefaultValues(attSyntax, ref elements, ref builder);
            }
        }

        token.ThrowIfCancellationRequested();

        var pluginDataFieldsInfo = new PluginDataFieldsInfo {
            ClassInfo = new ClassInfo {
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord,
                Name = classSymbol.Name,
            },
            Arguments = attArguments
        };

        return (pluginDataFieldsInfo, builder);
    }

    private static void AnalyzeGeneralDefaultValues(AttributeSyntax attSyntax, int classType,
        ref DefaultValueDictionary elements,
        ref ImmutableArray<DiagnosticInfo>.Builder builder) {
        if (attSyntax == null) {
            throw new ArgumentNullException(nameof(attSyntax));
        }

        if (attSyntax.ArgumentList is null) return;

        var datatypeSyntax = attSyntax.ArgumentList.DescendantNodes().OfType<TypeOfExpressionSyntax>()
            .FirstOrDefault()?.Type;
        var dataType = GetUnqualifiedName(datatypeSyntax?.ToString());
        if (string.IsNullOrEmpty(dataType)) return;

        var supportedDatatypes = SupportedDataTypes(classType);
        if (!supportedDatatypes.Contains(dataType)) {
            builder.Add(
                new DiagnosticInfo(NotSupportedDataType, datatypeSyntax!.GetLocation(),
                    [string.Join(", ", supportedDatatypes), ""])
            );

            if (dataType is "Boolean") {
                builder.Add(new DiagnosticInfo(ShouldNotUseBooleanType, datatypeSyntax.GetLocation(), []));
            }

            return;
        }

        var exprSyntaxes = attSyntax.ArgumentList.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();
        if (exprSyntaxes.Length == 0) return;

        if (exprSyntaxes.Length % 2 != 0) {
            builder.Add(new DiagnosticInfo(ArgumentsMustBePassedInPairs, exprSyntaxes.Last().GetLocation(), []));

            if (exprSyntaxes.Length < 2) return;
        }

        //  take two at once
        for (int i = 0; i < exprSyntaxes.Length / 2 * 2; i += 2) {
            var nameSyntax = exprSyntaxes[i];
            var valueSyntax = exprSyntaxes[i + 1];

            if (!nameSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                builder.Add(new DiagnosticInfo(NotExpectedDataType, nameSyntax!.GetLocation(), ["string"]));
                continue;
            }

            var name = nameSyntax.Token.ValueText;
            if (!DiagnoseIdCharacters(name, "GeneralAttribute", nameSyntax.GetLocation(), ref builder)) continue;

            var value = valueSyntax.Token.ValueText;

            var isRightType = dataType switch {
                "int" or "Integer" =>
                    valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) && int.TryParse(value, out _),
                "double" or "Double" or "Distance" =>
                    valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) && double.TryParse(value, out _),
                "string" or "String" =>
                    valueSyntax.IsKind(SyntaxKind.StringLiteralExpression),
                "DistanceList" =>
                    valueSyntax.IsKind(SyntaxKind.StringLiteralExpression) && IsDistanceList(value),
                _ => false
            };

            if (!isRightType) {
                builder.Add(new DiagnosticInfo(NotExpectedDataType, valueSyntax.GetLocation(), [dataType]));
                continue;
            }

            if (!elements.TryGetValue(dataType, out var valueDict)) {
                valueDict = new Dictionary<string, string>();
                elements.Add(dataType, valueDict);
            }

            try {
                valueDict.Add(name, value);
            } catch (ArgumentException) {
                builder.Add(new DiagnosticInfo(RegisterFieldOrPropertyMultiTimes, nameSyntax.GetLocation(), []));
            }
        }
    }

    private static void AnalyzeSeriesDefaultValues(AttributeSyntax attSyntax, ref DefaultValueDictionary elements,
        ref ImmutableArray<DiagnosticInfo>.Builder builder) {
        var argSyntaxes = attSyntax.ArgumentList?.Arguments;
        if (argSyntaxes is null) return;
        var attName = attSyntax.Name.ToString();
        if (!attName.EndsWith("Attribute")) attName += "Attribute";

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

                if (!DiagnoseIdCharacters(id, attName, argSyntax.GetLocation(), ref builder)) return;

                if (elements.ContainsKey(id)) {
                    builder.Add(new DiagnosticInfo(RegisterFieldOrPropertyMultiTimes, argSyntax.GetLocation(), [id]));
                    return;
                }

                continue;
            }

            //  style of (Property = value)
            paramName ??= argSyntax.NameEquals?.Name.Identifier.ValueText;
            if (paramName is null) continue;

            valueDict.Add(paramName, paramValue);
        }

        //  id != string.Empty
        elements.Add(id, valueDict);
    }

    private static void Generate(SourceProductionContext context, (PluginDataFieldsInfo, Version) pair) {
        var fieldsInfo = pair.Item1;
        var tsmVersion = pair.Item2;

        var singleFieldsDeclarationBuilder = new StringBuilder();
        var seriesFieldsDeclarationBuilder = new StringBuilder();
        var singleFieldsSetToDefaultBuilder = new StringBuilder();
        var seriesFieldsSetToDefaultBuilder = new StringBuilder();
        var createModifyMethodBuilder = new StringBuilder();

        foreach (var kvp in fieldsInfo.Arguments) {
            if (kvp.Key is "GeneralAttribute") {
                GenerateGeneralFields(kvp.Value, ref singleFieldsDeclarationBuilder,
                    ref singleFieldsSetToDefaultBuilder);
            } else {
                GenerateSeriesFields(kvp, tsmVersion, ref seriesFieldsDeclarationBuilder,
                    ref seriesFieldsSetToDefaultBuilder);
                GenerateCreateModifyMethod(kvp.Key, [.. kvp.Value.Select(keyValuePair => keyValuePair.Key)], tsmVersion,
                    ref createModifyMethodBuilder);
            }
        }

        var classInfo = fieldsInfo.ClassInfo;
        var generatedSourceText =
#if DEBUG
            $"//  <auto-generated/> at {DateTime.Now}\r\n" +
#else
            $"//  <auto-generated/>\r\n" +
#endif
            $"\r\n" +
            $"namespace {classInfo.NameSpace} {{\r\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name} {{\r\n" +
            $"{singleFieldsDeclarationBuilder}\r\n" +
            $"{seriesFieldsDeclarationBuilder}\r\n" +
            $"        /// <summary>\r\n" +
            $"        /// Returns true if the given value is set to the default value for this type.\r\n" +
            $"        /// </summary>\r\n" +
            $"        /// <param name=\"value\">The value to test.</param>\r\n" +
            $"        /// <returns>True if the value is set to the default.</returns>\r\n" +
            $"        private static bool IsDefaultValue(int value) {{\r\n" +
            $"            return value == int.MinValue;\r\n" +
            $"        }}\r\n" +
            $"        \r\n" +
            $"        /// <summary>\r\n" +
            $"        /// Returns true if the given value is set to the default value for this type.\r\n" +
            $"        /// </summary>\r\n" +
            $"        /// <param name=\"value\">The value to test.</param>\r\n" +
            $"        /// <returns>True if the value is set to the default.</returns>\r\n" +
            $"        private static bool IsDefaultValue(double value) {{\r\n" +
            $"            return value == int.MinValue;\r\n" +
            $"        }}\r\n" +
            $"        \r\n" +
            $"        /// <summary>\r\n" +
            $"        /// Returns true if the given value is set to the default value for this type.\r\n" +
            $"        /// </summary>\r\n" +
            $"        /// <param name=\"value\">The value to test.</param>\r\n" +
            $"        /// <returns>True if the value is set to the default.</returns>\r\n" +
            $"        private static bool IsDefaultValue(string value) {{\r\n" +
            $"            return string.IsNullOrEmpty(value);\r\n" +
            $"        }}\r\n" +
            $"        \r\n" +
            $"        /// <summary>\r\n" +
            $"        /// Set fields to default value if they are not set from user interface.\r\n" +
            $"        /// </summary>\r\n" +
            $"        public void SetFieldsToDefaultIfUnset() {{\r\n" +
            $"{singleFieldsSetToDefaultBuilder}\r\n" +
            $"{seriesFieldsSetToDefaultBuilder}\r\n" +
            $"        }}\r\n" +
            $"{createModifyMethodBuilder}\r\n" +
            $"    }}\r\n" +
            $"}}";

        context.AddSource($"{classInfo.Name}.g.cs", SourceText.From(generatedSourceText, Encoding.UTF8));
    }

    private static void GenerateSeriesFields(KeyValuePair<string, DefaultValueDictionary> kvp, Version tsmVersion,
        ref StringBuilder seriesFieldsDeclarationBuilder, ref StringBuilder seriesFieldsSetToDefaultBuilder) {
        var attName = kvp.Key;

        //  PartAttribute => Part
        var fieldPrefix = attName.Substring(0, kvp.Key.Length - 9);

        var infos = attName switch {
            "PartAttribute" => PartFieldInfos(tsmVersion),
            "PlateAttribute" => PlateFieldInfos(tsmVersion),
            "BoltAttribute" => BoltFieldInfos(tsmVersion),
            "StudAttribute" => StudFieldInfos(tsmVersion),
            "WeldAttribute" => WeldFieldInfos(tsmVersion),
            "ChamferAttribute" => ChamferFieldInfos(tsmVersion),
            _ => []
        };

        var cnt = 0;
        foreach (var idValuePair in kvp.Value) {
            cnt++;

            var id = idValuePair.Key;
            var inpId = cnt.ToString();

            var propertyDict = idValuePair.Value;

            foreach (var (property, dataType, presetValue, attributeNamePattern, inpAttributeNamePattern) in infos) {
                var fieldName = $"{fieldPrefix}{ToPropertyNameStyle(id)}{property}";
                var inpFieldName = $"Inp{fieldName}";

                if (!propertyDict.TryGetValue(property, out var value)) value = presetValue;
                if (dataType is "string") value = $"\"{value}\"";

                var attributerName = string.Format(attributeNamePattern, id);
                seriesFieldsDeclarationBuilder.AppendLine(
                    $"        \r\n" +
                    $"        [global::Tekla.Structures.Plugins.StructuresField(\"{attributerName}\")]\r\n" +
                    $"        public {dataType} {fieldName};");

                if (inpAttributeNamePattern == string.Empty) {
                    seriesFieldsSetToDefaultBuilder.AppendLine(
                        $"            if (IsDefaultValue({fieldName})) {fieldName} = {value};");
                } else {
                    var inpAttributeName = string.Format(inpAttributeNamePattern, inpId);
                    // max lenght is 19
                    if (inpAttributeName.Length > 19) inpAttributeName = inpAttributeName.Substring(0, 19);

                    seriesFieldsDeclarationBuilder.AppendLine(
                        $"        \r\n" +
                        $"        /// <summary>\r\n" +
                        $"        /// Used for INP compatibility, no need to access manually.\r\n" +
                        $"        /// </summary>\r\n" +
                        $"        [global::Tekla.Structures.Plugins.StructuresField(\"{inpAttributeName}\")]\r\n" +
                        $"        public {dataType} {inpFieldName};");

                    seriesFieldsSetToDefaultBuilder.AppendLine(
                        $"            if (!IsDefaultValue({inpFieldName})) {fieldName} = {inpFieldName};\r\n" +
                        $"            else if (IsDefaultValue({fieldName})) {fieldName} = {value};");
                }
            }
        }
    }

    private static void GenerateGeneralFields(DefaultValueDictionary kvp,
        ref StringBuilder singleFieldsDeclarationBuilder, ref StringBuilder singleFieldsSetToDefaultBuilder) {
        foreach (var dataTypeValuePair in kvp) {
            var dataType = dataTypeValuePair.Key;
            var parameterDict = dataTypeValuePair.Value;

            foreach (var paramValuePair in parameterDict) {
                var param = paramValuePair.Key;
                var value = paramValuePair.Value;
                if (dataType is "string") value = $"\"{value}\"";

                var fieldName = ToPropertyNameStyle(param);

                singleFieldsDeclarationBuilder.AppendLine(
                    $"        \r\n" +
                    $"        [global::Tekla.Structures.Plugins.StructuresField(\"{param}\")]\r\n" +
                    $"        public {dataType} {fieldName};");

                singleFieldsSetToDefaultBuilder.AppendLine(
                    $"            if (IsDefaultValue({fieldName})) {fieldName} = {value};");
            }
        }
    }

    private static void GenerateCreateModifyMethod(string attributeName, ICollection<string> ids, Version tsmVersion,
        ref StringBuilder builder) {
        const string TsmNameSpace = "global::Tekla.Structures.Model";
        const string TsdNameSpace = "global::Tekla.Structures.Datatype";

        foreach (var id in ids) {
            string propertyAccess;
            switch (attributeName) {
            case "PartAttribute":
                propertyAccess = $"Part{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <typeparamref name=\"T\"/> type model object for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\r\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\r\n" +
                    $"        public T CreatePart{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.Part, new() {{\r\n" +
                    $"            T part = new T();\r\n" +
                    $"            part.Name = {propertyAccess}Name;\r\n" +
                    $"            part.Profile.ProfileString = {propertyAccess}Profile;\r\n" +
                    $"            part.Material.MaterialString = {propertyAccess}Material;\r\n" +
                    $"            part.Finish = {propertyAccess}Finish;\r\n" +
                    $"            part.Class = {propertyAccess}Class.ToString();\r\n" +
                    $"            part.PartNumber.Prefix = {propertyAccess}PartPrefix;\r\n" +
                    $"            part.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\r\n" +
                    $"            part.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\r\n" +
                    $"            part.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\r\n" +
                    $"            return part;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given model object to make its properties consistent with part '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"part\">The part to be modified.</param>\r\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"part\"/>.</typeparam>\r\n" +
                    $"        public void ModifyToPart{ToPropertyNameStyle(id)}<T>(ref T part) where T : {TsmNameSpace}.Part {{\r\n" +
                    $"            part.Name = {propertyAccess}Name;\r\n" +
                    $"            part.Profile.ProfileString = {propertyAccess}Profile;\r\n" +
                    $"            part.Material.MaterialString = {propertyAccess}Material;\r\n" +
                    $"            part.Finish = {propertyAccess}Finish;\r\n" +
                    $"            part.Class = {propertyAccess}Class.ToString();\r\n" +
                    $"            part.PartNumber.Prefix = {propertyAccess}PartPrefix;\r\n" +
                    $"            part.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\r\n" +
                    $"            part.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\r\n" +
                    $"            part.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\r\n" +
                    $"        }}");
                break;
            case "PlateAttribute":
                propertyAccess = $"Plate{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <typeparamref name=\"T\"/> type model object for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\r\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\r\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\r\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.Brep\"/>.\r\n" +
                    $"        /// </exception>\r\n" +
                    $"        public T CreatePlate{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.Part, new() {{\r\n" +
                    $"            if (typeof(T) == typeof({TsmNameSpace}.Brep))\r\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.Brep)}}\\\" not supported.\");\r\n" +
                    $"            \r\n" +
                    $"            T plate = new T();\r\n" +
                    $"            plate.Name = {propertyAccess}Name;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.Beam\":\r\n" +
                    $"            case \"Tekla.Structures.Model.PolyBeam\":\r\n" +
                    $"            case \"Tekla.Structures.Model.SpiralBeam\":\r\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}*{{{propertyAccess}Breadth}}\";\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BentPlate\":\r\n" +
                    $"            case \"Tekla.Structures.Model.ContourPlate\":\r\n" +
                    $"            case \"Tekla.Structures.Model.LoftedPlate\":\r\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}\";\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            plate.Material.MaterialString = {propertyAccess}Material;\r\n" +
                    $"            plate.Finish = {propertyAccess}Finish;\r\n" +
                    $"            plate.Class = {propertyAccess}Class.ToString();\r\n" +
                    $"            plate.PartNumber.Prefix = {propertyAccess}PartPrefix;\r\n" +
                    $"            plate.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\r\n" +
                    $"            plate.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\r\n" +
                    $"            plate.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\r\n" +
                    $"            return plate;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given model object to make its properties consistent with plate '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"plate\">The plate to be modified.</param>\r\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"plate\"/>.</typeparam>\r\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\r\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.Brep\"/>.\r\n" +
                    $"        /// </exception>\r\n" +
                    $"        public void ModifyToPlate{ToPropertyNameStyle(id)}<T>(ref T plate) where T : {TsmNameSpace}.Part {{\r\n" +
                    $"            if (typeof(T) == typeof({TsmNameSpace}.Brep))\r\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.Brep)}}\\\" not supported.\");\r\n" +
                    $"            \r\n" +
                    $"            plate.Name = {propertyAccess}Name;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.Beam\":\r\n" +
                    $"            case \"Tekla.Structures.Model.PolyBeam\":\r\n" +
                    $"            case \"Tekla.Structures.Model.SpiralBeam\":\r\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}*{{{propertyAccess}Breadth}}\";\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BentPlate\":\r\n" +
                    $"            case \"Tekla.Structures.Model.ContourPlate\":\r\n" +
                    $"            case \"Tekla.Structures.Model.LoftedPlate\":\r\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}\";\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            plate.Material.MaterialString = {propertyAccess}Material;\r\n" +
                    $"            plate.Finish = {propertyAccess}Finish;\r\n" +
                    $"            plate.Class = {propertyAccess}Class.ToString();\r\n" +
                    $"            plate.PartNumber.Prefix = {propertyAccess}PartPrefix;\r\n" +
                    $"            plate.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\r\n" +
                    $"            plate.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\r\n" +
                    $"            plate.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\r\n" +
                    $"        }}");
                break;
            case "BoltAttribute":
                propertyAccess = $"Bolt{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <typeparamref name=\"T\"/> type model object for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\r\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\r\n" +
                    $"        public T CreateBolt{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.BoltGroup, new() {{\r\n" +
                    $"            {TsmNameSpace}.BoltGroup bolt = null;\r\n" +
                    $"            int shape = 0;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\r\n" +
                    $"                bolt = new {TsmNameSpace}.BoltArray();\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltCircle\":\r\n" +
                    $"                shape = 1;\r\n" +
                    $"                bolt = new {TsmNameSpace}.BoltCircle();\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\r\n" +
                    $"                shape = 2;\r\n" +
                    $"                bolt = new {TsmNameSpace}.BoltXYList();\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\r\n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\r\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\r\n" +
                    $"            bolt.Length = {propertyAccess}Length;\r\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\r\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\r\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\r\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\r\n" +
                    $"            bolt.BlindHoleDepth = {propertyAccess}BlindHoleDepth;\r\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\r\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\r\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\r\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\r\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\r\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\r\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\r\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;");

                if (tsmVersion.Major >= 2023) {
                    builder.AppendLine(
                        $"            bolt.SlotOffsetX = {propertyAccess}SlotOffsetX;\r\n" +
                        $"            bolt.SlotOffsetY = {propertyAccess}SlotOffsetY;"
                    );
                }

                builder.AppendLine(
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\r\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\r\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\r\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\r\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\r\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\r\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\r\n" +
                    $"            \r\n" +
                    $"            {TsdNameSpace}.DistanceList distListX;\r\n" +
                    $"            {TsdNameSpace}.DistanceList distListY;\r\n" +
                    $"            if (shape != 1) {{\r\n" +
                    $"                 distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistX, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"                 distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistY, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"            }}\r\n" +
                    $"            switch (shape) {{\r\n" +
                    $"            case 0:\r\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = bolt as {TsmNameSpace}.BoltArray;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltArray.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltArray.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            case 1:\r\n" +
                    $"                {TsmNameSpace}.BoltCircle boltCircle = bolt as {TsmNameSpace}.BoltCircle;\r\n" +
                    $"                boltCircle.NumberOfBolts = {propertyAccess}NumberOfBolts;\r\n" +
                    $"                boltCircle.Diameter = {propertyAccess}Diameter;\r\n" +
                    $"                break;" +
                    $"            case 2:\r\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = bolt as {TsmNameSpace}.BoltXYList;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            \r\n" +
                    $"            return bolt as T;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given model object to make its properties consistent with bolt '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"bolt\">The bolt to be modified.</param>\r\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"bolt\"/>.</typeparam>\r\n" +
                    $"        public void ModifyToBolt{ToPropertyNameStyle(id)}<T>(ref T bolt) where T : {TsmNameSpace}.BoltGroup {{\r\n" +
                    $"            int shape = 0;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltCircle\":\r\n" +
                    $"                shape = 1;\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\r\n" +
                    $"                shape = 2;\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\r\n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\r\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\r\n" +
                    $"            bolt.Length = {propertyAccess}Length;\r\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\r\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\r\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\r\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\r\n" +
                    $"            bolt.BlindHoleDepth = {propertyAccess}BlindHoleDepth;\r\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\r\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\r\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\r\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\r\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\r\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\r\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\r\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;");

                if (tsmVersion.Major >= 2023) {
                    builder.AppendLine(
                        $"            bolt.SlotOffsetX = {propertyAccess}SlotOffsetX;\r\n" +
                        $"            bolt.SlotOffsetY = {propertyAccess}SlotOffsetY;"
                    );
                }

                builder.AppendLine(
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\r\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\r\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\r\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\r\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\r\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\r\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\r\n" +
                    $"            \r\n" +
                    $"            {TsdNameSpace}.DistanceList distListX;\r\n" +
                    $"            {TsdNameSpace}.DistanceList distListY;\r\n" +
                    $"            if (shape != 1) {{\r\n" +
                    $"                 distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistX, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"                 distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistY, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"            }}\r\n" +
                    $"            switch (shape) {{\r\n" +
                    $"            case 0:\r\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = bolt as {TsmNameSpace}.BoltArray;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltArray.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltArray.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            case 1:\r\n" +
                    $"                {TsmNameSpace}.BoltCircle boltCircle = bolt as {TsmNameSpace}.BoltCircle;\r\n" +
                    $"                boltCircle.NumberOfBolts = {propertyAccess}NumberOfBolts;\r\n" +
                    $"                boltCircle.Diameter = {propertyAccess}Diameter;\r\n" +
                    $"                break;" +
                    $"            case 2:\r\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = bolt as {TsmNameSpace}.BoltXYList;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"        }}");
                break;
            case "StudAttribute":
                propertyAccess = $"Stud{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <typeparamref name=\"T\"/> type model object for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\r\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\r\n" +
                    $"        public T CreateStud{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.BoltGroup, new() {{\r\n" +
                    $"            {TsmNameSpace}.BoltGroup stud = null;\r\n" +
                    $"            int shape = 0;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\r\n" +
                    $"                stud = new {TsmNameSpace}.BoltArray();\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltCircle\":\r\n" +
                    $"                shape = 1;\r\n" +
                    $"                stud = new {TsmNameSpace}.BoltCircle();\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\r\n" +
                    $"                shape = 2;\r\n" +
                    $"                stud = new {TsmNameSpace}.BoltXYList();\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            stud.BoltStandard = {propertyAccess}Standard;\r\n" +
                    $"            stud.BoltSize = {propertyAccess}Size;\r\n" +
                    $"            stud.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            stud.Length = {propertyAccess}Length;\r\n" +
                    $"            stud.CutLength = {propertyAccess}CutLength;\r\n" +
                    $"            \r\n" +
                    $"            {TsdNameSpace}.DistanceList distListX;\r\n" +
                    $"            {TsdNameSpace}.DistanceList distListY;\r\n" +
                    $"            if (shape != 1) {{\r\n" +
                    $"                 distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistX, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"                 distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistY, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"            }}\r\n" +
                    $"            switch (shape) {{\r\n" +
                    $"            case 0:\r\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = stud as {TsmNameSpace}.BoltArray;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltArray.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltArray.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            case 1:\r\n" +
                    $"                {TsmNameSpace}.BoltCircle boltCircle = stud as {TsmNameSpace}.BoltCircle;\r\n" +
                    $"                boltCircle.NumberOfBolts = {propertyAccess}NumberOfBolts;\r\n" +
                    $"                boltCircle.Diameter = {propertyAccess}Diameter;\r\n" +
                    $"                break;" +
                    $"            case 2:\r\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = stud as {TsmNameSpace}.BoltXYList;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            \r\n" +
                    $"            return stud as T;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given model object to make its properties consistent with stud '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"stud\">The stud to be modified.</param>\r\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"stud\"/>.</typeparam>\r\n" +
                    $"        public void ModifyToStud{ToPropertyNameStyle(id)}<T>(ref T stud) where T : {TsmNameSpace}.BoltGroup {{\r\n" +
                    $"            int shape = 0;\r\n" +
                    $"            switch (typeof(T).ToString()) {{\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltCircle\":\r\n" +
                    $"                shape = 1;\r\n" +
                    $"                break;\r\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\r\n" +
                    $"                shape = 2;\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"            stud.BoltStandard = {propertyAccess}Standard;\r\n" +
                    $"            stud.BoltSize = {propertyAccess}Size;\r\n" +
                    $"            stud.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            stud.Length = {propertyAccess}Length;\r\n" +
                    $"            stud.CutLength = {propertyAccess}CutLength;\r\n" +
                    $"            \r\n" +
                    $"            {TsdNameSpace}.DistanceList distListX;\r\n" +
                    $"            {TsdNameSpace}.DistanceList distListY;\r\n" +
                    $"            if (shape != 1) {{\r\n" +
                    $"                 distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistX, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"                 distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistY, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\r\n" +
                    $"            }}\r\n" +
                    $"            switch (shape) {{\r\n" +
                    $"            case 0:\r\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = stud as {TsmNameSpace}.BoltArray;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltArray.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltArray.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            case 1:\r\n" +
                    $"                {TsmNameSpace}.BoltCircle boltCircle = stud as {TsmNameSpace}.BoltCircle;\r\n" +
                    $"                boltCircle.NumberOfBolts = {propertyAccess}NumberOfBolts;\r\n" +
                    $"                boltCircle.Diameter = {propertyAccess}Diameter;\r\n" +
                    $"                break;" +
                    $"            case 2:\r\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = stud as {TsmNameSpace}.BoltXYList;\r\n" +
                    $"                foreach (var dist in distListX) {{\r\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                foreach (var dist in distListY) {{\r\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Value);\r\n" +
                    $"                }}\r\n" +
                    $"                break;\r\n" +
                    $"            }}\r\n" +
                    $"        }}");
                break;
            case "WeldAttribute":
                propertyAccess = $"Weld{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <typeparamref name=\"T\"/> type model object for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\r\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\r\n" +
                    $"        public T CreateWeld{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.BaseWeld, new() {{\r\n" +
                    $"            T weld = new T();\r\n" +
                    $"            weld.PrefixAboveLine = {propertyAccess}PrefixAbove;\r\n" +
                    $"            weld.PrefixBelowLine = {propertyAccess}PrefixBelow;\r\n" +
                    $"            weld.TypeAbove = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeAbove.ToString(), true);\r\n" +
                    $"            weld.TypeBelow = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeBelow.ToString(), true);\r\n" +
                    $"            weld.SizeAbove = {propertyAccess}SizeAbove;\r\n" +
                    $"            weld.SizeBelow = {propertyAccess}SizeBelow;\r\n" +
                    $"            weld.AdditionalSizeAbove = {propertyAccess}AdditionalSizeAbove;\r\n" +
                    $"            weld.AdditionalSizeBelow = {propertyAccess}AdditionalSizeBelow;\r\n" +
                    $"            weld.AngleAbove = {propertyAccess}AngleAbove;\r\n" +
                    $"            weld.AngleBelow = {propertyAccess}AngleBelow;\r\n" +
                    $"            weld.ContourAbove = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourAbove.ToString(), true);\r\n" +
                    $"            weld.ContourBelow = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourBelow.ToString(), true);\r\n" +
                    $"            weld.FinishAbove = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishAbove.ToString(), true);\r\n" +
                    $"            weld.FinishBelow = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishBelow.ToString(), true);\r\n" +
                    $"            weld.RootFaceAbove = {propertyAccess}RootFaceAbove;\r\n" +
                    $"            weld.RootFaceBelow = {propertyAccess}RootFaceBelow;\r\n" +
                    $"            weld.EffectiveThroatAbove = {propertyAccess}EffectiveThroatAbove;\r\n" +
                    $"            weld.EffectiveThroatBelow = {propertyAccess}EffectiveThroatBelow;\r\n" +
                    $"            weld.RootOpeningAbove = {propertyAccess}RootOpeningAbove;\r\n" +
                    $"            weld.RootOpeningBelow = {propertyAccess}RootOpeningBelow;\r\n" +
                    $"            weld.AroundWeld = {propertyAccess}Around != 0;\r\n" +
                    $"            weld.ShopWeld = {propertyAccess}Shop != 0;\r\n" +
                    $"            weld.Placement = ({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum), {propertyAccess}Placement.ToString(), true);\r\n" +
                    $"            weld.Preparation = ({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum), {propertyAccess}Preparation.ToString(), true);\r\n" +
                    $"            weld.IntermittentType = ({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum)global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum), {propertyAccess}Intermittent.ToString(), true);\r\n" +
                    $"            weld.IncrementAmountAbove = {propertyAccess}IncrementAmountAbove;\r\n" +
                    $"            weld.IncrementAmountBelow = {propertyAccess}IncrementAmountBelow;\r\n" +
                    $"            weld.LengthAbove = {propertyAccess}LengthAbove;\r\n" +
                    $"            weld.LengthBelow = {propertyAccess}LengthBelow;\r\n" +
                    $"            weld.PitchAbove = {propertyAccess}PitchAbove;\r\n" +
                    $"            weld.PitchBelow = {propertyAccess}PitchBelow;\r\n" +
                    $"            weld.ReferenceText = {propertyAccess}ReferenceText;\r\n" +
                    $"            return weld;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given model object to make its properties consistent with weld '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"weld\">The weld to be modified.</param>\r\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"weld\"/>.</typeparam>\r\n" +
                    $"        public void ModifyToWeld{ToPropertyNameStyle(id)}<T>(ref T weld) where T : {TsmNameSpace}.BaseWeld {{\r\n" +
                    $"            weld.PrefixAboveLine = {propertyAccess}PrefixAbove;\r\n" +
                    $"            weld.PrefixBelowLine = {propertyAccess}PrefixBelow;\r\n" +
                    $"            weld.TypeAbove = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeAbove.ToString(), true);\r\n" +
                    $"            weld.TypeBelow = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeBelow.ToString(), true);\r\n" +
                    $"            weld.SizeAbove = {propertyAccess}SizeAbove;\r\n" +
                    $"            weld.SizeBelow = {propertyAccess}SizeBelow;\r\n" +
                    $"            weld.AdditionalSizeAbove = {propertyAccess}AdditionalSizeAbove;\r\n" +
                    $"            weld.AdditionalSizeBelow = {propertyAccess}AdditionalSizeBelow;\r\n" +
                    $"            weld.AngleAbove = {propertyAccess}AngleAbove;\r\n" +
                    $"            weld.AngleBelow = {propertyAccess}AngleBelow;\r\n" +
                    $"            weld.ContourAbove = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourAbove.ToString(), true);\r\n" +
                    $"            weld.ContourBelow = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourBelow.ToString(), true);\r\n" +
                    $"            weld.FinishAbove = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishAbove.ToString(), true);\r\n" +
                    $"            weld.FinishBelow = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishBelow.ToString(), true);\r\n" +
                    $"            weld.RootFaceAbove = {propertyAccess}RootFaceAbove;\r\n" +
                    $"            weld.RootFaceBelow = {propertyAccess}RootFaceBelow;\r\n" +
                    $"            weld.EffectiveThroatAbove = {propertyAccess}EffectiveThroatAbove;\r\n" +
                    $"            weld.EffectiveThroatBelow = {propertyAccess}EffectiveThroatBelow;\r\n" +
                    $"            weld.RootOpeningAbove = {propertyAccess}RootOpeningAbove;\r\n" +
                    $"            weld.RootOpeningBelow = {propertyAccess}RootOpeningBelow;\r\n" +
                    $"            weld.AroundWeld = {propertyAccess}Around != 0;\r\n" +
                    $"            weld.ShopWeld = {propertyAccess}Shop != 0;\r\n" +
                    $"            weld.Placement = ({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum), {propertyAccess}Placement.ToString(), true);\r\n" +
                    $"            weld.Preparation = ({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum), {propertyAccess}Preparation.ToString(), true);\r\n" +
                    $"            weld.IntermittentType = ({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum)global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum), {propertyAccess}Intermittent.ToString(), true);\r\n" +
                    $"            weld.IncrementAmountAbove = {propertyAccess}IncrementAmountAbove;\r\n" +
                    $"            weld.IncrementAmountBelow = {propertyAccess}IncrementAmountBelow;\r\n" +
                    $"            weld.LengthAbove = {propertyAccess}LengthAbove;\r\n" +
                    $"            weld.LengthBelow = {propertyAccess}LengthBelow;\r\n" +
                    $"            weld.PitchAbove = {propertyAccess}PitchAbove;\r\n" +
                    $"            weld.PitchBelow = {propertyAccess}PitchBelow;\r\n" +
                    $"            weld.ReferenceText = {propertyAccess}ReferenceText;\r\n" +
                    $"        }}");
                break;
            case "ChamferAttribute":
                propertyAccess = $"Chamfer{ToPropertyNameStyle(id)}";

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Create <see cref=\"global::Tekla.Structures.Model.Chamfer\"/> for id '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <returns>The <see cref=\"global::Tekla.Structures.Model.Chamfer\"/> created.</returns>\r\n" +
                    $"        public {TsmNameSpace}.Chamfer CreateChamfer{ToPropertyNameStyle(id)}() {{\r\n" +
                    $"            {TsmNameSpace}.Chamfer chamfer = new {TsmNameSpace}.Chamfer();\r\n" +
                    $"            chamfer.Type = ({TsmNameSpace}.Chamfer.ChamferTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.Chamfer.ChamferTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            chamfer.X = {propertyAccess}X;\r\n" +
                    $"            chamfer.Y = {propertyAccess}Y;\r\n" +
                    $"            chamfer.DZ1 = {propertyAccess}Dz1;\r\n" +
                    $"            chamfer.DZ2 = {propertyAccess}Dz2;\r\n" +
                    $"            return chamfer;\r\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \r\n" +
                    $"        /// <summary>\r\n" +
                    $"        /// Modify the given chamfer to make its properties consistent with chamfer '{id}'.\r\n" +
                    $"        /// </summary>\r\n" +
                    $"        /// <param name=\"chamfer\">The chamfer to be modified.</param>\r\n" +
                    $"        public void ModifyToChamfer{ToPropertyNameStyle(id)}(ref {TsmNameSpace}.Chamfer chamfer) {{\r\n" +
                    $"            chamfer.Type = ({TsmNameSpace}.Chamfer.ChamferTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.Chamfer.ChamferTypeEnum), {propertyAccess}Type.ToString(), true);\r\n" +
                    $"            chamfer.X = {propertyAccess}X;\r\n" +
                    $"            chamfer.Y = {propertyAccess}Y;\r\n" +
                    $"            chamfer.DZ1 = {propertyAccess}Dz1;\r\n" +
                    $"            chamfer.DZ2 = {propertyAccess}Dz2;\r\n" +
                    $"        }}");
                break;
            }
        }
    }
}