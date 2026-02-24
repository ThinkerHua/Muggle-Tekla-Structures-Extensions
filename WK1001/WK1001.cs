/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  WK1001.cs: "WK1001" connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Muggle.TeklaPlugins.Common.Geometry3d;
using Muggle.TeklaPlugins.Common.Model;
using Muggle.TeklaPlugins.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;

namespace Muggle.TeklaPlugins.WK1001 {
    public class PluginData {
        [StructuresField("prfStr_Pipe")]
        public string prfStr_Pipe;
        [StructuresField("thick_TEndplate")]
        public double thick_TEndplate;
        [StructuresField("thick_BEndplate")]
        public double thick_BEndplate;
        [StructuresField("diam_BEndplate")]
        public double diam_BEndplate;
        [StructuresField("thick_Stiffener")]
        public double thick_Stiffneer;
        [StructuresField("minDis")]
        public double minDis;
        [StructuresField("extLength_T")]
        public double extLenght_T;
        [StructuresField("extLength_B")]
        public double extLength_B;
        [StructuresField("materialStr")]
        public string materialStr;

    }
    [Plugin("WK1001")]
    [PluginUserInterface("Muggle.TeklaPlugins.WK1001.Views.MainWindow")]
    [SecondaryType(SecondaryType.SECONDARYTYPE_MULTIPLE)]
    [PositionType(PositionTypeEnum.END_END_PLANE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z)]
    public class WK1001 : ConnectionBase {
        #region Fields
        private Model _model;
        private PluginData _data;

        private string _prfStr_Pipe;
        private double _thick_TEndplate;
        private double _thick_BEndplate;
        private double _diam_BEndplate;
        private double _thick_Stiffneer;
        private double _minDis;
        private double _extLenght_T;
        private double _extLength_B;
        private string _materialStr;

        private List<Identifier> partIDs;
        private List<ProfileRect_Invariant> profiles;
        private List<Line> centerlines;
        private List<double> angles;//杆件间依次角度，即2号与1号之间、3号与2号之间...

        private TransformationPlane globalTP, originTP, workTP;
        #endregion

        #region Properties
        public Model Model {
            get => _model;
            set => _model = value;
        }
        public PluginData Data {
            get => _data;
            set {
                _data = value;
                GetValuesFromDialog();
            }
        }
        #endregion

