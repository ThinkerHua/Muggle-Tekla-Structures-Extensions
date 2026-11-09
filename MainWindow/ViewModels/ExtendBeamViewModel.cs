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
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Task = System.Threading.Tasks.Task;
using UiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ExtendBeamViewModel : ViewModelBase {
        public enum ExtendPosition {
            MiddlePlane,
            BoundingBoxPlane,
            Surface,
        }

        private static readonly string[] BeamNames = ["beam", "cg", "梁"];
        private static readonly string[] ColumnNames = ["column", "cc", "柱"];

        private static readonly string[] SteelMaterialPatterns = [
            @"09CuP(CrNi-[AB])?",
            @"(12Mn|16)CuCr",
            @"15MnCuCr(-QT)?",
            @"Q\d\d\d[ABCD]?",
            @"Steel_Undefined"
        ];

        private static readonly string[] ConcreteMaterialPatterns = [
            @"C\d\d",
            @"(Concrete|Formwork)_Undefined"
        ];

        private readonly Model _model;
        private readonly Picker _picker;
        private readonly UiSelector _uiSelector;

        private readonly IMessageBoxService _messageBoxService;

        [ObservableProperty] private double _tolerance;

        [ObservableProperty] private ExtendPosition _extendToSteelBeamPosition;

        [ObservableProperty] private ExtendPosition _extendToSteelColumnPosition;

        [ObservableProperty] private ExtendPosition _extendToConcreteBeamPosition;

        [ObservableProperty] private ExtendPosition _extendToConcreteColumnPosition;

        public ExtendBeamViewModel(IMessageBoxService messageBoxService) {
            this._messageBoxService = messageBoxService;

            _model = new Model();
            _picker = new Picker();
            _uiSelector = new UiSelector();
            Tolerance = 100;
            ExtendToSteelBeamPosition = ExtendPosition.MiddlePlane;
            ExtendToSteelColumnPosition = ExtendPosition.MiddlePlane;
            ExtendToConcreteBeamPosition = ExtendPosition.BoundingBoxPlane;
            ExtendToConcreteColumnPosition = ExtendPosition.BoundingBoxPlane;
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
                        var startPoint = DestinationPoint(beam.Identifier, beam.StartPoint, beam.EndPoint, beamCs);
                        var endPoint = DestinationPoint(beam.Identifier, beam.EndPoint, beam.StartPoint, beamCs);
                        beam.StartPoint = startPoint;
                        beam.EndPoint = endPoint;
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
            var itSelfIsColumn = IsColumn(_model.SelectModelObject(identifier) as Beam);

            var boundingBoxExtent = new Vector(Tolerance, Tolerance, Tolerance);
            var nearbyObjects = _model.GetModelObjectSelector()
                .GetObjectsByBoundingBox(sourcePoint - boundingBoxExtent, sourcePoint + boundingBoxExtent);

            var columns = new List<Beam>();
            var beams = new List<Part>();
            foreach (ModelObject obj in nearbyObjects) {
                if (obj is null || obj.Identifier.Equals(identifier)) continue;

                switch (obj) {
                case Beam beam:
                    if (!itSelfIsColumn && IsColumn(beam)) {
                        columns.Add(beam);
                    } else if (IsBeam(beam)) {
                        beams.Add(beam);
                    }

                    break;
                case PolyBeam polyBeam:
                    if (IsBeam(polyBeam)) {
                        beams.Add(polyBeam);
                    }

                    break;
                }
            }

            // extend to column
            if (!itSelfIsColumn) {
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
                        Distance.PointToPoint(column.StartPoint, column.EndPoint),
                        IsSteel(column) ? ExtendToSteelColumnPosition : ExtendToConcreteColumnPosition);
                }
            }

            if (beams.Count == 0) return sourcePoint;

            // extend to beam
            var partCsAndLength = beams.Select(part => {
                    var centerLine = part.GetCenterLine(false).Cast<Point>().ToList();

                    CoordinateSystem partCs = null;
                    var partLength = 0.0;

                    switch (part) {
                    case Beam beam:
                        partCs = beam.GetCoordinateSystem();
                        partLength = Distance.PointToPoint(centerLine.First(), centerLine.Last());
                        break;
                    case PolyBeam polyBeam:
                        var index = centerLine
                            .Take(centerLine.Count - 1)
                            .Zip(centerLine.Skip(1), (p1, p2) => new Line(p1, p2))
                            .Select((l, i) => (l, i))
                            .OrderBy(pair => Distance.PointToLine(sourcePoint, pair.l))
                            .First().i;

                        var css = polyBeam.GetPolybeamCoordinateSystemsEx();
                        partCs = css[index];

                        partLength = Distance.PointToPoint(centerLine[index], centerLine[index + 1]);
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
                pair.part, pair.partCs, pair.partLength,
                IsSteel(pair.part) ? ExtendToSteelBeamPosition : ExtendToConcreteBeamPosition);
        }

        private Point DestinationPoint(Point sourcePoint, Point anotherPoint, Part nearbyPart,
            CoordinateSystem nearbyPartCs, double nearbyPartLength, ExtendPosition extendPosition) {
            var beamLine = new Line(sourcePoint, anotherPoint);

            Point intersection = null;
            switch (extendPosition) {
            case ExtendPosition.MiddlePlane:
                var gPlane = new GeometricPlane(nearbyPartCs);
                intersection = Intersection.LineToPlane(beamLine, gPlane);
                if (intersection == null) {
                    gPlane = new GeometricPlane(nearbyPartCs.Origin, nearbyPartCs.AxisX,
                        nearbyPartCs.AxisX.Cross(nearbyPartCs.AxisY));
                    intersection = Intersection.LineToPlane(beamLine, gPlane);
                }

                break;
            case ExtendPosition.BoundingBoxPlane:
                double width = 0, height = 0;
                if (!nearbyPart.GetReportProperty("WIDTH", ref width))
                    if (!nearbyPart.GetReportProperty("PROFILE.WIDTH", ref width))
                        throw new Exception($"Cannot get \"PROFILE.WIDTH\" value from beam {nearbyPart.Identifier}");
                if (!nearbyPart.GetReportProperty("HEIGHT", ref height))
                    if (!nearbyPart.GetReportProperty("PROFILE.HEIGHT", ref height))
                        throw new Exception($"Cannot get \"PROFILE.HEIGHT\" value from beam {nearbyPart.Identifier}");

                var nearbyPartLine = new Line(nearbyPartCs.Origin, nearbyPartCs.AxisX);
                var centerPoint = IntersectionExtension.LineToLine(nearbyPartLine, beamLine).StartPoint;

                var obb = new OBB(centerPoint, nearbyPartCs.AxisX, nearbyPartCs.AxisY,
                    nearbyPartCs.AxisX.Cross(nearbyPartCs.AxisY),
                    nearbyPartLength * 0.5 + Tolerance, height * 0.5, width * 0.5);

                intersection = obb.IntersectionPointsWith(beamLine)
                    .OrderBy(p => Distance.PointToPoint(anotherPoint, p)).FirstOrDefault();
                break;
            case ExtendPosition.Surface:
                var solid = nearbyPart.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var extraVector = new Vector(sourcePoint - anotherPoint).GetNormal(10000);

                intersection = solid.Intersect(sourcePoint + extraVector, anotherPoint).Cast<Point>()
                    .OrderBy(p => Distance.PointToPoint(anotherPoint, p)).FirstOrDefault();
                break;
            }

            return intersection ?? sourcePoint;
        }

        private static bool IsColumn(Beam beam) {
            if (beam == null) {
                throw new ArgumentNullException(nameof(beam));
            }

            return beam.Type is Beam.BeamTypeEnum.COLUMN ||
                   ColumnNames.Any(n => beam.Name.Contains(n, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBeam(Part part) {
            return part == null
                ? throw new ArgumentNullException(nameof(part))
                : BeamNames.Any(n => part.Name.Contains(n, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsSteel(Part part) {
            return SteelMaterialPatterns.Any(p => Regex.IsMatch(part.Material.MaterialString, p));
        }

        private static bool IsConcrete(Part part) {
            return ConcreteMaterialPatterns.Any(p => Regex.IsMatch(part.Material.MaterialString, p));
        }
    }
}