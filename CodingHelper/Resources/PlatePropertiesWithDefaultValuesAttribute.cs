using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register the plate properties (with default values) that need to be generated for the applied class.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PlatePropertiesWithDefaultValuesAttribute : Attribute {

        /// <summary>
        /// Register plate properties with default values using the given number.
        /// </summary>
        /// <param name="plateNumber">The plate number used for identification, not some kind of property.</param>
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
        public PlatePropertiesWithDefaultValuesAttribute(uint plateNumber, 
            double thickness = 0, double breadth = 0, double height = 0, string material = "", string name = "", 
            string finish = "", int @class = 99, string assemblyPrefix = "A-", int assemblyStartNumber = 1, 
            string partPrefix = "P", int partStartNumber = 1) {

        }

        /// <summary>
        /// Register plate properties with default values using the given name.
        /// </summary>
        /// <param name="plateName">The plate name used for identification, not some kind of property.</param>
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
        public PlatePropertiesWithDefaultValuesAttribute(string plateName, 
            double thickness = 0, double breadth = 0, double height = 0, string material = "", string name = "", 
            string finish = "", int @class = 99, string assemblyPrefix = "A-", int assemblyStartNumber = 1, 
            string partPrefix = "P", int partStartNumber = 1) {

        }

    }

}