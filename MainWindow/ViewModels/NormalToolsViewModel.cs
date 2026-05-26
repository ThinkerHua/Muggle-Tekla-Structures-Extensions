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
 *  NormalToolsViewModel.cs: view model for the normal tools.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Internal;
using Muggle.TsExtensions.Common.ModelUI;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using TSMUI = Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class NormalToolsViewModel : ViewModelBase {
        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly TSMUI.ModelObjectSelector uiSelector = new TSMUI.ModelObjectSelector();

        private readonly IMessageBoxService messageBoxService;

        public NormalToolsViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void ShowModelObjectCoordinateSystem() {
            ModelObject obj;
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                while (true) {
                    obj = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT);
                    if (obj is PolyBeam polyBeam) {
                        var css = polyBeam.GetPolybeamCoordinateSystems();
                        foreach (CoordinateSystem cs in css) {
                            Internal.ShowCoordinateSystem(cs);
                        }
                    } else {
                        Internal.ShowCoordinateSystem(obj.GetCoordinateSystem());
                    }
                }
            } catch (Exception e) when (e.Message == App.UserInterrupt) {

            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
            }
        }

        [RelayCommand]
        private void SelectWeldedObjects() {
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                var weld = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_WELD) as BaseWeld;
                var parts = new ArrayList {
                    weld.MainObject,
                    weld.SecondaryObject,
                };

                uiSelector.Select(parts, false);
                model.CommitChanges();
            } catch (Exception e) when (e.Message == App.UserInterrupt) {

            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
            }
        }

        [RelayCommand]
        private void ReorderContourPoints() {
            ContourPlate plate;
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                plate = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select a contour plate:") as ContourPlate;
            } catch (Exception e) when (e.Message == App.UserInterrupt) {
                return;
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
                return;
            }

            if (plate == null) {
                Operation.DisplayPrompt("No contour plate selected.");
                return;
            }
            var point = picker.PickPoint("Select a point as the first point:");

            var contour = plate.Contour.ContourPoints.Cast<ContourPoint>();
            var index = contour
                .Select((p, i) => (i, Distance.PointToPoint(p, point)))
                .OrderBy(item => item.Item2).First().i;

            var newContour = contour.Skip(index).Concat(contour.Take(index)).ToList();
            plate.Contour.ContourPoints = new ArrayList(newContour);
            plate.Modify();
            model.CommitChanges();
        }

        [RelayCommand]
        private void ShowContourOrder() {
            ContourPlate plate;
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                plate = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select a contour plate:") as ContourPlate;
            } catch (Exception e) when (e.Message == App.UserInterrupt) {
                return;
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
                return;
            }

            if (plate == null) {
                Operation.DisplayPrompt("No contour plate selected.");
                return;
            }

            var drawer = new GraphicsDrawer();
            var cnt = -1;
            foreach (ContourPoint p in plate.Contour.ContourPoints) {
                ++cnt;
                drawer.DrawText(p, cnt.ToString(), ColorExtension.DarkRed);
            }
        }

        [RelayCommand]
        private void CopyWithDirection() {
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
                return;
            }

            var objectEnumerator = uiSelector.GetSelectedObjects();
            if (objectEnumerator == null || objectEnumerator.GetSize() == 0) {
                Operation.DisplayPrompt("No objects selected.");
                return;
            }

            var selectedObjects = new List<ModelObject>();
            BaseComponent component;
            foreach (ModelObject modelObject in objectEnumerator) {
                component = modelObject.GetFatherComponent();
                if (component == null) {
                    selectedObjects.Add(modelObject);
                }
            }

            Point origin, directionPoint;
            try {
                origin = picker.PickPoint("Select the source origin point:");
                directionPoint = picker.PickPoint("Select the source direction point:", origin);
            } catch {
                return;
            }
            var direction = new Vector(directionPoint - origin);
            if (direction.IsZero()) {
                Operation.DisplayPrompt("The direction vector cannot be zero.");
                return;
            }

            var axisX = new Vector(1000.0, 0.0, 0.0);
            var axisZ = new Vector(0.0, 0.0, 1000.0);
            CoordinateSystem sourceCS;
            if (Parallel.VectorToVector(direction, axisZ)) {
                sourceCS = new CoordinateSystem(origin, axisX, direction.Cross(axisX));
            } else {
                sourceCS = new CoordinateSystem(origin, direction, axisZ.Cross(direction));
            }

            Point targetOrigin;
            Point targetDirectionPoint;
            Vector targetDirection;
            CoordinateSystem targetCS;
            while (true) {
                try {
                    targetOrigin = picker.PickPoint("Select the target origin point:", directionPoint);
                    targetDirectionPoint = picker.PickPoint("Select the target direction point:", targetOrigin);
                } catch {
                    return;
                }

                targetDirection = new Vector(targetDirectionPoint - targetOrigin);
                if (targetDirection.IsZero()) {
                    Operation.DisplayPrompt("The target direction vector cannot be zero.");
                    continue;
                }

                if (Parallel.VectorToVector(targetDirection, axisZ)) {
                    targetCS = new CoordinateSystem(targetOrigin, axisX, targetDirection.Cross(axisX));
                } else {
                    targetCS = new CoordinateSystem(targetOrigin, targetDirection, axisZ.Cross(targetDirection));
                }

                foreach (ModelObject obj in selectedObjects) {
                    Operation.CopyObject(obj, sourceCS, targetCS);
                }

                model.CommitChanges();
            }
        }

        [RelayCommand]
        private void LocateToPrecisePosition() {
            //  微小的位移会被忽略，需要先移动较大的距离，再移回去
            void Move(Point point) {
                point.X = Math.Round(point.X, 0) + 10;
                point.Y = Math.Round(point.Y, 0) + 10;
                point.Z = Math.Round(point.Z, 0) + 10;
            }

            void MoveBack(Point point) {
                point.X -= 10;
                point.Y -= 10;
                point.Z -= 10;
            }


            ModelObjectEnumerator parts;
            List<Beam> beams = [];
            try {
                if (!model.GetConnectionStatus()) throw new InvalidOperationException(App.NotConnected);

                parts = uiSelector.GetSelectedObjects();
                if (parts == null || parts.GetSize() == 0) {
                    parts = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                    if (parts == null || parts.GetSize() == 0) {
                        throw new Exception("No parts were selected.");
                    }
                }

                foreach (var part in parts) {
                    if (part is Beam beam) {
                        Move(beam.StartPoint);
                        Move(beam.EndPoint);
                        beam.Modify();

                        beams.Add(beam);
                    }
                }

                model.CommitChanges();

                foreach (var beam in beams) {
                    MoveBack(beam.StartPoint);
                    MoveBack(beam.EndPoint);
                    beam.Modify();
                }

                model.CommitChanges();
            } catch (Exception e) when (e.Message == "User interrupt") {
                return;
            } catch (Exception e) {
                messageBoxService.ShowError(e.ToString());
            }
        }

        [RelayCommand]
        private void AlignColumnControlPointsWithCentroid() {
            var columns = uiSelector.GetSelectedObjects();

            if (columns.GetSize() == 0) {
                try {
                    columns = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, "Pick columns:");
                } catch (Exception e) when (e.Message == App.UserInterrupt) {
                    return;
                } catch (Exception e) {
                    messageBoxService.ShowError(e.ToString());
                }
            }

            if (columns.GetSize() == 0) return;

            foreach (var item in columns) {
                if (item is not Beam column) continue;

                var centerLine = column.GetCenterLine(false);
                var startPoint = centerLine[0] as Point;
                var endPoint = centerLine[centerLine.Count - 1] as Point;

                column.Position.Depth = Position.DepthEnum.MIDDLE;
                column.Position.Plane = Position.PlaneEnum.MIDDLE;

                column.StartPoint = startPoint;
                column.EndPoint = endPoint;

                column.Modify();
            }

            model.CommitChanges();
        }
    }
}
