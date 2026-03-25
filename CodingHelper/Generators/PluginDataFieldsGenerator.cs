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

namespace Muggle.TsExtensions.CodingHelper.Generators {
    [Generator]
    internal class PluginDataFieldsGenerator : IIncrementalGenerator {
        private static readonly string[] ConcernedAttributes = [
            "Muggle.TsExtensions.CodingHelper.Generators.PartFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.PlateFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.WeldFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltFieldsAttribute",
            "Muggle.TsExtensions.CodingHelper.Generators.BoltCircleFieldsAttribute"
        ];

        #region Initial files

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// </remarks>
        private const string PartFieldsAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the part(s) fields that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class PartFieldsAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the part(s) fields using the given number(s).
                    /// </summary>
                    public PartFieldsAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the part(s) fields using the given name(s).
                    /// </summary>
                    public PartFieldsAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string PlateFieldsAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the plate(s) fields that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class PlateFieldsAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the plate(s) fields using the given number(s).
                    /// </summary>
                    public PlateFieldsAttribute(params uint[] numbers)  { }
                    
                    /// <summary>
                    /// Register the plate(s) fields using the given name(s).
                    /// </summary>
                    public PlateFieldsAttribute(params string[] names)  { }
                    
                }
                
            }
            """;

        private const string WeldFieldsAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the weld(s) fields that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class WeldFieldsAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the weld(s) fields using the given number(s).
                    /// </summary>
                    public WeldFieldsAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the weld(s) fields using the given name(s).
                    /// </summary>
                    public WeldFieldsAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string BoltFieldsAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the bolt(s) fields that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class BoltFieldsAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the bolt(s) fields using the given number(s).
                    /// </summary>
                    public BoltFieldsAttribute(params uint[] numbers) { }
                    
                    /// <summary>
                    /// Register the bolt(s) fields using the given name(s).
                    /// </summary>
                    public BoltFieldsAttribute(params string[] names) { }
                    
                }
                
            }
            """;

        private const string BoltCircleFieldsAttribute =
            """
            using System;

            namespace Muggle.TsExtensions.CodingHelper.Generators {
                
                /// <summary>
                /// Register the bolt circle(s) fields that need to be generated for the applied class,
                /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
                /// cannot be used independently.
                /// </summary>
                /// <remarks>Mapping relationship between properties and attribute name pattern 
                /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
                /// see here</a>.</remarks>
                [AttributeUsage(AttributeTargets.Class)]
                public class BoltCircleFieldsAttribute : Attribute {
                    
                    /// <summary>
                    /// Register the bolt circle(s) fields using the given number(s).
                    /// </summary>
                    public BoltCircleFieldsAttribute(params uint[] numbers)  { }
                    
                    /// <summary>
                    /// Register the bolt circle(s) fields using the given name(s).
                    /// </summary>
                    public BoltCircleFieldsAttribute(params string[] names)  { }
                    
                }
                
            }
            """;

        #endregion

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

        private const string PartFieldsTemplate =
            """
                    
                    [StructuresField("PT{{nameOrNumber}}NAME")]
                    public string Part{{nameOrNumber}}Name;
                    
                    [StructuresField("PT{{nameOrNumber}}PRF")]
                    public string Part{{nameOrNumber}}Profile;
                    
                    [StructuresField("PT{{nameOrNumber}}MATL")]
                    public string Part{{nameOrNumber}}Material;
                    
                    [StructuresField("PT{{nameOrNumber}}FNSH")]
                    public string Part{{nameOrNumber}}Finish;
                    
                    [StructuresField("PT{{nameOrNumber}}CLS")]
                    public int Part{{nameOrNumber}}Class;
                    
                    [StructuresField("PT{{nameOrNumber}}ASMP")]
                    public string Part{{nameOrNumber}}AssemblyPrefix;
                    
                    [StructuresField("PT{{nameOrNumber}}ASMN")]
                    public int Part{{nameOrNumber}}AssemblyStartNumber;
                    
                    [StructuresField("PT{{nameOrNumber}}PTP")]
                    public string Part{{nameOrNumber}}PartPrefix;
                    
                    [StructuresField("PT{{nameOrNumber}}PTN")]
                    public int Part{{nameOrNumber}}PartStartNumber;
            """;

        private const string PlateFieldsTemplate =
            """
                    
                    [StructuresField("PL{{nameOrNumber}}NAME")]
                    public string Plate{{nameOrNumber}}Name;
                    
                    [StructuresField("PL{{nameOrNumber}}T")]
                    public double Plate{{nameOrNumber}}Thickness;
                    
                    [StructuresField("PL{{nameOrNumber}}B")]
                    public double Plate{{nameOrNumber}}Breadth;
                    
                    [StructuresField("PL{{nameOrNumber}}H")]
                    public double Plate{{nameOrNumber}}Height;
                    
                    [StructuresField("PL{{nameOrNumber}}MATL")]
                    public string Plate{{nameOrNumber}}Material;
                    
                    [StructuresField("PL{{nameOrNumber}}FNSH")]
                    public string Plate{{nameOrNumber}}Finish;
                    
                    [StructuresField("PL{{nameOrNumber}}CLS")]
                    public int Plate{{nameOrNumber}}Class;
                    
                    [StructuresField("PL{{nameOrNumber}}ASMP")]
                    public string Plate{{nameOrNumber}}AssemblyPrefix;
                    
                    [StructuresField("PL{{nameOrNumber}}ASMN")]
                    public int Plate{{nameOrNumber}}AssemblyStartNumber;
                    
                    [StructuresField("PL{{nameOrNumber}}PTP")]
                    public string Plate{{nameOrNumber}}PartPrefix;
                    
                    [StructuresField("PL{{nameOrNumber}}PTN")]
                    public int Plate{{nameOrNumber}}PartStartNumber;
            """;

        private const string WeldFieldsTemplate =
            """
                    
                    [StructuresField("W{{nameOrNumber}}SIZEA")]
                    public double Weld{{nameOrNumber}}SizeAbove;
                    
                    [StructuresField("W{{nameOrNumber}}SIZEB")]
                    public double Weld{{nameOrNumber}}SizeBelow;
                    
                    [StructuresField("W{{nameOrNumber}}TYPEA")]
                    public int Weld{{nameOrNumber}}TypeAbove;
                    
                    [StructuresField("W{{nameOrNumber}}TYPEB")]
                    public int Weld{{nameOrNumber}}TypeBelow;
                    
                    [StructuresField("W{{nameOrNumber}}ANGA")]
                    public double Weld{{nameOrNumber}}AngleAbove;
                    
                    [StructuresField("W{{nameOrNumber}}ANGB")]
                    public double Weld{{nameOrNumber}}AngleBelow;
                    
                    [StructuresField("W{{nameOrNumber}}CTRA")]
                    public int Weld{{nameOrNumber}}ContourAbove;
                    
                    [StructuresField("W{{nameOrNumber}}CTRB")]
                    public int Weld{{nameOrNumber}}ContourBelow;
                    
                    [StructuresField("W{{nameOrNumber}}FNSHA")]
                    public int Weld{{nameOrNumber}}FinishAbove;
                    
                    [StructuresField("W{{nameOrNumber}}FNSHB")]
                    public int Weld{{nameOrNumber}}FinishBelow;
                    
                    [StructuresField("W{{nameOrNumber}}FACEA")]
                    public double Weld{{nameOrNumber}}RootFaceAbove;
                    
                    [StructuresField("W{{nameOrNumber}}FACEB")]
                    public double Weld{{nameOrNumber}}RootFaceBelow;
                    
                    [StructuresField("W{{nameOrNumber}}THROA")]
                    public double Weld{{nameOrNumber}}EffectiveThroatAbove;
                    
                    [StructuresField("W{{nameOrNumber}}THROB")]
                    public double Weld{{nameOrNumber}}EffectiveThroatBelow;
                    
                    [StructuresField("W{{nameOrNumber}}OPNGA")]
                    public double Weld{{nameOrNumber}}RootOpeningAbove;
                    
                    [StructuresField("W{{nameOrNumber}}OPNGB")]
                    public double Weld{{nameOrNumber}}RootOpeningBelow;
                    
                    [StructuresField("W{{nameOrNumber}}INCRA")]
                    public int Weld{{nameOrNumber}}IncrementAmountAbove;
                    
                    [StructuresField("W{{nameOrNumber}}INCRB")]
                    public int Weld{{nameOrNumber}}IncrementAmountBelow;
                    
                    [StructuresField("W{{nameOrNumber}}LENA")]
                    public double Weld{{nameOrNumber}}LengthAbove;
                    
                    [StructuresField("W{{nameOrNumber}}LENB")]
                    public double Weld{{nameOrNumber}}LengthBelow;
                    
                    [StructuresField("W{{nameOrNumber}}PITA")]
                    public double Weld{{nameOrNumber}}PicthAbove;
                    
                    [StructuresField("W{{nameOrNumber}}PITB")]
                    public double Weld{{nameOrNumber}}PicthBelow;
                    
                    [StructuresField("W{{nameOrNumber}}ARND")]
                    public int Weld{{nameOrNumber}}Around;
                    
                    [StructuresField("W{{nameOrNumber}}SHOP")]
                    public int Weld{{nameOrNumber}}Shop;
                    
                    [StructuresField("W{{nameOrNumber}}PLACE")]
                    public int Weld{{nameOrNumber}}Placement;
                    
                    [StructuresField("W{{nameOrNumber}}PREP")]
                    public int Weld{{nameOrNumber}}Preparation;
                    
                    [StructuresField("W{{nameOrNumber}}INTMI")]
                    public int Weld{{nameOrNumber}}Intermittent;
                    
                    [StructuresField("W{{nameOrNumber}}TEXT")]
                    public string Weld{{nameOrNumber}}ReferenceText;
            """;

        private const string BoltFieldsTemplate =
            """
                    
                    [StructuresField("B{{nameOrNumber}}SIZE")]
                    public double Bolt{{nameOrNumber}}Size;
                    
                    [StructuresField("B{{nameOrNumber}}STD")]
                    public string Bolt{{nameOrNumber}}Standard;
                    
                    [StructuresField("B{{nameOrNumber}}DISTX")]
                    public string Bolt{{nameOrNumber}}DistXText;
                    
                    [StructuresField("B{{nameOrNumber}}DISTY")]
                    public string Bolt{{nameOrNumber}}DistYText;
                    
                    [StructuresField("B{{nameOrNumber}}TYPE")]
                    public int Bolt{{nameOrNumber}}Type;
                    
                    [StructuresField("B{{nameOrNumber}}THRD")]
                    public int Bolt{{nameOrNumber}}ThreadInMaterial;
                    
                    [StructuresField("B{{nameOrNumber}}CLEN")]
                    public double Bolt{{nameOrNumber}}CutLength;
                    
                    [StructuresField("B{{nameOrNumber}}XLEN")]
                    public double Bolt{{nameOrNumber}}ExtraLength;
                    
                    [StructuresField("B{{nameOrNumber}}TOL")]
                    public double Bolt{{nameOrNumber}}Tolerance;
                    
                    [StructuresField("B{{nameOrNumber}}PLAIN")]
                    public int Bolt{{nameOrNumber}}PlainType;
                    
                    [StructuresField("B{{nameOrNumber}}DEPTH")]
                    public double Bolt{{nameOrNumber}}BlindHoleDepth;
                    
                    [StructuresField("B{{nameOrNumber}}HOLE1")]
                    public int Bolt{{nameOrNumber}}Hole1;
                    
                    [StructuresField("B{{nameOrNumber}}HOLE2")]
                    public int Bolt{{nameOrNumber}}Hole2;
                    
                    [StructuresField("B{{nameOrNumber}}HOLE3")]
                    public int Bolt{{nameOrNumber}}Hole3;
                    
                    [StructuresField("B{{nameOrNumber}}HOLE4")]
                    public int Bolt{{nameOrNumber}}Hole4;
                    
                    [StructuresField("B{{nameOrNumber}}HOLE5")]
                    public int Bolt{{nameOrNumber}}Hole5;
                    
                    [StructuresField("B{{nameOrNumber}}HOLTY")]
                    public int Bolt{{nameOrNumber}}HoleType;
                    
                    [StructuresField("B{{nameOrNumber}}SLOTX")]
                    public double Bolt{{nameOrNumber}}SlottedHoleX;
                    
                    [StructuresField("B{{nameOrNumber}}SLOTY")]
                    public double Bolt{{nameOrNumber}}SlottedHoleY;
                    
                    [StructuresField("B{{nameOrNumber}}RSLOT")]
                    public int Bolt{{nameOrNumber}}RotateSlots;
                    
                    [StructuresField("B{{nameOrNumber}}ISBOT")]
                    public int Bolt{{nameOrNumber}}IsBolt;
                    
                    [StructuresField("B{{nameOrNumber}}NUT1")]
                    public int Bolt{{nameOrNumber}}UseNut1;
                    
                    [StructuresField("B{{nameOrNumber}}NUT2")]
                    public int Bolt{{nameOrNumber}}UseNut2;
                    
                    [StructuresField("B{{nameOrNumber}}WSHR1")]
                    public int Bolt{{nameOrNumber}}UseWasher1;
                    
                    [StructuresField("B{{nameOrNumber}}WSHR2")]
                    public int Bolt{{nameOrNumber}}UseWasher2;
                    
                    [StructuresField("B{{nameOrNumber}}WSHR3")]
                    public int Bolt{{nameOrNumber}}UseWasher3;
            """;

        private const string BoltCircleFieldsTemplate =
            """
                    
                    [StructuresField("BC{{nameOrNumber}}SIZE")]
                    public double BoltCircle{{nameOrNumber}}Size;
                    
                    [StructuresField("BC{{nameOrNumber}}STD")]
                    public string BoltCircle{{nameOrNumber}}Standard;
                    
                    [StructuresField("BC{{nameOrNumber}}NUM")]
                    public int BoltCircle{{nameOrNumber}}NumberOfBolts;
                    
                    [StructuresField("BC{{nameOrNumber}}DIAM")]
                    public double BoltCircle{{nameOrNumber}}Diameter;
                    
                    [StructuresField("BC{{nameOrNumber}}TYPE")]
                    public int BoltCircle{{nameOrNumber}}Type;
                    
                    [StructuresField("BC{{nameOrNumber}}THRD")]
                    public int BoltCircle{{nameOrNumber}}ThreadInMaterial;
                    
                    [StructuresField("BC{{nameOrNumber}}CLEN")]
                    public double BoltCircle{{nameOrNumber}}CutLength;
                    
                    [StructuresField("BC{{nameOrNumber}}XLEN")]
                    public double BoltCircle{{nameOrNumber}}ExtraLength;
                    
                    [StructuresField("BC{{nameOrNumber}}TOL")]
                    public double BoltCircle{{nameOrNumber}}Tolerance;
                    
                    [StructuresField("BC{{nameOrNumber}}PLAIN")]
                    public int BoltCircle{{nameOrNumber}}PlainType;
                    
                    [StructuresField("BC{{nameOrNumber}}DEPTH")]
                    public double BoltCircle{{nameOrNumber}}BlindHoleDepth;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLE1")]
                    public int BoltCircle{{nameOrNumber}}Hole1;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLE2")]
                    public int BoltCircle{{nameOrNumber}}Hole2;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLE3")]
                    public int BoltCircle{{nameOrNumber}}Hole3;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLE4")]
                    public int BoltCircle{{nameOrNumber}}Hole4;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLE5")]
                    public int BoltCircle{{nameOrNumber}}Hole5;
                    
                    [StructuresField("BC{{nameOrNumber}}HOLTY")]
                    public int BoltCircle{{nameOrNumber}}HoleType;
                    
                    [StructuresField("BC{{nameOrNumber}}SLOTX")]
                    public double BoltCircle{{nameOrNumber}}SlottedHoleX;
                    
                    [StructuresField("BC{{nameOrNumber}}SLOTY")]
                    public double BoltCircle{{nameOrNumber}}SlottedHoleY;
                    
                    [StructuresField("BC{{nameOrNumber}}RSLOT")]
                    public int BoltCircle{{nameOrNumber}}RotateSlots;
                    
                    [StructuresField("BC{{nameOrNumber}}ISBOT")]
                    public int BoltCircle{{nameOrNumber}}IsBolt;
                    
                    [StructuresField("BC{{nameOrNumber}}NUT1")]
                    public int BoltCircle{{nameOrNumber}}UseNut1;
                    
                    [StructuresField("BC{{nameOrNumber}}NUT2")]
                    public int BoltCircle{{nameOrNumber}}UseNut2;
                    
                    [StructuresField("BC{{nameOrNumber}}WSHR1")]
                    public int BoltCircle{{nameOrNumber}}UseWasher1;
                    
                    [StructuresField("BC{{nameOrNumber}}WSHR2")]
                    public int BoltCircle{{nameOrNumber}}UseWasher2;
                    
                    [StructuresField("BC{{nameOrNumber}}WSHR3")]
                    public int BoltCircle{{nameOrNumber}}UseWasher3;
            """;

        #endregion

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            context.RegisterPostInitializationOutput(ctx => {
                ctx.AddSource("PartFieldsAttribute.g.cs", SourceText.From(PartFieldsAttribute, Encoding.UTF8));
                ctx.AddSource("PlateFieldsAttribute.g.cs", SourceText.From(PlateFieldsAttribute, Encoding.UTF8));
                ctx.AddSource("WeldFieldsAttribute.g.cs", SourceText.From(WeldFieldsAttribute, Encoding.UTF8));
                ctx.AddSource("BoltFieldsAttribute.g.cs", SourceText.From(BoltFieldsAttribute, Encoding.UTF8));
                ctx.AddSource("BoltCircleFieldsAttribute.g.cs",
                    SourceText.From(BoltCircleFieldsAttribute, Encoding.UTF8));
            });

            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform)
                .Where(x => x != null);

            context.RegisterSourceOutput(provider, Generate);
        }

        private void Generate(SourceProductionContext context, AppliedClassInfo? classInfo) {
            if (!classInfo.HasValue) return;

            var builder = new StringBuilder();
            foreach (var kvp in classInfo.Value.AttributesInfo) {
                var template = kvp.Key switch {
                    "PartFieldsAttribute" => PartFieldsTemplate,
                    "PlateFieldsAttribute" => PlateFieldsTemplate,
                    "WeldFieldsAttribute" => WeldFieldsTemplate,
                    "BoltFieldsAttribute" => BoltFieldsTemplate,
                    "BoltCircleFieldsAttribute" => BoltCircleFieldsTemplate,
                    _ => throw new NotSupportedException()
                };
                foreach (var nameOrNumber in kvp.Value) {
                    var match = Regex.Match(nameOrNumber, InternalAttributesDiagnoser.SpecialCharacterPattern);
                    if (match.Success) continue;
                    
                    builder.AppendLine(template.Replace("{{nameOrNumber}}", nameOrNumber));
                }
            }

            var output = PluginDataClassTemplate
#if DEBUG
                .Replace("{{generatedAt}}", $" at {DateTime.Now}")
#else
                .Replace("{{generatedAt}}", string.Empty)
#endif
                .Replace("{{namespace}}", classInfo.Value.NameSpace)
                .Replace("{{accessibility}}", classInfo.Value.Accessibility.ToString().ToLower())
                .Replace("{{typeKind}}", classInfo.Value.IsRecord ? "record " : string.Empty)
                .Replace("{{className}}", classInfo.Value.Name)
                .Replace("{{fields}}", builder.ToString());

            context.AddSource($"{classInfo.Value.Name}.g.cs", SourceText.From(output, Encoding.UTF8));
        }

        private AppliedClassInfo? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token) {
            var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
            if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))) return null;

            return GeneratorHelper.GetClassInfo(syntaxContext, token, ConcernedAttributes);
        }

        private bool Predicate(SyntaxNode node, CancellationToken token) {
            if (token.IsCancellationRequested) return false;

            if (node is not ClassDeclarationSyntax classDeclarationSyntax ||
                classDeclarationSyntax.AttributeLists.Count == 0) {
                return false;
            }

            return true;
        }
    }
}