using System;
using System.Collections.Generic;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {

    /// <summary>
    /// Represents a mapping from parameter categories to argument values.
    /// This type extends <see cref="Dictionary{string, TValue}"/> with value equality
    /// semantics tailored for use as required information in the generator pipeline.
    /// </summary>
    /// <remarks>
    /// Key - parameter category. Maybe an attribute name, such as 'PartFieldsAttribute';
    /// or a parameter name, such as 'profile'; or other identifier that can distinguish categories.<br/>
    /// Value - argument values classified according to key.
    /// </remarks>
    /// <typeparam name="TValue">The type of the stored argument values. Must implement <see cref="IEquatable{T}"/>.</typeparam>
    internal class ArgumentsDictionary<TValue> : Dictionary<string, TValue>, IEquatable<ArgumentsDictionary<TValue>>
        where TValue : IEquatable<TValue> {

        public bool Equals(ArgumentsDictionary<TValue> other) {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            return this.Count == other.Count && 
                   this.Keys.All(key => other.ContainsKey(key) && this[key].Equals(other[key]));
        }

        public override bool Equals(object obj) {
            return obj is ArgumentsDictionary<TValue> other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return this.Aggregate(340188902, (cur, item) =>
                    ((cur * -1521134295) ^ item.Key.GetHashCode() * -1521134295) ^ item.Value.GetHashCode());
            }
        }
    }
}
