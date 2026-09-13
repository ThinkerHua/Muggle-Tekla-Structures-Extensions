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
        public enum Elevation {
            Top,
            Bottom,
            Middle
        }

        private readonly TransformationPlane _globalTp = new TransformationPlane();
        private readonly Model _model = new Model();
        private readonly Picker _picker = new Picker();
        private readonly UISelector _uiSelector = new UISelector();
        private readonly IMessageBoxService _messageBoxService;

        private readonly Localization _localization = App.Current.Localization;

        [ObservableProperty]
        private double _targetElevation = 0.0;

        [ObservableProperty]
        private bool _isAbsolutely = true;

        [ObservableProperty]
        private Elevation _elevationType = Elevation.Top;

        public MoveToElevationViewModel(IMessageBoxService messageBoxService) {
            this._messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void MoveToElevation() {
            if (!_model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

            var mobjects = _uiSelector.GetSelectedObjects();
            if (mobjects.GetSize() == 0) {
                try {
                    mobjects = _picker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS, _localization.GetText("prompt_Pick_objects"));
                } catch (Exception e) when (e.Message == App.UserInterrupt) {
                    return;
                } catch (Exception e) {
                    _messageBoxService.ShowError(e.ToString());
                    return;
                }
            }

            if (mobjects.GetSize() == 0) return;

            var elevation = 0.0;
            foreach (ModelObject obj in mobjects) {
                switch (ElevationType) {
                case Elevation.Top:
                    if (!obj.GetLevel(IsAbsolutely, true, out elevation)) continue;
                    break;
                case Elevation.Bottom:
                    if (!obj.GetLevel(IsAbsolutely, false, out elevation)) continue;
                    break;
                case Elevation.Middle:
                    if (!obj.GetLevel(IsAbsolutely, true, out elevation)) continue;
                    if (!obj.GetLevel(IsAbsolutely, false, out var bottomElevation)) continue;

                    elevation = (elevation + bottomElevation) * 0.5;
                    break;
                default:
                    break;
                }

                var vector = new Vector(0.0, 0.0, TargetElevation - elevation);
                if (IsAbsolutely) vector = vector.TransformFrom(_globalTp);
                Operation.MoveObject(obj, vector);
            }

            _model.CommitChanges();
        }
    }
}
