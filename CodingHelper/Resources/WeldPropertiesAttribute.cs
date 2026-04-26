using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register the weld(s) properties that need to be generated for the applied class,
    /// used by Muggle.TsExtensions.CodingHelper.Generators.ViewModelPropertiesGenerator,
    /// cannot be used independently.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [Obsolete("Might be removed in later version, use new attribute 'WeldPropertiesWithDefaultValuesAttribute' instead.")]
    [AttributeUsage(AttributeTargets.Class)]
    public class WeldPropertiesAttribute : Attribute {
        /// <summary>
        /// Register the weld(s) properties using the given number(s).
        /// </summary>
        public WeldPropertiesAttribute(params uint[] numbers) { }

        /// <summary>
        /// Register the weld(s) properties using the given name(s).
        /// </summary>
        public WeldPropertiesAttribute(params string[] names) { }
    }
}