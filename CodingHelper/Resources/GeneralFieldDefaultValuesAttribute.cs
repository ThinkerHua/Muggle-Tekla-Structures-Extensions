using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// Register default values for general field.
    /// </summary>
    /// <remarks>
    /// You need to manually call the "SetDataToDefaultIfUnset" method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class GeneralFieldDefaultValuesAttribute : Attribute {
        
        /// <summary>
        /// Register default values.
        /// </summary>
        /// <param name="nameValuePairs">Field name and default values, must be passed in pairs,
        /// such as ["param1", 12, "param2", 8.5, "param3", "value"].</param>
        public GeneralFieldDefaultValuesAttribute(params object[] nameValuePairs) { }
        
    }
    
}
