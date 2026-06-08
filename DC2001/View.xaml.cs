using System;
using System.Windows;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace Muggle.TsExtensions.DC2001 {
    public partial class View : PluginWindowBase {
        private ViewModel ViewModel { get; }

        public View(ViewModel viewModel) {
            InitializeComponent();
            ViewModel = viewModel;
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

        private void LeftCorbelProfileSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            catalog?.SelectedProfile = ViewModel.PartLeftCorbelProfile;
        }

        private void LeftCorbelProfileSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            ViewModel.PartLeftCorbelProfile = catalog?.SelectedProfile;
        }

        private void LeftCorbelMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PartLeftCorbelMaterial;
        }

        private void LeftCorbelMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PartLeftCorbelMaterial = catalog?.SelectedMaterial;
        }

        private void RightCorbelProfileSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            catalog?.SelectedProfile = ViewModel.PartRightCorbelProfile;
        }

        private void RightCorbelProfileSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfProfileCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfProfileCatalog;
            ViewModel.PartRightCorbelProfile = catalog?.SelectedProfile;
        }

        private void RightCorbelMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PartRightCorbelMaterial;
        }

        private void RightCorbelMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PartRightCorbelMaterial = catalog?.SelectedMaterial;
        }

        private void PrimStifMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PlatePrimStifMaterial;
        }

        private void PrimStifMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlatePrimStifMaterial = catalog?.SelectedMaterial;
        }

        private void LeftPadMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PlateLeftPadMaterial;
        }

        private void LeftPadMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateLeftPadMaterial = catalog?.SelectedMaterial;
        }

        private void RightPadMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PlateRightPadMaterial;
        }

        private void RightPadMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateRightPadMaterial = catalog?.SelectedMaterial;
        }

        private void LeftCorbelStifMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PlateLCorbelStifMaterial;
        }

        private void LeftCorbelStifMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateLCorbelStifMaterial = catalog?.SelectedMaterial;
        }

        private void RightCorbelStifMaterialSelectClicked(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            catalog?.SelectedMaterial = ViewModel.PlateRCorbelStifMaterial;
        }

        private void RightCorbelStifMaterialSelectionDone(object sender, EventArgs e) {
            while (sender is not WpfMaterialCatalog) {
                sender = LogicalTreeHelper.GetParent(sender as DependencyObject);
            }

            var catalog = sender as WpfMaterialCatalog;
            ViewModel.PlateRCorbelStifMaterial = catalog?.SelectedMaterial;
        }
    }
}