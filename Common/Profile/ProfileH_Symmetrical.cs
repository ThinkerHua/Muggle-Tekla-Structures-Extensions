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
 *  ProfileH_Symmetrical.cs: profile of symmetrical H
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 等截面H型钢和对称变截面H型钢（b2 == b1 的情形）。适用如下形式：
    /// <br/><see cref="PatternCollection.H_1"/>：<inheritdoc cref="PatternCollection.H_1"/>
    /// <br/><see cref="PatternCollection.H_2"/>：<inheritdoc cref="PatternCollection.H_2"/>
    /// <br/><see cref="PatternCollection.H_4"/>：<inheritdoc cref="PatternCollection.H_4"/>
    /// <br/><see cref="PatternCollection.H_5"/>：<inheritdoc cref="PatternCollection.H_5"/>
    /// <br/><see cref="PatternCollection.H_6"/>：<inheritdoc cref="PatternCollection.H_6"/>
    /// <br/><see cref="PatternCollection.H_7"/>：<inheritdoc cref="PatternCollection.H_7"/>
    /// </summary>
    public class ProfileH_Symmetrical : ProfileBase {

        /// <summary>
        /// 
        /// </summary>
        public double h1, h2, b1, b2, s, t1, t2;

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfileH_Symmetrical() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfileH_Symmetrical(string profileText) {
            ProfileText = profileText;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"><inheritdoc path="/param[1]"/></param>
        /// <param name="e"><inheritdoc path="/param[2]"/></param>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            var temp = (h1, h2, b1, b2, s, t1, t2);
            var text = e.NewText;
            try {
                if (string.IsNullOrEmpty(text))
                    throw new UnAcceptableProfileException(text);

                Match match = Regex.Match(text, PatternCollection.H_1);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.H_2);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.H_4);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.H_5);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.H_6);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.H_7);
                if (!match.Success)
                    throw new UnAcceptableProfileException(text);

                double.TryParse(match.Groups["h1"].Value, out h1);
                double.TryParse(match.Groups["h2"].Value, out h2);
                double.TryParse(match.Groups["b1"].Value, out b1);
                double.TryParse(match.Groups["b2"].Value, out b2);
                double.TryParse(match.Groups["s"].Value, out s);
                double.TryParse(match.Groups["t1"].Value, out t1);
                double.TryParse(match.Groups["t2"].Value, out t2);

                if (h2 == 0) h2 = h1;
                if (b2 == 0) b2 = b1;
                if (t2 == 0) t2 = t1;

                if (!text.Contains("I_VAR_A") && h2 != h1 || b2 != b1)
                    throw new UnAcceptableProfileException(text);
            } catch (UnAcceptableProfileException) {
                h1 = temp.h1; h2 = temp.h2; b1 = temp.b1; b2 = temp.b2;
                s = temp.s; t1 = temp.t1; t2 = temp.t2;
                throw;
            }
        }
    }
}
