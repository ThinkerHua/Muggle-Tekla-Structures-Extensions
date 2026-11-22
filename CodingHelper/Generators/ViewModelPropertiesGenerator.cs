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
 *  ViewModelPropertiesGenerator.cs: help to generate code for view model which used by plugin WPF UI.
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
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
public class ViewModelPropertiesGenerator : IIncrementalGenerator {

    private readonly record struct PropertyInfo(
        string PropertyName,
        string DataType,
        string PresetValue,
        string Validation,
        string AttributeNamePattern) {

        public string PropertyName { get; } = PropertyName;
        public string DataType { get; } = DataType;
        public string PresetValue { get; } = PresetValue;
        public string Validation { get; } = Validation;
        public string AttributeNamePattern { get; } = AttributeNamePattern;
    }

    private const string TsDatatype = "global::Tekla.Structures.Datatype";
    private const string TsDialog = "global::Tekla.Structures.Dialog";
    private const string CurrentCulture = "global::System.Globalization.CultureInfo.CurrentCulture";

    private static IReadOnlyList<PropertyInfo> PartPropertyInfos(Version tsmVersion) => [
        new("Name", "String", "", "string.IsNullOrEmpty(value)", "PT{0}NAME"),
        new("Profile", "String", "", "string.IsNullOrEmpty(value)", "PT{0}PRF"),
        new("Material", "String", "", "string.IsNullOrEmpty(value)", "PT{0}MATL"),
        new("Finish", "String", "", "string.IsNullOrEmpty(value)", "PT{0}FNSH"),
        new("Class", "Integer", "99", "value == int.MinValue", "PT{0}CLS"),
        new("PartPrefix", "String", "P", "string.IsNullOrEmpty(value)", "PT{0}PTP"),
        new("PartStartNumber", "Integer", "1", "value == int.MinValue", "PT{0}PTN"),
        new("AssemblyPrefix", "String", "A-", "string.IsNullOrEmpty(value)", "PT{0}ASMP"),
        new("AssemblyStartNumber", "Integer", "1", "value == int.MinValue", "PT{0}ASMN"),
    ];

    private static IReadOnlyList<PropertyInfo> PlatePropertyInfos(Version tsmVersion) => [
        new("Name", "String", "", "string.IsNullOrEmpty(value)", "PL{0}NAME"),
        new("Thickness", "Double", "0.0", "value == int.MinValue", "PL{0}T"),
        new("Breadth", "Double", "0.0", "value == int.MinValue", "PL{0}B"),
        new("Height", "Double", "0.0", "value == int.MinValue", "PL{0}H"),
        new("Material", "String", "", "string.IsNullOrEmpty(value)", "PL{0}MATL"),
        new("Finish", "String", "", "string.IsNullOrEmpty(value)", "PL{0}FNSH"),
        new("Class", "Integer", "99", "value == int.MinValue", "PL{0}CLS"),
        new("PartPrefix", "String", "P", "string.IsNullOrEmpty(value)", "PL{0}PTP"),
        new("PartStartNumber", "Integer", "1", "value == int.MinValue", "PL{0}PTN"),
        new("AssemblyPrefix", "String", "A-", "string.IsNullOrEmpty(value)", "PL{0}ASMP"),
        new("AssemblyStartNumber", "Integer", "1", "value == int.MinValue", "PL{0}ASMN"),
    ];

