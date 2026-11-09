/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025-2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  WK1001ViewModel.cs: view model for the WK1001 connection plugin
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class PluginsViewModel {

        private const double HalfPi = Math.PI * 0.5;

        private void RunWK1001() {
            var partEnum = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
            var parts = new ArrayList();
            foreach (Beam part in partEnum) {
                parts.Add(part);
            }

            AdjustPartsNormal(parts);

            var wk1001 = new Connection {
                Name = "WK1001", 
                Number = BaseComponent.PLUGIN_OBJECT_NUMBER, 
                PositionType = PositionTypeEnum.COLLISION_PLANE,
                AutoDirectionType = AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z,
                UpVector = new Vector(0,0,1).TransformFrom(new TransformationPlane()),
                Class = -1,
            };
            wk1001.LoadAttributesFromFile("standard");
            wk1001.SetPrimaryObject(parts[0] as ModelObject);
            parts.RemoveAt(0);
            wk1001.SetSecondaryObjects(parts);

            if (!wk1001.Insert())
                throw new Exception("Failed to insert connection \"WK1001\".");

            var modelObjects = new ArrayList { wk1001 };
            uiSelector.Select(modelObjects);
            model.CommitChanges();
        }

        /// <summary>
        /// 调整杆件法向
        /// </summary>
        private void AdjustPartsNormal(ArrayList parts) {

            if (parts is null) {
                throw new ArgumentNullException(nameof(parts));
            }

            var thisNormal = GetThisConnectionNormal(parts);
            foreach (Beam beam in parts) {
                var partCs = beam.GetCoordinateSystem();
                var partYzPlane = new GeometricPlane(partCs.Origin, partCs.AxisX);

                var thisProjectedNormal = ProjectionExtension.VectorToPlane(thisNormal, partYzPlane);
                var anotherNormal = GetAnotherConnectionNormal(beam);
                var anotherProjectedNormal = anotherNormal == null
                    ? thisProjectedNormal
                    : ProjectionExtension.VectorToPlane(anotherNormal, partYzPlane);

                var partNormal = new Vector(thisProjectedNormal.GetNormal() + anotherProjectedNormal.GetNormal());
                var degree = partNormal.GetAngleBetweenWithDirection(partCs.AxisY, partCs.AxisX)
                    / Math.PI * 180.0 + 45.0;
                degree = degree % 90.0 - 45.0;
                beam.Position.RotationOffset -= degree;
                beam.Modify();
            }

            model.CommitChanges();
        }

        private static List<Line> AdjustOriginDirection(IEnumerable<Line> lines) {
            var result = lines.ToList();
            var p00 = result[0].Origin;
            var p01 = p00 + result[0].Direction;
            var p10 = result[1].Origin;
            var p11 = p10 + result[1].Direction;

            if (Math.Min(Distance.PointToPoint(p00, p10), Distance.PointToPoint(p00, p11))
                > Math.Min(Distance.PointToPoint(p01, p10), Distance.PointToPoint(p01, p11))) {
                result[0].Origin += result[0].Direction;
                result[0].Direction *= -1;
            }

            var basePoint = result[0].Origin;
            for (int i = 1; i < result.Count; i++) {
                var p1 = result[i].Origin;
                var p2 = p1 + result[i].Direction;
                if (Distance.PointToPoint(p1, basePoint) > Distance.PointToPoint(p2, basePoint)) {
                    result[i].Origin += result[i].Direction;
                    result[i].Direction *= -1;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取当前节点的法向（在当前变换平面中的值）
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        private static Vector GetThisConnectionNormal(ArrayList parts) {

            if (parts is null) {
                throw new ArgumentNullException(nameof(parts));
            }

            var globalZ = new Vector(0, 0, 1000).TransformFrom(new TransformationPlane());

            var rods = parts.Cast<Beam>().ToArray();
            var centerlines = AdjustOriginDirection(rods.Select(rod => new Line(rod.StartPoint, rod.EndPoint)));

            //  只有两根杆件的情形
            if (rods.Length == 2) {
                var normal1 = GetRodNormal(rods[0]).GetNormal();
                var normal2 = GetRodNormal(rods[1]).GetNormal();
                return new Vector(normal1 + normal2);
            }

            //  多根杆件的情形
            //  选取与全局Z轴夹角最小的3根杆件
            var orderedIndexes = centerlines
                .Select((l, i) => (l.Direction, i))
                .OrderBy(pair => {
                    var radian = globalZ.GetAngleBetween(pair.Direction);
                    if (radian > HalfPi) return Math.PI - radian;
                    return radian;
                })
                .Select(pair => pair.i)
                .Take(3)
                .ToArray();

            var line0 = centerlines[orderedIndexes[0]];
            var line1 = centerlines[orderedIndexes[1]];
            var line2 = centerlines[orderedIndexes[2]];

            var seg = Intersection.LineToLine(line0, line1)
                      ?? Intersection.LineToLine(line0, line2)
                      ?? throw new InvalidOperationException(
                          "There are multiple parallel members, unable to creat the connection.");
            var origin = seg.StartPoint;

            var p0 = line0.Origin + line0.Direction.GetNormal(1000);
            var p1 = line1.Origin + line1.Direction.GetNormal(1000);
            var p2 = line2.Origin + line2.Direction.GetNormal(1000);

            var gPlane = GeometricPlaneFactory.ByPoints(p0, p1, p2);

            var normal = origin.Equals(Projection.PointToPlane(origin, gPlane))
                ? gPlane.GetNormal() //在同一平面上
                : new Vector(origin - Geometry3dOperation.CenterOfSphere(origin, p0, p1, p2)).GetNormal(); //不在同一平面上

            //  使法向基本朝上
            if (normal.Dot(globalZ) < 0) normal *= -1;

            return normal;
        }

        private static Vector GetRodNormal(Beam rod) {
            var globalZ = new Vector(0, 0, 1).TransformFrom(new TransformationPlane());

            var cs = rod.GetCoordinateSystem();
            var axisY = cs.AxisY;
            var axisZ = cs.AxisX.Cross(cs.AxisY);
            var angleY = globalZ.GetAngleBetweenPrecisely(axisY);
            var angleZ = globalZ.GetAngleBetweenPrecisely(axisZ);

            if (angleY > HalfPi) {
                angleY -= HalfPi;
                axisY *= -1;
            }

            if (angleZ > HalfPi) {
                angleZ -= HalfPi;
                axisZ *= -1;
            }

            return angleY <= angleZ ? axisY : axisZ;
        }

        private static Vector GetAnotherConnectionNormal(Beam beam) {

            if (beam is null) {
                throw new ArgumentNullException(nameof(beam));
            }

            var cmpntEnum = beam.GetComponents();
            if (cmpntEnum == null) return null;

            foreach (Connection connection in cmpntEnum) {
                if (connection.Name != "WK1001" || connection.Status != Tekla.Structures.ConnectionStatusEnum.STATUS_OK)
                    continue;

                var childrenEnum = connection.GetChildren();
                if (childrenEnum == null) continue;

                foreach (ModelObject child in childrenEnum) {
                    if (child is not Beam pipe) continue;
                    try {
                        _ = new ProfileCircular_Perfect(pipe.Profile.ProfileString);
                    } catch (UnAcceptableProfileException) {
                        continue;
                    }

                    return new Vector(pipe.StartPoint - pipe.EndPoint);
                }
            }

            return null;
        }
    }
}