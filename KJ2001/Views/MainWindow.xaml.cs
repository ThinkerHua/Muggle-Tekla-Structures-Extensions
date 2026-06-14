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
 *  MainWindow.xaml.cs: code behind for main window of KJ2001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Muggle.TsExtensions.Common.WPF.Localization;
using Muggle.TsExtensions.KJ2001.ViewModels;
using Tekla.Structures.Dialog;

namespace Muggle.TsExtensions.KJ2001.Views {
    /// <summary>
    /// Interaction logic for MainPluginWindow.xaml
    /// </summary>
    public partial class MainWindow : PluginWindowBase {
        // define event
        public MainWindowViewModel dataModel;

        public MainWindow(MainWindowViewModel DataModel) {
            TranslationService.Instance.ChangeLanguage(Localization.Language);

            InitializeComponent();
            dataModel = DataModel;
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

        private void BasePlateMaterialCatalogClicked(object sender, EventArgs e) {
            mctl_basePlateMaterial.SelectedMaterial = tbox_basePlateMaterial.Text;
        }
        private void BasePlateMaterialCatalogSelectionDone(object sender, EventArgs e) {
            tbox_basePlateMaterial.Text = mctl_basePlateMaterial.SelectedMaterial;
        }

        private void AnchorRodMaterialCatalogClicked(object sender, EventArgs e) {
            mctl_anchorRodMaterial.SelectedMaterial = tbox_anchorRodMaterial.Text;
        }
        private void AnchorRodMaterialCatalogSelectionDone(object sender, EventArgs e) {
            tbox_anchorRodMaterial.Text = mctl_anchorRodMaterial.SelectedMaterial;
        }

        private void WasherPlateMaterialCatalogClicked(object sender, EventArgs e) {
            mctl_washerPlateMaterial.SelectedMaterial = tbox_washerPlateMaterial.Text;
        }
        private void WasherPlateMaterialCatalogSelectionDone(object sender, EventArgs e) {
            tbox_washerPlateMaterial.Text = mctl_washerPlateMaterial.SelectedMaterial;
        }

        private void InnerStiffenerMaterialClicked(object sender, EventArgs e) {
            mctl_innerStiffenerMaterial.SelectedMaterial = tbox_innerStiffenerMaterial.Text;
        }

        private void InnerStiffenerMaterialSelectionDone(object sender, EventArgs e) {
            tbox_innerStiffenerMaterial.Text = mctl_innerStiffenerMaterial.SelectedMaterial;
        }
    }
}