    private static IReadOnlyList<PropertyInfo> WeldPropertyInfos(Version tsmVersion) => [
        new("PrefixAbove", "String", "", "string.IsNullOrEmpty(value)", "W{0}PRFXA"),
        new("PrefixBelow", "String", "", "string.IsNullOrEmpty(value)", "W{0}PRFXB"),
        new("TypeAbove", "Integer", "0", "value < 0 || value > 26", "W{0}TYPEA"),
        new("TypeBelow", "Integer", "0", "value < 0 || value > 26", "W{0}TYPEB"),
        new("SizeAbove", "Double", "0.0", "value == int.MinValue", "W{0}SIZEA"),
        new("SizeBelow", "Double", "0.0", "value == int.MinValue", "W{0}SIZEB"),
        new("AdditionalSizeAbove", "Double", "0.0", "value == int.MinValue", "W{0}ADDSA"),
        new("AdditionalSizeBelow", "Double", "0.0", "value == int.MinValue", "W{0}ADDSB"),
        new("AngleAbove", "Double", "0.0", "value == int.MinValue", "W{0}ANGA"),
        new("AngleBelow", "Double", "0.0", "value == int.MinValue", "W{0}ANGB"),
        new("ContourAbove", "Integer", "0", "value < 0 || value > 3", "W{0}CTRA"),
        new("ContourBelow", "Integer", "0", "value < 0 || value > 3", "W{0}CTRB"),
        new("FinishAbove", "Integer", "0", "value < 0 || value > 5", "W{0}FNSHA"),
        new("FinishBelow", "Integer", "0", "value < 0 || value > 5", "W{0}FNSHB"),
        new("RootFaceAbove", "Double", "0", "value == int.MinValue", "W{0}FACEA"),
        new("RootFaceBelow", "Double", "0", "value == int.MinValue", "W{0}FACEB"),
        new("EffectiveThroatAbove", "Double", "0.0", "value == int.MinValue", "W{0}THROA"),
        new("EffectiveThroatBelow", "Double", "0.0", "value == int.MinValue", "W{0}THROB"),
        new("RootOpeningAbove", "Double", "0.0", "value == int.MinValue", "W{0}OPNGA"),
        new("RootOpeningBelow", "Double", "0.0", "value == int.MinValue", "W{0}OPNGB"),
        new("Around", "Integer", "0", "value < 0 || value > 1", "W{0}ARND"),
        new("Shop", "Integer", "0", "value < 0 || value > 1", "W{0}SHOP"),
        new("Placement", "Integer", "0", "value < 0 || value > 2", "W{0}PLACE"),
        new("Preparation", "Integer", "0", "value < 0 || value > 3", "W{0}PREP"),
        new("Intermittent", "Integer", "0", "value < 0 || value > 2", "W{0}INTMI"),
        new("IncrementAmountAbove", "Integer", "0", "value == int.MinValue", "W{0}INCRA"),
        new("IncrementAmountBelow", "Integer", "0", "value == int.MinValue", "W{0}INCRB"),
        new("LengthAbove", "Double", "0.0", "value == int.MinValue", "W{0}LENA"),
        new("LengthBelow", "Double", "0.0", "value == int.MinValue", "W{0}LENB"),
        new("PitchAbove", "Double", "0.0", "value == int.MinValue", "W{0}PITA"),
        new("PitchBelow", "Double", "0.0", "value == int.MinValue", "W{0}PITB"),
        new("ReferenceText", "String", "", "string.IsNullOrEmpty(value)", "W{0}TEXT"),
    ];

