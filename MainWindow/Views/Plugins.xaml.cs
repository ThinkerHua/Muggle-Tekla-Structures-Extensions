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
 *  Plugins.xaml.cs: code behind for the view of Plugins.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Muggle.TsExtensions.MainWindow.ViewModels;

namespace Muggle.TsExtensions.MainWindow.Views {
    /// <summary>
    /// Plugins.xaml 的交互逻辑
    /// </summary>
    public partial class Plugins : Page {
        public Plugins() {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<PluginsViewModel>();
        }
    }
}
