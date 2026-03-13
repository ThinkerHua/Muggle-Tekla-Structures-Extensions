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
 *  MainWindow.xaml.cs: code behind for main window of MJ5001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Muggle.TsExtensions.MJ5001.ViewModels;
using Tekla.Structures.Dialog;

namespace Muggle.TsExtensions.MJ5001.Views {
    /// <summary>
    /// Interaction logic for MainPluginWindow.xaml
    /// </summary>
    public partial class MainWindow : PluginWindowBase {
        // define event
        public MainWindowViewModel dataModel;

        public MainWindow(MainWindowViewModel DataModel) {
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

        private void WpfSaveLoad_HelpOpenClicked(object sender, EventArgs e) {

        }

        private void AnchorRodMaterial_SelectClicked(object sender, EventArgs e) {
            catalogAnchorRodMaterial.SelectedMaterial = dataModel.AnchorRodMaterial;
            //catalogAnchorRodMaterial.SelectedMaterial = tboxAnchorRodMaterial.Text;
        }

        private void AnchorRodMaterial_SelectionDone(object sender, EventArgs e) {
            dataModel.AnchorRodMaterial = catalogAnchorRodMaterial.SelectedMaterial;
            //tboxAnchorRodMaterial.Text = catalogAnchorRodMaterial.SelectedMaterial;
        }

        private void EmbedmentMaterial_SelectClicked(object sender, EventArgs e) {
            catalogEmbedmentMaterial.SelectedMaterial = dataModel.EmbedmentMaterial;
            //catalogEmbedmentMaterial.SelectedMaterial = tboxEmbedmentMaterial.Text;
        }

        private void EmbedmentMaterial_SelectionDone(object sender, EventArgs e) {
            dataModel.EmbedmentMaterial = catalogEmbedmentMaterial.SelectedMaterial;
            //tboxEmbedmentMaterial.Text = catalogEmbedmentMaterial.SelectedMaterial;
        }

        private void CleatMaterial_SelectClicked(object sender, EventArgs e) {
            catalogCleatMaterial.SelectedMaterial = dataModel.CleatMaterial;
            //catalogCleatMaterial.SelectedMaterial = tboxCleatMaterial.Text;
        }

        private void CleatMaterial_SelectionDone(object sender, EventArgs e) {
            dataModel.CleatMaterial = catalogCleatMaterial.SelectedMaterial;
            //tboxCleatMaterial.Text = catalogCleatMaterial.SelectedMaterial;
        }
    }
}
