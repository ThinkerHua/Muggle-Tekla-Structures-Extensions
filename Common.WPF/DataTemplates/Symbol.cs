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
 *  Symbol.cs: represents a type or enum item, witch has a name and a drawing
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Windows;
using System.Windows.Media;

namespace Muggle.TsExtensions.Common.WPF.DataTemplates {

    /// <summary>
    /// 符号。
    /// </summary>
    public class Symbol : DependencyObject {

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(Symbol), new PropertyMetadata(default(string)));

        /// <summary>
        /// 符号名称。
        /// </summary>
        public string Name {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// 符号图像。
        /// </summary>
        /// <remarks>推荐高度12像素，推荐宽度不超过28像素。</remarks>
        public DrawingGroup Drawing { get; set; }

        /// <summary>
        /// 默认构造函数，<see cref="Name"/> 和 <see cref="Drawing"/> 均为 <see langword="null"/>。
        /// </summary>
        public Symbol() {

        }

        /// <summary>
        /// 使用给定名称和图像创建实例。
        /// </summary>
        /// <param name="name">给定名称。</param>
        /// <param name="drawing">给定图像。</param>
        public Symbol(string name, DrawingGroup drawing) {
            this.Name = name;
            this.Drawing = drawing;
        }
    }
}