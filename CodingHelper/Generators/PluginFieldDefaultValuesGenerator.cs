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
    ];

    /// <summary>
    /// Dictionary of default values.
    /// <list type="bullet">
    ///     <item>Key - attribute short name, such as "PartFieldDefaultValuesAttribute".</item>
    ///     <item>Value - dictionary of field name and default value.
    ///         <list type="bullet">
    ///             <item>Key - field name, such as "profile".</item>
    ///             <item>Value - default value.</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </summary>
    private ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> DefaultValues { get; }

    #region Initial files

    private const string PartFieldDefaultValuesAttribute =
        """
        using System;

        namespace Muggle.TsExtensions.CodingHelper.Generators {

            /// <summary>
            /// Register default value for part fields.
            /// </summary>
            /// <remarks>
            /// You need to manually call the "SetDataToDefaultIfUnset" method.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
            public class PartFieldDefaultValuesAttribute : Attribute {

                /// <summary>
                /// Register default value by part name.
                /// </summary>
                /// <param name="partName">The name registered by <see cref="PartFieldsAttribute"/>.</param>
                /// <param name="profile">The default value for the "ProfileString" property of "Part.Profile".</param>
                /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
                /// <param name="name">The default value for the "Name" property of "Part".</param>
                /// <param name="finish">The default value for the "Finish" property of "Part".</param>
                /// <param name="class">The default value for the "Class" property of "Part".</param>
                /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
                /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
                /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
                /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
                public PartFieldDefaultValuesAttribute(
                    string partName, string profile = "", string material = "", string name = "", string finish = "",
                    int @class = 99, string assemblyPrefix = "A", int assemblyStartNumber = 1, string partPrefix = "P",
                    int partStartNumber = 1) {
                    
                }

                /// <summary>
                /// Register default value by part number.
                /// </summary>
                /// <param name="partNumber">The number registered by <see cref="PartFieldsAttribute"/>.</param>
                /// <param name="profile">The default value for the "ProfileString" property of "Part.Profile".</param>
                /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
                /// <param name="name">The default value for the "Name" property of "Part".</param>
                /// <param name="finish">The default value for the "Finish" property of "Part".</param>
                /// <param name="class">The default value for the "Class" property of "Part".</param>
                /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
                /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
                /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
                /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
                public PartFieldDefaultValuesAttribute(
                    int partNumber, string profile = "", string material = "", string name = "", string finish = "",
                    int @class = 99, string assemblyPrefix = "A", int assemblyStartNumber = 1, string partPrefix = "P",
                    int partStartNumber = 1) {
                    
                }
            }
        }

        """;

    private const string PlateFieldDefaultValuesAttribute =
        """
        using System;

        namespace Muggle.TsExtensions.CodingHelper.Generators {
            
            /// <summary>
            /// Register default value for plate fields.
            /// </summary>
            /// <remarks>
            /// You need to manually call the "SetDataToDefaultIfUnset" method.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
            public class PlateFieldDefaultValuesAttribute : Attribute {
                
                /// <summary>
                /// Register default value by plate name.
                /// </summary>
                /// <param name="plateName">The name registered by <see cref="PartFieldsAttribute"/>.</param>
                /// <param name="thickness">The default value for plate's thickness.</param>
                /// <param name="breadth">The default value for plate's breadth.</param>
                /// <param name="height">The default value for plate's height.</param>
                /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
                /// <param name="name">The default value for the "Name" property of "Part".</param>
                /// <param name="finish">The default value for the "Finish" property of "Part".</param>
                /// <param name="class">The default value for the "Class" property of "Part".</param>
                /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
                /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
                /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
                /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
                public PlateFieldDefaultValuesAttribute(
                    string plateName, double thickness = 0, double breadth = 0, double height = 0, string material = "",
                    string name = "", string finish = "", int @class = 99, string assemblyPrefix = "A",
                    int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {
                    
                }

                /// <summary>
                /// Register default value by plate number.
                /// </summary>
                /// <param name="plateNumber">The number registered by <see cref="PartFieldsAttribute"/>.</param>
                /// <param name="thickness">The default value for plate's thickness.</param>
                /// <param name="breadth">The default value for plate's breadth.</param>
                /// <param name="height">The default value for plate's height.</param>
                /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
                /// <param name="name">The default value for the "Name" property of "Part".</param>
                /// <param name="finish">The default value for the "Finish" property of "Part".</param>
                /// <param name="class">The default value for the "Class" property of "Part".</param>
                /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
                /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
                /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
                /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
                public PlateFieldDefaultValuesAttribute(
                    int plateNumber, double thickness = 0, double breadth = 0, double height = 0, string material = "",
                    string name = "", string finish = "", int @class = 99, string assemblyPrefix = "A",
                    int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {
                    
                }
            }
        }

        """;

    private const string WeldFieldDefaultValuesAttribute =
        """
        using System;

        namespace Muggle.TsExtensions.CodingHelper.Generators {
            
            /// <summary>
            /// Register default value for weld fields.
            /// </summary>
            /// <remarks>
            /// You need to manually call the "SetDataToDefaultIfUnset" method.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
            public class WeldFieldDefaultValuesAttribute : Attribute {
                
                /// <summary>
                /// Register default value by weld name.
                /// </summary>
                /// <param name="weldName">The name registered by <see cref="WeldFieldsAttribute"/>.</param>
                /// <param name="typeAbove">The default value for the "TypeAbove" property of "BaseWeld".</param>
                /// <param name="typeBelow">The default value for the "TypeBelow" property of "BaseWeld".</param>
                /// <param name="sizeAbove">The default value for the "SizeAbove" property of "BaseWeld".</param>
                /// <param name="sizeBelow">The default value for the "SizeBelow" property of "BaseWeld".</param>
                /// <param name="angleAbove">The default value for the "AngleAbove" property of "BaseWeld".</param>
                /// <param name="angleBelow">The default value for the "AngleBelow" property of "BaseWeld".</param>
                /// <param name="contourAbove">The default value for the "ContourAbove" property of "BaseWeld".</param>
                /// <param name="contourBelow">The default value for the "ContourBelow" property of "BaseWeld".</param>
                /// <param name="finishAbove">The default value for the "FinishAbove" property of "BaseWeld".</param>
                /// <param name="finishBelow">The default value for the "FinishBelow" property of "BaseWeld".</param>
                /// <param name="rootFaceAbove">The default value for the "RootFaceAbove" property of "BaseWeld".</param>
                /// <param name="rootFaceBelow">The default value for the "RootFaceBelow" property of "BaseWeld".</param>
                /// <param name="effectiveThroatAbove">The default value for the "EffectiveThroatAbove" property of "BaseWeld".</param>
                /// <param name="effectiveThroatBelow">The default value for the "EffectiveThroatBelow" property of "BaseWeld".</param>
                /// <param name="rootOpeningAbove">The default value for the "RootOpeningAbove" property of "BaseWeld".</param>
                /// <param name="rootOpeningBelow">The default value for the "RootOpeningBelow" property of "BaseWeld".</param>
                /// <param name="incrementAmountAbove">The default value for the "IncrementAmountAbove" property of "BaseWeld".</param>
                /// <param name="incrementAmountBelow">The default value for the "IncrementAmountBelow" property of "BaseWeld".</param>
                /// <param name="lengthAbove">The default value for the "LengthAbove" property of "BaseWeld".</param>
                /// <param name="lengthBelow">The default value for the "LengthBelow" property of "BaseWeld".</param>
                /// <param name="pitchAbove">The default value for the "PitchAbove" property of "BaseWeld".</param>
                /// <param name="pitchBelow">The default value for the "PitchBelow" property of "BaseWeld".</param>
                /// <param name="around">The default value for the "AroundWeld" property of "BaseWeld".</param>
                /// <param name="shop">The default value for the "ShopWeld" property of "BaseWeld".</param>
                /// <param name="placement">The default value for the "Placement" property of "BaseWeld".</param>
                /// <param name="preparation">The default value for the "Preparation" property of "BaseWeld".</param>
                /// <param name="intermittent">The default value for the "IntermittentType" property of "BaseWeld".</param>
                /// <param name="referenceText">The default value for the "ReferenceText" property of "BaseWeld".</param>
                public WeldFieldDefaultValuesAttribute(
                    string weldName, int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
                    double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
                    int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
                    double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
                    double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
                    int incrementAmountAbove = 0, int incrementAmountBelow = 0,
                    double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
                    int around = 0, int shop = 0, int placement = 0, int preparation = 0,
                    int intermittent = 0, string referenceText = "") {
                    
                }
                
                /// <summary>
                /// Register default value by weld number.
                /// </summary>
                /// <param name="weldNumber">The number registered by <see cref="WeldFieldsAttribute"/>.</param>
                /// <param name="typeAbove">The default value for the "TypeAbove" property of "BaseWeld".</param>
                /// <param name="typeBelow">The default value for the "TypeBelow" property of "BaseWeld".</param>
                /// <param name="sizeAbove">The default value for the "SizeAbove" property of "BaseWeld".</param>
                /// <param name="sizeBelow">The default value for the "SizeBelow" property of "BaseWeld".</param>
                /// <param name="angleAbove">The default value for the "AngleAbove" property of "BaseWeld".</param>
                /// <param name="angleBelow">The default value for the "AngleBelow" property of "BaseWeld".</param>
                /// <param name="contourAbove">The default value for the "ContourAbove" property of "BaseWeld".</param>
                /// <param name="contourBelow">The default value for the "ContourBelow" property of "BaseWeld".</param>
                /// <param name="finishAbove">The default value for the "FinishAbove" property of "BaseWeld".</param>
                /// <param name="finishBelow">The default value for the "FinishBelow" property of "BaseWeld".</param>
                /// <param name="rootFaceAbove">The default value for the "RootFaceAbove" property of "BaseWeld".</param>
                /// <param name="rootFaceBelow">The default value for the "RootFaceBelow" property of "BaseWeld".</param>
                /// <param name="effectiveThroatAbove">The default value for the "EffectiveThroatAbove" property of "BaseWeld".</param>
                /// <param name="effectiveThroatBelow">The default value for the "EffectiveThroatBelow" property of "BaseWeld".</param>
                /// <param name="rootOpeningAbove">The default value for the "RootOpeningAbove" property of "BaseWeld".</param>
                /// <param name="rootOpeningBelow">The default value for the "RootOpeningBelow" property of "BaseWeld".</param>
                /// <param name="incrementAmountAbove">The default value for the "IncrementAmountAbove" property of "BaseWeld".</param>
                /// <param name="incrementAmountBelow">The default value for the "IncrementAmountBelow" property of "BaseWeld".</param>
                /// <param name="lengthAbove">The default value for the "LengthAbove" property of "BaseWeld".</param>
                /// <param name="lengthBelow">The default value for the "LengthBelow" property of "BaseWeld".</param>
                /// <param name="pitchAbove">The default value for the "PitchAbove" property of "BaseWeld".</param>
                /// <param name="pitchBelow">The default value for the "PitchBelow" property of "BaseWeld".</param>
                /// <param name="around">The default value for the "AroundWeld" property of "BaseWeld".</param>
                /// <param name="shop">The default value for the "ShopWeld" property of "BaseWeld".</param>
                /// <param name="placement">The default value for the "Placement" property of "BaseWeld".</param>
                /// <param name="preparation">The default value for the "Preparation" property of "BaseWeld".</param>
                /// <param name="intermittent">The default value for the "IntermittentType" property of "BaseWeld".</param>
                /// <param name="referenceText">The default value for the "ReferenceText" property of "BaseWeld".</param>
                public WeldFieldDefaultValuesAttribute(
                    int weldNumber, int typeAbove = 0, int typeBelow = 0, double sizeAbove = 0.0, double sizeBelow = 0.0,
                    double angleAbove = 0.0, double angleBelow = 0.0, int contourAbove = 0, int contourBelow = 0,
                    int finishAbove = 0, int finishBelow = 0, double rootFaceAbove = 0.0, double rootFaceBelow = 0.0,
                    double effectiveThroatAbove = 0.0, double effectiveThroatBelow = 0.0,
                    double rootOpeningAbove = 0.0, double rootOpeningBelow = 0.0,
                    int incrementAmountAbove = 0, int incrementAmountBelow = 0,
                    double lengthAbove = 0.0, double lengthBelow = 0.0, double pitchAbove = 0.0, double pitchBelow = 0.0,
                    int around = 0, int shop = 0, int placement = 0, int preparation = 0,
                    int intermittent = 0, string referenceText = "") {
                    
                }
            }
        }

        """;

    private const string BoltFieldDefaultValuesAttribute =
        """
        using System;

        namespace Muggle.TsExtensions.CodingHelper.Generators {
            
            /// <summary>
            /// Register default value for bolt fields.
            /// </summary>
            /// <remarks>
            /// You need to manually call the "SetDataToDefaultIfUnset" method.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
            public class BoltFieldDefaultValuesAttribute : Attribute {
                
                /// <summary>
                /// Register default value by bolt name.
                /// </summary>
                /// <param name="boltName">The name registered by <see cref="BoltFieldsAttribute"/>.</param>
                /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
                /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
                /// <param name="distXText">The default value used by the "AddBoltDistX" method of "BoltArray" or "BoltXYList".</param>
                /// <param name="distYText">The default value used by the "AddBoltDistY" method of "BoltArray" or "BoltXYList".</param>
                /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
                /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
                /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
                /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
                /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
                /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
                /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
                /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
                /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
                /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
                /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
                /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
                /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
                /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
                /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
                /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
                /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
                /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
                /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
                /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
                /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
                /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
                public BoltFieldDefaultValuesAttribute(
                    string boltName, double size = 8.0, string standard = "A", string distXText = "", string distYText = "", 
                    int type = 0, int threadInMaterial = 1, double cutLength = 100.0, double extraLength = 0.0, 
                    double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, int hole1 = 1, int hole2 = 1, 
                    int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, double slottedHoleX = 0.0, 
                    double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1, int useNut1 = 1, int useNut2 = 0, 
                    int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {
                    
                }
                
                /// <summary>
                /// Register default value by bolt number.
                /// </summary>
                /// <param name="boltNumber">The number registered by <see cref="BoltFieldsAttribute"/>.</param>
                /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
                /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
                /// <param name="distXText">The default value used by the "AddBoltDistX" method of "BoltArray" or "BoltXYList".</param>
                /// <param name="distYText">The default value used by the "AddBoltDistY" method of "BoltArray" or "BoltXYList".</param>
                /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
                /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
                /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
                /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
                /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
                /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
                /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
                /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
                /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
                /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
                /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
                /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
                /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
                /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
                /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
                /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
                /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
                /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
                /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
                /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
                /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
                /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
                public BoltFieldDefaultValuesAttribute(
                    int boltNumber, double size = 8.0, string standard = "A", string distXText = "", string distYText = "", 
                    int type = 0, int threadInMaterial = 1, double cutLength = 100.0, double extraLength = 0.0, 
                    double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, int hole1 = 1, int hole2 = 1, 
                    int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, double slottedHoleX = 0.0, 
                    double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1, int useNut1 = 1, int useNut2 = 0, 
                    int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {
                    
                }
            }
        }

        """;

    private const string BoltCircleFieldDefaultValuesAttribute =
        """
        using System;

        namespace Muggle.TsExtensions.CodingHelper.Generators {
            
            /// <summary>
            /// Register default value for bolt circle fields.
            /// </summary>
            /// <remarks>
            /// You need to manually call the "SetDataToDefaultIfUnset" method.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
            public class BoltCircleFieldDefaultValuesAttribute : Attribute {
                
                /// <summary>
                /// Register default value by bolt circle name.
                /// </summary>
                /// <param name="boltCircleName">The name registered by <see cref="BoltCircleFieldsAttribute"/>.</param>
                /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
                /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
                /// <param name="numberOfBolts">The default value for the "NumberOfBolts" property of "BoltCircle".</param>
                /// <param name="diameter">The default value for the "Diameter" property of "BoltCircle".</param>
                /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
                /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
                /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
                /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
                /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
                /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
                /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
                /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
                /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
                /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
                /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
                /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
                /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
                /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
                /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
                /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
                /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
                /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
                /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
                /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
                /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
                /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
                public BoltCircleFieldDefaultValuesAttribute(
                    string boltCircleName, double size = 8.0, string standard = "A", int numberOfBolts = 6, 
                    double diameter = 100.0, int type = 0, int threadInMaterial = 1, double cutLength = 100.0, 
                    double extraLength = 0.0, double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, 
                    int hole1 = 1, int hole2 = 1, int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, 
                    double slottedHoleX = 0.0, double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1, 
                    int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {
                    
                }
                
                /// <summary>
                /// Register default value by bolt circle number.
                /// </summary>
                /// <param name="boltCircleNumber">The number registered by <see cref="BoltCircleFieldsAttribute"/>.</param>
                /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
                /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
                /// <param name="numberOfBolts">The default value for the "NumberOfBolts" property of "BoltCircle".</param>
                /// <param name="diameter">The default value for the "Diameter" property of "BoltCircle".</param>
                /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
                /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
                /// <param name="cutLength">The default value for the "CutLength" property of "BoltGroup".</param>
                /// <param name="extraLength">The default value for the "ExtraLength" property of "BoltGroup".</param>
                /// <param name="tolerance">The default value for the "Tolerance" property of "BoltGroup".</param>
                /// <param name="plainType">The default value for the "PlainHoleType" property of "BoltGroup".</param>
                /// <param name="blindHoleDepth">The default value for the "BlindHoleDepth" property of "BoltGroup".</param>
                /// <param name="hole1">The default value for the "Hole1" property of "BoltGroup".</param>
                /// <param name="hole2">The default value for the "Hole2" property of "BoltGroup".</param>
                /// <param name="hole3">The default value for the "Hole3" property of "BoltGroup".</param>
                /// <param name="hole4">The default value for the "Hole4" property of "BoltGroup".</param>
                /// <param name="hole5">The default value for the "Hole5" property of "BoltGroup".</param>
                /// <param name="holeType">The default value for the "HoleType" property of "BoltGroup".</param>
                /// <param name="slottedHoleX">The default value for the "SlottedHoleX" property of "BoltGroup".</param>
                /// <param name="slottedHoleY">The default value for the "SlottedHoleY" property of "BoltGroup".</param>
                /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
                /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
                /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
                /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
                /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
                /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
                /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
                public BoltCircleFieldDefaultValuesAttribute(
                    int boltCircleNumber, double size = 8.0, string standard = "A", int numberOfBolts = 6, 
                    double diameter = 100.0, int type = 0, int threadInMaterial = 1, double cutLength = 100.0, 
                    double extraLength = 0.0, double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, 
                    int hole1 = 1, int hole2 = 1, int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, 
                    double slottedHoleX = 0.0, double slottedHoleY = 0.0, int rotateSlots = 2, int isBolt = 1, 
                    int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, int useWasher3 = 1) {
                    
                }
            }
        }

        """;

    #endregion

    #region Templates

    private const string PartialClassTemplate =
        """
        //  <auto-generated/>{{generatedAt}}

        namespace {{namespace}} {

            {{accessibility}} partial {{typeKind}}class {{className}} {
        {{method}}
            }
        }

        """;

    private const string SetDataToDefaultMethodTemplate =
        """
                
                /// <summary>
                /// Set data (fields) to default value if they are not setted from user interface.
                /// </summary>
                private void SetDataToDefaultIfUnset() {
        {{statements}}
                }
        """;

    private const string PartSetToDefaultStatementsTemplate =
        """
                    
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Profile))
                        {{dataPropertyName.}}{{modelObjectName}}Profile = "{{profileValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Material))
                        {{dataPropertyName.}}{{modelObjectName}}Material = "{{materialValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Name))
                        {{dataPropertyName.}}{{modelObjectName}}Name = "{{nameValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Finish))
                        {{dataPropertyName.}}{{modelObjectName}}Finish = "{{finishValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Class))
                        {{dataPropertyName.}}{{modelObjectName}}Class = {{classValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AssemblyPrefix))
                        {{dataPropertyName.}}{{modelObjectName}}AssemblyPrefix = "{{assemblyPrefixValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AssemblyStartNumber))
                        {{dataPropertyName.}}{{modelObjectName}}AssemblyStartNumber = {{assemblyStartNumberValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PartPrefix))
                        {{dataPropertyName.}}{{modelObjectName}}PartPrefix = "{{partPrefixValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PartStartNumber))
                        {{dataPropertyName.}}{{modelObjectName}}PartStartNumber = {{partStartNumberValue}};
        """;

    private const string PlateSetToDefaultStatementsTemplate =
        """
                    
                    if ({{dataPropertyName.}}{{modelObjectName}}Thickness <= 0)
                        {{dataPropertyName.}}{{modelObjectName}}Thickness = {{thicknessValue}};
                    if ({{dataPropertyName.}}{{modelObjectName}}Breadth <= 0)
                        {{dataPropertyName.}}{{modelObjectName}}Breadth = {{breadthValue}};
                    if ({{dataPropertyName.}}{{modelObjectName}}Height <= 0)
                        {{dataPropertyName.}}{{modelObjectName}}Height = {{heightValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Material))
                        {{dataPropertyName.}}{{modelObjectName}}Material = "{{materialValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Name))
                        {{dataPropertyName.}}{{modelObjectName}}Name = "{{nameValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Finish))
                        {{dataPropertyName.}}{{modelObjectName}}Finish = "{{finishValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Class))
                        {{dataPropertyName.}}{{modelObjectName}}Class = {{classValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AssemblyPrefix))
                        {{dataPropertyName.}}{{modelObjectName}}AssemblyPrefix = "{{assemblyPrefixValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AssemblyStartNumber))
                        {{dataPropertyName.}}{{modelObjectName}}AssemblyStartNumber = {{assemblyStartNumberValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PartPrefix))
                        {{dataPropertyName.}}{{modelObjectName}}PartPrefix = "{{partPrefixValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PartStartNumber))
                        {{dataPropertyName.}}{{modelObjectName}}PartStartNumber = {{partStartNumberValue}};
        """;

    private const string WeldSetToDefaultStatementsTemplate =
        """
                    
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}TypeAbove))
                        {{dataPropertyName.}}{{modelObjectName}}TypeAbove = {{typeAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}TypeBelow))
                        {{dataPropertyName.}}{{modelObjectName}}TypeBelow = {{typeBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SizeAbove))
                        {{dataPropertyName.}}{{modelObjectName}}SizeAbove = {{sizeAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SizeBelow))
                        {{dataPropertyName.}}{{modelObjectName}}SizeBelow = {{sizeBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AngleAbove))
                        {{dataPropertyName.}}{{modelObjectName}}AngleAbove = {{angleAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}AngleBelow))
                        {{dataPropertyName.}}{{modelObjectName}}AngleBelow = {{angleBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ContourAbove))
                        {{dataPropertyName.}}{{modelObjectName}}ContourAbove = {{contourAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ContourBelow))
                        {{dataPropertyName.}}{{modelObjectName}}ContourBelow = {{contourBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}FinishAbove))
                        {{dataPropertyName.}}{{modelObjectName}}FinishAbove = {{finishAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}FinishBelow))
                        {{dataPropertyName.}}{{modelObjectName}}FinishBelow = {{finishBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RootFaceAbove))
                        {{dataPropertyName.}}{{modelObjectName}}RootFaceAbove = {{rootFaceAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RootFaceBelow))
                        {{dataPropertyName.}}{{modelObjectName}}RootFaceBelow = {{rootFaceBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}EffectiveThroatAbove))
                        {{dataPropertyName.}}{{modelObjectName}}EffectiveThroatAbove = {{effectiveThroatAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}EffectiveThroatBelow))
                        {{dataPropertyName.}}{{modelObjectName}}EffectiveThroatBelow = {{effectiveThroatBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RootOpeningAbove))
                        {{dataPropertyName.}}{{modelObjectName}}RootOpeningAbove = {{rootOpeningAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RootOpeningBelow))
                        {{dataPropertyName.}}{{modelObjectName}}RootOpeningBelow = {{rootOpeningBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}IncrementAmountAbove))
                        {{dataPropertyName.}}{{modelObjectName}}IncrementAmountAbove = {{incrementAmountAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}IncrementAmountBelow))
                        {{dataPropertyName.}}{{modelObjectName}}IncrementAmountBelow = {{incrementAmountBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}LengthAbove))
                        {{dataPropertyName.}}{{modelObjectName}}LengthAbove = {{lengthAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}LengthBelow))
                        {{dataPropertyName.}}{{modelObjectName}}LengthBelow = {{lengthBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PitchAbove))
                        {{dataPropertyName.}}{{modelObjectName}}PitchAbove = {{pitchAboveValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PitchBelow))
                        {{dataPropertyName.}}{{modelObjectName}}PitchBelow = {{pitchBelowValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Around))
                        {{dataPropertyName.}}{{modelObjectName}}Around = {{aroundValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Shop))
                        {{dataPropertyName.}}{{modelObjectName}}Shop = {{shopValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Placement))
                        {{dataPropertyName.}}{{modelObjectName}}Placement = {{placementValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Preparation))
                        {{dataPropertyName.}}{{modelObjectName}}Preparation = {{preparationValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Intermittent))
                        {{dataPropertyName.}}{{modelObjectName}}Intermittent = {{intermittentValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ReferenceText))
                        {{dataPropertyName.}}{{modelObjectName}}ReferenceText = "{{referenceTextValue}}";
        """;

    private const string BoltSetToDefaultStatementsTemplate =
        """
                    
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Size))
                        {{dataPropertyName.}}{{modelObjectName}}Size = {{sizeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Standard))
                        {{dataPropertyName.}}{{modelObjectName}}Standard = "{{standardValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}DistXText))
                        {{dataPropertyName.}}{{modelObjectName}}DistXText = "{{distXTextValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}DistYText))
                        {{dataPropertyName.}}{{modelObjectName}}DistYText = "{{distYTextValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Type))
                        {{dataPropertyName.}}{{modelObjectName}}Type = {{typeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ThreadInMaterial))
                        {{dataPropertyName.}}{{modelObjectName}}ThreadInMaterial = {{threadInMaterialValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}CutLength))
                        {{dataPropertyName.}}{{modelObjectName}}CutLength = {{cutLengthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ExtraLength))
                        {{dataPropertyName.}}{{modelObjectName}}ExtraLength = {{extraLengthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Tolerance))
                        {{dataPropertyName.}}{{modelObjectName}}Tolerance = {{toleranceValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PlainType))
                        {{dataPropertyName.}}{{modelObjectName}}PlainType = {{plainTypeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}BlindHoleDepth))
                        {{dataPropertyName.}}{{modelObjectName}}BlindHoleDepth = {{blindHoleDepthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole1))
                        {{dataPropertyName.}}{{modelObjectName}}Hole1 = {{hole1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole2))
                        {{dataPropertyName.}}{{modelObjectName}}Hole2 = {{hole2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole3))
                        {{dataPropertyName.}}{{modelObjectName}}Hole3 = {{hole3Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole4))
                        {{dataPropertyName.}}{{modelObjectName}}Hole4 = {{hole4Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole5))
                        {{dataPropertyName.}}{{modelObjectName}}Hole5 = {{hole5Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}HoleType))
                        {{dataPropertyName.}}{{modelObjectName}}HoleType = {{holeTypeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SlottedHoleX))
                        {{dataPropertyName.}}{{modelObjectName}}SlottedHoleX = {{slottedHoleXValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SlottedHoleY))
                        {{dataPropertyName.}}{{modelObjectName}}SlottedHoleY = {{slottedHoleYValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RotateSlots))
                        {{dataPropertyName.}}{{modelObjectName}}RotateSlots = {{rotateSlotsValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}IsBolt))
                        {{dataPropertyName.}}{{modelObjectName}}IsBolt = {{isBoltValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseNut1))
                        {{dataPropertyName.}}{{modelObjectName}}UseNut1 = {{useNut1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseNut2))
                        {{dataPropertyName.}}{{modelObjectName}}UseNut2 = {{useNut2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher1))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher1 = {{useWasher1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher2))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher2 = {{useWasher2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher3))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher3 = {{useWasher3Value}};
        """;

    private const string BoltCircleSetToDefaultStatementsTemplate =
        """
                    
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Size))
                        {{dataPropertyName.}}{{modelObjectName}}Size = {{sizeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Standard))
                        {{dataPropertyName.}}{{modelObjectName}}Standard = "{{standardValue}}";
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}NumberOfBolts))
                        {{dataPropertyName.}}{{modelObjectName}}NumberOfBolts = {{numberOfBoltsValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Diameter))
                        {{dataPropertyName.}}{{modelObjectName}}Diameter = {{diameterValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Type))
                        {{dataPropertyName.}}{{modelObjectName}}Type = {{typeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ThreadInMaterial))
                        {{dataPropertyName.}}{{modelObjectName}}ThreadInMaterial = {{threadInMaterialValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}CutLength))
                        {{dataPropertyName.}}{{modelObjectName}}CutLength = {{cutLengthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}ExtraLength))
                        {{dataPropertyName.}}{{modelObjectName}}ExtraLength = {{extraLengthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Tolerance))
                        {{dataPropertyName.}}{{modelObjectName}}Tolerance = {{toleranceValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}PlainType))
                        {{dataPropertyName.}}{{modelObjectName}}PlainType = {{plainTypeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}BlindHoleDepth))
                        {{dataPropertyName.}}{{modelObjectName}}BlindHoleDepth = {{blindHoleDepthValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole1))
                        {{dataPropertyName.}}{{modelObjectName}}Hole1 = {{hole1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole2))
                        {{dataPropertyName.}}{{modelObjectName}}Hole2 = {{hole2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole3))
                        {{dataPropertyName.}}{{modelObjectName}}Hole3 = {{hole3Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole4))
                        {{dataPropertyName.}}{{modelObjectName}}Hole4 = {{hole4Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}Hole5))
                        {{dataPropertyName.}}{{modelObjectName}}Hole5 = {{hole5Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}HoleType))
                        {{dataPropertyName.}}{{modelObjectName}}HoleType = {{holeTypeValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SlottedHoleX))
                        {{dataPropertyName.}}{{modelObjectName}}SlottedHoleX = {{slottedHoleXValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}SlottedHoleY))
                        {{dataPropertyName.}}{{modelObjectName}}SlottedHoleY = {{slottedHoleYValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}RotateSlots))
                        {{dataPropertyName.}}{{modelObjectName}}RotateSlots = {{rotateSlotsValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}IsBolt))
                        {{dataPropertyName.}}{{modelObjectName}}IsBolt = {{isBoltValue}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseNut1))
                        {{dataPropertyName.}}{{modelObjectName}}UseNut1 = {{useNut1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseNut2))
                        {{dataPropertyName.}}{{modelObjectName}}UseNut2 = {{useNut2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher1))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher1 = {{useWasher1Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher2))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher2 = {{useWasher2Value}};
                    if (IsDefaultValue({{dataPropertyName.}}{{modelObjectName}}UseWasher3))
                        {{dataPropertyName.}}{{modelObjectName}}UseWasher3 = {{useWasher3Value}};
        """;

    #endregion

    public PluginFieldDefaultValuesGenerator() {
        var constTexts = new[] {
            PartFieldDefaultValuesAttribute, PlateFieldDefaultValuesAttribute, WeldFieldDefaultValuesAttribute,
            BoltFieldDefaultValuesAttribute, BoltCircleFieldDefaultValuesAttribute,
        };

        var dict = new Dictionary<string, ReadOnlyDictionary<string, string>>();
        foreach (var text in constTexts) {
            var defaultValues =
                GetDefaultValuesFromConst(CSharpSyntaxTree.ParseText(text), out string attributeName);
            dict.Add(attributeName, new ReadOnlyDictionary<string, string>(defaultValues));
        }

        DefaultValues = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dict);
    }

    private static Dictionary<string, string> GetDefaultValuesFromConst(
        SyntaxTree syntaxTree, out string attributeName) {
        attributeName = syntaxTree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>().First()
            .Identifier.ValueText;

        var dict = new Dictionary<string, string>();

        var paramListSyntaxes = syntaxTree.GetRoot().DescendantNodes().OfType<ParameterListSyntax>().First();
        for (int i = 1; i < paramListSyntaxes.Parameters.Count; i++) {
            var parameterSyntax = paramListSyntaxes.Parameters[i];
            var parameterId = parameterSyntax.Identifier.ValueText;
            var parameterValue = parameterSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault()?
                .Token.ValueText;
            dict.Add(parameterId, parameterValue);
        }

        return dict;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource("PartFieldDefaultValuesAttribute.g.cs",
                SourceText.From(PartFieldDefaultValuesAttribute, Encoding.UTF8));
            ctx.AddSource("PlateFieldDefaultValuesAttribute.g.cs",
                SourceText.From(PlateFieldDefaultValuesAttribute, Encoding.UTF8));
            ctx.AddSource("WeldFieldDefaultValuesAttribute.g.cs",
                SourceText.From(WeldFieldDefaultValuesAttribute, Encoding.UTF8));
            ctx.AddSource("BoltFieldDefaultValuesAttribute.g.cs",
                SourceText.From(BoltFieldDefaultValuesAttribute, Encoding.UTF8));
            ctx.AddSource("BoltCircleFieldDefaultValuesAttribute.g.cs",
                SourceText.From(BoltCircleFieldDefaultValuesAttribute, Encoding.UTF8));
        });

        var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform)
            .Where(x => x != default);

        context.RegisterSourceOutput(provider, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) {
        if (token.IsCancellationRequested) return false;

        if (node is not ClassDeclarationSyntax classDeclarationSyntax ||
            !classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) return false;

        if (classDeclarationSyntax.AttributeLists.Count > 0) return true;

        return classDeclarationSyntax.Members.Any(member =>
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

        IEnumerable<AttributeSyntax> attributeSyntaxes = null;
        IEnumerable<AttributeSyntax> fieldsFromAttSyntax = null;
        AttributeTargets appliedTarget = default;
        string targetMemberName = null;
        ITypeSymbol dataType = null;
        var placeCnt = 0;

        if (TryGetSpecificAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                ConcernedAttributes, ref attributeSyntaxes)) {
            //  When applied on class, the FieldsFromAttribute must also be applied
            if (!TryGetSpecificAttributes(classDeclarationSyntax.AttributeLists, semanticModel,
                    ["Muggle.TsExtensions.CodingHelper.Generators.FieldsFromAttribute"],
                    ref fieldsFromAttSyntax)) {
                return default;
            }

            placeCnt++;
            appliedTarget = AttributeTargets.Class;

            var expression = (TypeOfExpressionSyntax)fieldsFromAttSyntax.Single().ArgumentList!.Arguments[0].Expression;
            dataType = semanticModel.GetTypeInfo(expression.Type).Type;
        }

        _ = classDeclarationSyntax.Members.Where(member => {
            if (member is FieldDeclarationSyntax fieldSyntax &&
                TryGetSpecificAttributes(fieldSyntax.AttributeLists, semanticModel,
                    ConcernedAttributes, ref attributeSyntaxes)) {
                placeCnt++;
                appliedTarget = AttributeTargets.Field;

                targetMemberName = fieldSyntax.Declaration.Variables[0].Identifier.ValueText;
                dataType = semanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type;

                return true;
            }

            if (member is PropertyDeclarationSyntax propertySyntax &&
                TryGetSpecificAttributes(propertySyntax.AttributeLists, semanticModel,
                    ConcernedAttributes, ref attributeSyntaxes)) {
                placeCnt++;
                appliedTarget = AttributeTargets.Property;

                targetMemberName = propertySyntax.Identifier.ValueText;
                dataType = semanticModel.GetTypeInfo(propertySyntax.Type).Type;

                return true;
            }

            return false;

        }).ToArray();

        // no attribute or applied on over one place
        if (placeCnt != 1) return default;

        var dataTypePluginDataFieldsAttributeArguments = dataType.GetAttributes().Where(a =>
                PluginDataFieldsGenerator.ConcernedAttributes.Contains(a.AttributeClass?.ToDisplayString()))
            .Select(a => {
                var attName = a.AttributeClass!.ToDisplayString();
                attName = attName.Substring(attName.LastIndexOf('.') + 1);
                var attArgs = a.ConstructorArguments
                    .SelectMany(argArray => argArray.Values.Select(arg => arg.Value!.ToString()))
                    .OrderBy(s => s).ToArray();
                return (attName, attArgs);
            }).OrderBy(t => t.attName).ToArray();

        var attDict = new ArgumentsDictionary<DefaultValueDictionary>();

        foreach (var attSyntax in attributeSyntaxes) {
            var attTypeInfo = semanticModel.GetTypeInfo(attSyntax);
            var attQualifiedName = attTypeInfo.Type!.ToDisplayString();
            var attName = attQualifiedName.Substring(attQualifiedName.LastIndexOf('.') + 1);

            if (!attDict.TryGetValue(attName, out DefaultValueDictionary elementDict)) {
                elementDict = new DefaultValueDictionary();
                attDict.Add(attName, elementDict);
            }

            var argSyntaxes = attSyntax.ArgumentList?.Arguments;
            if (argSyntaxes == null) continue;

            var valueDict = new Dictionary<string, string>();

            string nameOrNumber = string.Empty;
            var index = -1;
            foreach (var argSyntax in argSyntaxes) {
                index++;

                var paramName = argSyntax.NameColon?.Name.Identifier.ValueText;
                var paramValue = ((LiteralExpressionSyntax)argSyntax.Expression).Token.ValueText;

                if (paramName == null && index == 0 ||
                    paramName != null &&
                    Regex.Match(paramName, "(part|plate|weld|bolt|boltCircle)N(ame|umber)").Success) {
                    nameOrNumber = paramValue;
                    continue;
                }

                paramName ??= PresetValues[attName].ElementAt(index - 1).Key;
                valueDict.Add(paramName, paramValue);
            }

            if (string.IsNullOrEmpty(nameOrNumber)) continue;
            if (elementDict.ContainsKey(nameOrNumber)) continue;

            //  data doesnt have these fields
            if (!dataTypePluginDataFieldsAttributeArguments.Any(t =>
                    t.attName.Substring(0, t.attName.Length - 10) == attName.Substring(0, attName.Length - 22) &&
                    t.attArgs.Contains(nameOrNumber))) {
                continue;
            }

            elementDict.Add(nameOrNumber, valueDict);
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
        if (info == default) return;

        var setToDefaultMethod = GenerateSetDataToDefaultMethod(info);

        var result = PartialClassTemplate
#if DEBUG
            .Replace("{{generatedAt}}", $" at {DateTime.Now}")
#else
            .Replace("{{generatedAt}}", string.Empty)
#endif
            .Replace("{{namespace}}", info.ClassInfo.NameSpace)
            .Replace("{{accessibility}}", info.ClassInfo.Accessibility.ToString().ToLower())
            .Replace("{{typeKind}}", info.ClassInfo.IsRecord ? "record" : string.Empty)
            .Replace("{{className}}", info.ClassInfo.Name)
            .Replace("{{method}}", setToDefaultMethod);

        context.AddSource($"{info.ClassInfo.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private string GenerateSetDataToDefaultMethod(PluginFieldDefaultValuesInfo info) {
        var builder = new StringBuilder();

        foreach (var kvp in info.Arguments) {
            var attName = kvp.Key;

            var template = attName switch {
                "PartFieldDefaultValuesAttribute" => PartSetToDefaultStatementsTemplate,
                "PlateFieldDefaultValuesAttribute" => PlateSetToDefaultStatementsTemplate,
                "WeldFieldDefaultValuesAttribute" => WeldSetToDefaultStatementsTemplate,
                "BoltFieldDefaultValuesAttribute" => BoltSetToDefaultStatementsTemplate,
                "BoltCircleFieldDefaultValuesAttribute" => BoltCircleSetToDefaultStatementsTemplate,
                _ => string.Empty
            };

            template = template.Replace("{{dataPropertyName.}}",
                info.TargetType == AttributeTargets.Class ? string.Empty : info.TargetMemberName + '.');

            var modelObjectName = attName.Substring(0, attName.Length - 27);
            modelObjectName = info.TargetType == AttributeTargets.Class
                ? ToPrivateFieldNameStyle(modelObjectName)
                : modelObjectName;

            foreach (var kvp2 in kvp.Value) {
                var nameOrNumber = kvp2.Key;

                var statements = template.Replace("{{modelObjectName}}", modelObjectName + nameOrNumber);

                var paramArgPair = kvp2.Value;
                foreach (var kvp3 in DefaultValues[attName]) {
                    var paramName = kvp3.Key;
                    if (!paramArgPair.TryGetValue(paramName, out var argument)) {
                        argument = kvp3.Value;
                    }

                    statements = statements.Replace($"{{{{{paramName}Value}}}}", argument);
                }

                builder.AppendLine(statements);
            }
        }

        return SetDataToDefaultMethodTemplate.Replace("{{statements}}", builder.ToString());
    }
}