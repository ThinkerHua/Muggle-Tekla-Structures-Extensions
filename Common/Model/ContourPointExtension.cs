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
 *  ContourPointExtension.cs: extension of Tekla.Structures.Model.ContourPoint
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="ContourPoint"/> 的扩展。
    /// </summary>
    public static class ContourPointExtension {
        /// <summary>
        /// 克隆一个实例。
        /// </summary>
        /// <param name="contourPoint">当前实例</param>
        /// <returns>克隆的实例。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ContourPoint Clone(this ContourPoint contourPoint) {
            if (contourPoint is null) {
                throw new ArgumentNullException(nameof(contourPoint));
            }

            var point = new Point(contourPoint.X, contourPoint.Y, contourPoint.Z);
            var chamfer = new Chamfer {
                Type = contourPoint.Chamfer.Type,
                X = contourPoint.Chamfer.X,
                Y = contourPoint.Chamfer.Y,
                DZ1 = contourPoint.Chamfer.DZ1,
                DZ2 = contourPoint.Chamfer.DZ2,
            };
            return new ContourPoint(point, chamfer);
        }
    }
}
