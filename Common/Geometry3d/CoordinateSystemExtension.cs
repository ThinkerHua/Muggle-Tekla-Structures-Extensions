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
 *  CoordinateSystemExtension.cs: extension of Tekla.Structures.Geometry3d.CoordinateSystem
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;

using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="CoordinateSystem"/> 的扩展。
    /// </summary>
    public static class CoordinateSystemExtension {
        /// <summary>
        /// 获取坐标系的字符串表示形式。
        /// </summary>
        /// <remarks>有关数字格式字符串的详细信息，请参阅
        /// <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings">标准数字格式字符串</a>
        /// 和 <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-numeric-format-strings">自定义数字格式字符串</a>。
        /// </remarks>
        /// <param name="cs">当前坐标系</param>
        /// <param name="format">复合格式字符串。默认值 null。</param>
        /// <returns>坐标系的字符串表示形式。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToString(this CoordinateSystem cs, string format = default) {
            if (cs is null) {
                throw new ArgumentNullException(nameof(cs));
            }

            return $"Origin = {cs.Origin.ToString(format)}\n" +
                $"AxisX = {cs.AxisX.ToString(format)}\n" +
                $"AxisY = {cs.AxisY.ToString(format)}\n";
        }
    }
}
