/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  ChamferExtension.cs: extension of Tekla.Structures.Model.Chamfer
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model;

/// <summary>
/// <see cref="Tekla.Structures.Model"/>.<see cref="Chamfer"/> 的扩展。
/// </summary>
public static class ChamferExtension {

    /// <summary>
    /// 获取当前 <see cref="Chamfer"/> 实例反转方向的新实例。
    /// </summary>
    /// <param name="chamfer">当前实例</param>
    /// <returns>反转方向的新实例。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Chamfer GetReversed(this Chamfer chamfer) {
        if (chamfer == null) {
            throw new ArgumentNullException(nameof(chamfer));
        }

        return new Chamfer {
            Type = chamfer.Type,
            X = chamfer.Type is Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING or Chamfer.ChamferTypeEnum.CHAMFER_ARC
                ? chamfer.X
                : chamfer.Y,
            Y = chamfer.Type is Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING or Chamfer.ChamferTypeEnum.CHAMFER_ARC
                ? chamfer.Y
                : chamfer.X,
            DZ1 = chamfer.DZ1,
            DZ2 = chamfer.DZ2,
        };
    }

}