        #region Constructor
        public WK1001(PluginData data) {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region MainMethod

        public override bool Run() {
            try {

                if (partIDs == null) {
                    partIDs = new List<Identifier> { Primary };
                    partIDs.AddRange(Secondaries);
                }

                var parts = partIDs.Select(id => Model.SelectModelObject(id) as Part);
                profiles = parts.Select(p => new ProfileRect_Invariant(p.Profile.ProfileString)).ToList();
                centerlines = parts.Select(p => {
                    var l = p.GetCenterLine(false);
                    var p1 = l[0] as Point;
                    var p2 = l[l.Count - 1] as Point;
                    return new Line(p1, p2);
                }).ToList();

                AdjustOriginDirection();

                globalTP ??= new TransformationPlane();
                originTP ??= _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                workTP ??= GetWorkTransformationPlane();
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);

                OrderParts();

                CreatConnection();

            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
        #endregion

        #region PrivateMethods
        private void GetValuesFromDialog() {
            _prfStr_Pipe = _data.prfStr_Pipe;
            _thick_TEndplate = _data.thick_TEndplate;
            _thick_BEndplate = _data.thick_BEndplate;
            _diam_BEndplate = _data.diam_BEndplate;
            _thick_Stiffneer = _data.thick_Stiffneer;
            _minDis = _data.minDis;
            _extLenght_T = _data.extLenght_T;
            _extLength_B = _data.extLength_B;
            _materialStr = _data.materialStr;

            if (IsDefaultValue(_thick_TEndplate))
                _thick_TEndplate = 40;
            if (IsDefaultValue(_thick_BEndplate))
                _thick_BEndplate = 40;
            if (IsDefaultValue(_thick_Stiffneer))
                _thick_Stiffneer = 25;
            if (IsDefaultValue(_minDis))
                _minDis = 50;
            if (IsDefaultValue(_extLenght_T))
                _extLenght_T = 20;
            if (IsDefaultValue(_extLength_B))
                _extLength_B = 20;
            if (IsDefaultValue(_materialStr))
                _materialStr = "Q345B";
        }

        private void AdjustOriginDirection() {
            var p00 = centerlines[0].Origin;
            var p01 = p00 + centerlines[0].Direction;
            var p10 = centerlines[1].Origin;
            var p11 = p10 + centerlines[1].Direction;

            if (Math.Min(Distance.PointToPoint(p00, p10), Distance.PointToPoint(p00, p11))
                > Math.Min(Distance.PointToPoint(p01, p10), Distance.PointToPoint(p01, p11))) {
                centerlines[0].Origin += centerlines[0].Direction;
                centerlines[0].Direction *= -1;
            }

            var basePoint = centerlines[0].Origin;
            for (int i = 1; i < centerlines.Count; i++) {
                var p1 = centerlines[i].Origin;
                var p2 = p1 + centerlines[i].Direction;
                if (Distance.PointToPoint(p1, basePoint) > Distance.PointToPoint(p2, basePoint)) {
                    centerlines[i].Origin += centerlines[i].Direction;
                    centerlines[i].Direction *= -1;
                }
            }

        }

        private TransformationPlane GetWorkTransformationPlane() {

            var globalZ = new Vector(0, 0, 1).TransformFrom(globalTP);

            Point origin;
            Vector axisX, axisY;

            //  只有两根杆件的情形
            if (centerlines.Count == 2) {
                axisX = centerlines.First().Direction.GetNormal();

                var seg0 = Intersection.LineToLine(centerlines.First(), centerlines.Last());
                if (seg0 == null) {
                    origin = centerlines.First().Origin;
                    axisY = globalZ.Cross(axisX);
                } else {
                    origin = seg0.StartPoint;
                    axisY = centerlines.Last().Direction.GetNormal();
                }

                return new TransformationPlane(origin, axisX, axisY);
            }

            //  多根杆件的情形
            var parts = partIDs.Select(id => Model.SelectModelObject(id) as Part);
            var directions = centerlines.Select(l => l.Direction);

            //  选取与全局Z轴夹角最小的3根杆件
            var orderedIndexs = directions
                .Select((v, i) => (v, i))
                .OrderBy(item => {
                    var degree = globalZ.GetAngleBetween(item.v);
                    if (degree > Math.PI * 0.5) return Math.PI - degree;
                    return degree;
                })
                .Select(item => item.i)
                .Take(3);

            var line0 = centerlines[orderedIndexs.ElementAt(0)];
            var line1 = centerlines[orderedIndexs.ElementAt(1)];
            var line2 = centerlines[orderedIndexs.ElementAt(2)];

            var seg1 = Intersection.LineToLine(line0, line1)
                ?? Intersection.LineToLine(line0, line2)
                ?? throw new InvalidOperationException("There are multiple parallel members, unable to creat the connection.");
            origin = seg1.StartPoint;

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

            axisY = Vector.Cross(normal, line0.Direction);
            axisX = Vector.Cross(axisY, normal);

            return new TransformationPlane(origin, axisX, axisY);
        }

        /// <summary>
        /// 按模型中实际的顺序（逆时针方向）调整零件顺序，
        /// 字段partIDs, profiles, centerlines, angles均调整。
        /// 同时将centerlines的原点和方向转换到workTP。
        /// </summary>
        private void OrderParts() {

            var axisX = new Vector(1, 0, 0);
            var axisZ = new Vector(0, 0, 1);
            var XYPlane = new GeometricPlane(new Point(), axisZ);
            angles = new List<double>();
            foreach (var line in centerlines) {
                line.Origin = new Point();
                line.Direction = line.Direction.TransformFrom(originTP);
                //当前先均按与X轴间角度赋值，后面再调整为杆件间依次角度
                angles.Add(axisX.GetAngleBetween_WithDirection(
                    ProjectionExtension.VectorToPlane(line.Direction, XYPlane),
                    axisZ));
            }

            var sortedIndex = angles
                .Select((a, i) => (a, i))
                .OrderBy(x => x.a)
                .Select(x => x.i)
                .ToArray();

            var newPartIDs = new Identifier[sortedIndex.Length];
            var newProfiles = new ProfileRect_Invariant[sortedIndex.Length];
            var newCenterlines = new Line[sortedIndex.Length];
            var newAngles = new double[sortedIndex.Length];
            for (int i = 0; i < sortedIndex.Length; i++) {
                newPartIDs[i] = partIDs[sortedIndex[i]];
                newProfiles[i] = profiles[sortedIndex[i]];
                newCenterlines[i] = centerlines[sortedIndex[i]];
                newAngles[i] = angles[sortedIndex[i]];
            }
            //调整为杆件间依次角度
            angles[0] = Math.PI * 2 + newAngles[0] - newAngles[newAngles.Length - 1];
            for (int i = 1; i < newAngles.Length; i++) {
                angles[i] = newAngles[i] - newAngles[i - 1];
            }

            partIDs.Clear(); partIDs.AddRange(newPartIDs);
            profiles.Clear(); profiles.AddRange(newProfiles);
            centerlines.Clear(); centerlines.AddRange(newCenterlines);
        }

        private void CreatConnection() {

            Point point1, point2, point3, point4;
            bool typeA = IsDefaultValue(_diam_BEndplate) || _diam_BEndplate == 0;
            var chamferNone = new Chamfer();
            var chamferArcPoint = new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

            #region 创建连接筒
            if (_prfStr_Pipe != string.Empty)
                goto Skip_prfStr_Pipe;

            var maxWidth = (from prf in profiles select prf.b1).Max();
            var minAngle = angles.Min();

            //此处求半径做了简化处理，正确值应当用以下公式计算
            //arcsin(0.5a/r)+arcsin(0.5b/r)+2*arcsin(0.5w/r)=angle
            //其中a, b分别为相邻杆件的宽度，angle为相邻杆件夹角
            //w为相邻杆件间最小间距，r为连接筒半径
            var diameter = (maxWidth + _minDis) / minAngle * 2;
            var diameterArray = from prf in ProfileCircular_Perfect.CommonlyUsed
                                where prf.d1 > diameter
                                select prf.d1;
            if (diameterArray.Count() != 0) {
                diameter = diameterArray.Min();
            } else {
                diameter = Math.Ceiling(diameter * 0.1) * 10;
            }

            var thickness = Math.Max(_thick_TEndplate, _thick_BEndplate);
            var thicknessArray = from prf in ProfileCircular_Perfect.CommonlyUsed
                                 where prf.d1 == diameter && prf.t >= thickness
                                 select prf.t;
            if (thicknessArray.Count() != 0) {
                thickness = thicknessArray.Min();
            } else {
                thickness = Math.Ceiling(thickness / 2) * 2;
            }

            _prfStr_Pipe = $"O{diameter}*{thickness}";

        Skip_prfStr_Pipe:;
            var prfPipe = new ProfileCircular_Perfect(_prfStr_Pipe);
            var maxHeight = (from prf in profiles select prf.h1).Max();
            point1 = new Point(0, 0, maxHeight * 0.5 + _extLenght_T);
            point2 = new Point(0, 0, -maxHeight * 0.5 - _extLength_B + (typeA ? 0 : _thick_BEndplate));
            var pipe = ModelOperation.CreatBeam(
                point1, point2,
                profileStr: _prfStr_Pipe,
                materialStr: _materialStr,
                partPrefix: "O");
            #endregion

            #region 创建端板
            point1 = new Point(point1);
            point1.X = (prfPipe.d1 - prfPipe.t * 2) * 0.5 - 2;
            point2 = new Point(0, point1.X, point1.Z);
            point3 = new Point(-point1.X, 0, point1.Z);
            point4 = new Point(0, -point1.X, point1.Z);
            var contour = new ArrayList {
                new ContourPoint(point1, chamferNone),
                new ContourPoint(point2, chamferArcPoint),
                new ContourPoint(point3, chamferNone),
                new ContourPoint(point4, chamferArcPoint),
            };
            var tEndPlate = ModelOperation.CreatContourPlate(contour, profileStr: "PL" + _thick_TEndplate, materialStr: _materialStr, depthEnum: Position.DepthEnum.BEHIND);

            point1 = new Point(point1);
            point2 = new Point(point2);
            point3 = new Point(point3);
            point4 = new Point(point4);
            point1.Z = typeA ? pipe.EndPoint.Z : pipe.EndPoint.Z - _thick_BEndplate;
            point2.Z = point1.Z;
            point3.Z = point1.Z;
            point4.Z = point1.Z;
            point1.X = typeA ? point1.X : _diam_BEndplate * 0.5;
            point2.Y = point1.X;
            point3.X = -point1.X;
            point4.Y = -point1.X;
            contour = new ArrayList {
                new ContourPoint(point1, chamferNone),
                new ContourPoint(point2, chamferArcPoint),
                new ContourPoint(point3, chamferNone),
                new ContourPoint(point4, chamferArcPoint),
            };
            var bEndPlate = ModelOperation.CreatContourPlate(contour, profileStr: "PL" + _thick_TEndplate, materialStr: _materialStr, depthEnum: Position.DepthEnum.FRONT);
            #endregion

            #region 创建加劲肋
            point1 = new Point(pipe.StartPoint);
            point1.Z -= _thick_TEndplate;
            point2 = new Point(pipe.EndPoint);
            point2.Z += typeA ? _thick_BEndplate : 0;
            var stif1 = ModelOperation.CreatBeam(
                point1, point2,
                profileStr: $"PL{_thick_Stiffneer}*{prfPipe.d1 - prfPipe.t * 2 - 4}",
                materialStr: _materialStr);
            var stif2 = ModelOperation.CreatBeam(
                point1, point2,
                profileStr: $"PL{_thick_Stiffneer}*{prfPipe.d1 * 0.5 - prfPipe.t - _thick_Stiffneer * 0.5 - 2}",
                materialStr: _materialStr,
                depthEnum: Position.DepthEnum.FRONT,
                depthOffset: _thick_Stiffneer * 0.5,
                rotationEnum: Position.RotationEnum.TOP);
            var stif3 = ModelOperation.CreatBeam(
                point1, point2,
                profileStr: $"PL{_thick_Stiffneer}*{prfPipe.d1 * 0.5 - prfPipe.t - _thick_Stiffneer * 0.5 - 2}",
                materialStr: _materialStr,
                depthEnum: Position.DepthEnum.BEHIND,
                depthOffset: _thick_Stiffneer * 0.5,
                rotationEnum: Position.RotationEnum.TOP);

            #endregion

            #region 切割、焊接
            point1 = new Point(pipe.StartPoint);
            point2 = new Point(pipe.EndPoint);
            point1.Z += 100;
            point2.Z -= 100;
            var booleanPart = ModelOperation.CreatBeam(point1, point2, profileStr: $"D{prfPipe.d1}", @class: BooleanPart.BooleanOperativeClassName);
            foreach (var part in partIDs.Select(id => Model.SelectModelObject(id) as Part)) {
                ModelOperation.ApplyBooleanOperation(part, booleanPart);
                ModelOperation.CreatWeld(part, pipe, position: Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z);
            }
            booleanPart.Delete();

            ModelOperation.CreatWeld(
                pipe, tEndPlate,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeAbove: 6,
                typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
            var weld = ModelOperation.CreatWeld(
                pipe, bEndPlate,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeAbove: 6,
                typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
            ModelOperation.CreatWeld(pipe, stif1);
            ModelOperation.CreatWeld(pipe, stif2);
            ModelOperation.CreatWeld(pipe, stif3);
            ModelOperation.CreatWeld(tEndPlate, stif1);
            ModelOperation.CreatWeld(tEndPlate, stif2);
            ModelOperation.CreatWeld(tEndPlate, stif3);
            ModelOperation.CreatWeld(bEndPlate, stif1);
            ModelOperation.CreatWeld(bEndPlate, stif2);
            ModelOperation.CreatWeld(bEndPlate, stif3);
            ModelOperation.CreatWeld(stif1, stif2);
            ModelOperation.CreatWeld(stif1, stif3);

            #endregion
        }
        #endregion
    }
}
