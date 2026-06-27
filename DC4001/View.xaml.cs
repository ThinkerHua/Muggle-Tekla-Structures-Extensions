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
 *  View.xaml.cs: code behind for main window of DC4001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Windows;
using Muggle.TsExtensions.Common.WPF.Localization;
using TsData = Tekla.Structures.Datatype;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace Muggle.TsExtensions.DC4001 {
    public partial class View : PluginWindowBase {
        private ViewModel ViewModel { get; }

        public View(ViewModel dataContext) {
            TranslationService.Instance.ChangeLanguage(Localization.Language);

            InitializeComponent();
            ViewModel = dataContext;
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
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ((TsData.String)ViewModel.GetType().GetProperty(catalog.Tag as string).GetValue(ViewModel)).Value;
        }

        private void WpfMaterialCatalog_SelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.GetType().GetProperty(catalog.Tag as string).SetValue(ViewModel, new TsData.String(catalog?.SelectedMaterial));
        }

        private void WpfProfileCatalog_SelectClicked(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            catalog?.SelectedProfile = ((TsData.String)ViewModel.GetType().GetProperty(catalog.Tag as string).GetValue(ViewModel)).Value;
        }

        private void WpfProfileCatalog_SelectionDone(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            ViewModel.GetType().GetProperty(catalog.Tag as string).SetValue(ViewModel, new TsData.String(catalog?.SelectedProfile));
        }
    }
}