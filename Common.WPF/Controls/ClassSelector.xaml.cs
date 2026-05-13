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
 *  ClassSelector.xaml.cs: code behind for ClassSelector user control.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Muggle.TsExtensions.Common.WPF.Controls {
    /// <summary>
    /// ClassSelector.xaml 的交互逻辑
    /// </summary>
    public partial class ClassSelector : UserControl {

        /// <summary>
        /// Indicates the selected class value.
        /// </summary>
        [Category("Data"), Description("Indicates the selected class value.")]
        public int Value {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(ClassSelector), new PropertyMetadata(1));

        /// <summary>
        /// The width of the image corresponding to the class value.
        /// </summary>
        [Category("Present"), Description("The width of the image corresponding to the class value.")]
        public double ImageWidth {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(nameof(ImageWidth), typeof(double), typeof(ClassSelector), new PropertyMetadata(38.0));

        /// <summary>
        /// The height of the image corresponding to the class value.
        /// </summary>
        [Category("Present"), Description("The height of the image corresponding to the class value.")]
        public double ImageHeight {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(nameof(ImageHeight), typeof(double), typeof(ClassSelector), new PropertyMetadata(12.0));


        public ClassSelector() {
            InitializeComponent();
        }
    }
}
