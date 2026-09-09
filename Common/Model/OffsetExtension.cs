/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  OffsetExtension.cs: extension of Tekla.Structures.Model.Offset
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="Offset"/> 的扩展。
    /// </summary>
    public static class OffsetExtension {
        /// <summary>
        /// 克隆一个实例。
        /// </summary>
        /// <param name="offset">当前实例</param>
        /// <returns>克隆的实例。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Offset Clone(this Offset offset) {
            if (offset is null) {
                throw new ArgumentNullException(nameof(offset));
            }

            return new Offset() {
                Dx = offset.Dx,
                Dy = offset.Dy,
                Dz = offset.Dz,
            };
        }
    }
}
