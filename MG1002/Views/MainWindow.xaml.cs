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
 *  MainWindow.xaml.cs: code behind for main window of MG1002
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Windows;
using Muggle.TsExtensions.Common.WPF.Localization;
using Muggle.TsExtensions.MG1002.ViewModels;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace Muggle.TsExtensions.MG1002.Views {
    public partial class MainWindow : PluginWindowBase {
        private readonly MainWindowViewModel _viewModel;
        public MainWindow(MainWindowViewModel viewModel) {
            TranslationService.Instance.ChangeLanguage(Localization.Language);

            InitializeComponent();
            _viewModel = viewModel;
        }

        private void WPFOkApplyModifyGetOnOffCancel_ApplyClicked(object sender, EventArgs e) {
            this.Apply();
        }

        private void WPFOkApplyModifyGetOnOffCancel_CancelClicked(object sender, EventArgs e) {
            this.Close();
        }

        private void WPFOkApplyModifyGetOnOffCancel_GetClicked(object sender, EventArgs e) {
            this.Get();
        }

        private void WPFOkApplyModifyGetOnOffCancel_ModifyClicked(object sender, EventArgs e) {
            this.Modify();
        }

        private void WPFOkApplyModifyGetOnOffCancel_OkClicked(object sender, EventArgs e) {
            this.Apply();
            this.Close();
        }

        private void WPFOkApplyModifyGetOnOffCancel_OnOffClicked(object sender, EventArgs e) {
            this.ToggleSelection();
        }

        private void WpfMaterialCatalog_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = _viewModel.Material;
        }

        private void WpfMaterialCatalog_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            _viewModel.Material = catalog.SelectedMaterial;
        }
    }
}
