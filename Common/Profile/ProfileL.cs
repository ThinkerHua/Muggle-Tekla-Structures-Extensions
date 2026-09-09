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
 *  ProfileH.cs: profile of H
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 角钢。适用如下形式：
    /// <br/><see cref="PatternCollection.L_1"/>：<inheritdoc cref="PatternCollection.L_1"/>
    /// <br/><see cref="PatternCollection.L_2"/>：<inheritdoc cref="PatternCollection.L_2"/>
    /// <br/><see cref="PatternCollection.L_3"/>：<inheritdoc cref="PatternCollection.L_3"/>
    /// </summary>
    public class ProfileL : ProfileBase {
        /// <summary>
        /// 
        /// </summary>
        public double h, b, s, t;

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfileL() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfileL(string profileText) {
            ProfileText = profileText;
        }

        /// <inheritdoc/>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            var tmp = (h, b, s, t);
            try {
                if (string.IsNullOrEmpty(e.NewText))
                    throw new UnAcceptableProfileException(e.NewText);

                Match match = Regex.Match(e.NewText, PatternCollection.L_1);
                if (!match.Success)
                    match = Regex.Match(e.NewText, PatternCollection.L_2);
                if (!match.Success)
                    match = Regex.Match(e.NewText, PatternCollection.L_3);
                if (!match.Success)
                    throw new UnAcceptableProfileException(e.NewText);

                double.TryParse(match.Groups["h"].Value, out h);
                double.TryParse(match.Groups["b"].Value, out b);
                double.TryParse(match.Groups["s"].Value, out s);
                double.TryParse(match.Groups["t"].Value, out t);

                if (b == 0) b = h;
                if (t == 0) t = s;
            } catch (UnAcceptableProfileException) {
                h = tmp.h; b = tmp.b; t = tmp.t; s = tmp.s;
                throw;
            }
        }
    }
}
