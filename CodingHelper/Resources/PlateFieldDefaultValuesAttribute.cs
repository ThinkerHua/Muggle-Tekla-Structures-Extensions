using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register default values for plate fields.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class PlateFieldDefaultValuesAttribute : Attribute {
        /// <summary>
        /// Register default values by plate name.
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
            string name = "", string finish = "", int @class = 99, string assemblyPrefix = "A-",
            int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {

        }

        /// <summary>
        /// Register default values by plate number.
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
            string name = "", string finish = "", int @class = 99, string assemblyPrefix = "A-",
            int assemblyStartNumber = 1, string partPrefix = "P", int partStartNumber = 1) {

        }
    }
}