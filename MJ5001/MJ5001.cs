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
 *  MJ5001.cs: H-beam connect to concrete
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using TSD = Tekla.Structures.Datatype;
using TSG = Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.MJ5001 {
    public class PluginData {
        #region Fields
        [StructuresField("gap")]
        public double gap;

        [StructuresField("ratHole_radius")]
        public double ratHole_radius;

        [StructuresField("embedment_THK")]
        public double embedment_thickness;

        [StructuresField("embedment_WDTH")]
        public double embedment_width;

        [StructuresField("embedment_EXTN")]
        public double embedment_exten;

        [StructuresField("embedment_MATL")]
        public string embedment_material;

        [StructuresField("anchorRod_LEN")]
        public double anchorRod_length;

        [StructuresField("anchorRod_size")]
        public double anchorRod_size;

        [StructuresField("anchorRod_MATL")]
        public string anchorRod_material;

        [StructuresField("anchorRod_disList_X")]
        public string anchorRod_disListStr_X;

        [StructuresField("anchorRod_disList_Y")]
        public string anchorRod_disListStr_Y;

        [StructuresField("cleat_THK")]
        public double cleat_thickness;

        [StructuresField("cleat_WDTH")]
        public double cleat_width;

        [StructuresField("cleat_dis_innerEdge")]
        public double cleat_dis_with_innerEdge;

        [StructuresField("cleat_MATL")]
        public string cleat_material;

        [StructuresField("bolt_STD")]
        public string bolt_standard;

        [StructuresField("bolt_size")]
        public double bolt_size;

        [StructuresField("bolt_disList_X")]
        public string bolt_disListStr_X;

        [StructuresField("bolt_disList_Y")]
        public string bolt_distListStr_Y;
        #endregion
    }

    [Plugin("MJ5001")]
    [PluginUserInterface("Muggle.TsExtensions.MJ5001.Views.MainWindow")]
    [InputObjectDependency(InputObjectDependency.DEPENDENT)]
    public class MJ5001 : PluginBase {
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private double gap;
        private double ratHole_radius;
        private double embedment_thickness;
        private double embedment_width;
        private double embedment_exten;
        private string embedment_material;
        private double anchorRod_length;
        private double anchorRod_size;
        private string anchorRod_material;
        private string anchorRod_disListStr_X;
        private string anchorRod_disListStr_Y;
        private double cleat_thickness;
        private double cleat_width;
        private double cleat_dis_with_innerEdge;
        private string cleat_material;
        private string bolt_standard;
        private double bolt_size;
        private string bolt_disListStr_X;
        private string bolt_distListStr_Y;

        private DistanceList anchorRod_disList_X;
        private DistanceList anchorRod_disList_Y;
        private DistanceList bolt_disList_X;
        private DistanceList bolt_disList_Y;
        private Offset bolt_startOffset;

        private TransformationPlane originTP;
        private TransformationPlane workTP;

        private Identifier beamID;
        private Identifier concreteID;
        private TSG.Point[] points;
        #endregion

        #region Properties
        private Model Model {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public MJ5001(PluginData data) {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides

        public override bool Run(List<InputDefinition> Input) {
            try {
                GetValuesFromDialog();

                beamID = Input[0].GetInput() as Identifier;
                concreteID = Input[1].GetInput() as Identifier;
                points = (Input[2].GetInput() as ArrayList).Cast<TSG.Point>().ToArray();

                originTP ??= Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                workTP ??= GetWorkPlane();

                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);
                points = points.Select(p => p.Transform(originTP, workTP)).ToArray();//转换到工作平面
                CreatConnection();

                return true;
            } catch (UnAcceptableProfileException) {
                MessageBox.Show("Only H-Beam is supported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception e) {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// 定义输入。
        /// </summary>
        /// <returns>集合第一个元素为梁ID，第二个元素为安放预埋件的混凝土构件ID，
        /// 第三个元素是面（点集合，ArraryList类型）。</returns>
        public override List<InputDefinition> DefineInput() {
            InputDefinition beamInput = null, concreteInput = null, faceInput = null;
            try {
                var picker = new Picker();
                var beam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);
                var face = picker.PickFace();

                beamInput = new InputDefinition(beam.Identifier);

                var enumerator = face.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var item = enumerator.Current as InputItem;
                    if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_POLYGON) {
                        var points = item.GetData() as ArrayList;
                        faceInput = new InputDefinition(points);
                    }
                    if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_1_OBJECT) {
                        var obj = item.GetData() as ModelObject;
                        concreteInput = new InputDefinition(obj.Identifier);
                    }
                }
            } catch (Exception e) when (e.Message != "User interrupt") {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return new List<InputDefinition>();
            }

            return new List<InputDefinition> { beamInput, concreteInput, faceInput };
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog() {
            gap = Data.gap;
            ratHole_radius = Data.ratHole_radius;
            embedment_thickness = Data.embedment_thickness;
            embedment_width = Data.embedment_width;
            embedment_exten = Data.embedment_exten;
            embedment_material = Data.embedment_material;
            anchorRod_length = Data.anchorRod_length;
            anchorRod_size = Data.anchorRod_size;
            anchorRod_material = Data.anchorRod_material;
            anchorRod_disListStr_X = Data.anchorRod_disListStr_X;
            anchorRod_disListStr_Y = Data.anchorRod_disListStr_Y;
            cleat_thickness = Data.cleat_thickness;
            cleat_width = Data.cleat_width;
            cleat_dis_with_innerEdge = Data.cleat_dis_with_innerEdge;
            cleat_material = Data.cleat_material;
            bolt_standard = Data.bolt_standard;
            bolt_size = Data.bolt_size;
            bolt_disListStr_X = Data.bolt_disListStr_X;
            bolt_distListStr_Y = Data.bolt_distListStr_Y;

            if (IsDefaultValue(gap))
                gap = 15.0;
            if (IsDefaultValue(ratHole_radius))
                ratHole_radius = 30.0;
            if (IsDefaultValue(embedment_thickness))
                embedment_thickness = 20.0;
            if (IsDefaultValue(embedment_width))
                embedment_width = 300.0;
            if (IsDefaultValue(embedment_exten))
                embedment_exten = 90.0;
            if (IsDefaultValue(embedment_material))
                embedment_material = "Q345B";
            if (IsDefaultValue(anchorRod_length))
                anchorRod_length = 150.0;
            if (IsDefaultValue(anchorRod_size))
                anchorRod_size = 20.0;
            if (IsDefaultValue(anchorRod_material))
                anchorRod_material = "Q345B";
            if (IsDefaultValue(anchorRod_disListStr_X))
                anchorRod_disListStr_X = "250 4*125";
            if (IsDefaultValue(anchorRod_disListStr_Y))
                anchorRod_disListStr_Y = "160";
            if (IsDefaultValue(cleat_thickness))
                cleat_thickness = 12.0;
            if (IsDefaultValue(cleat_width))
                cleat_width = 95.0;
            if (IsDefaultValue(cleat_dis_with_innerEdge))
                cleat_dis_with_innerEdge = 40.0;
            if (IsDefaultValue(cleat_material))
                cleat_material = "Q345B";
            if (IsDefaultValue(bolt_standard))
                bolt_standard = "HS10.9";
            if (IsDefaultValue(bolt_size))
                bolt_size = 20.0;
            if (IsDefaultValue(bolt_disListStr_X))
                bolt_disListStr_X = "40 7*70";
            if (IsDefaultValue(bolt_distListStr_Y))
                bolt_distListStr_Y = "0";

            anchorRod_disList_X = DistanceList.Parse(
                anchorRod_disListStr_X,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            anchorRod_disList_Y = DistanceList.Parse(
                anchorRod_disListStr_Y,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            //  增加此操作是为了模仿系统螺栓组的行为
            //  否则实际使用时，距离列第一个值要手动填写为0
            if (anchorRod_disList_Y.First().Value != 0.0)
                anchorRod_disList_Y.Insert(0, new TSD.Distance(0.0, TSD.Distance.CurrentUnitType));
            bolt_disList_X = DistanceList.Parse(
                bolt_disListStr_X,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            bolt_startOffset = new Offset { Dx = bolt_disList_X.First().Value, Dy = 0.0, Dz = 0.0 };
            bolt_disList_X.RemoveAt(0);
            bolt_disList_Y = DistanceList.Parse(
                bolt_distListStr_Y,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            //  增加此操作是为了模仿系统螺栓组的行为
            //  否则实际使用时，距离列第一个值要手动填写为0
            if (bolt_disList_Y.First().Value != 0.0)
                bolt_disList_Y.Insert(0, new TSD.Distance(0.0, TSD.Distance.CurrentUnitType));
        }

        private TransformationPlane GetWorkPlane() {
            var part = Model.SelectModelObject(beamID) as Part;
            var plane = GeometricPlaneFactory.ByPoints(points[0], points[1], points[2]);

            var centerLine = part.GetCenterLine(false).Cast<TSG.Point>().ToArray();
            var firstPoint = centerLine.First();
            var lastPoint = centerLine.Last();
            var sameDirection = true;

            TSG.Point origin;
            TSG.Vector axisX, axisY;
            if (TSG.Distance.PointToPlane(firstPoint, plane) < TSG.Distance.PointToPlane(lastPoint, plane)) {
                origin = Intersection.LineToPlane(
                    new Line(firstPoint, centerLine[1]),
                    plane);
            } else {
                origin = Intersection.LineToPlane(
                    new Line(lastPoint, centerLine[centerLine.Length - 2]),
                    plane);
                sameDirection = false;
            }

            if (part is Beam beam) {
                axisX = sameDirection ? beam.GetCoordinateSystem().AxisX : beam.GetCoordinateSystem().AxisX * -1;
                axisY = beam.GetCoordinateSystem().AxisY;
            } else if (part is PolyBeam polyBeam) {
                var css = polyBeam.GetPolybeamCoordinateSystems().Cast<CoordinateSystem>();
                var cs = sameDirection ? css.First() : css.Last();
                axisX = sameDirection ? cs.AxisX : cs.AxisX * -1;
                axisY = cs.AxisY;
            } else {
                throw new NotSupportedException($"Unsupported part type: {part.GetType()}.");
            }

            return new TransformationPlane(origin, axisX, axisY);
        }

        private void CreatConnection() {
#if DEBUG
            var msg = string.Empty;
#endif
            double height = 0.0, width = 0.0, webThickness = 0.0, flangeThickness = 0.0;
            var beam = Model.SelectModelObject(beamID) as Part;
            if (!beam.GetReportProperty("HEIGHT", ref height) || !beam.GetReportProperty("WIDTH", ref width)) {
                throw new Exception("Failed to get the height and width of the beam.");
            }
            /*  这段代码对于某些截面类型不适用，获取不到数值
             *  beam.GetReportProperty("WEB_THICKNESS", ref webThickness);
             *  if (!beam.GetReportProperty("FLANGE_THICKNESS", ref flangeThickness)) {
             *      beam.GetReportProperty("FlANGE_THICKNESS_U", ref flangeThickness);
             *  }
            */

            var profile = new ProfileH_Symmetrical(beam.Profile.ProfileString);
            if (profile.h1 != profile.h2 || profile.t1 != profile.t2)  //仅处理等截面H型钢
                throw new UnAcceptableProfileException(beam.Profile.ProfileString);
            webThickness = profile.s;
            flangeThickness = profile.t1;

            var concrete = Model.SelectModelObject(concreteID) as Part;

            var axisX = new TSG.Vector(1, 0, 0);
            var axisY = new TSG.Vector(0, 1, 0);
            var axisZ = new TSG.Vector(0, 0, 1);

            var beamTopLine = new Line(new TSG.Point(0, height / 2, 0), axisX);
            var beamBottomLine = new Line(new TSG.Point(0, -height / 2, 0), axisX);
            var embedmentPlane = GeometricPlaneFactory.ByPoints(points[0], points[1], points[2]);

            #region 创建预埋板
            var topIntersection = Intersection.LineToPlane(beamTopLine, embedmentPlane);
            var bottomIntersection = Intersection.LineToPlane(beamBottomLine, embedmentPlane);

            var embedmentWorkTP_AxisX = new TSG.Vector(topIntersection).GetNormal(1.0);
            var point1 = topIntersection + embedmentWorkTP_AxisX * embedment_exten;
            var point2 = bottomIntersection - embedmentWorkTP_AxisX * embedment_exten;

            var embedmentWorkTP_AxisY = embedmentPlane.Normal.Cross(embedmentWorkTP_AxisX).GetNormal(1.0);
            var embedmentWorkTP = new TransformationPlane(
                embedmentPlane.Origin,
                embedmentWorkTP_AxisY,
                embedmentWorkTP_AxisX);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(embedmentWorkTP);

            var embedment = ModelOperation.CreatBeam(
                point1.Transform(workTP, embedmentWorkTP),
                point2.Transform(workTP, embedmentWorkTP),
                name: "EMBEDMENT",
                profileStr: $"PL{embedment_thickness}*{embedment_width}",
                materialStr: embedment_material,
                assemblyPrefix: "MJ-",
                planeEnum: Position.PlaneEnum.MIDDLE,
                depthEnum: Position.DepthEnum.FRONT,
                rotationEnum: Position.RotationEnum.FRONT);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);

            ModelOperation.ApplyBooleanOperation(concrete, embedment);
            #endregion

            #region 布置锚杆
            var anchorRod_StartOffset = embedmentPlane.Normal.GetNormal(embedment_thickness);
            var anchorRod_EndOffset = embedmentPlane.Normal.GetNormal(-anchorRod_length);
            var x = 0.0; var y = 0.0;
            var yTotal = anchorRod_disList_Y.Sum(y => y.Value);
            for (int i = 0; i < anchorRod_disList_X.Count; i++) {
                x += anchorRod_disList_X[i].Value;
                for (int j = 0; j < anchorRod_disList_Y.Count; j++) {
                    y += anchorRod_disList_Y[j].Value;
                    point2 = point1 - embedmentWorkTP_AxisX * x + embedmentWorkTP_AxisY * (yTotal * 0.5 - y) - anchorRod_StartOffset;
                    var rod = ModelOperation.CreatBeam(
                        point2, point2 + anchorRod_EndOffset,
                        name: "ANCHOR ROD",
                        profileStr: "D" + anchorRod_size,
                        materialStr: anchorRod_material,
                        partPrefix: "D");
                    ModelOperation.CreatWeld(embedment, rod);
                }
                y = 0.0;
            }
            #endregion

            #region 梁末端对齐
            var fittingGeometryPlane = new GeometricPlane(
                (topIntersection + bottomIntersection).Multiply(0.5) + embedmentPlane.Normal.GetNormal(gap),
                embedmentWorkTP_AxisX,
                axisZ);
            var fitting = new Fitting {
                Father = beam,
                Plane = new Plane {
                    Origin = fittingGeometryPlane.Origin,
                    AxisX = embedmentWorkTP_AxisX,
                    AxisY = axisZ
                }
            };
            fitting.Insert();
            #endregion

            #region 开鼠洞
            var chamferNone = new Chamfer();
            var chamferArcPoint = new Chamfer { Type = Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT };
            point1 = Intersection.LineToPlane(
                beamTopLine.Offset(flangeThickness, LineExtension.OffsetDirectionEnum.RIGHT),
                fittingGeometryPlane);
            point2 = point1 + axisX * ratHole_radius;
            var point3 = point1 - axisY * ratHole_radius;
            var point4 = point1 - axisX * ratHole_radius;

            var contour = new ArrayList{
                new ContourPoint(point2, chamferNone),
                new ContourPoint(point3, chamferArcPoint),
                new ContourPoint(point4, chamferNone),
            };
            var cutPart = ModelOperation.CreatBooleanOperationPolygon(contour, width);
            ModelOperation.ApplyBooleanOperation(beam, cutPart);
            cutPart.Delete();

            point1 = Intersection.LineToPlane(
                beamBottomLine.Offset(flangeThickness, LineExtension.OffsetDirectionEnum.LEFT),
                fittingGeometryPlane);
            point2 = point1 + axisX * ratHole_radius;
            point3 = point1 + axisY * ratHole_radius;
            point4 = point1 - axisX * ratHole_radius;

            contour = new ArrayList{
                new ContourPoint(point2, chamferNone),
                new ContourPoint(point3, chamferArcPoint),
                new ContourPoint(point4, chamferNone),
            };
            cutPart = ModelOperation.CreatBooleanOperationPolygon(contour, width);
            ModelOperation.ApplyBooleanOperation(beam, cutPart);
            cutPart.Delete();
            #endregion

            #region 创建连接板
            point1 = Intersection.LineToPlane(
                beamTopLine.Offset(flangeThickness + cleat_dis_with_innerEdge, LineExtension.OffsetDirectionEnum.RIGHT),
                embedmentPlane);
            point4 = Intersection.LineToPlane(
                beamBottomLine.Offset(flangeThickness + cleat_dis_with_innerEdge, LineExtension.OffsetDirectionEnum.LEFT),
                embedmentPlane);

            var angle = axisX.GetAngleBetween_WithDirection(embedmentWorkTP_AxisX, axisZ);
            var vector = axisX.GetNormal(cleat_width / Math.Sin(angle));
            point2 = point1 + vector;
            point3 = point4 + vector;

            var cleat1 = ModelOperation.CreatContourPlate(
                new TSG.Point[] { point1, point2, point3, point4 },
                name: "CLEAT",
                profileStr: $"PL{cleat_thickness}",
                materialStr: cleat_material,
                depthEnum: Position.DepthEnum.FRONT,
                depthOffset: webThickness * 0.5);
            var cleat2 = ModelOperation.CreatContourPlate(
                new TSG.Point[] { point1, point2, point3, point4 },
                name: "CLEAT",
                profileStr: $"PL{cleat_thickness}",
                materialStr: cleat_material,
                depthEnum: Position.DepthEnum.BEHIND,
                depthOffset: webThickness * 0.5);

            ModelOperation.CreatWeld(embedment, cleat1);
            ModelOperation.CreatWeld(embedment, cleat2);
            #endregion

            #region 布置螺栓
            point1 += axisX.GetNormal((cleat_width + gap) * 0.5 / Math.Sin(angle));
            y = 0.0;
            yTotal = bolt_disList_Y.Sum(y => y.Value);
            var zeroDisList = new DistanceList { new TSD.Distance(0.0, TSD.Distance.CurrentUnitType) };
            for (int j = 0; j < bolt_disList_Y.Count; j++) {
                y += bolt_disList_Y[j].Value;
                point2 = point1 + axisX * (y / Math.Sin(angle));
                ModelOperation.CreatBoltArray(
                    beam, cleat1, new[] { cleat2 },
                    point2, point2 - embedmentWorkTP_AxisX * 100,
                    bolt_disList_X, zeroDisList,
                    position: new Position { Rotation = Position.RotationEnum.FRONT },
                    startOffset: bolt_startOffset,
                    bolt_standard: bolt_standard, bolt_size: bolt_size);
            }
            #endregion
        }

        #endregion
    }
}
