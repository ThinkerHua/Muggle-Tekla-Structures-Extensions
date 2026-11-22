namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register chamfer data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class ChamferAttribute : System.Attribute {
        
        public string Id { get; set; }
        
        public int Type { get; set; }
        
        public double X { get; set; }
        
        public double Y { get; set; }
        
        public double Dz1 { get; set; }
        
        public double Dz2 { get; set; }

        /// <summary>
        /// Register chamfer data by the given id.
        /// </summary>
        /// <remarks>The default values is: Type = 0, X = 0.0, Y = 0.0, Dz1 = 0.0, Dz2 = 0.0.</remarks>
        /// <param name="id">The chamfer id.</param>
        public ChamferAttribute(uint id) {
            this.Id = id.ToString();
            this.Type = 0;
            this.X = 0.0;
            this.Y = 0.0;
            this.Dz1 = 0.0;
            this.Dz2 = 0.0;
        }

        /// <summary>
        /// Register chamfer data by the given id.
        /// </summary>
        /// <remarks>The default values is: Type = 0, X = 0.0, Y = 0.0, Dz1 = 0.0, Dz2 = 0.0.</remarks>
        /// <param name="id">The chamfer id.</param>
        public ChamferAttribute(string id) {
            this.Id = id;
            this.Type = 0;
            this.X = 0.0;
            this.Y = 0.0;
            this.Dz1 = 0.0;
            this.Dz2 = 0.0;
        }
    }
}