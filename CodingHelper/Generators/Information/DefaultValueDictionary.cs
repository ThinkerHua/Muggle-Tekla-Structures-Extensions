using System;
using System.Collections.Generic;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Key - model object name or number, such as "Brace1".</item>
    ///     <item>Value -
    ///         <list type="bullet">
    ///             <item>Key - property name, such as "Profile".</item>
    ///             <item>Value - property default value, such as "HM244*175*7*11".</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </remarks>
    internal class DefaultValueDictionary : Dictionary<string, Dictionary<string, string>>, IEquatable<DefaultValueDictionary> {

        /*private Dictionary<string, Dictionary<string, string>> _dict;

        public DefaultValueDictionary(ref Dictionary<string, Dictionary<string, string>> dict) {
            _dict = dict;
        }

        public DefaultValueDictionary(IDictionary<string, Dictionary<string, string>> dict) {
            _dict = new Dictionary<string, Dictionary<string, string>>(dict);
        }*/

        public bool Equals(DefaultValueDictionary other) {

            if (other == null || this.Count != other.Count) return false;

            foreach (var kvp in this) {
                var nameOrNumber = kvp.Key;
                if (!other.TryGetValue(nameOrNumber, out Dictionary<string, string> otherKvp)) return false;

                if (kvp.Value.Count != otherKvp.Count) return false;

                foreach (var kvp2 in kvp.Value) {
                    var property = kvp2.Key;
                    if (!otherKvp.TryGetValue(property, out string value)) return false;

                    if (kvp2.Value != value) return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            return obj is DefaultValueDictionary other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                const int hashCode = 340188902;

                return this.Aggregate(hashCode, (cur, kvp) =>
                    kvp.Value.Aggregate((cur * -1521134295) ^ kvp.Key.GetHashCode(), (cur2, kvp2) =>
                        (((cur2 * -1521134295) ^ kvp2.Key.GetHashCode()) * -1521134295) ^ kvp2.Value.GetHashCode()));
            }
        }
    }
}
