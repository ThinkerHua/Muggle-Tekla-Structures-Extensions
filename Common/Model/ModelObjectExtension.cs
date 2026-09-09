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
 *  ModelObjectExtension.cs: extension of Tekla.Structures.Model.ModelObject
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="ModelObject"/> 的扩展。
    /// </summary>
    public static class ModelObjectExtension {
        /// <summary>
        /// 获取模型对象的标高，单位 mm。
        /// </summary>
        /// <param name="modelObject">当前模型对象</param>
        /// <param name="isAbsolutely">true 则获取绝对标高（全局工作平面），false 则获取相对标高（当前工作平面）</param>
        /// <param name="isTopLevel">true 则获取顶标高，false 则获取底标高</param>
        /// <param name="level">获取到的标高值，获取失败时其值为 <see cref="double.MinValue"/></param>
        /// <returns>获取成功返回 true，失败返回 false。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool GetLevel(this ModelObject modelObject, bool isAbsolutely, bool isTopLevel, out double level) {
            if (modelObject is null) {
                throw new ArgumentNullException(nameof(modelObject));
            }

            var levelName = isTopLevel ? "TOP_LEVEL" : "BOTTOM_LEVEL";
            if (isAbsolutely) levelName = string.Concat(levelName, "_GLOBAL");

            var levelStr = string.Empty;
            level = double.MinValue;
            if (!(modelObject.GetReportProperty(levelName, ref levelStr))) return false;

            if (!double.TryParse(levelStr, out level)) return false;

            level *= 1000.0;

            return true;
        }
    }
}
