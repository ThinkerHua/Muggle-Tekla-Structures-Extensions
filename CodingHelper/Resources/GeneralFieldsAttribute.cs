using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// Register the general type fields that need to be generated for the applied class.
    /// </summary>
    /// <remarks>The passed-in names are used directly for 'StructuresFieldAttribute',
    /// while the corresponding public field names are converted to PascalCase.</remarks>
    [AttributeUsage(AttributeTargets.Class,  AllowMultiple = true)]
    public class GeneralFieldsAttribute : Attribute {
        public Type Type { get; set; }
        public string[] Names { get; set; }

        /// <summary>
        /// Register the general type fields using the given names.
        /// </summary>
        /// <param name="type">The data type of fields. Only support '<see cref="int"/>', '<see cref="double"/>',
        /// '<see cref="string"/>'.</param>
        /// <param name="names">Names that are passed to 'StructuresFieldAttribute'.</param>
        public GeneralFieldsAttribute(Type type, params string[] names) {
            Type = type;
            Names = names;
        }
        
    }
    
}
