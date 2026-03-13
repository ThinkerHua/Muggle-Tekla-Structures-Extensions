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
 *  ProfileCircular.cs: profile of circular
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 圆钢或圆管（通用形式，含椭圆）。适用如下形式：
    /// <br/><see cref="PatternCollection.CIRC_1"/>：<inheritdoc cref="PatternCollection.CIRC_1"/>
    /// <br/><see cref="PatternCollection.CIRC_2"/>：<inheritdoc cref="PatternCollection.CIRC_2"/>
    /// <br/><see cref="PatternCollection.CIRC_3"/>：<inheritdoc cref="PatternCollection.CIRC_3"/>
    /// <br/><see cref="PatternCollection.CIRC_4"/>：<inheritdoc cref="PatternCollection.CIRC_4"/>
    /// <br/><see cref="PatternCollection.CIRC_5"/>：<inheritdoc cref="PatternCollection.CIRC_5"/>
    /// </summary>
    public class ProfileCircular : ProfileBase {
        /// <summary>
        /// 
        /// </summary>
        public double d1, r1, d2, r2, t;

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfileCircular() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfileCircular(string profileText) {
            ProfileText = profileText;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"><inheritdoc path="/param[1]"/></param>
        /// <param name="e"><inheritdoc path="/param[2]"/></param>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            var temp = (d1, r1, d2, r2, t);
            var text = e.NewText;
            try {
                if (string.IsNullOrEmpty(text))
                    throw new UnAcceptableProfileException(text);

                Match match = Regex.Match(text, PatternCollection.CIRC_1);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CIRC_2);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CIRC_3);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CIRC_4);
                if (!match.Success)
                    match = Regex.Match(text, PatternCollection.CIRC_5);
                if (!match.Success)
                    throw new UnAcceptableProfileException(text);

                double.TryParse(match.Groups["d1"].Value, out d1);
                double.TryParse(match.Groups["r1"].Value, out r1);
                double.TryParse(match.Groups["d2"].Value, out d2);
                double.TryParse(match.Groups["r2"].Value, out r2);
                double.TryParse(match.Groups["t"].Value, out t);

                if (r1 == 0) r1 = d1;
                if (d2 == 0) d2 = d1;
                if (r2 == 0) r2 = r1;
            } catch (UnAcceptableProfileException) {
                d1 = temp.d1; r1 = temp.r1; d2 = temp.d2; r2 = temp.r2; t = temp.t;
                throw;
            }
        }
    }
}
