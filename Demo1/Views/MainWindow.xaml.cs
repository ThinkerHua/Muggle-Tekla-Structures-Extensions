using System;
using System.Windows;
using Muggle.TsExtensions.Demo1.ViewModels;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace Muggle.TsExtensions.Demo1.Views {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : PluginWindowBase {
        private MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel dataContext) {
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

        private void Plate1MaterialSelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.Plate1Material;
        }

        private void Plate1MaterialSelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.Plate1Material = catalog.SelectedMaterial;
        }

        private void Plate2MaterialSelectClicked(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.Plate2Material;
        }

        private void Plate2MaterialSelectionDone(object sender, EventArgs e) {
            while (!(sender is WpfMaterialCatalog)) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.Plate2Material = catalog.SelectedMaterial;
        }
    }
}