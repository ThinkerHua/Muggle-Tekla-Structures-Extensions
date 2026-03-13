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
 *  UnAcceptableProfileException.cs: exception of unacceptable profile
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;

namespace Muggle.TsExtensions.Common.Profile {
    /// <summary>
    /// 不支持的截面或不支持的截面参数引发的异常。
    /// </summary>
    public class UnAcceptableProfileException : Exception {

        /// <summary>
        /// 创建引发异常的信息为空的实例。
        /// </summary>
        public UnAcceptableProfileException() { }

        /// <summary>
        /// 根据给定引发异常的信息创建实例。
        /// </summary>
        /// <param name="message">引发异常的信息</param>
        public UnAcceptableProfileException(string message)
            : base(string.Format("不支持此类型截面 或 不支持此类型截面的当前参数：{0}", message)) {

        }
    }
}
