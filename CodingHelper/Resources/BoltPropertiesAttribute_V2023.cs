using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register the bolt properties (with default values) that need to be generated for the applied class.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BoltPropertiesAttribute : Attribute {
        public string Id { get; set; }
        public double Size { get; set; }
        public string Standard { get; set; }
        public string DistX { get; set; }
        public string DistY { get; set; }
        public int Type { get; set; }
        public double ThreadInMaterial { get; set; }
        public double Length { get; set; }
        public double CutLength { get; set; }
        public double ExtraLength { get; set; }
        public double Tolerance { get; set; }
        public int PlainType { get; set; }
        public double BlindHoleDepth { get; set; }
        public int Hole1 { get; set; }
        public int Hole2 { get; set; }
        public int Hole3 { get; set; }
        public int Hole4 { get; set; }
        public int Hole5 { get; set; }
        public int HoleType { get; set; }
        public double SlottedHoleX { get; set; }
        public double SlottedHoleY { get; set; }
        public double SlotOffsetX { get; set; }
        public double SlotOffsetY { get; set; }
        public int RotateSlots { get; set; }
        public int IsBolt { get; set; }
        public int UseNut1 { get; set; }
        public int UseNut2 { get; set; }
        public int UseWasher1 { get; set; }
        public int UseWasher2 { get; set; }
        public int UseWasher3 { get; set; }

        /// <summary>
        /// Register bolt properties with preset default values using the given id.
        /// </summary>
        /// <remarks>The preset default values is: Size = 8.0, Standard = "A", DistX = "", DistY = "",
        /// Type = 0, ThreadInMaterial = 1, Length = 40.0, CutLength = 100.0, ExtraLength = 0.0, Tolerance = 2.0,
        /// PlainType = 0, BlindHoleDepth = 0.0, Hole1 = 1, Hole2 = 1, Hole3 = 0, Hole4 = 0, Hole5 = 0, HoleType = 0,
        /// SlottedHoleX = 0.0, SlottedHoleY = 0.0, SlotOffsetX = 0.0, SlotOffsetY = 0.0, RotateSlots = 2,
        /// IsBolt = 1, UseNut1 = 1, UseNut2 = 0, UseWasher1 = 0, UseWasher2 = 0, UseWasher3 = 1.</remarks>
        /// <param name="id">The bolt id.</param>
        public BoltPropertiesAttribute(uint id) {
            Id = id.ToString();
            Size = 8.0;
            Standard = "A";
            DistX = "";
            DistY = "";
            Type = 0;
            ThreadInMaterial = 1;
            Length = 40.0;
            CutLength = 100.0;
            ExtraLength = 0.0;
            Tolerance = 2.0;
            PlainType = 0;
            BlindHoleDepth = 0.0;
            Hole1 = 1;
            Hole2 = 1;
            Hole3 = 0;
            Hole4 = 0;
            Hole5 = 0;
            HoleType = 0;
            SlottedHoleX = 0.0;
            SlottedHoleY = 0.0;
            SlotOffsetX = 0.0;
            SlotOffsetY = 0.0;
            RotateSlots = 2;
            IsBolt = 1;
            UseNut1 = 1;
            UseNut2 = 0;
            UseWasher1 = 0;
            UseWasher2 = 0;
            UseWasher3 = 1;
        }

        /// <summary>
        /// Register bolt properties with preset default values using the given id.
        /// </summary>
        /// <remarks>The preset default values is: Size = 8.0, Standard = "A", DistX = "", DistY = "",
        /// Type = 0, ThreadInMaterial = 1, Length = 40.0, CutLength = 100.0, ExtraLength = 0.0, Tolerance = 2.0,
        /// PlainType = 0, BlindHoleDepth = 0.0, Hole1 = 1, Hole2 = 1, Hole3 = 0, Hole4 = 0, Hole5 = 0, HoleType = 0,
        /// SlottedHoleX = 0.0, SlottedHoleY = 0.0, SlotOffsetX = 0.0, SlotOffset = 0.0, RotateSlots = 2,
        /// IsBolt = 1, UseNut1 = 1, UseNut2 = 0, UseWasher1 = 0, UseWasher2 = 0, UseWasher3 = 1.</remarks>
        /// <param name="id">The bolt id.</param>
        public BoltPropertiesAttribute(string id) {
            Id = id;
            Size = 8.0;
            Standard = "A";
            DistX = "";
            DistY = "";
            Type = 0;
            ThreadInMaterial = 1;
            Length = 40.0;
            CutLength = 100.0;
            ExtraLength = 0.0;
            Tolerance = 2.0;
            PlainType = 0;
            BlindHoleDepth = 0.0;
            Hole1 = 1;
            Hole2 = 1;
            Hole3 = 0;
            Hole4 = 0;
            Hole5 = 0;
            HoleType = 0;
            SlottedHoleX = 0.0;
            SlottedHoleY = 0.0;
            SlotOffsetX = 0.0;
            SlotOffsetY = 0.0;
            RotateSlots = 2;
            IsBolt = 1;
            UseNut1 = 1;
            UseNut2 = 0;
            UseWasher1 = 0;
            UseWasher2 = 0;
            UseWasher3 = 1;
        }
        
        /// <summary>
        /// Register bolt properties with default values using the given id.
        /// </summary>
        /// <param name="id">The bolt id.</param>
        /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
        /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
        /// <param name="distX">The default value used by the "AddBoltDistX" method of "BoltArray" or "BoltXYList".</param>
        /// <param name="distY">The default value used by the "AddBoltDistY" method of "BoltArray" or "BoltXYList".</param>
        /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
        /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
        /// <param name="length">The default value for the "Length" property of "BoltGroup".</param>
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
        /// <param name="slotOffsetX">The default value for the "SlotOffsetX" property of "BoltGroup".</param>
        /// <param name="slotOffsetY">The default value for the "SlotOffsetY" property of "BoltGroup".</param>
        /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
        /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
        /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
        /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
        /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
        /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
        /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
        public BoltPropertiesAttribute(uint id,
            double size = 8.0, string standard = "A", string distX = "", string distY = "", int type = 0,
            int threadInMaterial = 1, double length = 40.0, double cutLength = 100.0, double extraLength = 0.0, 
            double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, int hole1 = 1, int hole2 = 1, 
            int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, double slottedHoleX = 0.0, 
            double slottedHoleY = 0.0, double slotOffsetX = 0.0, double slotOffsetY = 0.0, int rotateSlots = 2, 
            int isBolt = 1, int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, 
            int useWasher3 = 1) {
            
            Id = id.ToString();
            Size = size;
            Standard = standard;
            DistX = distX;
            DistY = distY;
            Type = type;
            ThreadInMaterial = threadInMaterial;
            Length = length;
            CutLength = cutLength;
            ExtraLength = extraLength;
            Tolerance = tolerance;
            PlainType = plainType;
            BlindHoleDepth = blindHoleDepth;
            Hole1 = hole1;
            Hole2 = hole2;
            Hole3 = hole3;
            Hole4 = hole4;
            Hole5 = hole5;
            HoleType = holeType;
            SlottedHoleX = slottedHoleX;
            SlottedHoleY = slottedHoleY;
            SlotOffsetX = slotOffsetX;
            SlotOffsetY = slotOffsetY;
            RotateSlots = rotateSlots;
            IsBolt = isBolt;
            UseNut1 = useNut1;
            UseNut2 = useNut2;
            UseWasher1 = useWasher1;
            UseWasher2 = useWasher2;
            UseWasher3 = useWasher3;
        }
        
        /// <summary>
        /// Register bolt properties with default values using the given id.
        /// </summary>
        /// <param name="id">The bolt id.</param>
        /// <param name="size">The default value for the "BoltSize" property of "BoltGroup".</param>
        /// <param name="standard">The default value for the "BoltStandard" property of "BoltGroup".</param>
        /// <param name="distX">The default value used by the "AddBoltDistX" method of "BoltArray" or "BoltXYList".</param>
        /// <param name="distY">The default value used by the "AddBoltDistY" method of "BoltArray" or "BoltXYList".</param>
        /// <param name="type">The default value for the "BoltType" property of "BoltGroup".</param>
        /// <param name="threadInMaterial">The default value for the "ThreadInMaterial" property of "BoltGroup".</param>
        /// <param name="length">The default value for the "Length" property of "BoltGroup".</param>
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
        /// <param name="slotOffsetX">The default value for the "SlotOffsetX" property of "BoltGroup".</param>
        /// <param name="slotOffsetY">The default value for the "SlotOffsetY" property of "BoltGroup".</param>
        /// <param name="rotateSlots">The default value for the "RotateSlots" property of "BoltGroup".</param>
        /// <param name="isBolt">The default value for the "Bolt" property of "BoltGroup".</param>
        /// <param name="useNut1">The default value for the "Nut1" property of "BoltGroup".</param>
        /// <param name="useNut2">The default value for the "Nut2" property of "BoltGroup".</param>
        /// <param name="useWasher1">The default value for the "Washer1" property of "BoltGroup".</param>
        /// <param name="useWasher2">The default value for the "Washer2" property of "BoltGroup".</param>
        /// <param name="useWasher3">The default value for the "Washer3" property of "BoltGroup".</param>
        public BoltPropertiesAttribute(string id,
            double size = 8.0, string standard = "A", string distX = "", string distY = "", int type = 0,
            int threadInMaterial = 1, double length = 40.0, double cutLength = 100.0, double extraLength = 0.0, 
            double tolerance = 2.0, int plainType = 0, double blindHoleDepth = 0.0, int hole1 = 1, int hole2 = 1, 
            int hole3 = 0, int hole4 = 0, int hole5 = 0, int holeType = 0, double slottedHoleX = 0.0, 
            double slottedHoleY = 0.0, double slotOffsetX = 0.0, double slotOffsetY = 0.0, int rotateSlots = 2,
            int isBolt = 1, int useNut1 = 1, int useNut2 = 0, int useWasher1 = 0, int useWasher2 = 0, 
            int useWasher3 = 1) {
            
            Id = id;
            Size = size;
            Standard = standard;
            DistX = distX;
            DistY = distY;
            Type = type;
            ThreadInMaterial = threadInMaterial;
            Length = length;
            CutLength = cutLength;
            ExtraLength = extraLength;
            Tolerance = tolerance;
            PlainType = plainType;
            BlindHoleDepth = blindHoleDepth;
            Hole1 = hole1;
            Hole2 = hole2;
            Hole3 = hole3;
            Hole4 = hole4;
            Hole5 = hole5;
            HoleType = holeType;
            SlottedHoleX = slottedHoleX;
            SlottedHoleY = slottedHoleY;
            SlotOffsetX = slotOffsetX;
            SlotOffsetY = slotOffsetY;
            RotateSlots = rotateSlots;
            IsBolt = isBolt;
            UseNut1 = useNut1;
            UseNut2 = useNut2;
            UseWasher1 = useWasher1;
            UseWasher2 = useWasher2;
            UseWasher3 = useWasher3;
        }
        
    }
    
}