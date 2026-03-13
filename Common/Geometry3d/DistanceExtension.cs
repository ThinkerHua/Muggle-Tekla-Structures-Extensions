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
 *  DistanceExtension.cs: extension of Tekla.Structures.Geometry3d.Distance
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Distance"/> 的扩展。
    /// </summary>
    public static class DistanceExtension {
        /// <summary>
        /// 求两条给定直线间的最短距离。对直线退化成点，即 Direction 为零向量的情形也能适用。
        /// </summary>
        /// <param name="line1">给定直线</param>
        /// <param name="line2">给定直线</param>
        /// <returns>两条直线间的最短距离。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double LineToLine(Line line1, Line line2) {
            if (line1 is null) {
                throw new ArgumentNullException(nameof(line1));
            }

            if (line2 is null) {
                throw new ArgumentNullException(nameof(line2));
            }

            var v = new Vector(line2.Origin - line1.Origin);
            if (v.IsZero()) return 0;

            var n = Vector.Cross(line1.Direction, line2.Direction);

            double dis;
            var situation = 0b00;
            situation |= line1.Direction.IsZero() ? 0b01 : 0b00;
            situation |= line2.Direction.IsZero() ? 0b10 : 0b00;
            switch (situation) {
                case 0b00:
                    if (n.IsZero()) {
                        dis = Vector.Cross(v, line2.Direction).GetLength() / line2.Direction.GetLength();
                    } else {
                        dis = Math.Abs(Vector.Dot(v, n) / n.GetLength());
                    }
                    break;
                case 0b01:
                    dis = Vector.Cross(v, line2.Direction).GetLength() / line2.Direction.GetLength();
                    break;
                case 0b10:
                    dis = Vector.Cross(v, line1.Direction).GetLength() / line1.Direction.GetLength();
                    break;
                case 0b11:
                default:
                    dis = v.GetLength();
                    break;
            }

            return dis;
        }
    }
}
