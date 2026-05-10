using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// Generate fields that one-to-one corresponds with each field in data.
    /// </summary>
    /// <remarks>
    /// <para>You need to manually call the "GetFieldValuesFrom" method at an appropriate location.</para>
    /// <para>Only register fields that are manually written and fields registered by
    /// <see cref="PartFieldsAttribute"/>, 
    /// <see cref="PlateFieldsAttribute"/>, 
    /// <see cref="WeldFieldsAttribute"/>, 
    /// <see cref="BoltFieldsAttribute"/>, 
    /// <see cref="BoltCircleFieldsAttribute"/>,
    /// <see cref="GeneralFieldsAttribute"/> within <see cref="Muggle.TsExtensions.CodingHelper.Generators"/>.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class FieldsFromAttribute : Attribute {
        
        /// <summary>
        /// Register the public fields of the data class to this class.
        /// </summary>
        /// <param name="dataType">The type to register fields from.</param>
        public FieldsFromAttribute(Type dataType) { }
        
    }

}