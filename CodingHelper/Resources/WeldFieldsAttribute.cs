using System;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    /// <summary>
    /// Register the weld fields that need to be generated for the applied class,
    /// used by Muggle.TsExtensions.CodingHelper.Generators.PluginDataFieldsGenerator,
    /// cannot be used independently.
    /// </summary>
    /// <remarks>Mapping relationship between properties and attribute name pattern 
    /// <a href="https://github.com/ThinkerHua/Muggle-Tekla-Structures-Extensions/blob/master/CodingHelper/AttributeNameReference.md">
    /// see here</a>.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class WeldFieldsAttribute : Attribute {
        public string[] Ids { get; set; }

        /// <summary>
        /// Register the weld fields using the given ids.
        /// </summary>
        public WeldFieldsAttribute(params uint[] ids) {
            Ids = ids.Select(id => id.ToString()).ToArray();
        }

        /// <summary>
        /// Register the weld fields using the given ids.
        /// </summary>
        public WeldFieldsAttribute(params string[] ids) {
            Ids = ids;
        }
    }
}