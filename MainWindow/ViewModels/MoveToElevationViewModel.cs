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
 *  MoveToElevationViewModel.cs: view model for the move to elevation operation.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Dialog;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using UISelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class MoveToElevationViewModel : ViewModelBase {
        public enum ElevationTypeEnum {
            Top,
            Bottom,
            Middle
        }

        private readonly TransformationPlane globalTP = new TransformationPlane();
        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly UISelector uiSelector = new UISelector();
        private readonly IMessageBoxService messageBoxService;

        private readonly Localization localLization = App.Current.Localization;

        [ObservableProperty]
        private double targetElevation = 0.0;

        [ObservableProperty]
        private bool isAbsolutely = true;

        [ObservableProperty]
        private ElevationTypeEnum elevationType = ElevationTypeEnum.Top;

        public MoveToElevationViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void MoveToElevation() {
            if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NOT_CONNECTED);

            var mobjects = uiSelector.GetSelectedObjects();
            if (mobjects.GetSize() == 0) {
                try {
                    mobjects = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS, localLization.GetText("prompt_Pick_objects"));
                } catch (Exception e) when (e.Message == App.USER_INTERRUPT) {
                    return;
                } catch (Exception e) {
                    messageBoxService.ShowError(e.ToString());
                    return;
                }
            }

            if (mobjects.GetSize() == 0) return;

            var elevation = 0.0;
            foreach (ModelObject obj in mobjects) {
                switch (ElevationType) {
                case ElevationTypeEnum.Top:
                    if (!obj.GetLevel(IsAbsolutely, true, out elevation)) continue;
                    break;
                case ElevationTypeEnum.Bottom:
                    if (!obj.GetLevel(IsAbsolutely, false, out elevation)) continue;
                    break;
                case ElevationTypeEnum.Middle:
                    if (!obj.GetLevel(IsAbsolutely, true, out elevation)) continue;
                    var bottomElevation = 0.0;
                    if (!obj.GetLevel(IsAbsolutely, false, out bottomElevation)) continue;

                    elevation = (elevation + bottomElevation) * 0.5;
                    break;
                default:
                    break;
                }

                var vector = new Vector(0.0, 0.0, TargetElevation - elevation);
                if (IsAbsolutely) vector = vector.TransformFrom(globalTP);
                Operation.MoveObject(obj, vector);
            }

            model.CommitChanges();
        }
    }
}
