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
 *  BoltSpecialHole.xaml.cs: code behind for BoltSpecialHole user control.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Muggle.TsExtensions.Common.WPF.Controls {
    /// <summary>
    /// BoltSpecialHole.xaml 的交互逻辑
    /// </summary>
    public partial class BoltSpecialHole : UserControl {
        public BoltSpecialHole() {
            InitializeComponent();
            MouseLeftButtonDown += ChangeState;
        }

        /// <summary>
        /// Indicates whether the hole 1 is used.
        /// </summary>
        [Category("Holes"), Description("Indicates whether the hole 1 is used.")]
        public bool SpecialHole1 {
            get { return (bool)GetValue(SpecialHole1Property); }
            set { SetValue(SpecialHole1Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpecialHole1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecialHole1Property =
            DependencyProperty.Register(nameof(SpecialHole1), typeof(bool), typeof(BoltSpecialHole), new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether the hole 2 is used.
        /// </summary>
        [Category("Holes"), Description("Indicates whether the hole 2 is used.")]
        public bool SpecialHole2 {
            get { return (bool)GetValue(SpecialHole2Property); }
            set { SetValue(SpecialHole2Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpecialHole2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecialHole2Property =
            DependencyProperty.Register(nameof(SpecialHole2), typeof(bool), typeof(BoltSpecialHole), new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether the hole 3 is used.
        /// </summary>
        [Category("Holes"), Description("Indicates whether the hole 3 is used.")]
        public bool SpecialHole3 {
            get { return (bool)GetValue(SpecialHole3Property); }
            set { SetValue(SpecialHole3Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpecialHole3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecialHole3Property =
            DependencyProperty.Register(nameof(SpecialHole3), typeof(bool), typeof(BoltSpecialHole), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the hole 4 is used.
        /// </summary>
        [Category("Holes"), Description("Indicates whether the hole 4 is used.")]
        public bool SpecialHole4 {
            get { return (bool)GetValue(SpecialHole4Property); }
            set { SetValue(SpecialHole4Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpecialHole4.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecialHole4Property =
            DependencyProperty.Register(nameof(SpecialHole4), typeof(bool), typeof(BoltSpecialHole), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the hole 5 is used.
        /// </summary>
        [Category("Holes"), Description("Indicates whether the hole 5 is used.")]
        public bool SpecialHole5 {
            get { return (bool)GetValue(SpecialHole5Property); }
            set { SetValue(SpecialHole5Property, value); }
        }

        // Using a DependencyProperty as the backing store for SpecialHole5.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecialHole5Property =
            DependencyProperty.Register(nameof(SpecialHole5), typeof(bool), typeof(BoltSpecialHole), new PropertyMetadata(false));


        private void ChangeState(object sender, MouseButtonEventArgs e) {
            var control = (BoltSpecialHole)sender;
            var height = control.ActualHeight;

            var proportion = e.GetPosition(control).Y / height;

            if (proportion > 18 / 109.0 && proportion < 28 / 109.0) SpecialHole1 = !SpecialHole1;
            if (proportion > 30 / 109.0 && proportion < 40 / 109.0) SpecialHole2 = !SpecialHole2;
            if (proportion > 42 / 109.0 && proportion < 52 / 109.0) SpecialHole3 = !SpecialHole3;
            if (proportion > 54 / 109.0 && proportion < 64 / 109.0) SpecialHole4 = !SpecialHole4;
            if (proportion > 66 / 109.0 && proportion < 76 / 109.0) SpecialHole5 = !SpecialHole5;
        }
    }
}
