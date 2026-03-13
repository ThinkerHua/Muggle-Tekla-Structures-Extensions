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
 *  MainWindow.xaml.cs: code behind for main window of KJ1001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Windows;
using Muggle.TsExtensions.KJ1001.ViewModels;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace Muggle.TsExtensions.KJ1001.Views {
    /// <summary>
    /// Interaction logic for MainPluginWindow.xaml
    /// </summary>
    public partial class MainWindow : PluginWindowBase {
        // define event
        private readonly MainWindowViewModel _dataModel;

        public MainWindow(MainWindowViewModel DataModel) {
            InitializeComponent();
            _dataModel = DataModel;
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

        private void STF_MATL_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = _dataModel.StiffenerMaterial;
        }

        private void STF_MATL_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            _dataModel.StiffenerMaterial = catalog.SelectedMaterial;
        }

        private void Cover_MATL_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = _dataModel.CoverMaterial;
        }

        private void Cover_MATL_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            _dataModel.CoverMaterial = catalog.SelectedMaterial;
        }

        private void CNXPL_MATL_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = _dataModel.ConnectionPlateMaterial;
        }

        private void CNXPL_MATL_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            _dataModel.ConnectionPlateMaterial = catalog.SelectedMaterial;
        }

        private void ShortBeam_PRF_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfProfileCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            catalog.SelectedProfile = _dataModel.ShortBeamProfile;
        }

        private void ShortBeam_PRF_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfProfileCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            _dataModel.ShortBeamProfile = catalog.SelectedProfile;
        }

        private void ShortBeam_MATL_SelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = _dataModel.ShortBeamMaterial;
        }

        private void ShortBeam_MATL_SelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            _dataModel.ShortBeamMaterial = catalog.SelectedMaterial;
        }
    }
}
