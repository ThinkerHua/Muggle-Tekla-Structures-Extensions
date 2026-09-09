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
 *  ProjectOntoPlaneViewModel.cs: view model for the project model object onto plane operation.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.MainWindow.Services;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class ProjectOntoPlaneViewModel : ViewModelBase {
        public enum ProjectionDirectionEnum {
            Perpendicular,
            GlobalZ,
            Manually,
        }

        public enum WayToGetPlaneEnum {
            PickFace,
            Pick3Points,
        }

        private Model Model { get; }
        private Picker Picker { get; }
        private ModelObjectSelector UiSelector { get; }
        private IMessageBoxService MessageBox { get; }

        [ObservableProperty] private ProjectionDirectionEnum _projectionDirection;

        [ObservableProperty] private WayToGetPlaneEnum _wayToGetPlane;

        [ObservableProperty] private bool _makeSolidUnderPlane;

        public ProjectOntoPlaneViewModel(IMessageBoxService messageBox) {
            ProjectionDirection = ProjectionDirectionEnum.GlobalZ;
            WayToGetPlane = WayToGetPlaneEnum.PickFace;
            MakeSolidUnderPlane = false;

            Model = new Model();
            Picker = new Picker();
            UiSelector = new ModelObjectSelector();
            MessageBox = messageBox;
        }

        [RelayCommand]
        private void ProjectBeamOntoPlane() {
            var currentTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try {
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());

                var selectedObjects = UiSelector.GetSelectedObjects();
                if (selectedObjects.GetSize() == 0) {
                    selectedObjects = Picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                }

                if (selectedObjects.GetSize() == 0) return;

                Point[] points = new Point[3];
                switch (WayToGetPlane) {
                case WayToGetPlaneEnum.PickFace:
                    var input = Picker.PickFace();

                    foreach (InputItem item in input) {
                        if (item.GetInputType() != InputItem.InputTypeEnum.INPUT_POLYGON) continue;

                        var array = (ArrayList)item.GetData();
                        points[0] = (Point)array[0];
                        points[1] = (Point)array[1];
                        points[2] = (Point)array[array.Count - 1];

                        break;
                    }

                    break;
                case WayToGetPlaneEnum.Pick3Points:
                default:

                    points[0] = Picker.PickPoint();
                    for (int i = 1; i < 3; i++) {
                        points[i] = Picker.PickPoint(points[i - 1]);
                    }

                    break;
                }

                var plane = new GeometricPlane(
                    points[0], new Vector(points[1] - points[0]), new Vector(points[2] - points[0]));

                Vector direction;
                switch (ProjectionDirection) {
                case ProjectionDirectionEnum.Perpendicular:
                    direction = plane.Normal;
                    break;
                case ProjectionDirectionEnum.GlobalZ:
                    var z = new Vector(0, 0, 1000);
                    if (z.Dot(plane.GetNormal()) == 0.0) {
                        throw new Exception(
                            "Projection cannot be done, " +
                            "because the normal of the selected plane is perpendicular to the global Z-axis.");
                    }

                    direction = z;
                    break;
                case ProjectionDirectionEnum.Manually:
                default:
                    var p0 = Picker.PickPoint(App.Current.Localization.GetText("albl_pick_origin_point"));
                    var p1 = Picker.PickPoint(
                        App.Current.Localization.GetText("albl_pick_end_point_to_define_the_direction"));
                    direction = new Vector(p1 - p0);
                    break;
                }

                foreach (var selectedObject in selectedObjects) {
                    switch (selectedObject) {
                    case Beam beam:
                        ProjectBeam(beam, plane, direction,
                            ProjectionDirection is ProjectionDirectionEnum.GlobalZ && MakeSolidUnderPlane);
                        break;
                    case PolyBeam polyBeam:
                        ProjectPolyBeam(polyBeam, plane, direction,
                            ProjectionDirection is ProjectionDirectionEnum.GlobalZ && MakeSolidUnderPlane);
                        break;
                    case ContourPlate contourPlate:
                        ProjectContourPlate(contourPlate, plane, direction,
                            ProjectionDirection is ProjectionDirectionEnum.GlobalZ && MakeSolidUnderPlane);
                        break;
                    }
                }

                Model.CommitChanges();
            } catch (Exception e) when (e.Message == App.UserInterrupt) {
                // ignored
            } catch (Exception e) {
                MessageBox.ShowError(e.ToString());
            } finally {
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTp);
            }
        }

        private static void ProjectContourPlate(ContourPlate contourPlate, GeometricPlane plane, Vector direction,
            bool solidUnderPlane) {
            foreach (ContourPoint cp in contourPlate.Contour.ContourPoints) {
                var point = IntersectionPoint(cp, direction, plane);
                cp.X = point.X;
                cp.Y = point.Y;
                cp.Z = point.Z;
            }

            contourPlate.Modify();

            if (solidUnderPlane) {
                throw new NotImplementedException();
            }
        }

        private static void ProjectBeam(Beam beam, GeometricPlane plane, Vector direction, bool solidUnderPlane) {
            beam.StartPoint = IntersectionPoint(beam.StartPoint, direction, plane);
            beam.EndPoint = IntersectionPoint(beam.EndPoint, direction, plane);
            beam.Modify();
        
#if RELEASE
            if (solidUnderPlane) {
                throw new NotImplementedException();

                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                var p1 = centerLine[0];
                var p2 = centerLine[centerLine.Length - 1];

                var solid = beam.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var faces = solid.GetAllFaces();

                var vs = new Vector(centerLine[0] - centerLine[1]);
                var ve = new Vector(centerLine[centerLine.Length - 1] - centerLine[centerLine.Length - 2]);

                var startFace = faces.Single(f =>
                    Parallel.VectorToVector(f.Normal, vs) &&
                    f.Normal.Dot(vs) > 0 &&
                    Distance.PointToPlane(p1, f.ToGeometricPlane()) <= GeometryConstants.DISTANCE_EPSILON);
                var endFace = faces.Single(f =>
                    Parallel.VectorToVector(f.Normal, ve) &&
                    f.Normal.Dot(ve) > 0 &&
                    Distance.PointToPlane(p2, f.ToGeometricPlane()) <= GeometryConstants.DISTANCE_EPSILON);

                var startTranslationVector = startFace.GetAllVertexes()
                    .Select(v => {
                        var line = new Line(v, direction);
                        return new Vector(Intersection.LineToPlane(line, plane) - v);
                    })
                    .OrderBy(v => v.Dot(direction) < 0 ? v.GetLength() * -1 : v.GetLength())
                    .First();
                var endDirection = endFace.GetAllVertexes()
                    .Select(v => {
                        var line = new Line(v, direction);
                        return new Vector(Intersection.LineToPlane(line, plane) - v);
                    })
                    .OrderBy(v => v.Dot(direction) < 0 ? v.GetLength() * -1 : v.GetLength())
                    .First();

                beam.StartPoint += startTranslationVector;
                beam.EndPoint += endDirection;
                beam.Modify();
            }
#endif
        }

        private static void ProjectPolyBeam(PolyBeam polyBeam, GeometricPlane plane, Vector direction,
            bool solidUnderPlane) {
            foreach (ContourPoint cp in polyBeam.Contour.ContourPoints) {
                var point = IntersectionPoint(cp, direction, plane);
                cp.X = point.X;
                cp.Y = point.Y;
                cp.Z = point.Z;
            }

            polyBeam.Modify();

#if RELEASE
            if (solidUnderPlane) {
                throw new NotImplementedException();

                var centerLine = polyBeam.GetCenterLine(false).Cast<Point>().ToArray();
                var solid = polyBeam.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var faces = solid.GetAllFaces();
                var coordinateSystems = polyBeam.GetPolybeamCoordinateSystemsEx();

                var contourPoints = polyBeam.Contour.ContourPoints;
                for (int i = 0; i < contourPoints.Count; i++) {
                    var contourPoint = (ContourPoint)contourPoints[i];

                    List<Point> points;
                    if (i == 0 || i == contourPoints.Count - 1) {
                        var v = i == 0
                            ? new Vector(centerLine[0] - centerLine[1])
                            : new Vector(centerLine[i] - centerLine[i - 1]);
                        var face = faces.Single(f =>
                            Parallel.VectorToVector(f.Normal, v) &&
                            f.Normal.Dot(v) > 0 &&
                            Distance.PointToPlane(contourPoint, f.ToGeometricPlane()) <=
                            GeometryConstants.DISTANCE_EPSILON);
                        points = face.GetAllVertexes();
                    } else {
                        var cs = coordinateSystems.First(c =>
                            Distance.PointToPlane(contourPoint, new GeometricPlane(c.Origin, c.AxisX, c.AxisY)) <=
                            GeometryConstants.DISTANCE_EPSILON);

                        points = new List<Point>();
                        var enumerator =
                            solid.GetAllIntersectionPoints(cs.Origin, cs.Origin + cs.AxisX, cs.Origin + cs.AxisY);
                        while (enumerator.MoveNext()) {
                            points.Add(enumerator.Current as Point);
                        }
                    }

                    var vector = points
                        .Select(v => {
                            var line = new Line(v, direction);
                            return new Vector(Intersection.LineToPlane(line, plane) - v);
                        })
                        .OrderBy(v => v.Dot(direction) < 0 ? v.GetLength() * -1 : v.GetLength())
                        .First();

                    contourPoint.X += vector.X;
                    contourPoint.Y += vector.Y;
                    contourPoint.Z += vector.Z;
                    contourPoints[i] = contourPoint;
                }

                polyBeam.Modify();
            }
#endif
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