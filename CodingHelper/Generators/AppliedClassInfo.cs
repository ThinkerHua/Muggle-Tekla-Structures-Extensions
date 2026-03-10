/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  AppliedClassInfo.cs: used by generators within "Muggle.TsExtensions.CodingHelper.Analyzers" namespace.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    internal record struct AppliedClassInfo {
        public string Name;
        public string NameSpace;
        public Accessibility Accessibility;
        public bool IsRecord;
        public Dictionary<string, HashSet<string>> AttributesInfo;

        public readonly bool Equals(AppliedClassInfo other) {
            if (!(Name == other.Name &&
                  NameSpace == other.NameSpace &&
                  Accessibility == other.Accessibility &&
                  IsRecord == other.IsRecord)) {
                return false;
            }

            if (ReferenceEquals(AttributesInfo, other.AttributesInfo)) return true;

            if (AttributesInfo is null || other.AttributesInfo is null) return false;

            if (AttributesInfo.Count != other.AttributesInfo.Count) return false;

            var orderedX = ToOrderedArray(AttributesInfo);
            var orderedY = ToOrderedArray(other.AttributesInfo);

            if (!orderedX.Select(kvp => kvp.Key).SequenceEqual(orderedY.Select(kvp => kvp.Key))) return false;

            for (int i = 0; i < AttributesInfo.Count; i++) {
                if (!orderedX[i].Value.SequenceEqual(orderedY[i].Value)) return false;
            }

            return true;

            static KeyValuePair<string, string[]>[] ToOrderedArray(Dictionary<string, HashSet<string>> dict) {
                return dict.OrderBy(kvp => kvp.Key).Select(kvp =>
                    new KeyValuePair<string, string[]>(kvp.Key, kvp.Value.OrderBy(v => v).ToArray())).ToArray();
            }
        }

        public override readonly int GetHashCode() {
            unchecked {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (NameSpace != null ? NameSpace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Accessibility;
                hashCode = (hashCode * 397) ^ IsRecord.GetHashCode();
                hashCode = (hashCode * 397) ^ (AttributesInfo != null
                    ? AttributesInfo.Aggregate(hashCode, (cur, pair) =>
                        pair.Value.Aggregate((cur * 397) ^ pair.Key.GetHashCode(), (current, val) =>
                            (current * 397) ^ val.GetHashCode()))
                    : 0);
                return hashCode;
            }
        }
    }
}
