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
 *  PluginFieldDefaultValuesGenerator.cs: help to generate set data to default method for plugin.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Muggle.TsExtensions.CodingHelper.Generators.Information;
using static Muggle.TsExtensions.CodingHelper.Generators.GeneratorHelper;
using static Muggle.TsExtensions.CodingHelper.Diagnosers.InternalAttributesDiagnoser;

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
internal class PluginFieldDefaultValuesGenerator : IIncrementalGenerator {
    internal static readonly string[] ConcernedAttributes = [
        "Muggle.TsExtensions.CodingHelper.Generators.PartFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.ChamferFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.GeneralFieldDefaultValuesAttribute"
    ];

    /// <summary>
    /// Dictionary of preset values.
    /// <list type="bullet">
    ///     <item>Key - attribute short name, such as "PartFieldDefaultValuesAttribute".</item>
    ///     <item>Value - dictionary of field name and preset value.
    ///         <list type="bullet">
    ///             <item>Key - field name, such as "profile".</item>
    ///             <item>Value - preset value.</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </summary>
    private ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> PresetValues { get; }

    public PluginFieldDefaultValuesGenerator() {
        var dict = new Dictionary<string, ReadOnlyDictionary<string, string>>();

        foreach (var text in GetAttributeSourceTexts(ConcernedAttributes.Take(ConcernedAttributes.Length - 1))) {
            var defaultValues =
                GetDefaultValuesFromSyntaxTree(CSharpSyntaxTree.ParseText(text), out var attributeName);
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

        var diagnostics = provider.SelectMany(static (x, _) => x.DiagnosticInfos);
        context.RegisterSourceOutput(diagnostics, static (spc, info) => {
            spc.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location, info.Arguments));
        });

        var pluginFieldDefaultValues = provider.Select(static (x, _) => x.Value).Where(x => x != default);
        context.RegisterSourceOutput(pluginFieldDefaultValues, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        return node is ClassDeclarationSyntax classDeclarationSyntax &&
               (classDeclarationSyntax.AttributeLists.Count > 0 || classDeclarationSyntax.Members.Any(member =>
                   member switch {
                       FieldDeclarationSyntax { AttributeLists.Count: > 0 } or
                           PropertyDeclarationSyntax { AttributeLists.Count: > 0 } => true,
                       _ => false
                   })
               );
    }

    private GatheredInfo<PluginFieldDefaultValuesInfo> Transform(GeneratorSyntaxContext context,
        CancellationToken token) {

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null) return default;

        var result = new GatheredInfo<PluginFieldDefaultValuesInfo>(default, []);
        var diagnosticInfos = result.DiagnosticInfos;

        AttributeSyntax[] attSyntaxes = null;

        var placeCnt = AnalyzeAppliedPlaces(classDeclarationSyntax, semanticModel, ref attSyntaxes, ref diagnosticInfos,
            out var appliedTarget, out var dataMemberType, out var targetMemberName);

