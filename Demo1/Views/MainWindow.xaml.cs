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

        private void PartPrimaryProfileSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            catalog.SelectedProfile = ViewModel.PartPrimaryProfile;
        }

        private void PartPrimaryProfileSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            ViewModel.PartPrimaryProfile = catalog.SelectedProfile;
        }

        private void PartPrimaryMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.PartPrimaryMaterial;
        }

        private void PartPrimaryMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PartPrimaryMaterial = catalog.SelectedMaterial;
        }

        private void PartSecondaryProfileSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            catalog.SelectedProfile = ViewModel.PartSecondaryProfile;
        }

        private void PartSecondaryProfileSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfProfileCatalog;
            ViewModel.PartSecondaryProfile = catalog.SelectedProfile;
        }

        private void PartSecondaryMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.PartSecondaryMaterial;
        }

        private void PartSecondaryMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PartSecondaryMaterial = catalog.SelectedMaterial;
        }

        private void PlateStiffenerMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.PlateStiffenerMaterial;
        }

        private void PlateStiffenerMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateStiffenerMaterial = catalog.SelectedMaterial;
        }

        private void PlateSpliceMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            catalog.SelectedMaterial = ViewModel.PlateSpliceMaterial;
        }

        private void PlateSpliceMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }
            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateSpliceMaterial = catalog.SelectedMaterial;
        }
    }
}