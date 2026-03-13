/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  GeometricPlaneFactory.cs: factory of Tekla.Structures.Geometry3d.GeometricPlane
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// 几何平面构造器。
    /// </summary>
    public static class GeometricPlaneFactory {
        /// <summary>
        /// 根据两条直线构造一个几何平面。
        /// </summary>
        /// <param name="line1">构造几何平面所需的直线</param>
        /// <param name="line2">构造几何平面所需的直线</param>
        /// <param name="gPlane">成功构造时由此参数输出几何平面。
        ///     <para><code>gPlane.Origin == line1.Origin</code></para>
        ///     <para>两线平行时：<code>gPlane.Normal == line1.Direction.Cross(new Vector(line2.Origin - line1.Origin))</code></para>
        ///     <para>两线相交时：<code>gPlane.Normal == line1.Direction.Cross(line2.Direction)</code></para>
        /// </param>
        /// <returns>
        /// <list type="bullet">
        ///     <item>-1: 有无数个解，即两条直线共线。<paramref name="gPlane"/> == null。</item>
        ///     <item>0: 无解，两条直线不共面。<paramref name="gPlane"/> == null。</item>
        ///     <item>1: 有且仅有唯一解，此时由输出参数 <paramref name="gPlane"/> 输出成功构造的几何平面。</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">参数的 Direction 属性不得为零向量。</exception>
        public static int ByLines(Line line1, Line line2, out GeometricPlane gPlane) {
            if (line1 is null) {
                throw new ArgumentNullException(nameof(line1));
            }

            if (line2 is null) {
                throw new ArgumentNullException(nameof(line2));
            }

            if (line1.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line1)}”.Direction 不应为零向量。");

            if (line2.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line2)}”.Direction 不应为零向量。");

            gPlane = null;
            if (Vector.Cross(line1.Direction, line2.Direction).IsZero()) {
                //平行

                //共线
                if (Distance.PointToLine(line1.Origin, line2) == 0) return -1;

                //不共线
                gPlane = new GeometricPlane(line1.Origin, line1.Direction, new Vector(line2.Origin - line1.Origin));
            } else {
                //不平行
                var lineSegment = IntersectionExtension.LineToLine(line1, line2);
                if (!lineSegment.StartPoint.Equals(lineSegment.EndPoint)) return 0;//不共面

                gPlane = new GeometricPlane(line1.Origin, line1.Direction, line2.Direction);
            }

            return 1;
        }
        /// <summary>
        /// 根据三点构造一个几何平面。
        /// </summary>
        /// <param name="point1">给定点1</param>
        /// <param name="point2">给定点2</param>
        /// <param name="point3">给定点3</param>
        /// <returns>成功构造的几何平面。
        /// <code>
        /// Origin == point1;
        /// Normal == Vector.Cross(new Vector(point2 - point1), new Vector(point3 - point1));
        /// </code>
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">给定点不应相等。</exception>
        public static GeometricPlane ByPoints(Point point1, Point point2, Point point3) {
            if (point1 is null) {
                throw new ArgumentNullException(nameof(point1));
            }

            if (point2 is null) {
                throw new ArgumentNullException(nameof(point2));
            }

            if (point3 is null) {
                throw new ArgumentNullException(nameof(point3));
            }

            if (point1.Equals(point2) || point1.Equals(point3) || point2.Equals(point3))
                throw new ArgumentException("给定点不应相等。");

            return new GeometricPlane(point1, new Vector(point2 - point1), new Vector(point3 - point1));
        }
        /// <summary>
        /// 用 <see cref="Tekla.Structures.Model"/>.<see cref="Plane"/> 构造一个几何平面。
        /// </summary>
        /// <param name="plane"></param>
        /// <returns>成功构造的几何平面。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static GeometricPlane ByPlane(Plane plane) {
            if (plane is null) {
                throw new ArgumentNullException(nameof(plane));
            }

            return new GeometricPlane(plane.Origin, plane.AxisX, plane.AxisY);
        }
    }
}
