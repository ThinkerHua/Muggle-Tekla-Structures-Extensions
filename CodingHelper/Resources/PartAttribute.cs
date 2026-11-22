namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register part data.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name patterns 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class PartAttribute : System.Attribute {
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Profile { get; set; }
        
        public string Material { get; set; }
        
        public string Finish { get; set; }
        
        public int Class { get; set; }
        
        public string PartPrefix { get; set; }
        
        public int PartStartNumber { get; set; }
        
        public string AssemblyPrefix { get; set; }
        
        public int AssemblyStartNumber { get; set; }

        /// <summary>
        /// Register part data by the given id.
        /// </summary>
        /// <remarks>The default value is: Name = "", Profile = "", Material = "", Finish = "", Class = 99,
        /// PartPrefix = "P", PartStartNumber = 1, AssemblyPrefix = "A-", AssemblyStartNumber = 1.</remarks>
        /// <param name="id">The part id.</param>
        public PartAttribute(uint id) {
            Id = id.ToString();
            Name = "";
            Profile = "";
            Material = "";
            Finish = "";
            Class = 99;
            PartPrefix = "P";
            PartStartNumber = 1;
            AssemblyPrefix = "A-";
            AssemblyStartNumber = 1;
        }

        /// <summary>
        /// Register part data by the given id.
        /// </summary>
        /// <remarks>The default value is: Name = "", Profile = "", Material = "", Finish = "", Class = 99,
        /// PartPrefix = "P", PartStartNumber = 1, AssemblyPrefix = "A-", AssemblyStartNumber = 1.</remarks>
        /// <param name="id">The part id.</param>
        public PartAttribute(string id) {
            Id = id;
            Name = "";
            Profile = "";
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