    private static IReadOnlyList<PropertyInfo> BoltPropertyInfos(Version tsmVersion) {
        var result = new List<PropertyInfo> {
            new("Shape", "Integer", "0", "value == int.MinValue", "B{0}SHAPE"),
            new("Standard", "String", "A", "string.IsNullOrEmpty(value)", "B{0}STD"),
            new("Size", "Distance", "8.0", "value.Value == int.MinValue || value.Value == 0.0", "B{0}SIZE"),
            new("DistX", "DistanceList", "0.0", "value.Count == 0", "B{0}DISTX"),
            new("DistY", "DistanceList", "0.0", "value.Count == 0", "B{0}DISTY"),
            new("NumberOfBolts", "Integer", "6", "value == int.MinValue", "B{0}NUM"),
            new("Diameter", "Double", "100.0", "value == int.MinValue", "B{0}DIAM"),
            new("Type", "Integer", "0", "value < 0 || value > 1", "B{0}TYPE"),
            new("ThreadInMaterial", "Integer", "1", "value < 0 || value > 1", "B{0}THRD"),
            new("Length", "Double", "40.0", "value == int.MinValue", "B{0}LEN"),
            new("CutLength", "Double", "100.0", "value == int.MinValue", "B{0}CLEN"),
            new("ExtraLength", "Double", "0.0", "value == int.MinValue", "B{0}XLEN"),
            new("Tolerance", "Double", "2.0", "value == int.MinValue", "B{0}TOL"),
            new("PlainType", "Integer", "0", "value < 0 || value > 1", "B{0}PLAIN"),
            new("BlindHoleDepth", "Double", "0.0", "value == int.MinValue", "B{0}DEPTH"),
        };

        if (tsmVersion is not null && tsmVersion.Major >= 2023) {
            result.Add(new("HoleType", "Integer", "0", "value < 0 || value > 3", "B{0}HOLTY"));
        } else {
            result.Add(new("HoleType", "Integer", "0", "value < 0 || value > 2", "B{0}HOLTY"));
        }

        result.AddRange([
            new("SlottedHoleX", "Double", "0.0", "value == int.MinValue", "B{0}SLOTX"),
            new("SlottedHoleY", "Double", "0.0", "value == int.MinValue", "B{0}SLOTY"),
        ]);

        if (tsmVersion is not null && tsmVersion.Major >= 2023) {
            result.AddRange([
                new("SlotOffsetX", "Double", "0.0", "value == int.MinValue", "B{0}SOFFX"),
                new("SlotOffsetY", "Double", "0.0", "value == int.MinValue", "B{0}SOFFY"),
            ]);
        }

        result.AddRange([
            new("RotateSlots", "Integer", "2", "value < 0 || value > 2", "B{0}RSLOT"),
            new("Hole1", "Integer", "1", "value < 0 || value > 1", "B{0}HOLE1"),
            new("Hole2", "Integer", "1", "value < 0 || value > 1", "B{0}HOLE2"),
            new("Hole3", "Integer", "0", "value < 0 || value > 1", "B{0}HOLE3"),
            new("Hole4", "Integer", "0", "value < 0 || value > 1", "B{0}HOLE4"),
            new("Hole5", "Integer", "0", "value < 0 || value > 1", "B{0}HOLE5"),
            new("IsBolt", "Integer", "1", "value < 0 || value > 1", "B{0}ISBOT"),
            new("UseNut1", "Integer", "1", "value < 0 || value > 1", "B{0}NUT1"),
            new("UseNut2", "Integer", "0", "value < 0 || value > 1", "B{0}NUT2"),
            new("UseWasher1", "Integer", "0", "value < 0 || value > 1", "B{0}WSHR1"),
            new("UseWasher2", "Integer", "0", "value < 0 || value > 1", "B{0}WSHR2"),
            new("UseWasher3", "Integer", "1", "value < 0 || value > 1", "B{0}WSHR3"),
        ]);

        return result;
    }

    private static IReadOnlyList<PropertyInfo> StudPropertyInfos(Version tsmVersion) => [
        new("Shape", "Integer", "0", "value == int.MinValue", "S{0}SHAPE"),
        new("Standard", "String", "STUD", "string.IsNullOrEmpty(value)", "S{0}STD"),
        new("Size", "Distance", "10.0", "value.Value == int.MinValue || value.Value == 0.0", "S{0}SIZE"),
        new("DistX", "DistanceList", "0.0", "value.Count == 0", "S{0}DISTX"),
        new("DistY", "DistanceList", "0.0", "value.Count == 0", "S{0}DISTY"),
        new("NumberOfBolts", "Integer", "6", "value == int.MinValue", "S{0}NUM"),
        new("Diameter", "Double", "100.0", "value == int.MinValue", "S{0}DIAM"),
        new("Type", "Integer", "0", "value < 0 || value > 1", "S{0}TYPE"),
        new("Length", "Double", "40.0", "value == int.MinValue", "S{0}LEN"),
        new("CutLength", "Double", "100.0", "value == int.MinValue", "S{0}CLEN"),
    ];

