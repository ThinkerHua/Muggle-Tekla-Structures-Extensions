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
 *  ProfileRect_Invariant.cs: profile of invariant rect
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 矩形管（等截面形式）。适用如下形式：
    /// <br/><see cref="PatternCollection.CFH_J_1"/>：<inheritdoc cref="PatternCollection.CFH_J_1"/>
    /// <br/><see cref="PatternCollection.CFH_J_2"/>：<inheritdoc cref="PatternCollection.CFH_J_2"/>
    /// <br/><see cref="PatternCollection.CFH_J_3"/>：<inheritdoc cref="PatternCollection.CFH_J_3"/>
    /// <br/><see cref="PatternCollection.RECT_1"/>：<inheritdoc cref="PatternCollection.RECT_1"/>
    /// <br/><see cref="PatternCollection.RECT_2"/>：<inheritdoc cref="PatternCollection.RECT_2"/>
    /// <br/><see cref="PatternCollection.RECT_3"/>：<inheritdoc cref="PatternCollection.RECT_3"/>
    /// <br/><see cref="PatternCollection.RECT_4"/>：<inheritdoc cref="PatternCollection.RECT_4"/>
    /// <br/><see cref="PatternCollection.RECT_5"/>：<inheritdoc cref="PatternCollection.RECT_5"/>
    /// </summary>
    public class ProfileRect_Invariant : ProfileRect {

        /// <summary>
        /// 创建各字段值均为 0.0 的实例。
        /// </summary>
        public ProfileRect_Invariant() : base() { }

        /// <summary>
        /// 根据给定截面文本创建实例，同时为字段赋值。
        /// </summary>
        /// <param name="profileText">给定截面文本</param>
        public ProfileRect_Invariant(string profileText) : base(profileText) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sender"><inheritdoc path="/param[1]"/></param>
        /// <param name="e"><inheritdoc path="/param[2]"/></param>
        protected override void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e) {
            base.SetFieldsValue(sender, e);
            if (h2 != h1 || b2 != b1)
                throw new UnAcceptableProfileException(e.NewText);
        }
    }
}
