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

            return new ContourPoint {
                X = contourPoint.X,
                Y = contourPoint.Y,
                Z = contourPoint.Z,
                Chamfer = new Chamfer {
                    Type = contourPoint.Chamfer.Type,
                    X = contourPoint.Chamfer.X,
                    Y = contourPoint.Chamfer.Y,
                    DZ1 = contourPoint.Chamfer.DZ1,
                    DZ2 = contourPoint.Chamfer.DZ2,
                }
            };
        }
        
        /// <summary>
        /// 计算此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的和。
        /// </summary>
        /// <param name="cp">此 <see cref="ContourPoint"/></param>
        /// <param name="vector">给定 <see cref="Vector"/></param>
        /// <returns>此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的和。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ContourPoint Addition(this ContourPoint cp, Vector vector) {
            if (cp == null) {
                throw new ArgumentNullException(nameof(cp));
            }

            if (vector == null) {
                throw new ArgumentNullException(nameof(vector));
            }

            var cp2 = cp.Clone();
            cp2.X += vector.X;
            cp2.Y += vector.Y;
            cp2.Z += vector.Z;
            return cp2;
        }

        /// <summary>
        /// 计算此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的差。
        /// </summary>
        /// <param name="cp">此 <see cref="ContourPoint"/></param>
        /// <param name="vector">给定 <see cref="Vector"/></param>
        /// <returns>此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的差。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ContourPoint Subtraction(this ContourPoint cp, Vector vector) {
            if (cp == null) {
                throw new ArgumentNullException(nameof(cp));
            }

            if (vector == null) {
                throw new ArgumentNullException(nameof(vector));
            }

            var cp2 = cp.Clone();
            cp2.X -= vector.X;
            cp2.Y -= vector.Y;
            cp2.Z -= vector.Z;
            return cp2;
        }
    }
}