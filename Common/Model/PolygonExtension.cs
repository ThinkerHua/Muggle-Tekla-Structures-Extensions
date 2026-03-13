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
 *  PolygonExtension.cs: extension of Tekla.Structures.Model.Polygon
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="Polygon"/> 的扩展。
    /// </summary>
    public static class PolygonExtension {
        /// <summary>
        /// 克隆一个实例。
        /// </summary>
        /// <param name="polygon">当前实例</param>
        /// <returns>克隆的实例。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Polygon Clone(this Polygon polygon) {
            if (polygon is null) {
                throw new ArgumentNullException(nameof(polygon));
            }

            var np = new Polygon();
            foreach (Point point in polygon.Points) {
                np.Points.Add(new Point(point));
            }

            return np;
        }
    }
}
