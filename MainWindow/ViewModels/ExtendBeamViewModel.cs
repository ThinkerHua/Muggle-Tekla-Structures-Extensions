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
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Task = System.Threading.Tasks.Task;
using UiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ExtendBeamViewModel : ViewModelBase {
        public enum ExtendPositionEnum {
            MiddlePlane,
            BoundingBoxPlane,
            Surface,
        }

        private static readonly string[] BeamNames = ["beam", "梁"];
        private static readonly string[] ColumnNames = ["column", "柱"];

        private readonly Model _model;
        private readonly Picker _picker;
        private readonly UiSelector _uiSelector;

        private readonly IMessageBoxService _messageBoxService;

        [ObservableProperty] private double _tolerance;

        [ObservableProperty] private ExtendPositionEnum _extendToBeamPosition;

        [ObservableProperty] private ExtendPositionEnum _extendToColumnPosition;

        public ExtendBeamViewModel(IMessageBoxService messageBoxService) {
            this._messageBoxService = messageBoxService;

            _model = new Model();
            _picker = new Picker();
            _uiSelector = new UiSelector();
            Tolerance = 100;
            ExtendToBeamPosition = ExtendPositionEnum.MiddlePlane;
            ExtendToColumnPosition = ExtendPositionEnum.BoundingBoxPlane;
        }

        [RelayCommand]
        private void Extend() {
            try {
                var objects = _uiSelector.GetSelectedObjects();
                if (objects.GetSize() == 0) {
                    objects = _picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, "prompt_Pick_objects");
                }

                if (objects.GetSize() == 0) return;

                var taskList = new List<Task>();
                foreach (var obj in objects) {
                    if (obj is not Beam beam) continue;
                    var beamCs = beam.GetCoordinateSystem();

                    taskList.Add(Task.Run(Action));
                    continue;

                    void Action() {
                        beam.StartPoint = DestinationPoint(beam.Identifier, beam.StartPoint, beam.EndPoint, beamCs);
                        beam.EndPoint = DestinationPoint(beam.Identifier, beam.EndPoint, beam.StartPoint, beamCs);
                        beam.Modify();
                    }
                }

                Task.WaitAll([.. taskList]);

                _model.CommitChanges();
            } catch (Exception e) when (e.Message == App.UserInterrupt) {
                // ignore
            } catch (Exception e) {
                _messageBoxService.ShowError(e.ToString());
            }
        }

        private Point DestinationPoint(Identifier identifier, Point sourcePoint, Point anotherPoint,
            CoordinateSystem beamCs) {

            var boundingBoxExtent = new Vector(Tolerance, Tolerance, Tolerance);

            var nearbyObjects = _model.GetModelObjectSelector()
                .GetObjectsByBoundingBox(sourcePoint - boundingBoxExtent, sourcePoint + boundingBoxExtent);

            var columns = new List<Beam>();
            var beams = new List<Part>();
            foreach (ModelObject obj in nearbyObjects) {
                if (obj.Identifier.Equals(identifier)) continue;

                switch (obj) {
                case Beam beam:
                    if (beam.Type is Beam.BeamTypeEnum.COLUMN ||
                        ColumnNames.Any(n => beam.Name.Contains(n, StringComparison.OrdinalIgnoreCase))) {
                        columns.Add(beam);
                    } else if (BeamNames.Any(n => beam.Name.Contains(n, StringComparison.OrdinalIgnoreCase))) {
                        beams.Add(beam);
                    }

                    break;
                case PolyBeam polyBeam:
                    if (BeamNames.Any(n => polyBeam.Name.Contains(n, StringComparison.OrdinalIgnoreCase))) {
                        beams.Add(polyBeam);
                    }

                    break;
                }
            }

            // extend to column
            Beam column = null;
            switch (columns.Count) {
            case 0:
                break;
            case 1:
                column = columns[0];
                break;
            default:
                column = columns.Select(c => (c, c.GetSolid()))
                    .OrderBy(pair => pair.Item2.MaximumPoint.Z)
                    .FirstOrDefault().c;
                break;
            }

            if (column != null) {
                return DestinationPoint(sourcePoint, anotherPoint, column, column.GetCoordinateSystem(),
                    Distance.PointToPoint(column.StartPoint, column.EndPoint), ExtendToColumnPosition);
            }

            if (beams.Count == 0) return sourcePoint;
            
            // extend to beam
            var partCsAndLength = beams.Select(part => {
                    CoordinateSystem partCs = null;
                    double partLength = 0.0;

                    switch (part) {
                    case Beam beam:
                        partCs = beam.GetCoordinateSystem();
                        partLength = Distance.PointToPoint(beam.StartPoint, beam.EndPoint);
                        break;
                    case PolyBeam polyBeam:
                        var css = polyBeam.GetPolybeamCoordinateSystems().Cast<CoordinateSystem>().ToList();
                        var index = css
                            .Take(css.Count - 1)
                            .Zip(css.Skip(1), (cs1, cs2) => new Line(cs1.Origin, cs2.Origin))
                            .Select((l, i) => (l, i))
                            .OrderBy(pair => Distance.PointToLine(sourcePoint, pair.l))
                            .First().i;
                        partCs = css[index];

                        var centerLine = polyBeam.GetCenterLine(false);
                        partLength = index == css.Count - 1
                            ? Distance.PointToPoint(partCs.Origin, centerLine[centerLine.Count - 1] as Point)
                            : Distance.PointToPoint(partCs.Origin, css[index + 1].Origin);
                        break;
                    }

                    return (part, partCs, partLength);
                })
                .ToList();

            var notParallel = partCsAndLength.Where(pair => {
                var partCs = pair.partCs;
                return !Parallel.VectorToVector(partCs.AxisX.Cross(partCs.AxisY), beamCs.AxisX.Cross(beamCs.AxisY));
            }).ToList();

            var pair = notParallel.Any() ? notParallel.First() : partCsAndLength.First();

            return DestinationPoint(sourcePoint, anotherPoint,
                pair.part, pair.partCs, pair.partLength, ExtendToBeamPosition);
        }

        private Point DestinationPoint(Point sourcePoint, Point anotherPoint, Part nearbyPart,
            CoordinateSystem nearbyPartCs, double nearbyPartLength, ExtendPositionEnum extendPosition) {

            var beamLine = new Line(sourcePoint, anotherPoint);

            Point intersection = null;
            switch (extendPosition) {
            case ExtendPositionEnum.MiddlePlane:
                var gPlane = new GeometricPlane(nearbyPartCs);
                intersection = Intersection.LineToPlane(beamLine, gPlane);
                if (intersection == null) {
                    gPlane = new GeometricPlane(nearbyPartCs.Origin, nearbyPartCs.AxisX,
                        nearbyPartCs.AxisX.Cross(nearbyPartCs.AxisY));
                    intersection = Intersection.LineToPlane(beamLine, gPlane);
                }

                break;
            case ExtendPositionEnum.BoundingBoxPlane:
                double width = 0, height = 0;
                if (!nearbyPart.GetReportProperty("WIDTH", ref width))
                    if (!nearbyPart.GetReportProperty("PROFILE.WIDTH", ref width))
                        throw new Exception($"Cannot get \"PROFILE.WIDTH\" value from beam {nearbyPart.Identifier}");
                if (!nearbyPart.GetReportProperty("HEIGHT", ref height))
                    if (!nearbyPart.GetReportProperty("PROFILE.HEIGHT", ref height))
                        throw new Exception($"Cannot get \"PROFILE.HEIGHT\" value from beam {nearbyPart.Identifier}");

                var centerLine = nearbyPart.GetCenterLine(false).Cast<Point>().ToArray();
                var centerPoint = (centerLine.First() + centerLine.Last()).Multiply(0.5);

                var obb = new OBB(centerPoint, nearbyPartCs.AxisX, nearbyPartCs.AxisY,
                    nearbyPartCs.AxisX.Cross(nearbyPartCs.AxisY),
                    nearbyPartLength * 0.5 + Tolerance, height * 0.5, width * 0.5);

                intersection = obb.IntersectionPointsWith(beamLine)
                    .OrderBy(p => Distance.PointToPoint(anotherPoint, p)).FirstOrDefault();
                break;
            case ExtendPositionEnum.Surface:
                var solid = nearbyPart.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var extraVector = new Vector(sourcePoint - anotherPoint).GetNormal(10000);

                intersection = solid.Intersect(sourcePoint + extraVector, anotherPoint).Cast<Point>()
                    .OrderBy(p => Distance.PointToPoint(anotherPoint, p)).FirstOrDefault();
                break;
            }

            return intersection ?? sourcePoint;
        }
    }
}