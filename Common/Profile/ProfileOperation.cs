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
 *  ProfileOperation.cs: operations about Profile
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 截面相关操作。
    /// </summary>
    public static class ProfileOperation {
        /// <summary>
        /// 获取截面前缀。
        /// </summary>
        /// <param name="profileText">截面文本</param>
        /// <returns>截面前缀</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception">不是有效的截面文本时引发。</exception>
        public static string GetProfilePrefix(string profileText) {
            if (string.IsNullOrEmpty(profileText)) {
                throw new ArgumentException($"“{nameof(profileText)}”不能为 null 或空。", nameof(profileText));
            }

            const string pattern = @"\A(?<prefix>[^0-9]+)[0-9].+\Z";
            var match = Regex.Match(profileText, pattern);

            return !match.Success ? throw new Exception("Not a valid profile text.") : match.Groups["prefix"].Value;
        }
    }
}