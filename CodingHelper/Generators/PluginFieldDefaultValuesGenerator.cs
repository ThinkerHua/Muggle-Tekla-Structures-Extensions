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

namespace Muggle.TsExtensions.CodingHelper.Generators;

[Generator]
internal class PluginFieldDefaultValuesGenerator : IIncrementalGenerator {
    internal static readonly string[] ConcernedAttributes = [
        "Muggle.TsExtensions.CodingHelper.Generators.PartFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldDefaultValuesAttribute",
        "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldDefaultValuesAttribute",
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

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        if (node is not ClassDeclarationSyntax classDeclarationSyntax ||
            !classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            return false;

        return classDeclarationSyntax.AttributeLists.Count > 0 ||
               classDeclarationSyntax.Members.Any(member =>
                   member switch {
                       FieldDeclarationSyntax { AttributeLists.Count: > 0 } or
                           PropertyDeclarationSyntax { AttributeLists.Count: > 0 } => true,
                       _ => false
                   });
    }

    private PluginFieldDefaultValuesInfo Transform(GeneratorSyntaxContext context, CancellationToken token) {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol == null) return default;

        AttributeSyntax[] attributeSyntaxes = null;
        AttributeSyntax[] fieldsFromAttSyntax = null;
        AttributeTargets appliedTarget = default;
        string targetMemberName = null;
        ITypeSymbol dataMemberType = null;
        var placeCnt = 0;

        #region analyze applied places count, and get attribute syntaxes

        if (TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ConcernedAttributes, ref attributeSyntaxes)) {
            //  When applied on class, the FieldsFromAttribute must also be applied
            if (!TryGetMatchedAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    [PluginFieldsGenerator.ConcernedAttribute], ref fieldsFromAttSyntax)) {
                return default;
            }

            placeCnt++;
            appliedTarget = AttributeTargets.Class;

