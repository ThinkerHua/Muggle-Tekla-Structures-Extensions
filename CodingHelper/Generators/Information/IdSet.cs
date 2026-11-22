using System;
using System.Collections.Generic;
using System.Linq;

namespace Muggle.TsExtensions.CodingHelper.Generators.Information {

    /// <summary>
    /// A hash set of id, such as ["Part1", "Part2", "Part3"].
    /// </summary>
    /// <remarks>Using the Enumerable.SequenceEqual method for equality comparison,
    /// means that two instances are considered equal 
    /// only if the elements at corresponding positions in both instances are all equal.</remarks>
    internal class IdSet : HashSet<string>, IEquatable<IdSet> {

        public bool Equals(IdSet other) {
            return other is not null && this.SequenceEqual(other);
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
