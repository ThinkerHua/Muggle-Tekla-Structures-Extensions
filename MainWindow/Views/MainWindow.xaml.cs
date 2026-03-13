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
 *  MainWindow.xaml.cs: code behind for the view of all tools and plugins.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.MainWindow.Services;
using Muggle.TsExtensions.MainWindow.ViewModels;
using Tekla.Structures.Dialog;

namespace Muggle.TsExtensions.MainWindow.Views {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowBase {
        public MainWindow(MainWindowViewModel mainWindowViewModel, INavigationService navigationService) {
            InitializeComponent();

            navigationService.SetFrame(frame);
            DataContext = mainWindowViewModel;
        }
    }
}
