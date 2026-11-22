namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register plate data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class PlateAttribute : System.Attribute {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public double Thickness { get; set; }
        
        public double Breadth { get; set; }
        
        public double Height { get; set; }
        
        public string Material { get; set; }
        
        public string Finish { get; set; }
        
        public int Class { get; set; }
        
        public string AssemblyPrefix { get; set; }
        
        public int AssemblyStartNumber { get; set; }
        
        public string PartPrefix { get; set; }
        
        public int PartStartNumber { get; set; }

        /// <summary>
        /// Register plate data by the given id.
        /// </summary>
        /// <remarks>The default values is: Name = "", Thickness = 0, Breadth = 0, Height = 0, Material = "",
        /// Finish = "", Class = 99, PartPrefix = "P", PartStartNumber = 1, AssemblyPrefix = "A-",
        /// AssemblyStartNumber = 1.</remarks>
        /// <param name="id">The plate id.</param>
        public PlateAttribute(uint id) {
            Id = id.ToString();
            Name = "";
            Thickness = 0.0;
            Breadth = 0.0;
            Height = 0.0;
            Material = "";
            Finish = "";
            Class = 99;
            PartPrefix = "P";
            PartStartNumber = 1;
            AssemblyPrefix = "A-";
            AssemblyStartNumber = 1;
        }

        /// <summary>
        /// Register plate data by the given id.
        /// </summary>
        /// <remarks>The default values is: Name = "", Thickness = 0, Breadth = 0, Height = 0, Material = "",
        /// Finish = "", Class = 99, PartPrefix = "P", PartStartNumber = 1, AssemblyPrefix = "A-",
        /// AssemblyStartNumber = 1.</remarks>
        /// <param name="id">The plate id.</param>
        public PlateAttribute(string id) {
            Id = id;
            Name = "";
            Thickness = 0.0;
            Breadth = 0.0;
            Height = 0.0;
            Material = "";
            Finish = "";
            Class = 99;
            PartPrefix = "P";
            PartStartNumber = 1;
            AssemblyPrefix = "A-";
            AssemblyStartNumber = 1;
        }
    }
}