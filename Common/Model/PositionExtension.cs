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
 *  PositionExtension.cs: extension of Tekla.Structures.Model.Position
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="Position"/> 的扩展。
    /// </summary>
    public static class PositionExtension {
        /// <summary>
        /// 克隆一个实例。
        /// </summary>
        /// <param name="position">当前实例</param>
        /// <returns>克隆的实例。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Position Clone(this Position position) {
            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            return new Position() {
                Plane = position.Plane,
                PlaneOffset = position.PlaneOffset,
                Rotation = position.Rotation,
                RotationOffset = position.RotationOffset,
                Depth = position.Depth,
                DepthOffset = position.DepthOffset,
            };
        }
    }
}
