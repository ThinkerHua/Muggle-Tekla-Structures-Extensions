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
 *  ProfileBase.cs: the base class for all profile classes
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 截面文本变更事件参数。
    /// </summary>
    public class ProfileTextChangingEventArgs : EventArgs {
        /// <summary>
        /// 当前截面文本。
        /// </summary>
        public string CurrentText { get; }

        /// <summary>
        /// 新截面文本。
        /// </summary>
        public string NewText { get; }

        /// <summary>
        /// 使用当前和新截面文本创建事件参数实例。
        /// </summary>
        /// <param name="currentText">当前截面文本</param>
        /// <param name="newText">新截面文本</param>
        public ProfileTextChangingEventArgs(string currentText, string newText) {
            CurrentText = currentText ?? string.Empty;
            NewText = newText ?? string.Empty;
        }
    }

    /// <summary>
    /// 表示将用于处理截面文本变更事件的方法。
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    public delegate void ProfileTextChangingEventHandler(ProfileBase sender, ProfileTextChangingEventArgs e);

    /// <summary>
    /// 定义型材截面的属性、方法、事件。
    /// </summary>
    public abstract class ProfileBase {
        private string _profileText;
        private ProfileTextChangingEventHandler _profileTextChangingEventHandler;

        /// <summary>
        /// 截面文本
        /// </summary>
        public string ProfileText {
            get => _profileText;
            set {
                try {
                    var e = new ProfileTextChangingEventArgs(_profileText, value);
                    OnProfileTextChanging(e);
                    _profileText = value;
                } catch (UnAcceptableProfileException) {
                    //引发UnAcceptableProfileException时，不改变ProfileText
                    throw;
                }
            }
        }

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        /// <remarks>为 <see cref="ProfileTextChanging"/> 事件注册 <see cref="SetFieldsValue"/> 方法。</remarks>
        protected ProfileBase() {
            _profileTextChangingEventHandler += SetFieldsValue;
        }

        /// <summary>
        /// 截面文本变更事件。
        /// </summary>
        public event ProfileTextChangingEventHandler ProfileTextChanging {
            add {
                if (value == SetFieldsValue)
                    _profileTextChangingEventHandler -= value; //避免重复添加
                _profileTextChangingEventHandler += value;
            }
            remove {
                if (value != SetFieldsValue)
                    _profileTextChangingEventHandler -= value; //不允许移除 SetFieldsValue 方法
            }
        }

        /// <summary>
        /// 引发 <see cref="ProfileTextChanging"/> 事件。
        /// </summary>
        /// <param name="e">事件参数</param>
        protected void OnProfileTextChanging(ProfileTextChangingEventArgs e) {
            _profileTextChangingEventHandler?.Invoke(this, e);
        }

        /// <summary>
        /// 设置字段值。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        /// <exception cref="UnAcceptableProfileException">当<paramref name="e"/> 的 
        /// <see cref="ProfileTextChangingEventArgs.NewText"/> 是不支持的截面或不支持的截面参数时引发。</exception>
        protected abstract void SetFieldsValue(ProfileBase sender, ProfileTextChangingEventArgs e);
    }
}
