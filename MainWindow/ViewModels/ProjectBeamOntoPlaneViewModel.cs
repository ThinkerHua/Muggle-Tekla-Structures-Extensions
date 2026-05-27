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
 *  ProjectBeamOntoPlaneViewModel.cs: view model for the project beam onto plane operation.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ProjectBeamOntoPlaneViewModel : ViewModelBase {
        public enum ProjectionDirectionEnum {
            Perpendicular,
            GlobalZ,
        }

        public enum WayToGetPlaneEnum {
            PickFace,
            Pick3Points,
        }

        private Model Model { get; }
        private Picker Picker { get; }
        private Tekla.Structures.Model.UI.ModelObjectSelector UiSelector { get; }
        private IMessageBoxService MessageBox { get; }

        [ObservableProperty] private ProjectionDirectionEnum _projectionDirection;

        [ObservableProperty] private WayToGetPlaneEnum _wayToGetPlane;

        public ProjectBeamOntoPlaneViewModel(IMessageBoxService messageBox) {
            ProjectionDirection = ProjectionDirectionEnum.GlobalZ;
            WayToGetPlane = WayToGetPlaneEnum.PickFace;

            Model = new Model();
            Picker = new Picker();
            UiSelector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            MessageBox = messageBox;
        }

        [RelayCommand]
        private void ProjectBeamOntoPlane() {
            var currentTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try {
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());

                var selectedObjects = UiSelector.GetSelectedObjects();
                if (selectedObjects.GetSize() == 0) {
                    try {
                        selectedObjects = Picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                    } catch (Exception e) when (e.Message == App.UserInterrupt) {
                        return;
                    } catch (Exception e) {
                        MessageBox.ShowError(e.ToString());
                        return;
                    }
                }

                if (selectedObjects.GetSize() == 0) return;

                Point[] points = null;
                try {
                    switch (WayToGetPlane) {
                    case WayToGetPlaneEnum.PickFace:
                        var input = Picker.PickFace();

                        foreach (InputItem item in input) {
                            if (item.GetInputType() != InputItem.InputTypeEnum.INPUT_POLYGON) continue;
                            points = [.. (item.GetData() as ArrayList)!.Cast<Point>()];
                            break;
                        }

                        break;
                    case WayToGetPlaneEnum.Pick3Points:
                    default:
                        points = new Point[3];

                        points[0] = Picker.PickPoint();
                        for (int i = 1; i < 3; i++) {
                            points[i] = Picker.PickPoint(points[i - 1]);
                        }

                        break;
                    }
                } catch (Exception e) when (e.Message == App.UserInterrupt) {
                    return;
                } catch (Exception e) {
                    MessageBox.ShowError(e.ToString());
                    return;
                }

                var plane = new GeometricPlane(
                    points![0], new Vector(points[1] - points[0]), new Vector(points[2] - points[0]));

                var direction = ProjectionDirection switch {
                    ProjectionDirectionEnum.Perpendicular => plane.Normal,
                    _ => new Vector(0, 0, 1000),
                };

                foreach (var selectedObject in selectedObjects) {
                    if (selectedObject is not Beam beam) continue;
                    beam.StartPoint = IntersectionPoint(beam.StartPoint, direction, plane);
                    beam.EndPoint = IntersectionPoint(beam.EndPoint, direction, plane);
                    beam.Modify();
                }

                Model.CommitChanges();
            } catch {
                // ignored
            } finally {
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTp);
            }
        }

        private static Point IntersectionPoint(Point point, Vector vector, GeometricPlane plane) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            if (plane is null) {
                throw new ArgumentNullException(nameof(plane));
            }

            var line = new Line(point, vector);

            return Intersection.LineToPlane(line, plane);
        }
    }
}