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
 *  ExtendBeamViewModel.cs: view model for the extend beam operation.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Task = System.Threading.Tasks.Task;
using UiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ExtendBeamViewModel : ViewModelBase {
        public enum ExtendToColumnPositionEnum {
            MiddlePlane,
            BoundingBoxPlane,
            Surface,
        }

        private readonly string[] BeamNames = ["beam", "梁"];
        private readonly string[] ColumnNames = ["column", "柱"];

        private readonly Model model = new Model();
        private readonly Picker picker = new Picker();
        private readonly UiSelector uiSelector = new UiSelector();

        private readonly IMessageBoxService messageBoxService;

        [ObservableProperty]
        private double _tolerance = 100;

        [ObservableProperty]
        private ExtendToColumnPositionEnum _extendToColumnPosition;

        public ExtendBeamViewModel(IMessageBoxService messageBoxService) {
            this.messageBoxService = messageBoxService;
        }

        [RelayCommand]
        private void Extend() {
            var objects = uiSelector.GetSelectedObjects();
            try {
                if (objects.GetSize() == 0) {
                    objects = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, "prompt_Pick_objects");
                }
            } catch { }
            if (objects.GetSize() == 0) return;

            var taskList = new List<Task>();
            foreach (var obj in objects) {
                if (obj is not Beam beam) continue;
                if (!BeamNames.Contains(beam.Name.ToLower())) continue;

                var beamLine = new Line(beam.StartPoint, beam.EndPoint);

                void action() {
                    beam.StartPoint = DestinationPoint(beam.StartPoint, beam.EndPoint);
                    beam.EndPoint = DestinationPoint(beam.EndPoint, beam.StartPoint);
                    beam.Modify();
                }

                taskList.Add(Task.Run(action));
            }

            Task.WaitAll([.. taskList]);

            model.CommitChanges();
        }

        private Point DestinationPoint(Point sourcePoint, Point anotherPoint) {
            var boundingBoxExtent = new Vector(Tolerance, Tolerance, Tolerance);

            var beamLine = new Line(sourcePoint, anotherPoint);
            var nearbyObjects = model.GetModelObjectSelector().GetObjectsByBoundingBox(sourcePoint - boundingBoxExtent, sourcePoint + boundingBoxExtent);

            var objects = new List<Beam>();
            var beams = new List<Beam>();
            foreach (var item in nearbyObjects) {
                if (item is not Beam beam) continue;

                objects.Add(beam);
            }

            var column = objects.FirstOrDefault(b => ColumnNames.Contains(b.Name.ToLower()));
            if (column != null) {
                return NearestPointOnColumn(sourcePoint, anotherPoint, column);
            }

            foreach (var beam in objects) {
                var partCS = beam.GetCoordinateSystem();
                var gPlane = new GeometricPlane(partCS);

                var intersection = Intersection.LineToPlane(beamLine, gPlane);
                if (intersection == null) {
                    gPlane = new GeometricPlane(partCS.Origin, partCS.AxisX, partCS.AxisX.Cross(partCS.AxisY));
                    intersection = Intersection.LineToPlane(beamLine, gPlane);
                }
                if (intersection == null) continue;

                return intersection;
            }

            return sourcePoint;
        }

        private Point NearestPointOnColumn(Point sourcePoint, Point anotherPoint, Beam column) {
            double width = 0, height = 0, length = 0;
            if (!column.GetReportProperty("WIDTH", ref width))
                if (!column.GetReportProperty("PROFILE.WIDTH", ref width))
                    throw new Exception($"Cannot get \"PROFILE.WIDTH\" value from beam {column.Identifier}");
            if (!column.GetReportProperty("HEIGHT", ref height))
                if (!column.GetReportProperty("PROFILE.HEIGHT", ref height))
                    throw new Exception($"Cannot get \"PROFILE.HEIGHT\" value from beam {column.Identifier}");
            if (!column.GetReportProperty("LENGTH", ref length))
                throw new Exception($"Cannot get \"LENGTH\" value from beam {column.Identifier}");

            Point intersection = null;

            var columnCS = column.GetCoordinateSystem();
            var beamLine = new Line(sourcePoint, anotherPoint);

            switch (ExtendToColumnPosition) {
            case ExtendToColumnPositionEnum.MiddlePlane:
                var gPlane = new GeometricPlane(columnCS);
                intersection = Intersection.LineToPlane(beamLine, gPlane);
                if (intersection == null) {
                    gPlane = new GeometricPlane(columnCS.Origin, columnCS.AxisX, columnCS.AxisX.Cross(columnCS.AxisY));
                    intersection = Intersection.LineToPlane(beamLine, gPlane);
                }
                break;
            case ExtendToColumnPositionEnum.BoundingBoxPlane:
                var centerLine = column.GetCenterLine(false).Cast<Point>();
                var centerPoint = (centerLine.First() + centerLine.Last()).Multiply(0.5);

                var obb = new OBB(centerPoint, columnCS.AxisX, columnCS.AxisY, columnCS.AxisX.Cross(columnCS.AxisY),
                    length * 0.5, height * 0.5, width * 0.5);

                intersection = obb.IntersectionPointsWith(beamLine).OrderBy(p => Distance.PointToPoint(sourcePoint, p)).FirstOrDefault();
                break;
            case ExtendToColumnPositionEnum.Surface:
                var solid = column.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var extraVector = new Vector(sourcePoint - anotherPoint).GetNormal(10000);

                intersection = solid.Intersect(sourcePoint + extraVector, anotherPoint).Cast<Point>().OrderBy(p => Distance.PointToPoint(anotherPoint, p)).FirstOrDefault();
                break;
            default:
                break;
            }

            return intersection ?? sourcePoint;
        }
    }
}
