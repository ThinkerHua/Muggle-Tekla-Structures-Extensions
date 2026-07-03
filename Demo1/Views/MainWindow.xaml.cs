using System;
using System.Windows;
using Muggle.TsExtensions.Common.WPF.Localization;
using Muggle.TsExtensions.Demo1.ViewModels;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;
using TsData = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Demo1.Views {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : PluginWindowBase {
        private MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel dataContext) {
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