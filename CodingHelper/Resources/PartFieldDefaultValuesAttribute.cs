using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register default values for part fields.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class PartFieldDefaultValuesAttribute : Attribute {
        public string Id { get; set; }
        public string Profile { get; set; }
        public string Material { get; set; }
        public string Name { get; set; }
        public string Finish { get; set; }
        public int Class { get; set; }
        public string AssemblyPrefix { get; set; }
        public int AssemblyStartNumber { get; set; }
        public string PartPrefix { get; set; }
        public int PartStartNumber { get; set; }

        /// <summary>
        /// Register preset default values by the given id.
        /// </summary>
        /// <remarks>The preset default value is: Profile = "", Material = "", Name = "", Finish = "", Class = 99,
        /// AssemblyPrefix = "A-", AssemblyStartNumber = 1, PartPrefix = "P", PartStartNumber = 1.</remarks>
        /// <param name="id">The id registered by <see cref="PartFieldsAttribute"/>.</param>
        public PartFieldDefaultValuesAttribute(uint id) {
            Id = id.ToString();
            Profile = "";
            Material = "";
            Name = "";
            Finish = "";
            Class = 99;
            AssemblyPrefix = "A-";
            AssemblyStartNumber = 1;
            PartPrefix = "P";
            PartStartNumber = 1;
        }

        /// <summary>
        /// Register preset default values by the given id.
        /// </summary>
        /// <remarks>The preset default value is: Profile = "", Material = "", Name = "", Finish = "", Class = 99,
        /// AssemblyPrefix = "A-", AssemblyStartNumber = 1, PartPrefix = "P", PartStartNumber = 1.</remarks>
        /// <param name="id">The id registered by <see cref="PartFieldsAttribute"/>.</param>
        public PartFieldDefaultValuesAttribute(string id) {
            Id = id;
            Profile = "";
            Material = "";
            Name = "";
            Finish = "";
            Class = 99;
            AssemblyPrefix = "A-";
            AssemblyStartNumber = 1;
            PartPrefix = "P";
            PartStartNumber = 1;
        }
        
        /// <summary>
        /// Register default values by the given id.
        /// </summary>
        /// <param name="id">The id registered by <see cref="PartFieldsAttribute"/>.</param>
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
            uint id, string profile = "", string material = "", string name = "", string finish = "",
            int @class = 99, string assemblyPrefix = "A-", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1) {
            
            Id = id.ToString();
            Profile = profile;
            Material = material;
            Name = name;
            Finish = finish;
            Class = @class;
            AssemblyPrefix = assemblyPrefix;
            AssemblyStartNumber = assemblyStartNumber;
            PartPrefix = partPrefix;
            PartStartNumber = partStartNumber;
        }
        
        /// <summary>
        /// Register default values by the given id.
        /// </summary>
        /// <param name="id">The id registered by <see cref="PartFieldsAttribute"/>.</param>
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
            string id, string profile = "", string material = "", string name = "", string finish = "",
            int @class = 99, string assemblyPrefix = "A-", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1) {
            
            Id = id;
            Profile = profile;
            Material = material;
            Name = name;
            Finish = finish;
            Class = @class;
            AssemblyPrefix = assemblyPrefix;
            AssemblyStartNumber = assemblyStartNumber;
            PartPrefix = partPrefix;
            PartStartNumber = partStartNumber;
        }
    }
}