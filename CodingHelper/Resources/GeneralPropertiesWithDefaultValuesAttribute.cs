using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// Register properties with default values for view model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The passed-in names are used directly for 'StructuresDialogAttribute',
    /// and used for property names after converted to PascalCase.
    /// </para>
    /// <example>Using namespace: 
    /// <code>
    /// using Muggle.TsExtensions.CodingHelper.Generators;
    /// using Tekla.Structures.Datatype;
    /// </code>
    /// apply attribute to view model class like this:
    /// <code>
    /// [GeneralPropertiesWithDefaultValuesAttribute(typeof(DistanceList), "Param", "2*70.0")]
    /// </code>
    /// then the code behind will be generated like this:
    /// <code>
    /// private global::Tekla.Structures.Datatype.DistanceList _param;
    /// [global::Tekla.Structures.Dialog.StructuresDialog("Param", typeof(global::Tekla.Structures.Datatype.DistanceList))]
    /// public global::Tekla.Structures.Datatype.DistanceList Param {
    ///     get {
    ///         return _param;
    ///     }
    ///     set {
    ///         _param = value.Count == 0 ? global::Tekla.Structures.Datatype.DistanceList.Parse("2*70.0") : value;
    ///         OnPropertyChanged();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class GeneralPropertiesWithDefaultValuesAttribute : Attribute {
        
        /// <summary>
        /// Register property and default values.
        /// </summary>
        /// <param name="type">The data type of properties. Only support 'Integer', 'Double', 'Distance',
        /// 'DistanceList', 'String' within 'Tekla.Structures.Datatype' namespace.
        /// <b>Should not use 'Boolean' type, use 'Integer' instead.</b> Although Tekla officially states support
        /// for the 'Boolean' type, but in reality, the 'Boolean' type cannot be properly passed into the Plugin.
        /// </param>
        /// <param name="nameValuePairs">Property name and default values, must be passed in pairs.
        /// The data type of default value should comply with the following rules: 
        /// <code>
        /// | The value of argument 'type' | The data type of default value |
        /// | ---------------------------- | ------------------------------ |
        /// |          Boolean             |             N/A                |
        /// |          Integer             |             int                |
        /// |          Double              |             double             |
        /// |          Distance            |             double             |
        /// |          DistanceList        |             string             |
        /// |          String              |             string             |
        /// </code>
        /// <example>
        /// For 'Integer' type property: ["Param1", 8, "Param2", 10, "Param3", 12]
        /// </example>
        /// </param>
        public GeneralPropertiesWithDefaultValuesAttribute(Type type, params object[] nameValuePairs) { }
        
    }
    
}