        switch (placeCnt) {
        // no attribute
        case 0:
            return result;
        case > 0 when !classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)):
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(NotPartial, classDeclarationSyntax.Identifier.GetLocation(),
                    [classDeclarationSyntax.Identifier.ValueText])
            );
            result.DiagnosticInfos = diagnosticInfos;
            return result;
        // applied on over one place
        case > 1:
            result.DiagnosticInfos = diagnosticInfos;
            return result;
        }

        var dataClassAttArgs = dataMemberType.GetAttributes()
            .Where(a => PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass?.ToDisplayString()))
            .Select(a => {
                var attName = a.AttributeClass!.ToDisplayString();
                attName = attName.Substring(attName.LastIndexOf('.') + 1);

                return attName switch {
                    "GeneralFieldsAttribute" =>
                        // (type, ids) => (int, [param1, param2, ...])
                        (category: a.ConstructorArguments
                                .Single(typedConstant => typedConstant.Kind == TypedConstantKind.Type).Value!
                                .ToString(),
                            ids: a.ConstructorArguments
                                .Single(typedConstant => typedConstant.Kind == TypedConstantKind.Array).Values
                                .Select(arg => arg.Value!.ToString())
                                .ToArray()
                        ),
                    _ =>
                        // (attribute, ids) => (PartFieldsAttribute, [Part1, Part2, ...])
                        (category: attName,
                            ids: a.ConstructorArguments
                                .Where(typedConstant => typedConstant.Kind == TypedConstantKind.Array)
                                .SelectMany(typedConstant => typedConstant.Values.Select(arg => arg.Value!.ToString()))
                                .OrderBy(s => s)
                                .ToArray()
                        )
                };
            }).OrderBy(t => t.category).ToArray();

        //  Key - attribute name
        //  Value - 
        //      Key - for 'GeneralFieldDefaultValuesAttribute' is data type, like 'int' or 'double' or 'string',
        //            for other attributes is id
        //      Value - 
        //          Key - for 'GeneralFieldDefaultValuesAttribute' is id,
        //                for other attributes is property name of model object
        //          Value - default value
        var attDict = new ArgumentsDictionary<DefaultValueDictionary>();
        foreach (var attSyntax in attSyntaxes) {
            var attName = GetAttributeName(attSyntax, semanticModel);

            if (!attDict.TryGetValue(attName, out var elementDict)) {
                elementDict = new DefaultValueDictionary();
                attDict.Add(attName, elementDict);
            }

            if (attName == "GeneralFieldDefaultValuesAttribute") {
                AnalyzeGeneralFieldDefaultValues(attSyntax, dataClassAttArgs,
                    ref elementDict, ref diagnosticInfos);
            } else {
                AnalyzeSeriesFieldDefaultValues(attSyntax, attName, dataClassAttArgs, PresetValues,
                    ref elementDict, ref diagnosticInfos);
            }
        }

        if (attDict.Count == 0) {
            result.DiagnosticInfos = diagnosticInfos;
            return default;
        }

        // use preset default values for missing ids
        foreach (var (category, ids) in dataClassAttArgs) {
            DefaultValueDictionary elementDict;

            if (category.EndsWith("Attribute")) {
                var att = category.Insert(category.Length - 10, "DefaultValue");
                if (!attDict.TryGetValue(att, out elementDict)) {
                    elementDict = new DefaultValueDictionary();
                    attDict.Add(att, elementDict);
                }

                foreach (var id in ids) {
                    if (elementDict.TryGetValue(id, out var defaultValueDict)) continue;

                    defaultValueDict = new Dictionary<string, string>();
                    elementDict.Add(id, defaultValueDict);
                }
            } else {
                if (!attDict.TryGetValue("GeneralFieldDefaultValuesAttribute", out elementDict)) {
                    elementDict = new DefaultValueDictionary();
                    attDict.Add("GeneralFieldDefaultValuesAttribute", elementDict);
                }

                if (!elementDict.TryGetValue(category, out var defaultValueDict)) {
                    defaultValueDict = new Dictionary<string, string>();
                    elementDict.Add(category, defaultValueDict);
                }

                foreach (var id in ids) {
                    if (defaultValueDict.TryGetValue(id, out var defaultValue)) continue;

                    defaultValue = category switch {
                        "int" => 0.ToString(),
                        "double" => 0.0.ToString(CultureInfo.InvariantCulture),
                        _ => string.Empty
                    };
                    defaultValueDict.Add(id, defaultValue);
                }
            }
        }

        token.ThrowIfCancellationRequested();

        result.Value = new PluginFieldDefaultValuesInfo {
            ClassInfo = new ClassInfo {
                Name = classDeclarationSyntax.Identifier.Text,
                NameSpace = classSymbol.ContainingNamespace.ToDisplayString(),
                Accessibility = classSymbol.DeclaredAccessibility,
                IsRecord = classSymbol.IsRecord
            },
            TargetType = appliedTarget,
            TargetMemberName = targetMemberName,
            Arguments = attDict
        };
        result.DiagnosticInfos = diagnosticInfos;

        return result;

    }

    private static int AnalyzeAppliedPlaces(ClassDeclarationSyntax classDclrSyntax, SemanticModel semanticModel,
        ref AttributeSyntax[] attSyntaxes, ref ImmutableArray<DiagnosticInfo> diagnosticInfos,
        out AttributeTargets appliedTarget, out ITypeSymbol dataType, out string targetMemberName) {

        attSyntaxes ??= [];
        appliedTarget = 0;
        dataType = null;
        targetMemberName = null;

        var placeCnt = 0;
        if (TryGetMatchedAttributes(classDclrSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                ref attSyntaxes)) {
            placeCnt++;
            appliedTarget = AttributeTargets.Class;

            //  When applied on class, the FieldsFromAttribute must also be applied
            AttributeSyntax[] fieldsFromAttSyntax = null;
            if (!TryGetMatchedAttributes(classDclrSyntax.AttributeLists, semanticModel,
                    [PluginFieldsGenerator.ConcernedAttribute], ref fieldsFromAttSyntax)) {
                foreach (var attSyntax in attSyntaxes) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(FieldsFromAttributeNotApplied, attSyntax.GetLocation(),
                            [attSyntax.Name.ToString()])
                    );
                }
            }

            var expression = fieldsFromAttSyntax.Single().DescendantNodes().OfType<TypeOfExpressionSyntax>().Single();
            dataType = semanticModel.GetTypeInfo(expression.Type).Type;
        }

        foreach (var memberDclrSyntax in classDclrSyntax.Members) {
            switch (memberDclrSyntax) {
            case FieldDeclarationSyntax fieldSyntax when
                TryGetMatchedAttributes(fieldSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attSyntaxes):
                placeCnt++;
                appliedTarget = AttributeTargets.Field;

                if (placeCnt > 1) {
                    foreach (var attSyntax in attSyntaxes) {
                        diagnosticInfos = diagnosticInfos.Add(
                            new DiagnosticInfo(AppliedOnOverOnePlace, attSyntax.GetLocation(), [attSyntax.Name])
                        );
                    }
                }

                targetMemberName = fieldSyntax.Declaration.Variables[0].Identifier.ValueText;
                dataType = semanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type;

                break;
            case PropertyDeclarationSyntax propertySyntax when
                TryGetMatchedAttributes(propertySyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attSyntaxes):
                placeCnt++;
                appliedTarget = AttributeTargets.Property;

                if (placeCnt > 1) {
                    foreach (var attSyntax in attSyntaxes) {
                        diagnosticInfos = diagnosticInfos.Add(
                            new DiagnosticInfo(AppliedOnOverOnePlace, attSyntax.GetLocation(), [attSyntax.Name])
                        );
                    }
                }

                targetMemberName = propertySyntax.Identifier.ValueText;
                dataType = semanticModel.GetTypeInfo(propertySyntax.Type).Type;

                break;
            }
        }

        return placeCnt;
    }

    private static void AnalyzeGeneralFieldDefaultValues(AttributeSyntax attSyntax,
        (string category, string[] attArgs)[] dataClassAttributeArguments,
        ref DefaultValueDictionary elementDict,
        ref ImmutableArray<DiagnosticInfo> diagnosticInfos) {

        if (attSyntax.ArgumentList is null) return;

        var exprSyntaxes = attSyntax.ArgumentList.DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();
        if (exprSyntaxes.Length == 0) return;

        if (exprSyntaxes.Length % 2 != 0) {
            diagnosticInfos = diagnosticInfos.Add(
                new DiagnosticInfo(ArgumentsMustBePassedInPairs, exprSyntaxes.Last().GetLocation(), [])
            );

            if (exprSyntaxes.Length < 2) return;
        }

        //  take two at once
        for (int i = 0; i < exprSyntaxes.Length / 2 * 2; i += 2) {
            var nameSyntax = exprSyntaxes[i];
            var valueSyntax = exprSyntaxes[i + 1];

            if (!nameSyntax.IsKind(SyntaxKind.StringLiteralExpression)) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(NotExpectedDataType, nameSyntax!.GetLocation(), ["string"])
                );
                continue;
            }

            var name = nameSyntax.Token.ValueText;
            var value = valueSyntax.Token.ValueText;

            var type = dataClassAttributeArguments
                .SingleOrDefault(t => !t.category.EndsWith("Attribute") && t.attArgs.Contains(name)).category;

            //  data class doesnt have this field
            if (string.IsNullOrEmpty(type)) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(DataTypeDoesNotContainTheseFields, nameSyntax.GetLocation(), [])
                );
                continue;
            }

            var isRightType = type switch {
                "int" => valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) && int.TryParse(value, out _),
                "double" => valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) && double.TryParse(value, out _),
                "string" => valueSyntax.IsKind(SyntaxKind.StringLiteralExpression),
                _ => false
            };

            if (!isRightType) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(NotExpectedDataType, valueSyntax.GetLocation(), [type])
                );
                continue;
            }

            if (!elementDict.TryGetValue(type, out var valueDict)) {
                valueDict = new Dictionary<string, string>();
                elementDict.Add(type, valueDict);
            }

            try {
                valueDict.Add(name, value);
            } catch (ArgumentException) {
                diagnosticInfos = diagnosticInfos.Add(
                    new DiagnosticInfo(SetDefaultValueMultiTimes, nameSyntax.GetLocation(), [])
                );
            }
        }
    }

    private static void AnalyzeSeriesFieldDefaultValues(AttributeSyntax attSyntax, string attName,
        (string category, string[] attArgs)[] dataClassAttributeArguments,
        ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> presetValues,
        ref DefaultValueDictionary elementDict,
        ref ImmutableArray<DiagnosticInfo> diagnosticInfos) {

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

                if (elementDict.ContainsKey(id)) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(SetDefaultValueMultiTimes, argSyntax.GetLocation(), []));
                    return;
                }

                if (!dataClassAttributeArguments.Any(t =>
                        t.category.EndsWith("Attribute") &&
                        //  PartFieldsAttribute => PartField
                        //  PartFieldDefaultValuesAttribute => PartField
                        t.category.Substring(0, t.category.Length - 10) == attName.Substring(0, attName.Length - 22) &&
                        t.attArgs.Contains(id))) {
                    diagnosticInfos = diagnosticInfos.Add(
                        new DiagnosticInfo(DataTypeDoesNotContainTheseFields, argSyntax.GetLocation(), []));
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

    private void Generate(SourceProductionContext context, PluginFieldDefaultValuesInfo info) {
        var generalFieldStatementsBuilder = new StringBuilder();
        var seriesFieldStatementsBuilder = new StringBuilder();
        var creatorsBuilder = new StringBuilder();

        foreach (var kvp in info.Arguments) {
            var attName = kvp.Key;

            switch (attName) {
            case "PartFieldDefaultValuesAttribute":
            case "PlateFieldDefaultValuesAttribute":
            case "WeldFieldDefaultValuesAttribute":
            case "BoltFieldDefaultValuesAttribute":
            case "BoltCircleFieldDefaultValuesAttribute":
            case "ChamferFieldDefaultValuesAttribute":
                seriesFieldStatementsBuilder.Append(
                    GenerateSeriesFields(attName, kvp.Value, info.TargetType, info.TargetMemberName, PresetValues));
                creatorsBuilder.Append(
                    GenerateCreatorsAndModifiers(attName, kvp.Value.Keys, info.TargetType, info.TargetMemberName));
                break;
            case "GeneralFieldDefaultValuesAttribute":
                generalFieldStatementsBuilder.Append(
                    GenerateGeneralFields(kvp.Value, info.TargetType, info.TargetMemberName));
                break;
            }
        }

        var classInfo = info.ClassInfo;
        var result =
            $"//  <auto-generated/>" +
#if DEBUG
            $" at {DateTime.Now}" +
#endif
            $"\n\n" +
            $"namespace {classInfo.NameSpace} {{\n" +
            $"    {classInfo.Accessibility.ToString().ToLower()} partial {(classInfo.IsRecord ? "record " : "")}class {classInfo.Name} {{\n" +
            $"        /// <summary>\n" +
            $"        /// Set data (fields) to default value if they are not set from user interface.\n" +
            $"        /// </summary>\n" +
            $"        private void SetDataToDefaultIfUnset() {{\n" +
            $"{generalFieldStatementsBuilder}" +
            $"{seriesFieldStatementsBuilder}" +
            $"        }}\n" +
            $"{creatorsBuilder}" +
            $"    }}\n" +
            $"}}";

        context.AddSource($"{info.ClassInfo.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static string GenerateGeneralFields(DefaultValueDictionary defaultValueDictionary,
        AttributeTargets dataMemberType, string dataMemberName) {

        var builder = new StringBuilder();

        var memberAccess = dataMemberType is AttributeTargets.Class ? string.Empty : dataMemberName + ".";

        foreach (var kvp in defaultValueDictionary) {
            var type = kvp.Key;

            foreach (var kvp2 in kvp.Value) {
                var fieldName = dataMemberType is AttributeTargets.Class
                    ? ToPrivateFieldNameStyle(kvp2.Key)
                    : ToPropertyNameStyle(kvp2.Key);

                var fieldAccess = $"{memberAccess}{fieldName}";

                builder.AppendLine($"            if (IsDefaultValue({fieldAccess}))");
                builder.AppendLine(type is "string"
                    ? $"                {fieldAccess} = \"{kvp2.Value}\";"
                    : $"                {fieldAccess} = {kvp2.Value};");
            }
        }

        return builder.ToString();
    }

    private static string GenerateSeriesFields(string attName, DefaultValueDictionary defaultValueDict,
        AttributeTargets dataMemberType, string dataMemberName,
        ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> presetValues) {

        var fieldInfos = attName switch {
            "PartFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.PartFieldInfos,
            "PlateFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.PlateFieldInfos,
            "WeldFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.WeldFieldInfos,
            "BoltFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.BoltFieldInfos,
            "BoltCircleFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.BoltCircleFieldInfos,
            "ChamferFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.ChamferFieldInfos,
            _ => []
        };

        var fieldPrefix = attName.Substring(0, attName.Length - 27);
        var memberAccess = string.Empty;
        switch (dataMemberType) {
        case AttributeTargets.Class:
            fieldPrefix = ToPrivateFieldNameStyle(fieldPrefix);
            break;
        case AttributeTargets.Field:
        case AttributeTargets.Property:
            memberAccess = dataMemberName + ".";
            break;
        }

        var builder = new StringBuilder();

        foreach (var kvp in defaultValueDict) {
            var id = kvp.Key;
            var paramArgPair = kvp.Value;

            foreach (var (propertyName, _, propertyDataType) in fieldInfos) {
                var paramName = ToLocalVariableNameStyle(propertyName);
                if (!paramArgPair.TryGetValue(paramName, out var defaultValue)) {
                    if (!presetValues[attName].TryGetValue(paramName, out defaultValue))
                        continue;
                }

                var fieldAccess = $"{memberAccess}{fieldPrefix}{ToPropertyNameStyle(id)}{propertyName}";

                if (attName == "PlateFieldDefaultValuesAttribute" &&
                    propertyName is "Thickness" or "Breadth" or "Height") {
                    builder.AppendLine($"            if ({fieldAccess} <= 0)");
                } else {
                    builder.AppendLine($"            if (IsDefaultValue({fieldAccess}))");
                }

                builder.AppendLine(propertyDataType is "string"
                    ? $"                {fieldAccess} = \"{defaultValue}\";"
                    : $"                {fieldAccess} = {defaultValue};");
            }
        }

        return builder.ToString();
    }

    private static string GenerateCreatorsAndModifiers(string attName, ICollection<string> ids, AttributeTargets targetType,
        string targetMemberName) {

        const string TsmNameSpace = "global::Tekla.Structures.Model";
        const string TsdNameSpace = "global::Tekla.Structures.Datatype";
        var memberAccess = targetType is AttributeTargets.Class ? string.Empty : targetMemberName + ".";

        var builder = new StringBuilder();

        foreach (var id in ids) {
            string propertyAccess;
            switch (attName) {
            case "PartFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_part{ToPropertyNameStyle(id)}"
                    : $"Part{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <typeparamref name=\"T\"/> type model object for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\n" +
                    $"        private T CreatPart{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.Part, new() {{\n" +
                    $"            T part = new T();\n" +
                    $"            part.Name = {propertyAccess}Name;\n" +
                    $"            part.Profile.ProfileString = {propertyAccess}Profile;\n" +
                    $"            part.Material.MaterialString = {propertyAccess}Material;\n" +
                    $"            part.Finish = {propertyAccess}Finish;\n" +
                    $"            part.Class = {propertyAccess}Class.ToString();\n" +
                    $"            part.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\n" +
                    $"            part.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\n" +
                    $"            part.PartNumber.Prefix = {propertyAccess}PartPrefix;\n" +
                    $"            part.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\n" +
                    $"            return part;\n" +
                    $"        }}");
                
                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given model object to make its properties consistent with part '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"part\">The part to be modified.</param>\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"part\"/>.</typeparam>\n" +
                    $"        private void ModifyToPart{ToPropertyNameStyle(id)}<T>(ref T part) where T : {TsmNameSpace}.Part {{\n" +
                    $"            part.Name = {propertyAccess}Name;\n" +
                    $"            part.Profile.ProfileString = {propertyAccess}Profile;\n" +
                    $"            part.Material.MaterialString = {propertyAccess}Material;\n" +
                    $"            part.Finish = {propertyAccess}Finish;\n" +
                    $"            part.Class = {propertyAccess}Class.ToString();\n" +
                    $"            part.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\n" +
                    $"            part.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\n" +
                    $"            part.PartNumber.Prefix = {propertyAccess}PartPrefix;\n" +
                    $"            part.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\n" +
                    $"        }}");
                break;
            case "PlateFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_plate{ToPropertyNameStyle(id)}"
                    : $"Plate{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <typeparamref name=\"T\"/> type model object for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.Brep\"/>.\n" +
                    $"        /// </exception>\n" +
                    $"        private T CreatPlate{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.Part, new() {{\n" +
                    $"            if (typeof(T) == typeof({TsmNameSpace}.Brep))\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.Brep)}}\\\" not supported.\");\n" +
                    $"            \n" +
                    $"            T plate = new T();\n" +
                    $"            plate.Name = {propertyAccess}Name;\n" +
                    $"            switch (typeof(T).ToString()) {{\n" +
                    $"            case \"Tekla.Structures.Model.Beam\":\n" +
                    $"            case \"Tekla.Structures.Model.PolyBeam\":\n" +
                    $"            case \"Tekla.Structures.Model.SpiralBeam\":\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}*{{{propertyAccess}Breadth}}\";\n" +
                    $"                break;\n" +
                    $"            case \"Tekla.Structures.Model.BentPlate\":\n" +
                    $"            case \"Tekla.Structures.Model.ContourPlate\":\n" +
                    $"            case \"Tekla.Structures.Model.LoftedPlate\":\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}\";\n" +
                    $"                break;\n" +
                    $"            }}\n" +
                    $"            plate.Material.MaterialString = {propertyAccess}Material;\n" +
                    $"            plate.Finish = {propertyAccess}Finish;\n" +
                    $"            plate.Class = {propertyAccess}Class.ToString();\n" +
                    $"            plate.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\n" +
                    $"            plate.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\n" +
                    $"            plate.PartNumber.Prefix = {propertyAccess}PartPrefix;\n" +
                    $"            plate.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\n" +
                    $"            return plate;\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given model object to make its properties consistent with plate '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"plate\">The plate to be modified.</param>\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"plate\"/>.</typeparam>\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.Brep\"/>.\n" +
                    $"        /// </exception>\n" +
                    $"        private void ModifyToPlate{ToPropertyNameStyle(id)}<T>(ref T plate) where T : {TsmNameSpace}.Part {{\n" +
                    $"            if (typeof(T) == typeof({TsmNameSpace}.Brep))\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.Brep)}}\\\" not supported.\");\n" +
                    $"            \n" +
                    $"            plate.Name = {propertyAccess}Name;\n" +
                    $"            switch (typeof(T).ToString()) {{\n" +
                    $"            case \"Tekla.Structures.Model.Beam\":\n" +
                    $"            case \"Tekla.Structures.Model.PolyBeam\":\n" +
                    $"            case \"Tekla.Structures.Model.SpiralBeam\":\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}*{{{propertyAccess}Breadth}}\";\n" +
                    $"                break;\n" +
                    $"            case \"Tekla.Structures.Model.BentPlate\":\n" +
                    $"            case \"Tekla.Structures.Model.ContourPlate\":\n" +
                    $"            case \"Tekla.Structures.Model.LoftedPlate\":\n" +
                    $"                plate.Profile.ProfileString = $\"PL{{{propertyAccess}Thickness}}\";\n" +
                    $"                break;\n" +
                    $"            }}\n" +
                    $"            plate.Material.MaterialString = {propertyAccess}Material;\n" +
                    $"            plate.Finish = {propertyAccess}Finish;\n" +
                    $"            plate.Class = {propertyAccess}Class.ToString();\n" +
                    $"            plate.AssemblyNumber.Prefix = {propertyAccess}AssemblyPrefix;\n" +
                    $"            plate.AssemblyNumber.StartNumber = {propertyAccess}AssemblyStartNumber;\n" +
                    $"            plate.PartNumber.Prefix = {propertyAccess}PartPrefix;\n" +
                    $"            plate.PartNumber.StartNumber = {propertyAccess}PartStartNumber;\n" +
                    $"        }}");
                break;
            case "WeldFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_weld{ToPropertyNameStyle(id)}"
                    : $"Weld{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <typeparamref name=\"T\"/> type model object for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\n" +
                    $"        private T CreatWeld{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.BaseWeld, new() {{\n" +
                    $"            T weld = new T();\n" +
                    $"            weld.SizeAbove = {propertyAccess}SizeAbove;\n" +
                    $"            weld.SizeBelow = {propertyAccess}SizeBelow;\n" +
                    $"            weld.TypeAbove = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeAbove.ToString(), true);\n" +
                    $"            weld.TypeBelow = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeBelow.ToString(), true);\n" +
                    $"            weld.AngleAbove = {propertyAccess}AngleAbove;\n" +
                    $"            weld.AngleBelow = {propertyAccess}AngleBelow;\n" +
                    $"            weld.ContourAbove = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourAbove.ToString(), true);\n" +
                    $"            weld.ContourBelow = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourBelow.ToString(), true);\n" +
                    $"            weld.FinishAbove = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishAbove.ToString(), true);\n" +
                    $"            weld.FinishBelow = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishBelow.ToString(), true);\n" +
                    $"            weld.RootFaceAbove = {propertyAccess}RootFaceAbove;\n" +
                    $"            weld.RootFaceBelow = {propertyAccess}RootFaceBelow;\n" +
                    $"            weld.EffectiveThroatAbove = {propertyAccess}EffectiveThroatAbove;\n" +
                    $"            weld.EffectiveThroatBelow = {propertyAccess}EffectiveThroatBelow;\n" +
                    $"            weld.RootOpeningAbove = {propertyAccess}RootOpeningAbove;\n" +
                    $"            weld.RootOpeningBelow = {propertyAccess}RootOpeningBelow;\n" +
                    $"            weld.IncrementAmountAbove = {propertyAccess}IncrementAmountAbove;\n" +
                    $"            weld.IncrementAmountBelow = {propertyAccess}IncrementAmountBelow;\n" +
                    $"            weld.LengthAbove = {propertyAccess}LengthAbove;\n" +
                    $"            weld.LengthBelow = {propertyAccess}LengthBelow;\n" +
                    $"            weld.PitchAbove = {propertyAccess}PitchAbove;\n" +
                    $"            weld.PitchBelow = {propertyAccess}PitchBelow;\n" +
                    $"            weld.AroundWeld = {propertyAccess}Around != 0;\n" +
                    $"            weld.ShopWeld = {propertyAccess}Shop != 0;\n" +
                    $"            weld.Placement = ({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum), {propertyAccess}Placement.ToString(), true);\n" +
                    $"            weld.Preparation = ({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum), {propertyAccess}Preparation.ToString(), true);\n" +
                    $"            weld.IntermittentType = ({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum)global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum), {propertyAccess}Intermittent.ToString(), true);\n" +
                    $"            weld.ReferenceText = {propertyAccess}ReferenceText;\n" +
                    $"            return weld;\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given model object to make its properties consistent with weld '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"weld\">The weld to be modified.</param>\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"weld\"/>.</typeparam>\n" +
                    $"        private void ModifyToWeld{ToPropertyNameStyle(id)}<T>(ref T weld) where T : {TsmNameSpace}.BaseWeld {{\n" +
                    $"            weld.SizeAbove = {propertyAccess}SizeAbove;\n" +
                    $"            weld.SizeBelow = {propertyAccess}SizeBelow;\n" +
                    $"            weld.TypeAbove = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeAbove.ToString(), true);\n" +
                    $"            weld.TypeBelow = ({TsmNameSpace}.BaseWeld.WeldTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldTypeEnum), {propertyAccess}TypeBelow.ToString(), true);\n" +
                    $"            weld.AngleAbove = {propertyAccess}AngleAbove;\n" +
                    $"            weld.AngleBelow = {propertyAccess}AngleBelow;\n" +
                    $"            weld.ContourAbove = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourAbove.ToString(), true);\n" +
                    $"            weld.ContourBelow = ({TsmNameSpace}.BaseWeld.WeldContourEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldContourEnum), {propertyAccess}ContourBelow.ToString(), true);\n" +
                    $"            weld.FinishAbove = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishAbove.ToString(), true);\n" +
                    $"            weld.FinishBelow = ({TsmNameSpace}.BaseWeld.WeldFinishEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldFinishEnum), {propertyAccess}FinishBelow.ToString(), true);\n" +
                    $"            weld.RootFaceAbove = {propertyAccess}RootFaceAbove;\n" +
                    $"            weld.RootFaceBelow = {propertyAccess}RootFaceBelow;\n" +
                    $"            weld.EffectiveThroatAbove = {propertyAccess}EffectiveThroatAbove;\n" +
                    $"            weld.EffectiveThroatBelow = {propertyAccess}EffectiveThroatBelow;\n" +
                    $"            weld.RootOpeningAbove = {propertyAccess}RootOpeningAbove;\n" +
                    $"            weld.RootOpeningBelow = {propertyAccess}RootOpeningBelow;\n" +
                    $"            weld.IncrementAmountAbove = {propertyAccess}IncrementAmountAbove;\n" +
                    $"            weld.IncrementAmountBelow = {propertyAccess}IncrementAmountBelow;\n" +
                    $"            weld.LengthAbove = {propertyAccess}LengthAbove;\n" +
                    $"            weld.LengthBelow = {propertyAccess}LengthBelow;\n" +
                    $"            weld.PitchAbove = {propertyAccess}PitchAbove;\n" +
                    $"            weld.PitchBelow = {propertyAccess}PitchBelow;\n" +
                    $"            weld.AroundWeld = {propertyAccess}Around != 0;\n" +
                    $"            weld.ShopWeld = {propertyAccess}Shop != 0;\n" +
                    $"            weld.Placement = ({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPlacementTypeEnum), {propertyAccess}Placement.ToString(), true);\n" +
                    $"            weld.Preparation = ({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldPreparationTypeEnum), {propertyAccess}Preparation.ToString(), true);\n" +
                    $"            weld.IntermittentType = ({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum)global::System.Enum.Parse(typeof({TsmNameSpace}.BaseWeld.WeldIntermittentTypeEnum), {propertyAccess}Intermittent.ToString(), true);\n" +
                    $"            weld.ReferenceText = {propertyAccess}ReferenceText;\n" +
                    $"        }}");
                break;
            case "BoltFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_bolt{ToPropertyNameStyle(id)}"
                    : $"Bolt{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <typeparamref name=\"T\"/> type model object for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <typeparam name=\"T\">The model object type want to get.</typeparam>\n" +
                    $"        /// <returns>The <typeparamref name=\"T\"/> type model object created.</returns>\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.BoltCircle\"/>.\n" +
                    $"        /// </exception>\n" +
                    $"        private T CreatBolt{ToPropertyNameStyle(id)}<T>() where T : {TsmNameSpace}.BoltGroup, new() {{\n" +
                    $"            {TsmNameSpace}.BoltGroup bolt = null;\n" +
                    $"            switch (typeof(T).ToString()) {{\n" +
                    $"            case \"Tekla.Structures.Model.BoltCircle\":\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.BoltCircle)}}\\\" not supported.\");\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\n" +
                    $"                bolt = new {TsmNameSpace}.BoltArray();\n" +
                    $"                break;\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\n" +
                    $"                bolt = new {TsmNameSpace}.BoltXYList();\n" +
                    $"                break;\n" +
                    $"            }}\n" +
                    $"            \n" +
                    $"            {TsdNameSpace}.DistanceList distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistXText, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\n" +
                    $"            {TsdNameSpace}.DistanceList distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistYText, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\n" +
                    $"            \n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\n" +
                    $"            bolt.Length = {propertyAccess}Length;\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;\n" +
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\n" +
                    $"            \n" +
                    $"            switch (typeof(T).ToString()) {{\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = bolt as {TsmNameSpace}.BoltArray;\n" +
                    $"                foreach (var dist in distListX) {{\n" +
                    $"                    boltArray.AddBoltDistX(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                foreach (var dist in distListY) {{\n" +
                    $"                    boltArray.AddBoltDistY(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                bolt = boltArray;\n" +
                    $"                break;\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = bolt as {TsmNameSpace}.BoltXYList;\n" +
                    $"                foreach (var dist in distListX) {{\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                foreach (var dist in distListY) {{\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                bolt = boltXYList;\n" +
                    $"                break;\n" +
                    $"            }}\n" +
                    $"            \n" +
                    $"            return bolt as T;\n" +
                    $"        }}\n");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given model object to make its properties consistent with bolt '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"bolt\">The bolt to be modified.</param>\n" +
                    $"        /// <typeparam name=\"T\">The original type of <paramref name=\"bolt\"/>.</typeparam>\n" +
                    $"        /// <exception cref=\"global::System.NotSupportedException\">\n" +
                    $"        /// Throw when <typeparamref name=\"T\"/> is <see cref=\"global::Tekla.Structures.Model.BoltCircle\"/>.\n" +
                    $"        /// </exception>\n" +
                    $"        private void ModifyToBolt{ToPropertyNameStyle(id)}<T>(ref T bolt) where T : {TsmNameSpace}.BoltGroup {{\n" +
                    $"            if (typeof(T) == typeof({TsmNameSpace}.BoltCircle)) {{\n" +
                    $"                throw new global::System.NotSupportedException($\"Type \\\"{{typeof({TsmNameSpace}.BoltCircle)}}\\\" not supported.\");\n" +
                    $"            }}\n" +
                    $"            \n" +
                    $"            {TsdNameSpace}.DistanceList distListX = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistXText, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\n" +
                    $"            {TsdNameSpace}.DistanceList distListY = {TsdNameSpace}.DistanceList.Parse({propertyAccess}DistYText, global::System.Globalization.CultureInfo.CurrentCulture, {TsdNameSpace}.Distance.CurrentUnitType);\n" +
                    $"            \n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\n" +
                    $"            bolt.Length = {propertyAccess}Length;\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;\n" +
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\n" +
                    $"            \n" +
                    $"            switch (typeof(T).ToString()) {{\n" +
                    $"            case \"Tekla.Structures.Model.BoltArray\":\n" +
                    $"                {TsmNameSpace}.BoltArray boltArray = bolt as {TsmNameSpace}.BoltArray;\n" +
                    $"                foreach (var dist in distListX) {{\n" +
                    $"                    boltArray.AddBoltDistX(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                foreach (var dist in distListY) {{\n" +
                    $"                    boltArray.AddBoltDistY(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                bolt = boltArray as T;\n" +
                    $"                break;\n" +
                    $"            case \"Tekla.Structures.Model.BoltXYList\":\n" +
                    $"                {TsmNameSpace}.BoltXYList boltXYList = bolt as {TsmNameSpace}.BoltXYList;\n" +
                    $"                foreach (var dist in distListX) {{\n" +
                    $"                    boltXYList.AddBoltDistX(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                foreach (var dist in distListY) {{\n" +
                    $"                    boltXYList.AddBoltDistY(dist.Millimeters);\n" +
                    $"                }}\n" +
                    $"                bolt = boltXYList as T;\n" +
                    $"                break;\n" +
                    $"            }}\n" +
                    $"        }}\n");
                break;
            case "BoltCircleFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_boltCircle{ToPropertyNameStyle(id)}"
                    : $"BoltCircle{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <see cref=\"global::Tekla.Structures.Model.BoltCircle\"/> type model object for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <returns>The <see cref=\"global::Tekla.Structures.Model.BoltCircle\"/> type model object created.</returns>\n" +
                    $"        private {TsmNameSpace}.BoltCircle CreatBoltCircle{ToPropertyNameStyle(id)}() {{\n" +
                    $"            {TsmNameSpace}.BoltCircle bolt = new {TsmNameSpace}.BoltCircle();\n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\n" +
                    $"            bolt.NumberOfBolts = {propertyAccess}NumberOfBolts;\n" +
                    $"            bolt.Diameter = {propertyAccess}Diameter;\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\n" +
                    $"            bolt.Length = {propertyAccess}Length;\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;\n" +
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\n" +
                    $"            return bolt;\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given model object to make its properties consistent with bolt circle '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"bolt\">The bolt circle to be modified.</param>\n" +
                    $"        private void ModifyToBoltCircle{ToPropertyNameStyle(id)}(ref {TsmNameSpace}.BoltCircle bolt) {{\n" +
                    $"            bolt.BoltSize = {propertyAccess}Size;\n" +
                    $"            bolt.BoltStandard = {propertyAccess}Standard;\n" +
                    $"            bolt.NumberOfBolts = {propertyAccess}NumberOfBolts;\n" +
                    $"            bolt.Diameter = {propertyAccess}Diameter;\n" +
                    $"            bolt.BoltType = ({TsmNameSpace}.BoltGroup.BoltTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            bolt.ThreadInMaterial = ({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltThreadInMaterialEnum), {propertyAccess}ThreadInMaterial.ToString(), true);\n" +
                    $"            bolt.Length = {propertyAccess}Length;\n" +
                    $"            bolt.CutLength = {propertyAccess}CutLength;\n" +
                    $"            bolt.ExtraLength = {propertyAccess}ExtraLength;\n" +
                    $"            bolt.Tolerance = {propertyAccess}Tolerance;\n" +
                    $"            bolt.PlainHoleType = ({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltPlainHoleTypeEnum), {propertyAccess}PlainType.ToString(), true);\n" +
                    $"            bolt.Hole1 = {propertyAccess}Hole1 != 0;\n" +
                    $"            bolt.Hole2 = {propertyAccess}Hole2 != 0;\n" +
                    $"            bolt.Hole3 = {propertyAccess}Hole3 != 0;\n" +
                    $"            bolt.Hole4 = {propertyAccess}Hole4 != 0;\n" +
                    $"            bolt.Hole5 = {propertyAccess}Hole5 != 0;\n" +
                    $"            bolt.HoleType = ({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltHoleTypeEnum), {propertyAccess}HoleType.ToString(), true);\n" +
                    $"            bolt.SlottedHoleX = {propertyAccess}SlottedHoleX;\n" +
                    $"            bolt.SlottedHoleY = {propertyAccess}SlottedHoleY;\n" +
                    $"            bolt.RotateSlots = ({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.BoltGroup.BoltRotateSlotsEnum), {propertyAccess}RotateSlots.ToString(), true);\n" +
                    $"            bolt.Bolt = {propertyAccess}IsBolt != 0;\n" +
                    $"            bolt.Nut1 = {propertyAccess}UseNut1 != 0;\n" +
                    $"            bolt.Nut2 = {propertyAccess}UseNut2 != 0;\n" +
                    $"            bolt.Washer1 = {propertyAccess}UseWasher1 != 0;\n" +
                    $"            bolt.Washer2 = {propertyAccess}UseWasher2 != 0;\n" +
                    $"            bolt.Washer3 = {propertyAccess}UseWasher3 != 0;\n" +
                    $"        }}");
                break;
            case "ChamferFieldDefaultValuesAttribute":
                propertyAccess = memberAccess + (targetType is AttributeTargets.Class
                    ? $"_chamfer{ToPropertyNameStyle(id)}"
                    : $"Chamfer{ToPropertyNameStyle(id)}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Creat <see cref=\"global::Tekla.Structures.Model.Chamfer\"/> for id '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <returns>The <see cref=\"global::Tekla.Structures.Model.Chamfer\"/> created.</returns>\n" +
                    $"        private {TsmNameSpace}.Chamfer CreatChamfer{ToPropertyNameStyle(id)}() {{\n" +
                    $"            {TsmNameSpace}.Chamfer chamfer = new {TsmNameSpace}.Chamfer();\n" +
                    $"            chamfer.Type = ({TsmNameSpace}.Chamfer.ChamferTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.Chamfer.ChamferTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            chamfer.X = {propertyAccess}X;\n" +
                    $"            chamfer.Y = {propertyAccess}Y;\n" +
                    $"            chamfer.DZ1 = {propertyAccess}Dz1;\n" +
                    $"            chamfer.DZ2 = {propertyAccess}Dz2;\n" +
                    $"            return chamfer;\n" +
                    $"        }}");

                builder.AppendLine(
                    $"        \n" +
                    $"        /// <summary>\n" +
                    $"        /// Modify the given chamfer to make its properties consistent with chamfer '{id}'.\n" +
                    $"        /// </summary>\n" +
                    $"        /// <param name=\"chamfer\">The chamfer to be modified.</param>\n" +
                    $"        private void ModifyToChamfer{ToPropertyNameStyle(id)}(ref {TsmNameSpace}.Chamfer chamfer) {{\n" +
                    $"            chamfer.Type = ({TsmNameSpace}.Chamfer.ChamferTypeEnum) global::System.Enum.Parse(typeof({TsmNameSpace}.Chamfer.ChamferTypeEnum), {propertyAccess}Type.ToString(), true);\n" +
                    $"            chamfer.X = {propertyAccess}X;\n" +
                    $"            chamfer.Y = {propertyAccess}Y;\n" +
                    $"            chamfer.DZ1 = {propertyAccess}Dz1;\n" +
                    $"            chamfer.DZ2 = {propertyAccess}Dz2;\n" +
                    $"        }}");
                break;
            }
        }

        return builder.ToString();
    }
}