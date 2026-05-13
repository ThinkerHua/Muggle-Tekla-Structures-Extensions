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
 *  BoltAssembly.xaml.cs: code behind for BoltAssembly user control.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Muggle.TsExtensions.Common.WPF.Controls {
    /// <summary>
    /// BoltAssembly.xaml 的交互逻辑
    /// </summary>
    public partial class BoltAssembly : UserControl {
        public BoltAssembly() {
            InitializeComponent();
            MouseLeftButtonDown += ChangeState;
        }

        /// <summary>
        /// Indicates whether the instance is a bolt or just a hole. 
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the instance is a bolt or just a hole.")]
        public bool Bolt {
            get { return (bool)GetValue(BoltProperty); }
            set { SetValue(BoltProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bolt.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoltProperty =
            DependencyProperty.Register(nameof(Bolt), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the washer 1 is used in the assembly. 
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the washer 1 is used in the assembly.")]
        public bool Washer1 {
            get { return (bool)GetValue(Washer1Property); }
            set { SetValue(Washer1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Washer1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Washer1Property =
            DependencyProperty.Register(nameof(Washer1), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the washer 2 is used in the assembly. 
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the washer 2 is used in the assembly.")]
        public bool Washer2 {
            get { return (bool)GetValue(Washer2Property); }
            set { SetValue(Washer2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Washer2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Washer2Property =
            DependencyProperty.Register(nameof(Washer2), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the washer 3 is used in the assembly. 
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the washer 3 is used in the assembly.")]
        public bool Washer3 {
            get { return (bool)GetValue(Washer3Property); }
            set { SetValue(Washer3Property, value); }
        }

        // Using a DependencyProperty as the backing store for Washer3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Washer3Property =
            DependencyProperty.Register(nameof(Washer3), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the nut 1 is used in the assembly.
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the nut 1 is used in the assembly.")]
        public bool Nut1 {
            get { return (bool)GetValue(Nut1Property); }
            set { SetValue(Nut1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Nut1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Nut1Property =
            DependencyProperty.Register(nameof(Nut1), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));

        /// <summary>
        /// Indicates whether the nut 2 is used in the assembly.
        /// </summary>
        [Category("Assembly"), Description("Indicates whether the nut 2 is used in the assembly.")]
        public bool Nut2 {
            get { return (bool)GetValue(Nut2Property); }
            set { SetValue(Nut2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Nut2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Nut2Property =
            DependencyProperty.Register(nameof(Nut2), typeof(bool), typeof(BoltAssembly), new PropertyMetadata(false));


        private void ChangeState(object sender, MouseButtonEventArgs e) {
            var control = (BoltAssembly)sender;
            var height = control.ActualHeight;

            var proportion = e.GetPosition(control).Y / height;

            if (proportion > 0 / 130.0 && proportion < 16 / 130.0) Bolt = !Bolt;
            if (proportion > 18 / 130.0 && proportion < 36 / 130.0) Washer1 = !Washer1;
            if (proportion > 37 / 130.0 && proportion < 55 / 130.0) Washer2 = !Washer2;
            if (proportion > 57 / 130.0 && proportion < 79 / 130.0) Washer3 = !Washer3;
            if (proportion > 81 / 130.0 && proportion < 97 / 130.0) Nut1 = !Nut1;
            if (proportion > 99 / 130.0 && proportion < 115 / 130.0) Nut2 = !Nut2;
            if (proportion > 115 / 130.0 && proportion < 130 / 130.0) Bolt = !Bolt;
        }
    }
}
