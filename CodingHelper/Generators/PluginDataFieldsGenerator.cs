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
using FieldInfo = (string Name, string AttributeName, string Type);

namespace Muggle.TsExtensions.CodingHelper.Generators {
    [Generator]
    internal class PluginDataFieldsGenerator : IIncrementalGenerator {
        internal static readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldsAttribute"
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

        #region Templates

        private const string PluginDataClassTemplate =
            """
            //  <auto-generated/>{{generatedAt}}
            using Tekla.Structures.Plugins;

            namespace {{namespace}} {
                
                {{accessibility}} partial {{typeKind}}class {{className}} {
            {{fields}}        
                }
                
            }
            """;

        private const string PluginDataFieldDeclareTemplate =
            """
                    
                    [StructuresField("{{modelObjectTypeAbbreviation}}{{nameOrNumber}}{{attributeName}}")]
                    public {{dataType}} {{modelObjectType}}{{nameOrNumber}}{{propertyName}};
            """;

        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            context.RegisterPostInitializationOutput(ctx => {
                foreach (var attribute in ConcernedAttributes) {
                    var shortName = attribute.Substring(attribute.LastIndexOf('.') + 1);
                    ctx.AddSource($"{shortName}.g.cs",
                        SourceText.From(GetResourceAsString($"{shortName}.cs"), Encoding.UTF8));
                }
            });

            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform)
                .Where(x => x != default);

            context.RegisterSourceOutput(provider, Generate);
        }

        private bool Predicate(SyntaxNode node, CancellationToken token) {
            if (token.IsCancellationRequested) return false;

            if (node is not ClassDeclarationSyntax classDeclarationSyntax ||
                classDeclarationSyntax.AttributeLists.Count == 0) {
                return false;
            }

            return true;
        }

        private PluginDataFieldsInfo Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token) {
            var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
            if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return default;

            return GetPluginDataFieldsInfo(syntaxContext, token, ConcernedAttributes);
        }

        private void Generate(SourceProductionContext context, PluginDataFieldsInfo classInfo) {
            if (classInfo == default) return;

            var builder = new StringBuilder();
            foreach (var kvp in classInfo.Arguments) {
                var attributeName = kvp.Key;

                var modelObjectType = attributeName.Substring(0, kvp.Key.Length - 15);
                var modelObjectAbbreviation = modelObjectType switch {
                    "Part" => "PT",
                    "Plate" => "PL",
                    "Weld" => "W",
                    "Bolt" => "B",
                    "BoltCircle" => "BC",
                    _ => string.Empty
                };

                var template = PluginDataFieldDeclareTemplate
                    .Replace("{{modelObjectType}}", modelObjectType)
                    .Replace("{{modelObjectTypeAbbreviation}}", modelObjectAbbreviation);

                var infos = attributeName switch {
                    "PartFieldsAttribute" => PartFieldInfos,
                    "PlateFieldsAttribute" => PlateFieldInfos,
                    "WeldFieldsAttribute" => WeldFieldInfos,
                    "BoltFieldsAttribute" => BoltFieldInfos,
                    "BoltCircleFieldsAttribute" => BoltCircleFieldInfos,
                    _ => []
                };

                var templateBuilder = new StringBuilder();
                foreach (var info in infos) {
                    templateBuilder.AppendLine(template
                        .Replace("{{propertyName}}", info.Name)
                        .Replace("{{dataType}}", info.Type)
                        .Replace("{{attributeName}}", info.AttributeName));
                }

                template = templateBuilder.ToString();

                foreach (var nameOrNumber in kvp.Value) {
                    var match = Regex.Match(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern);
                    if (match.Success) continue;

                    builder.Append(template.Replace("{{nameOrNumber}}", nameOrNumber));
                }
            }

            var output = PluginDataClassTemplate
#if DEBUG
                .Replace("{{generatedAt}}", $" at {DateTime.Now}")
#else
                .Replace("{{generatedAt}}", string.Empty)
#endif
                .Replace("{{namespace}}", classInfo.ClassInfo.NameSpace)
                .Replace("{{accessibility}}", classInfo.ClassInfo.Accessibility.ToString().ToLower())
                .Replace("{{typeKind}}", classInfo.ClassInfo.IsRecord ? "record " : string.Empty)
                .Replace("{{className}}", classInfo.ClassInfo.Name)
                .Replace("{{fields}}", builder.ToString());

            context.AddSource($"{classInfo.ClassInfo.Name}.g.cs", SourceText.From(output, Encoding.UTF8));
        }
    }
}