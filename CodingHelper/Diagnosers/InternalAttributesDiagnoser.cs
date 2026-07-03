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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Muggle.TsExtensions.CodingHelper.Generators.Information;

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
        "Attribute '{0}' should only be applied on one place (Class, Field or Property)",
        Category,
        DiagnosticSeverity.Error,
        true);

    internal static readonly DiagnosticDescriptor DataTypeDoesNotContainTheseFields = new(
        "MTSECH012",
        "Target data type doesn't contain these fields",
        "Check whether the target data type contains these fields",
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

    internal static readonly DiagnosticDescriptor ShouldNotUseBooleanType = new(
        "MTSECH021",
        "Should not use 'Boolean' type",
        "Should not use 'Boolean' type, use 'Integer' type instead",
        Category,
        DiagnosticSeverity.Error,
        true);

    #endregion

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        NotPartial, NotImplementINotifyPropertyChanged, LengthExceedLimitation, ContainsSpecialCharacters,
        ContainsUnsuggestedCharacters, FieldsFromAttributeNotApplied, AppliedOnOverOnePlace,
        DataTypeDoesNotContainTheseFields, SetDefaultValueMultiTimes, NameStartsWithNumber,
        ArgumentsMustBePassedInPairs, NotSupportedDataType, NotExpectedDataType, RegisterFieldOrPropertyMultiTimes,
        ShouldNotUseBooleanType
    ];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }

    internal static string PrefixSuffixExample(string attributeName) {
        if (attributeName is null) return string.Empty;
        if (attributeName.StartsWith("Part")) return "PT<Id>MATL";
        if (attributeName.StartsWith("Plate")) return "PL<Id>MATL";
        if (attributeName.StartsWith("Weld")) return "W<Id>SIZEA";
        if (attributeName.StartsWith("BoltCircle")) return "BC<Id>PLAIN";
        if (attributeName.StartsWith("Bolt")) return "B<Id>DISTX";
        if (attributeName.StartsWith("Chamfer")) return "CF<Id>TYPE";
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
        if (attributeName is null) return int.MinValue;
        if (attributeName.StartsWith("Part")) return 13;
        if (attributeName.StartsWith("Plate")) return 13;
        if (attributeName.StartsWith("Weld")) return 13;
        if (attributeName.StartsWith("BoltCircle")) return 12;
        if (attributeName.StartsWith("Bolt")) return 13;
        if (attributeName.StartsWith("Chamfer")) return 13;
        return 19;
    }

    internal static HashSet<string> SupportedDataTypes(string attributeName) => attributeName switch {
        "GeneralFieldsAttribute" => ["int", "double", "string"],
        "GeneralPropertiesAttribute" =>
            ["Integer", "Double", "Distance", "DistanceList", "String"],
        _ => []
    };

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
}