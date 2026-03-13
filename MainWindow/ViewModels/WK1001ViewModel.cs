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
 *  WK1001ViewModel.cs: view model for the WK1001 connection plugin
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.MainWindow.ViewModels {
    public partial class PluginsViewModel {
        private void RunWK1001() {
            try {
                var partEnum = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS);
                var parts = new ArrayList();
                foreach (Beam part in partEnum) {
                    parts.Add(part);
                }

                AdjustPartsNormal(parts);

                var wk1001 = new Connection {
                    Name = "WK1001",
                    Number = BaseComponent.PLUGIN_OBJECT_NUMBER,
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
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 调整杆件法向
        /// </summary>
        private void AdjustPartsNormal(ArrayList parts) {

            if (parts is null) {
                throw new ArgumentNullException(nameof(parts));
            }

            CoordinateSystem partCS;
            GeometricPlane partYZPlane;
            Vector thisProjectedNormal, anotherNormal, anotherProjectedNormal, partNormal;
            double degree;
            var thisNormal = GetThisNormal(parts);
            foreach (Beam beam in parts) {
                partCS = beam.GetCoordinateSystem();
                partYZPlane = new GeometricPlane(partCS.Origin, partCS.AxisX);
                thisProjectedNormal = ProjectionExtension.VectorToPlane(thisNormal, partYZPlane);
                anotherNormal = GetAnotherNormal(beam);
                if (anotherNormal == null) {
                    anotherProjectedNormal = thisProjectedNormal;
                } else {
                    anotherProjectedNormal = ProjectionExtension.VectorToPlane(
                        anotherNormal,
                        partYZPlane);
                }

                partNormal = new Vector(thisProjectedNormal.GetNormal() + anotherProjectedNormal.GetNormal());
                degree = partNormal.GetAngleBetween_WithDirection(partCS.AxisY, partCS.AxisX) / Math.PI * 180.0 + 45.0;
                degree = degree % 90.0 - 45.0;
                beam.Position.RotationOffset -= degree;
                beam.Modify();
            }

            model.CommitChanges();
        }

        private static IEnumerable<Line> AdjustOriginDirection(IEnumerable<Line> lines) {
            var lineArr = lines.ToArray();
            var p00 = lineArr[0].Origin;
            var p01 = p00 + lineArr[0].Direction;
            var p10 = lineArr[1].Origin;
            var p11 = p10 + lineArr[1].Direction;

            if (Math.Min(Distance.PointToPoint(p00, p10), Distance.PointToPoint(p00, p11))
                > Math.Min(Distance.PointToPoint(p01, p10), Distance.PointToPoint(p01, p11))) {
                lineArr[0].Origin += lineArr[0].Direction;
                lineArr[0].Direction *= -1;
            }

            var basePoint = lineArr[0].Origin;
            for (int i = 1; i < lineArr.Length; i++) {
                var p1 = lineArr[i].Origin;
                var p2 = p1 + lineArr[i].Direction;
                if (Distance.PointToPoint(p1, basePoint) > Distance.PointToPoint(p2, basePoint)) {
                    lineArr[i].Origin += lineArr[i].Direction;
                    lineArr[i].Direction *= -1;
                }
            }

            return lineArr;
        }

        /// <summary>
        /// 获取当前节点的法向（在当前变换平面中的值）
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        private static Vector GetThisNormal(ArrayList parts) {

            if (parts is null) {
                throw new ArgumentNullException(nameof(parts));
            }

            var globalZ = new Vector(0, 0, 1000).TransformFrom(new TransformationPlane());

            var members = parts.Cast<Beam>();
            var centerlines = members.Select(m => new Line(m.StartPoint, m.EndPoint));

            centerlines = AdjustOriginDirection(centerlines);

            Vector axisX, axisY;
            //  只有两根杆件的情形
            if (members.Count() == 2) {
                axisX = centerlines.First().Direction;

                if (Parallel.VectorToVector(centerlines.First().Direction, centerlines.Last().Direction)) {
                    axisY = globalZ.Cross(axisX);
                } else {
                    axisY = centerlines.Last().Direction.GetNormal();
                }

                return axisX.Cross(axisY);
            }

            //  多根杆件的情形
            //  选取与全局Z轴夹角最小的3根杆件
            var orderedIndexs = centerlines
                .Select((l, i) => (l.Direction, i))
                .OrderBy(item => {
                    var degree = globalZ.GetAngleBetween(item.Direction);
                    if (degree > Math.PI * 0.5) return Math.PI - degree;
                    return degree;
                })
                .Select(item => item.i)
                .Take(3);

            var line0 = centerlines.ElementAt(orderedIndexs.ElementAt(0));
            var line1 = centerlines.ElementAt(orderedIndexs.ElementAt(1));
            var line2 = centerlines.ElementAt(orderedIndexs.ElementAt(2));

            var seg = Intersection.LineToLine(line0, line1)
                ?? Intersection.LineToLine(line0, line2)
                ?? throw new InvalidOperationException("There are multiple parallel members, unable to creat the connection.");
            var origin = seg.StartPoint;

            var p0 = line0.Origin + line0.Direction.GetNormal(1000);
            var p1 = line1.Origin + line1.Direction.GetNormal(1000);
            var p2 = line2.Origin + line2.Direction.GetNormal(1000);

            Vector normal;
            var gplane = GeometricPlaneFactory.ByPoints(p0, p1, p2);
            if (origin == Projection.PointToPlane(origin, gplane)) {
                //在同一平面上
                normal = gplane.GetNormal();
            } else {
                //不在同一平面上
                normal = new Vector(origin - Geometry3dOperation.CenterOfSphere(origin, p0, p1, p2)).GetNormal();
            }
            //  使法向基本朝上
            if (normal.Dot(globalZ) < 0) normal *= -1;

            return normal;
        }

        private static Vector GetAnotherNormal(Beam beam) {

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
                    if (child.GetType() != typeof(Beam) || ((Beam)child).PartNumber.Prefix != "O")
                        continue;

                    var tube = child as Beam;
                    var anotherNormal = new Vector(tube.StartPoint - tube.EndPoint);

                    return anotherNormal;
                }
            }
            return null;
        }
    }
}
