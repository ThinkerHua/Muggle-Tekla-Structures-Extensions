/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  SelectBooleans.xaml.cs: code behind for the view of SelectBooleans.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Muggle.TsExtensions.MainWindow.ViewModels;

namespace Muggle.TsExtensions.MainWindow.Views {
    /// <summary>
    /// SelectBooleans.xaml 的交互逻辑
    /// </summary>
    public partial class SelectBooleans : Page {
        public SelectBooleans() {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<SelectBooleansViewModel>();
        }
    }
}
