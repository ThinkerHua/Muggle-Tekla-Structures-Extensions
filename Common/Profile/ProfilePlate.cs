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
 *  ProfilePlate.cs: profile of plate
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Text.RegularExpressions;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 板。适用如下形式：
    /// <br/><see cref="PatternCollection.PL_1"/>：<inheritdoc cref="PatternCollection.PL_1"/>
    /// </summary>
    public class ProfilePlate : ProfileBase {

        /// <summary>
        /// 
        /// </summary>
        public double t, b, l;

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfilePlate() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfilePlate(string profileText) {
            ProfileText = profileText;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"><inheritdoc path="/param[1]"/></param>
        /// <param name="e"><inheritdoc path="/param[2]"/></param>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            var temp = (t, b, l);
            var text = e.NewText;
            try {
                if (string.IsNullOrEmpty(text))
                    throw new UnAcceptableProfileException(text);

                Match match = Regex.Match(text, PatternCollection.PL_1);
                if (!match.Success)
                    throw new UnAcceptableProfileException(text);

                double.TryParse(match.Groups["t"].Value, out t);
                double.TryParse(match.Groups["b"].Value, out b);
                double.TryParse(match.Groups["l"].Value, out l);
            } catch (UnAcceptableProfileException) {
                t = temp.t; b = temp.b; l = temp.l;
                throw;
            }
        }
    }
}
