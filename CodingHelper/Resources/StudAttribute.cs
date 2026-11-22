namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// Register stud data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class StudAttribute : System.Attribute {
        
        public string Id { get; set; }
        
        public double Size { get; set; }
        
        public string Standard { get; set; }
        
        public double Length { get; set; }
        
        public double CutLength { get; set; }
        
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
        
        /// <summary>
        /// 0 - means this stud is a BoltArray<br/>
        /// 1 - means this stud is a BoltCircle<br/>
        /// 2 - means this stud is a BoltXYList<br/>
        /// </summary>
        public int Shape { get; set; }
        
        /// <summary>
        /// Register stud data by the given id.
        /// </summary>
        /// <remarks>The default values is: Size = 10.0, Standard = "STUD", Length = 40.0, CutLength = 100.0,
        /// DistX = "0.0", DistY = "0.0", NumberOfBolts = 6, Diameter = 100.0, Type = 0, Shape = 0.</remarks>
        /// <param name="id">The stud id.</param>
        public StudAttribute(uint id) {
            Id = id.ToString();
            Size = 10.0;
            Standard = "STUD";
            Length = 40.0;
            CutLength = 100.0;
            DistX = "0.0";
            DistY = "0.0";
            NumberOfBolts = 6;
            Diameter = 100.0;
            Type = 0;
            Shape = 0;
        }
        
        /// <summary>
        /// Register stud data by the given id.
        /// </summary>
        /// <remarks>The default values is: Size = 10.0, Standard = "STUD", Length = 40.0, CutLength = 100.0,
        /// DistX = "0.0", DistY = "0.0", NumberOfBolts = 6, Diameter = 100.0, Type = 0, Shape = 0.</remarks>
        /// <param name="id">The stud id.</param>
        public StudAttribute(string id) {
            Id = id;
            Size = 10.0;
            Standard = "STUD";
            Length = 40.0;
            CutLength = 100.0;
            DistX = "0.0";
            DistY = "0.0";
            NumberOfBolts = 6;
            Diameter = 100.0;
            Type = 0;
            Shape = 0;
        }
    }
}