            var expression = fieldsFromAttSyntax.Single().DescendantNodes().OfType<TypeOfExpressionSyntax>().Single();
            dataMemberType = semanticModel.GetTypeInfo(expression.Type).Type;
        }

        _ = classDeclarationSyntax.Members.Where(member => {
            switch (member) {
            case FieldDeclarationSyntax fieldSyntax when
                TryGetMatchedAttributes(fieldSyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attributeSyntaxes):
                placeCnt++;
                appliedTarget = AttributeTargets.Field;

                targetMemberName = fieldSyntax.Declaration.Variables[0].Identifier.ValueText;
                dataMemberType = semanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type;

                return true;
            case PropertyDeclarationSyntax propertySyntax when
                TryGetMatchedAttributes(propertySyntax.AttributeLists, semanticModel, ConcernedAttributes,
                    ref attributeSyntaxes):
                placeCnt++;
                appliedTarget = AttributeTargets.Property;

                targetMemberName = propertySyntax.Identifier.ValueText;
                dataMemberType = semanticModel.GetTypeInfo(propertySyntax.Type).Type;

                return true;
            default:
                return false;
            }
        }).ToArray();

        // no attribute or applied on over one place
        if (placeCnt != 1) return default;

        #endregion

        #region analyze data class applied attribute arguments

        var dataClassAttributeArguments = dataMemberType.GetAttributes()
            .Where(a => PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass?.ToDisplayString()))
            .Select(a => {
                var attName = a.AttributeClass!.ToDisplayString();
                attName = attName.Substring(attName.LastIndexOf('.') + 1);

                return attName switch {
                    "GeneralFieldsAttribute" =>
                        // (type, names) => (int, [param1, param2, ...])
                        (category: a.ConstructorArguments
                                .Single(typedConstant => typedConstant.Kind == TypedConstantKind.Type).Value!
                                .ToString(),
                            attArgs: a.ConstructorArguments
                                .Single(typedConstant => typedConstant.Kind == TypedConstantKind.Array).Values
                                .Select(arg => arg.Value!.ToString())
                                .ToArray()
                        ),
                    _ =>
                        // (attribute, nameOrNumbers) => (PartFieldsAttribute, [Part1, Part2, ...])
                        (category: attName,
                            attArgs: a.ConstructorArguments
                                .SelectMany(typedConstant => typedConstant.Values.Select(arg => arg.Value!.ToString()))
                                .OrderBy(s => s)
                                .ToArray()
                        )
                };
            }).OrderBy(t => t.category).ToArray();

        #endregion

        //  Key - attribute name
        //  Value - 
        //      Key - for 'GeneralFieldsWithDefaultValuesAttribute' is data type,
        //            for other attributes is name or number
        //      Value - 
        //          Key - for 'GeneralFieldsWithDefaultValuesAttribute' is parameter name,
        //                for other attributes is property name of model object
        //          Value - default value
        var attDict = new ArgumentsDictionary<DefaultValueDictionary>();

        foreach (var attSyntax in attributeSyntaxes) {
            var attName = GetAttributeName(attSyntax, semanticModel);

            if (!attDict.TryGetValue(attName, out var elementDict)) {
                elementDict = new DefaultValueDictionary();
                attDict.Add(attName, elementDict);
            }

            if (attName == "GeneralFieldDefaultValuesAttribute") {
                var exprSyntaxes = attSyntax.ArgumentList?
                    .DescendantNodes().OfType<LiteralExpressionSyntax>().ToArray();
                if (exprSyntaxes is null || exprSyntaxes.Length < 2 || exprSyntaxes.Length % 2 != 0)
                    continue;

                //  take two at once
                for (int i = 0; i < exprSyntaxes.Length; i += 2) {
                    var nameSyntax = exprSyntaxes[i];
                    var valueSyntax = exprSyntaxes[i + 1];

                    if (!nameSyntax.IsKind(SyntaxKind.StringLiteralExpression)) continue;
                    var name = nameSyntax.Token.ValueText;
                    var value = valueSyntax.Token.ValueText;

                    //  data class doesnt have these fields
                    if (!dataClassAttributeArguments.Any(t => 
                            !t.category.EndsWith("Attribute") && t.attArgs.Contains(name)))
                        continue;

                    var type = dataClassAttributeArguments.FirstOrDefault(t => t.attArgs.Contains(name)).category;

                    switch (type) {
                    case "int":
                        if (!valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                            !int.TryParse(value, out _))
                            continue;
                        break;
                    case "double":
                        if (!valueSyntax.IsKind(SyntaxKind.NumericLiteralExpression) ||
                            !double.TryParse(value, out _))
                            continue;
                        break;
                    case "string":
                        if (!valueSyntax.IsKind(SyntaxKind.StringLiteralExpression))
                            continue;
                        break;
                    default:
                        continue;
                    }

                    if (!elementDict.TryGetValue(type, out var valueDict)) {
                        valueDict = new Dictionary<string, string>();
                        elementDict.Add(type, valueDict);
                    }

                    valueDict.Add(name, value);
                }
            } else {
                var argSyntaxes = attSyntax.ArgumentList?.Arguments;
                if (argSyntaxes == null) continue;

                var valueDict = new Dictionary<string, string>();

                var nameOrNumber = string.Empty;
                var index = -1;
                foreach (var argSyntax in argSyntaxes) {
                    index++;

                    var paramName = argSyntax.NameColon?.Name.Identifier.ValueText;
                    var paramValue = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

                    if (paramName == null && index == 0 ||
                        paramName != null &&
                        Regex.IsMatch(paramName, "(part|plate|weld|bolt|boltCircle)N(ame|umber)")) {
                        nameOrNumber = paramValue;

                        if (elementDict.ContainsKey(nameOrNumber) ||
                            !dataClassAttributeArguments.Any(t =>
                                t.category.EndsWith("Attribute") &&
                                //  PartFieldsAttribute => PartField
                                //  PartFieldDefaultValuesAttribute => PartField
                                t.category.Substring(0, t.category.Length - 10) ==
                                attName.Substring(0, attName.Length - 22) &&
                                t.attArgs.Contains(nameOrNumber))) 
                            goto ContinueForeachAttribute;

                        continue;
                    }

                    paramName ??= PresetValues[attName].ElementAt(index - 1).Key;
                    valueDict.Add(paramName, paramValue);
                }

                elementDict.Add(nameOrNumber, valueDict);
            }

            ContinueForeachAttribute: ;
        }

        if (attDict.Count == 0) return default;

        token.ThrowIfCancellationRequested();

        return new PluginFieldDefaultValuesInfo {
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
    }

    private void Generate(SourceProductionContext context, PluginFieldDefaultValuesInfo info) {
        var generalFieldStatementsBuilder = new StringBuilder();
        var specificFieldStatementsBuilder = new StringBuilder();

        foreach (var kvp in info.Arguments) {
            var attName = kvp.Key;

            switch (attName) {
            case "PartFieldDefaultValuesAttribute":
            case "PlateFieldDefaultValuesAttribute":
            case "WeldFieldDefaultValuesAttribute":
            case "BoltFieldDefaultValuesAttribute":
            case "BoltCircleFieldDefaultValuesAttribute":
                specificFieldStatementsBuilder.Append(
                    GenerateSeriesFields(kvp, info.TargetType, info.TargetMemberName));
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
            $"{specificFieldStatementsBuilder}" +
            $"        }}\n" +
            $"    }}\n" +
            $"}}";

        context.AddSource($"{info.ClassInfo.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private string GenerateSeriesFields(KeyValuePair<string, DefaultValueDictionary> kvp,
        AttributeTargets dataMemberType, string dataMemberName) {
        var attName = kvp.Key;

        var fieldInfos = attName switch {
            "PartFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.PartFieldInfos,
            "PlateFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.PlateFieldInfos,
            "WeldFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.WeldFieldInfos,
            "BoltFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.BoltFieldInfos,
            "BoltCircleFieldDefaultValuesAttribute" => PluginDataFieldsGenerator.BoltCircleFieldInfos,
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

        foreach (var kvp2 in kvp.Value) {
            var nameOrNumber = kvp2.Key;
            var paramArgPair = kvp2.Value;

            foreach (var (propertyName, _, propertyDataType) in fieldInfos) {
                var paramName = ToLocalVariableNameStyle(propertyName);
                if (!paramArgPair.TryGetValue(paramName, out var defaultValue)) {
                    if (!PresetValues[attName].TryGetValue(paramName, out defaultValue))
                        continue;
                }

                var fieldAccess = $"{memberAccess}{fieldPrefix}{nameOrNumber}{propertyName}";

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

    private static string GenerateGeneralFields(DefaultValueDictionary defaultValueDictionary,
        AttributeTargets dataMemberType, string dataMemberName) {
        var builder = new StringBuilder();

        var memberAccess = dataMemberType is AttributeTargets.Class ? string.Empty : dataMemberName + ".";

        foreach (var kvp in defaultValueDictionary) {
            var type = kvp.Key;

            foreach (var kvp2 in kvp.Value) {
                var fieldName = kvp2.Key;
                if (dataMemberType is AttributeTargets.Class) fieldName = ToPrivateFieldNameStyle(fieldName);

                var fieldAccess = $"{memberAccess}{fieldName}";

                builder.AppendLine($"            if (IsDefaultValue({fieldAccess}))");
                builder.AppendLine(type is "string"
                    ? $"                {fieldAccess} = \"{kvp2.Value}\";"
                    : $"                {fieldAccess} = {kvp2.Value};");
            }
        }

        return builder.ToString();
    }
}