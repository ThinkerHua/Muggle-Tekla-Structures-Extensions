namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register bolt data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class BoltAttribute : System.Attribute {
        
        public string Id { get; set; }
        
        /// <summary>
        /// 0 - means this bolt is a BoltArray<br/>
        /// 1 - means this bolt is a BoltCircle<br/>
        /// 2 - means this bolt is a BoltXYList<br/>
        /// </summary>
        public int Shape { get; set; }
        
        public string Standard { get; set; }
        
        public double Size { get; set; }
        
        /// <summary>
        /// Only used when <see cref="Shape"/> is 0 or 2.
        /// </summary>
        public string DistX { get; set; }
        
        /// <summary>
        /// Only used when <see cref="Shape"/> is 0 or 2.
        /// </summary>
        public string DistY { get; set; }
        
        /// <summary>
        /// Only used when <see cref="Shape"/> is 1.
        /// </summary>
        public int NumberOfBolts { get; set; }
        
        /// <summary>
        /// Only used when <see cref="Shape"/> is 1.
        /// </summary>
        public double Diameter { get; set; }
        
        public int Type { get; set; }
        
        public double ThreadInMaterial { get; set; }
        
        public double Length { get; set; }
        
        public double CutLength { get; set; }
        
        public double ExtraLength { get; set; }
        
        public double Tolerance { get; set; }
        
        public int PlainType { get; set; }
        
        public double BlindHoleDepth { get; set; }
        
        public int HoleType { get; set; }
        
        public double SlottedHoleX { get; set; }
        
        public double SlottedHoleY { get; set; }
        
        public int RotateSlots { get; set; }
        
        public int Hole1 { get; set; }
        
        public int Hole2 { get; set; }
        
        public int Hole3 { get; set; }
        
        public int Hole4 { get; set; }
        
        public int Hole5 { get; set; }
        
        public int IsBolt { get; set; }
        
        public int UseNut1 { get; set; }
        
        public int UseNut2 { get; set; }
        
        public int UseWasher1 { get; set; }
        
        public int UseWasher2 { get; set; }
        
        public int UseWasher3 { get; set; }

        /// <summary>
        /// Register bolt data by the given id.
        /// </summary>
        /// <remarks>The default values is: Shape = 0, Standard = "A", Size = 8.0, DistX = "0.0", DistY = "0.0",
        /// NumberOfBolts = 6, Diameter = 100.0, Type = 0, ThreadInMaterial = 1, Length = 40.0, CutLength = 100.0,
        /// ExtraLength = 0.0, Tolerance = 2.0, PlainType = 0, BlindHoleDepth = 0.0, HoleType = 0, SlottedHoleX = 0.0,
        /// SlottedHoleY = 0.0, RotateSlots = 2, Hole1 = 1, Hole2 = 1, Hole3 = 0, Hole4 = 0, Hole5 = 0, IsBolt = 1,
        /// UseNut1 = 1, UseNut2 = 0, UseWasher1 = 0, UseWasher2 = 0, UseWasher3 = 1.</remarks>
        /// <param name="id">The bolt id.</param>
        public BoltAttribute(uint id) {
            Id = id.ToString();
            Shape = 0;
            Standard = "A";
            Size = 8.0;
            DistX = "0.0";
            DistY = "0.0";
            NumberOfBolts = 6;
            Diameter = 100.0;
            Type = 0;
            ThreadInMaterial = 1;
            Length = 40.0;
            CutLength = 100.0;
            ExtraLength = 0.0;
            Tolerance = 2.0;
            PlainType = 0;
            BlindHoleDepth = 0.0;
            HoleType = 0;
            SlottedHoleX = 0.0;
            SlottedHoleY = 0.0;
            RotateSlots = 2;
            Hole1 = 1;
            Hole2 = 1;
            Hole3 = 0;
            Hole4 = 0;
            Hole5 = 0;
            IsBolt = 1;
            UseNut1 = 1;
            UseNut2 = 0;
            UseWasher1 = 0;
            UseWasher2 = 0;
            UseWasher3 = 1;
        }

        /// <summary>
        /// Register bolt data by the given id.
        /// </summary>
        /// <remarks>The default values is: Shape = 0, Standard = "A", Size = 8.0, DistX = "0.0", DistY = "0.0",
        /// NumberOfBolts = 6, Diameter = 100.0, Type = 0, ThreadInMaterial = 1, Length = 40.0, CutLength = 100.0,
        /// ExtraLength = 0.0, Tolerance = 2.0, PlainType = 0, BlindHoleDepth = 0.0, HoleType = 0, SlottedHoleX = 0.0,
        /// SlottedHoleY = 0.0, RotateSlots = 2, Hole1 = 1, Hole2 = 1, Hole3 = 0, Hole4 = 0, Hole5 = 0, IsBolt = 1,
        /// UseNut1 = 1, UseNut2 = 0, UseWasher1 = 0, UseWasher2 = 0, UseWasher3 = 1.</remarks>
        /// <param name="id">The bolt id.</param>
        public BoltAttribute(string id) {
            Id = id;
            Shape = 0;
            Standard = "A";
            Size = 8.0;
            DistX = "0.0";
            DistY = "0.0";
            NumberOfBolts = 6;
            Diameter = 100.0;
            Type = 0;
            ThreadInMaterial = 1;
            Length = 40.0;
            CutLength = 100.0;
            ExtraLength = 0.0;
            Tolerance = 2.0;
            PlainType = 0;
            BlindHoleDepth = 0.0;
            HoleType = 0;
            SlottedHoleX = 0.0;
            SlottedHoleY = 0.0;
            RotateSlots = 2;
            Hole1 = 1;
            Hole2 = 1;
            Hole3 = 0;
            Hole4 = 0;
            Hole5 = 0;
            IsBolt = 1;
            UseNut1 = 1;
            UseNut2 = 0;
            UseWasher1 = 0;
            UseWasher2 = 0;
            UseWasher3 = 1;
        }
    }
}