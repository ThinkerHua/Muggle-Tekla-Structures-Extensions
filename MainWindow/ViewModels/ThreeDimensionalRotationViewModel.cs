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
 *  ThreeDimensionalRotationViewModel.cs: view model for the ThreeDimensionalRotation tool.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ThreeDimensionalRotationViewModel : ViewModelBase {

        public enum Axis {
            PartCsAxisX,
            PartCsAxisY,
            PartCsAxisZ,
            ManualSelect,
        }

        public enum Angle {
            AngleValue,
            ManualSelect,
        }

        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly TSMUI.ModelObjectSelector uiSelector = new TSMUI.ModelObjectSelector();

        private readonly IMessageBoxService messageBoxService;

        [ObservableProperty]
        private Axis axisType = Axis.PartCsAxisX;

        [ObservableProperty]
        private Angle angleType = Angle.AngleValue;

        [ObservableProperty]
        private double degrees = 90.0;

        [ObservableProperty]
        private bool targetDirectionIsNormalOfPlane = true;

        public ThreeDimensionalRotationViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void ThreeDimensionalRotation() {
            try {
                while (true) {
                    Action(AxisType, AngleType, Degrees, TargetDirectionIsNormalOfPlane);
                }
            } catch (Exception e) when (e.Message == App.UserInterrupt) {

            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
            }
        }

        private void Action(Axis axis, Angle angle, double degrees, bool targetDirectionIsNormalOfPlane) {
            if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

            var selectedObjects = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS, "选择要旋转的对象。");
            if (selectedObjects == null || selectedObjects.GetSize() == 0) {
                Operation.DisplayPrompt("没有选择对象。");
                return;
            }

            ModelObject modelObject;
            CoordinateSystem partCS;
            Line line = null;
            Matrix matrix = null;
            Point origin = null, directionPoint = null;
            switch (axis) {
            case Axis.ManualSelect:
                origin = picker.PickPoint("选择旋转轴的起点。");
                directionPoint = picker.PickPoint("选择旋转轴的方向。", origin);
                var direction = new Vector(directionPoint - origin);
                if (direction.IsZero()) {
                    Operation.DisplayPrompt("旋转轴的方向不能为零向量。");
                    return;
                }

                line = new Line(origin, direction);
                break;
            default:
                selectedObjects.MoveNext();
                modelObject = selectedObjects.Current;
                selectedObjects.Reset();

                partCS = modelObject.GetCoordinateSystem();
                switch (axis) {
                case Axis.PartCsAxisX:
                    line = new Line(partCS.Origin, partCS.AxisX);
                    break;
                case Axis.PartCsAxisY:
                    line = new Line(partCS.Origin, partCS.AxisY);
                    break;
                case Axis.PartCsAxisZ:
                    line = new Line(partCS.Origin, partCS.AxisX.Cross(partCS.AxisY));
                    break;
                default:
                    break;
                }
                break;
            }

            switch (angle) {
            case Angle.AngleValue:
                matrix = MatrixFactoryExtension.Rotate(line, degrees * Math.PI / 180.0);
                break;
            case Angle.ManualSelect:
                var pointStart = picker.PickPoint("选择旋转起始方向。", directionPoint);
                var directionStart = new Vector(pointStart - Projection.PointToLine(pointStart, line));
                if (directionStart.IsZero()) {
                    Operation.DisplayPrompt("旋转起始方向不能为零向量。");
                    return;
                }
                Vector directionEnd = null;

                if (!targetDirectionIsNormalOfPlane) {
                    var pointEnd = picker.PickPoint("选择旋转结束方向。", pointStart);
                    directionEnd = new Vector(pointEnd - Projection.PointToLine(pointEnd, line));
                    if (directionEnd.IsZero()) {
                        Operation.DisplayPrompt("旋转结束方向不能为零向量。");
                        return;
                    }
                } else {
                    var face = picker.PickFace("选择平面。");
                    var enumerator = face.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        var inputItem = enumerator.Current as InputItem;
                        if (inputItem.GetInputType() == InputItem.InputTypeEnum.INPUT_POLYGON) {
                            var points = inputItem.GetData() as ArrayList;
                            directionEnd = new Vector((points[2] as Point) - (points[1] as Point))
                                .Cross(new Vector((points[0] as Point) - (points[1] as Point)));

                            break;
                        }
                    }
                }

                var radians = directionStart.GetAngleBetweenWithDirection(directionEnd, line.Direction);
                matrix = MatrixFactoryExtension.Rotate(line, radians);
                break;
            default:
                break;
            }

            foreach (ModelObject obj in selectedObjects) {
                ModelOperation.MoveObject(obj, matrix);
            }

            model.CommitChanges();
        }
    }
}
