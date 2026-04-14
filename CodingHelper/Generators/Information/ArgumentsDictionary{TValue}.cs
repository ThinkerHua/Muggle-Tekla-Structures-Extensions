using System;
using System.Collections.Generic;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {

    /// <summary>
    /// Represents a mapping from attribute names to their parsed argument values.
    /// This type extends <see cref="Dictionary{TKey, TValue}"/> with value equality
    /// semantics tailored for use as required information in the generator pipeline.
    /// </summary>
    /// <remarks>
    /// Key - attribute name, such as "PartFieldsAttribute".<br/>
    /// Value - attribute arguments, such as ["Base", "Main", "Sec1", "Sec2"].
    /// </remarks>
    /// <typeparam name="TValue">The type of the stored argument values. Must implement <see cref="IEquatable{T}"/>.</typeparam>
    internal class ArgumentsDictionary<TValue> : Dictionary<string, TValue>, IRequiredInformation<ArgumentsDictionary<TValue>>
        where TValue : IEquatable<TValue> {

        /*private Dictionary<string, TValue> _dict;

        public ArgumentsDictionary(ref Dictionary<string, TValue> dict) {
            _dict = dict;
        }

        public ArgumentsDictionary(IDictionary<string, TValue> dict) {
            _dict = new Dictionary<string, TValue>(dict);
        }*/

        public bool Equals(ArgumentsDictionary<TValue> other) {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (this.Count != other.Count) return false;

            foreach (var key in this.Keys) {
                if (!other.ContainsKey(key) || this[key].Equals(other[key])) return false;
            }

            return true;
        }

        public override bool Equals(object obj) {
            if (obj is not ArgumentsDictionary<TValue> other) return false;

            return this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return this.Aggregate(340188902, (cur, item) =>
                    ((cur * -1521134295) ^ item.Key.GetHashCode() * -1521134295) ^ item.Value.GetHashCode());
            }
        }
    }
}
