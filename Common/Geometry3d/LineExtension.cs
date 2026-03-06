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
 *  LineExtension.cs: extension of Tekla.Structures.Geometry3d.Line
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Line"/> 的扩展。
    /// </summary>
    public static class LineExtension {
        /// <summary>
        /// 偏移方向。
        /// </summary>
        public enum OffsetDirectionEnum {
            /// <summary>
            /// 向左偏移。
            /// </summary>
            LEFT,
            /// <summary>
            /// 向右偏移。
            /// </summary>
            RIGHT,
        }
        /// <summary>
        /// 获取当前直线偏移后的直线，适用于XY平面。
        /// </summary>
        /// <param name="line">当前直线</param>
        /// <param name="distance">偏移距离</param>
        /// <param name="direction">偏移方向，向左或向右。</param>
        /// <returns>偏移后的直线。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Line Offset(this Line line, double distance, OffsetDirectionEnum direction) {
            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            Vector vector = line.Direction.Cross(new Vector(0, 0, direction > 0 ? 1 : -1));
            vector.Normalize(distance);

            return Offset(line, vector);
        }
        /// <summary>
        /// 获取当前直线偏移后的直线，适用于三维坐标系。
        /// </summary>
        /// <param name="line">当前直线</param>
        /// <param name="vector">偏移向量</param>
        /// <returns>偏移后的直线。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Line Offset(this Line line, Vector vector) {
            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            Point point = new Point(line.Origin);
            point.Translate(vector);
            Line newline = new Line(point, new Vector(line.Direction));

            return newline;
        }
    }
}
