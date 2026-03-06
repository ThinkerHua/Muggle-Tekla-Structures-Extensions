/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  PointComparer.cs: comparer of Tekla.Structures.Geometry3d.Point
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Point"/> 类的比较器。
    /// </summary>
    public class PointComparer : IEqualityComparer<Point> {
        /// <summary>
        /// 判断给定两个点是否相等。
        /// </summary>
        /// <param name="point1">给定点1</param>
        /// <param name="point2">给定点2</param>
        /// <returns>相等返回 true，否则返回 false。</returns>
        public bool Equals(Point point1, Point point2) {
            if (object.ReferenceEquals(point1, point2)) return true;

            if (point1 is null || point2 is null) return false;

            return point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z;
        }
        /// <summary>
        /// 获取给定点的哈希代码。
        /// </summary>
        /// <param name="point">给定点</param>
        /// <returns>给定点的哈希代码。</returns>
        public int GetHashCode(Point point) {
            if (point is null) return 0;

            return point.GetHashCode();
        }
    }
}
