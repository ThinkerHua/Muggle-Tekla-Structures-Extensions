using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register the part properties (with default values) that need to be generated for the applied class.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PartPropertiesWithDefaultValuesAttribute : Attribute {

        /// <summary>
        /// Register part properties with default values using the given number.
        /// </summary>
        /// <param name="partNumber">The part number used for identification, not some kind of property.</param>
        /// <param name="profile">The default value for the "ProfileString" property of "Part.Profile".</param>
        /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
        /// <param name="name">The default value for the "Name" property of "Part".</param>
        /// <param name="finish">The default value for the "Finish" property of "Part".</param>
        /// <param name="class">The default value for the "Class" property of "Part".</param>
        /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
        /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
        /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
        /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
        public PartPropertiesWithDefaultValuesAttribute(uint partNumber,
            string profile = "", string material = "", string name = "", string finish = "", int @class = 99,
            string assemblyPrefix = "A-", int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {

        }

        /// <summary>
        /// Register part properties with default values using the given name.
        /// </summary>
        /// <param name="partName">The part name used for identification, not some kind of property.</param>
        /// <param name="profile">The default value for the "ProfileString" property of "Part.Profile".</param>
        /// <param name="material">The default value for the "MaterialString" property of "Part.Material".</param>
        /// <param name="name">The default value for the "Name" property of "Part".</param>
        /// <param name="finish">The default value for the "Finish" property of "Part".</param>
        /// <param name="class">The default value for the "Class" property of "Part".</param>
        /// <param name="assemblyPrefix">The default value for the "Prefix" property of "Part.AssemblyNumber".</param>
        /// <param name="assemblyStartNumber">The default value for the "StartNumber" property of "Part.AssemblyNumber".</param>
        /// <param name="partPrefix">The default value for the "Prefix" property of "Part.PartNumber".</param>
        /// <param name="partStartNumber">The default value for the "StartNumber" property of "Part.PartNumber".</param>
        public PartPropertiesWithDefaultValuesAttribute(string partName,
            string profile = "", string material = "", string name = "", string finish = "", int @class = 99,
            string assemblyPrefix = "A-", int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {

        }

    }

}