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
 *  ProfileRect.cs: profile of rect
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 矩形管（通用形式，含冷弯矩形管和焊接矩形管）。适用如下形式：
    /// <br/><see cref="PatternCollection.CFH_J_1"/>：<inheritdoc cref="PatternCollection.CFH_J_1"/>
    /// <br/><see cref="PatternCollection.CFH_J_2"/>：<inheritdoc cref="PatternCollection.CFH_J_2"/>
    /// <br/><see cref="PatternCollection.CFH_J_3"/>：<inheritdoc cref="PatternCollection.CFH_J_3"/>
    /// <br/><see cref="PatternCollection.CFH_J_4"/>：<inheritdoc cref="PatternCollection.CFH_J_4"/>
    /// <br/><see cref="PatternCollection.RECT_1"/>：<inheritdoc cref="PatternCollection.RECT_1"/>
    /// <br/><see cref="PatternCollection.RECT_2"/>：<inheritdoc cref="PatternCollection.RECT_2"/>
    /// <br/><see cref="PatternCollection.RECT_3"/>：<inheritdoc cref="PatternCollection.RECT_3"/>
    /// <br/><see cref="PatternCollection.RECT_4"/>：<inheritdoc cref="PatternCollection.RECT_4"/>
    /// <br/><see cref="PatternCollection.RECT_5"/>：<inheritdoc cref="PatternCollection.RECT_5"/>
    /// </summary>
    public class ProfileRect : ProfileBase {

        /// <summary>
        /// 
        /// </summary>
        public double h1, h2, b1, b2, s, t;

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfileRect() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfileRect(string profileText) {
            ProfileText = profileText;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"><inheritdoc path="/param[1]"/></param>
        /// <param name="e"><inheritdoc path="/param[2]"/></param>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            var temp = (h1, h2, b1, b2, s, t);
            var text = e.NewText;
            try {
                if (string.IsNullOrEmpty(text))
                    throw new UnAcceptableProfileException(text);

                var match = Regex.Match(text, PatternCollection.CFH_J_1);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CFH_J_2);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CFH_J_3);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CFH_J_4);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.RECT_1);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.RECT_2);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.RECT_3);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.RECT_4);
                if (match.Success) {
                    double.TryParse(match.Groups["b1"].Value, out b1);
                    double.TryParse(match.Groups["b2"].Value, out b2);
                } else if (!match.Success) {
                    match = Regex.Match(text, PatternCollection.RECT_5);
                }
                if (!match.Success)
                    throw new UnAcceptableProfileException(text);

                double.TryParse(match.Groups["h1"].Value, out h1);
                double.TryParse(match.Groups["h2"].Value, out h2);
                double.TryParse(match.Groups["s"].Value, out s);
                double.TryParse(match.Groups["t"].Value, out t);

                if (h2 == 0) h2 = h1;
                if (b1 == 0) b1 = h1;
                if (b2 == 0) b2 = b1;
                if (t == 0) t = s;

            } catch (UnAcceptableProfileException) {
                h1 = temp.h1; h2 = temp.h2; b1 = temp.b1; b2 = temp.b2;
                s = temp.s; t = temp.t;
                throw;
            }
        }
    }
}
