using System;
using System.Collections.Generic;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {

    /// <summary>
    /// A hash set of id, such as ["Part1", "Part2", "Part3"].
    /// </summary>
    internal class IdSet : HashSet<string>, IEquatable<IdSet> {

        public bool Equals(IdSet other) {
            if (other is null) return false;

            return this.Count == other.Count && this.OrderBy(s => s).SequenceEqual(other.OrderBy(s => s));
        }

        public override bool Equals(object obj) {
            return obj is IdSet other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                const int hashCode = -1537116874;
                return this.Aggregate(hashCode, (cur, str) => (cur * -1521134295) ^ str.GetHashCode());
            }
        }
    }
}