    private static IReadOnlyList<PropertyInfo> ChamferPropertyInfos(Version tsmVersion) => [
        new("Type", "Integer", "0", "value < 0 || value > 7", "CF{0}TYPE"),
        new("X", "Double", "0.0", "value == int.MinValue", "CF{0}X"),
        new("Y", "Double", "0.0", "value == int.MinValue", "CF{0}Y"),
        new("Dz1", "Double", "0.0", "value == int.MinValue", "CF{0}DZ1"),
        new("Dz2", "Double", "0.0", "value == int.MinValue", "CF{0}DZ2"),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => {
            ctx.AddSource("NotificationObject.g.cs",
                SourceText.From(GetResourceAsString("NotificationObject.cs"), Encoding.UTF8));
            ctx.AddSource("ConnectionViewModel.g.cs",
                SourceText.From(GetResourceAsString("ConnectionViewModel.cs"), Encoding.UTF8));
            ctx.AddSource("DetailViewModel.g.cs",
                SourceText.From(GetResourceAsString("DetailViewModel.cs"), Encoding.UTF8));
            ctx.AddSource("CustomPartViewModel.g.cs",
                SourceText.From(GetResourceAsString("CustomPartViewModel.cs"), Encoding.UTF8));

            // no need to register attribute source, already registered in PluginDataFieldsGenerator
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

        var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Where(x => x != default);

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var propertiesDefaultValues = provider
            .Select(static (x, _) => x.Value)
            .Combine(versionProvider)
            .Where(x => x.Left != default && x.Right != null);
        context.RegisterSourceOutput(propertiesDefaultValues, Generate);
    }

    private static bool Predicate(SyntaxNode syntaxNode, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static GatheredInfo<PluginDataFieldsInfo> Transform(GeneratorSyntaxContext context,
        CancellationToken token) {
        var semanticModel = context.SemanticModel;
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null ||
            !classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged"))
            return default; // it is not a view model

        var gatheredInfo = PluginDataFieldsGenerator.Transform(classDeclarationSyntax, semanticModel, token, 1);

        return gatheredInfo == default
            ? default
            : new GatheredInfo<PluginDataFieldsInfo>(gatheredInfo.Item1, gatheredInfo.Item2.ToImmutableArray());
    }

    private static void Generate(SourceProductionContext context, (PluginDataFieldsInfo, Version) pair) {
        var info = pair.Item1;
        var tsmVersion = pair.Item2;

        var fieldsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();

        foreach (var attDict in info.Arguments) {
            var attName = attDict.Key;
            if (attName == "GeneralAttribute") {
                GenerateGeneralSource(attDict.Value, ref fieldsBuilder, ref propertiesBuilder);
            } else {
                GenerateSeriesSource(attName, attDict.Value, tsmVersion, ref fieldsBuilder, ref propertiesBuilder);
            }
        }

        var generatedSourceText =
#if DEBUG
            $"//  <auto-generated/> at {DateTime.Now}\r\n" +
#else
            $"//  <auto-generated/>\r\n" +
#endif
            $"\r\n" +
            $"namespace {info.ClassInfo.NameSpace} {{\r\n" +
            $"    {info.ClassInfo.Accessibility.ToString().ToLower()} partial {(info.ClassInfo.IsRecord ? "record " : "")}class {info.ClassInfo.Name} {{\r\n" +
            $"{fieldsBuilder}\r\n" +
            $"{propertiesBuilder}\r\n" +
            $"    }}\r\n" +
            $"}}";

        context.AddSource($"{info.ClassInfo.Name}.g.cs", generatedSourceText);
    }

    private static void GenerateGeneralSource(DefaultValueDictionary defaultValueDict, ref StringBuilder fieldsBuilder,
        ref StringBuilder propertiesBuilder) {
        foreach (var kvp in defaultValueDict) {
            var dataType = kvp.Key;

            var validation = dataType switch {
                "Integer" or "Double" => "value == int.MinValue",
                "Distance" => "value.Value == int.MinValue",
                "DistanceList" => "value.Count == 0 || value.Count == 1 && value[0].Value == 0.0",
                "String" => "string.IsNullOrEmpty(value)",
                _ => string.Empty
            };

            foreach (var kvp2 in kvp.Value) {
                var attributeName = kvp2.Key;
                var fieldName = ToPrivateFieldNameStyle(attributeName);
                var propertyName = ToPropertyNameStyle(attributeName);

                fieldsBuilder.AppendLine(
                    $"        private {TsDatatype}.{dataType} {fieldName};");

                var defaultValue = kvp2.Value;
                var valueIfTrue = dataType switch {
                    "Integer" or "Double" => $"{defaultValue}",
                    "Distance" => $"new {TsDatatype}.{dataType}({defaultValue})",
                    "DistanceList" =>
                        $"{TsDatatype}.{dataType}.Parse(\"{defaultValue}\", {CurrentCulture}, {TsDatatype}.Distance.CurrentUnitType)",
                    "String" => $"\"{defaultValue}\"",
                    _ => string.Empty
                };
                var equalsValueClause = $"{validation} ? {valueIfTrue} : value";

                propertiesBuilder.AppendLine(
                    $"        \r\n" +
                    $"        [{TsDialog}.StructuresDialog(\"{attributeName}\", typeof({TsDatatype}.{dataType}))]\r\n" +
                    $"        public {TsDatatype}.{dataType} {propertyName} {{\r\n" +
                    $"            get {{\r\n" +
                    $"                return {fieldName};\r\n" +
                    $"            }}\r\n" +
                    $"            set {{\r\n" +
                    $"                {fieldName} = {equalsValueClause};\r\n" +
                    $"                OnPropertyChanged(\"{propertyName}\");\r\n" +
                    $"            }}\r\n" +
                    $"        }}");
            }
        }
    }

    private static void GenerateSeriesSource(string attributeName, DefaultValueDictionary defaultValueDict,
        Version tsmVersion, ref StringBuilder fieldsBuilder, ref StringBuilder propertiesBuilder) {
        var propertyInfos = attributeName switch {
            "PartAttribute" => PartPropertyInfos(tsmVersion),
            "PlateAttribute" => PlatePropertyInfos(tsmVersion),
            "BoltAttribute" => BoltPropertyInfos(tsmVersion),
            "StudAttribute" => StudPropertyInfos(tsmVersion),
            "WeldAttribute" => WeldPropertyInfos(tsmVersion),
            "ChamferAttribute" => ChamferPropertyInfos(tsmVersion),
            _ => []
        };
        var prefix = attributeName.Substring(0, attributeName.Length - 9);

        foreach (var idDict in defaultValueDict) {
            var id = idDict.Key;
            var defaultValues = idDict.Value;

            foreach (var (property, dataType, presetValue, validation, attributeNamePattern) in propertyInfos) {
                var fieldName = $"{ToPrivateFieldNameStyle(prefix)}{ToPropertyNameStyle(id)}{property}";
                var propertyName = $"{prefix}{ToPropertyNameStyle(id)}{property}";

                fieldsBuilder.AppendLine($"        private {TsDatatype}.{dataType} {fieldName};");

                if (!defaultValues.TryGetValue(property, out var defaultValue)) defaultValue = presetValue;
                var valueIfTrue = dataType switch {
                    "Integer" or "Double" => $"{defaultValue}",
                    "Distance" => $"new {TsDatatype}.{dataType}({defaultValue})",
                    "DistanceList" =>
                        $"{TsDatatype}.{dataType}.Parse(\"{defaultValue}\", {CurrentCulture}, {TsDatatype}.Distance.CurrentUnitType)",
                    "String" => $"\"{defaultValue}\"",
                    _ => string.Empty
                };
                propertiesBuilder.AppendLine(
                    $"        \r\n" +
                    $"        [{TsDialog}.StructuresDialog(\"{string.Format(attributeNamePattern, ToPropertyNameStyle(id))}\", typeof({TsDatatype}.{dataType}))]\r\n" +
                    $"        public {TsDatatype}.{dataType} {propertyName} {{\r\n" +
                    $"            get {{\r\n" +
                    $"                return {fieldName};\r\n" +
                    $"            }}\r\n" +
                    $"            set {{\r\n" +
                    $"                {fieldName} = {validation} ? {valueIfTrue} : value;\r\n" +
                    $"                OnPropertyChanged(\"{propertyName}\");\r\n" +
                    $"            }}\r\n" +
                    $"        }}");
            }
        }
    }
}