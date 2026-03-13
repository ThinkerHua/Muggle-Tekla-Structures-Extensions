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
 *  KJ1002.cs: Sideway restraint of beam flange
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
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using DistanceList = Tekla.Structures.Datatype.DistanceList;
using Point = Tekla.Structures.Geometry3d.Point;
using TSD = Tekla.Structures.Datatype;
using Vector = Tekla.Structures.Geometry3d.Vector;

namespace Muggle.TsExtensions.KJ1002 {
    public class PluginData {
        #region Fields

        [StructuresField("sectionLEN")]
        public double SectionLength;

        [StructuresField("bracePRF")]
        public string BraceProfileStr;

        [StructuresField("STIF_THK")]
        public double StiffenerThickness;

        [StructuresField("gussetTHK")]
        public double GussetThickness;

        [StructuresField("clearance")]
        public double Clearance;

        [StructuresField("boltStd")]
        public string BoltStandard;

        [StructuresField("boltSize")]
        public double BoltSize;

        [StructuresField("bolt_Positions")]
        public string BoltPositionsStr;

        [StructuresField("EXTD_Distance")]
        public double ExtendedDistance;

        [StructuresField("creatUpperSplices")]
        public int CreatUpperSplices;

        [StructuresField("material")]
        public string MaterialStr;

        #endregion
    }

    [Plugin("KJ1002")]
    [PluginUserInterface("Muggle.TsExtensions.KJ1002.Views.MainWindow")]
    [SecondaryType(SecondaryType.SECONDARYTYPE_MULTIPLE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z)]
    public class KJ1002 : ConnectionBase {
        #region Private class

        private class GussetIdentifiers {
            public Identifier pre_lower;
            public Identifier pre_upper;
            public Identifier nxt_lower;
            public Identifier nxt_upper;
        }

        #endregion

        #region Fields

        private Model model;
        private PluginData data;

        private double sectionLength;
        private string braceProfileStr;
        private double stiffenerThickness;
        private double gussetThickness;
        private double clearance;
        private string boltStandard;
        private double boltSize;
        private string boltPositionsStr;
        private double extendedDistance;
        private int creatUpperSplices;
        private string materialStr;

        private DistanceList boltPositions;

        #endregion

        #region Properties

        private Model Model {
            get { return this.model; }
            set { this.model = value; }
        }

        private PluginData Data {
            get { return this.data; }
            set { this.data = value; }
        }

        #endregion

        #region Constructor

        public KJ1002(PluginData data) {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides

        public override bool Run() {
            IEnumerable<Identifier> beamIDs;
            IEnumerable<bool> reverseDirection;
            TransformationPlane originTP, workTP;

            try {
                GetValuesFromDialog();

                beamIDs = GetBeamIdentifiers();
                reverseDirection = DetermineIfNeedToReverseDirection(beamIDs);

                Verify(beamIDs, reverseDirection);

                originTP = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                workTP = GetWorkTransformationPlane(beamIDs, reverseDirection);
#if REMOVE
                Internal.ShowTransformationPlane(originTP, 1, UI.GraphicPolyLine.LineType.DashedAndDotted);
                Internal.ShowTransformationPlane(workTP, 4, UI.GraphicPolyLine.LineType.Solid);
#endif
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);

                beamIDs = OrderBeams(beamIDs, ref reverseDirection);

                if (IsDefaultValue(sectionLength) || sectionLength <= 0.0) {
                    sectionLength = CalculateSectionLength(beamIDs, reverseDirection);
                }

                CreatSplices(beamIDs, reverseDirection);

                return true;
            } catch (Exception e) {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region Private methods

        private void GetValuesFromDialog() {
            sectionLength = data.SectionLength;
            braceProfileStr = data.BraceProfileStr;
            stiffenerThickness = data.StiffenerThickness;
            gussetThickness = data.GussetThickness;
            clearance = data.Clearance;
            boltStandard = data.BoltStandard;
            boltSize = data.BoltSize;
            boltPositionsStr = data.BoltPositionsStr;
            extendedDistance = data.ExtendedDistance;
            creatUpperSplices = data.CreatUpperSplices;
            materialStr = data.MaterialStr;

            if (IsDefaultValue(braceProfileStr)) {
                braceProfileStr = "L70*5.0";
            }

            _ = new ProfileL(braceProfileStr); //  仅支持角钢，或不符合则抛出异常

            if (IsDefaultValue(stiffenerThickness)) {
                stiffenerThickness = 8.0;
            }

            if (IsDefaultValue(gussetThickness)) {
                gussetThickness = 8.0;
            }

            if (IsDefaultValue(clearance)) {
                clearance = 50.0;
            }

            if (IsDefaultValue(boltStandard)) {
                boltStandard = "TS10.9";
            }

            if (IsDefaultValue(boltSize)) {
                boltSize = 16.0;
            }

            if (IsDefaultValue(boltPositionsStr)) {
                boltPositionsStr = "50 70 50";
            }

            if (IsDefaultValue(extendedDistance)) {
                extendedDistance = 30.0;
            }

            if (IsDefaultValue(creatUpperSplices)) {
                creatUpperSplices = 0;
            }

            if (IsDefaultValue(materialStr)) {
                materialStr = "Q345B";
            }

            boltPositions = DistanceList.Parse(boltPositionsStr, System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
        }

        /// <summary>
        /// 在一个集合里统一管理所有梁ID。
        /// </summary>
        /// <remarks>
        /// 第一个元素是主梁ID，后续元素依次是按顺序输入的梁ID。
        /// </remarks>
        /// <returns>所有梁ID。</returns>
        private IEnumerable<Identifier> GetBeamIdentifiers() {
            return new[] { Primary }.Concat(Secondaries);
        }

        /// <summary>
        /// 判断梁方向是否需要反转。
        /// </summary>
        /// <remarks>
        /// 以远离节点的方向为正向，否则需要反转。
        /// </remarks>
        /// <param name="beamIDs"></param>
        /// <returns>与 <paramref name="beamIDs"/> 各元素对应，是否需反转方向。</returns>
        private IEnumerable<bool> DetermineIfNeedToReverseDirection(IEnumerable<Identifier> beamIDs) {
            var reverse = new bool[beamIDs.Count()];

            var segments = beamIDs.Select(id => {
                var beam = model.SelectModelObject(id) as Beam;
                var centerLine = beam.GetCenterLine(false).Cast<Point>();
                return new LineSegment(centerLine.First(), centerLine.Last());
            });

            var primSeg = segments.First();
            var secdSeg = segments.ElementAt(1);

            var d1 = Distance.PointToPoint(primSeg.StartPoint, secdSeg.StartPoint);
            var d2 = Distance.PointToPoint(primSeg.StartPoint, secdSeg.EndPoint);
            var d3 = Distance.PointToPoint(primSeg.EndPoint, secdSeg.StartPoint);
            var d4 = Distance.PointToPoint(primSeg.EndPoint, secdSeg.EndPoint);

            if (Math.Min(d1, d2) > Math.Min(d3, d4)) reverse[0] = true;
            if (Math.Min(d1, d3) > Math.Min(d2, d4)) reverse[1] = true;

            var basePoint = reverse[0] ? primSeg.EndPoint : primSeg.StartPoint;

            var otherSegs = segments.Skip(2);

            for (int i = 0; i < otherSegs.Count(); i++) {
                var seg = otherSegs.ElementAt(i);
                if (Distance.PointToPoint(seg.StartPoint, basePoint) >
                    Distance.PointToPoint(seg.EndPoint, basePoint)) {
                    reverse[i + 2] = true;
                }
            }

            return reverse;
        }

        /// <summary>
        /// 验证输入的梁是否满足正常连接节点。
        /// </summary>
        /// <param name="beamIDs"></param>
        /// <param name="reverseDirection"></param>
        /// <exception cref="Exception"></exception>
        private void Verify(IEnumerable<Identifier> beamIDs, IEnumerable<bool> reverseDirection) {
            var beams = beamIDs.Select(id => model.SelectModelObject(id) as Beam);
            var centerLines = beams.Zip(reverseDirection, (beam, reverse) => {
                var centerLine = beam.GetCenterLine(false).Cast<Point>();
                var line = new Line(centerLine.First(), centerLine.Last());
                if (reverse) line.Direction *= -1;
                return line;
            });

            var primLine = centerLines.First();

            //  不能全部平行
            if (centerLines.Skip(1)
                .All(line => Parallel.VectorToVector(line.Direction, primLine.Direction))) {
                throw new Exception("Can't apply connection because all beams are parallel.");
            }

            //  所有向量必须共面
            var firstNotParallelLineIndex = centerLines.Skip(1)
                .Select((line, index) => (line, index))
                .First(item => !Parallel.VectorToVector(item.line.Direction, primLine.Direction))
                .index + 1;
            var plane = new GeometricPlane(
                new Point(),
                primLine.Direction,
                centerLines.ElementAt(firstNotParallelLineIndex).Direction);
            if (centerLines.Skip(1)
                .Take(firstNotParallelLineIndex - 1)
                .Concat(centerLines.Skip(firstNotParallelLineIndex + 1))
                .Any(line => Math.Abs(plane.Normal.GetAngleBetween_Precisely(line.Direction) - Math.PI * 0.5) > GeometryConstants.ANGULAR_EPSILON)) {
                throw new Exception("All beam vectors must be coplanar.");
            }

            //  必须交汇于同一点
            var projectionLines =
                centerLines.Select(line => Projection.LineToPlane(line, plane));
            var p = IntersectionExtension
                .LineToLine(projectionLines.First(), projectionLines.ElementAt(firstNotParallelLineIndex)).StartPoint;
            if (projectionLines.Skip(1)
                .Take(firstNotParallelLineIndex - 1)
                .Concat(projectionLines.Skip(firstNotParallelLineIndex + 1))
                .Any(line => Distance.PointToLine(p, line) >= GeometryConstants.DISTANCE_EPSILON)) {
                throw new Exception("All beam projection center lines must intersect at the same point.");
            }

            /*  other verification logic */
        }

        /// <summary>
        /// 获取工作变换平面。
        /// </summary>
        /// <remarks>
        /// 以主零件中心线为X轴，以远离节点的方向为正方向；
        /// 以所有零件中心线向量的共同法向量为Z轴，正方向与全局Z轴方向趋近，
        /// 若共同法向量与全局Z轴垂直，则以主零件和第一个非平行次零件按右手螺旋法则确定正方向；
        /// 以所有零件中心线在XY平面上投影的共同交汇点为原点。
        /// </remarks>
        /// <param name="beamIDs"></param>
        /// <param name="reverseDirection"></param>
        /// <returns>工作变换平面。</returns>
        /// <exception cref="NotImplementedException"></exception>
        private TransformationPlane GetWorkTransformationPlane(IEnumerable<Identifier> beamIDs, IEnumerable<bool> reverseDirection) {

            var lines = beamIDs.Select(id => {
                var beam = model.SelectModelObject(id) as Beam;
                var centerLine = beam.GetCenterLine(false).Cast<Point>();
                return new Line(centerLine.First(), centerLine.Last());
            });

            var primLine = lines.First();
            if (reverseDirection.First()) primLine.Direction *= -1;

            var axisX = primLine.Direction;

            var firstNotParallelLineIndex = lines.Skip(1)
                .Select((line, index) => (line, index))
                .First(item => !Parallel.VectorToVector(item.line.Direction, axisX))
                .index + 1;
            var firstNotParallelLine = lines.ElementAt(firstNotParallelLineIndex);
            if (reverseDirection.ElementAt(firstNotParallelLineIndex)) firstNotParallelLine.Direction *= -1;

            var origin = IntersectionExtension.LineToLine(primLine, firstNotParallelLine).StartPoint;

            var normal = axisX.Cross(firstNotParallelLine.Direction);
            var globalZ = new Vector(0, 0, 100).TransformFrom(new TransformationPlane());
            if (normal.Dot(globalZ) < 0) normal *= -1;

            var axisY = normal.Cross(axisX);

            return new TransformationPlane(origin, axisX, axisY);
        }

        /// <summary>
        /// 将梁沿Z轴正方向按右手螺旋法则排序，第一个元素是主零件ID。
        /// </summary>
        /// <param name="beamIDs"></param>
        /// <param name="reverseDirection">指示对应的梁是否反转方向（以起点靠近中心为正向）</param>
        /// <returns>排序后的梁ID。</returns>
        private IEnumerable<Identifier> OrderBeams(IEnumerable<Identifier> beamIDs, ref IEnumerable<bool> reverseDirection) {
            var zip = beamIDs.Zip(reverseDirection, (id, r) => (id, r)).Select(z => {
                var beam = model.SelectModelObject(z.id) as Beam;
                var centerLine = beam.GetCenterLine(false).Cast<Point>();
                var vector = new Vector(centerLine.Last() - centerLine.First());
                if (z.r) vector *= -1;

                return (z.id, vector, reverse: z.r);
            });

            var primVector = zip.First().vector;
            var axisZ = new Vector(0, 0, 100);

            var ordered = zip.Take(1)
                .Concat(zip.Skip(1).OrderBy(z =>
                    primVector.GetAngleBetween_WithDirection(z.vector, axisZ)))
                .ToArray();

            reverseDirection = ordered.Select(o => o.reverse);
            return ordered.Select(o => o.id);
        }

        /// <summary>
        /// 计算梁的跨度。
        /// </summary>
        /// <remarks>
        /// 以梁两端的柱中心水平距离为梁的跨度，
        /// 搜索不到另一端的柱时，以当前柱中心到梁另一端端点的水平距离为梁的跨度。
        /// </remarks>
        /// <param name="identifier"></param>
        /// <param name="reverseDirection"></param>
        /// <returns>梁的跨度。</returns>
        private double CalculateBeamSpan(Identifier identifier, bool reverseDirection) {
            var names = new[] { "column", "柱" };

            var beam = model.SelectModelObject(identifier) as Beam;
            var beamLine = beam.GetCenterLine(false);
            var p = (reverseDirection ? beamLine[0] : beamLine[beamLine.Count - 1]) as Point;

            var minPoint = new Point(p.X - 10, p.Y - 10, p.Z - 10);
            var maxPoint = new Point(p.X + 10, p.Y + 10, p.Z + 10);
            var enumerator = model.GetModelObjectSelector().GetObjectsByBoundingBox(minPoint, maxPoint);

            Beam column = null;
            foreach (ModelObject obj in enumerator) {
                if (!(obj is Beam col)) continue;
                var colName = col.Name.ToLower();
                if (col.Type == Beam.BeamTypeEnum.COLUMN || names.Any(name => colName.Contains(name))) {
                    column = col;
                    break;
                }
            }

            if (column == null)
                return new Vector(p).GetLength();//完整应当计算两头距离，误差不大

            var columnLine = column.GetCenterLine(false);
            return new Vector(IntersectionExtension.LineToLine(
                        new Line(new Point(), p),
                        new Line(columnLine[0] as Point, columnLine[columnLine.Count - 1] as Point))
                    .StartPoint)
                .GetLength();
        }

        /// <summary>
        /// 计算支撑点到中心点节长。
        /// </summary>
        /// <param name="beamIDs"></param>
        /// <param name="reverseDirection"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private double CalculateSectionLength(IEnumerable<Identifier> beamIDs, IEnumerable<bool> reverseDirection) {
            var maxSpan = double.MinValue;
            var maxHeight = double.MinValue;
            var cnt = -1;
            foreach (var id in beamIDs) {
                ++cnt;
                var span = CalculateBeamSpan(id, reverseDirection.ElementAt(cnt));
                maxSpan = Math.Max(maxSpan, span);

                var beam = model.SelectModelObject(id) as Beam;
                var height = 0.0;
                if (!beam.GetReportProperty("HEIGHT", ref height)) {
                    if (!beam.GetReportProperty("PART.HEIGHT", ref height)) {
                        if (!beam.GetReportProperty("PART.PROFILE.HEIGHT", ref height)) {
                            throw new Exception($"Can't get beam height for {beam.Identifier}.");
                        }
                    }
                }

                maxHeight = Math.Max(maxHeight, height);
            }

            return Math.Max(maxSpan * 0.15, maxHeight);
        }

        /// <summary>
        /// 计算支持与梁之间的连接板各角点。
        /// </summary>
        /// <remarks>
        /// 以梁为X轴正方向，梁与支撑组成的平面为XY平面；仅计算二维坐标。
        /// <code>
        /// 各角点顺序：
        ///          5
        ///          .
        ///         /  `.
        ///        /     `.
        ///     0 /        `.
        ///       \          `.
        ///        \ 1         `. 4
        ///         |           |
        ///         |___________|
        ///         2           3
        /// </code>
        /// </remarks>
        /// <param name="beamWidth">梁宽</param>
        /// <param name="beamWebThickness">梁腹板厚</param>
        /// <param name="braceWidth">支撑宽</param>
        /// <param name="sectionLength">支撑点到中心点节长</param>
        /// <param name="braceBeamAngle">支撑与梁夹角，弧度制，(0, 0.5PI)</param>
        /// <param name="extendedDistance">连接板相对支撑外扩距离</param>
        /// <param name="coincidentLength">支撑与连接板重合段长度</param>
        /// <param name="clearance"></param>
        /// <param name="braceOnRight">表明支撑是在梁左侧还是右侧</param>
        /// <returns>连接板各角点。</returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<Point> CalculateGussetPoints(double beamWidth, double beamWebThickness, double braceWidth, double sectionLength,
            double braceBeamAngle, double extendedDistance, double coincidentLength, double clearance, bool braceOnRight) {

            if (braceBeamAngle <= 0 || braceBeamAngle >= Math.PI * 0.5) {
                throw new ArgumentOutOfRangeException(nameof(braceBeamAngle),
                    "The angle between the brace and the beam must be in the range (0, 90) degrees.");
            }

            var beamLine = new Line(new Point(), new Vector(1, 0, 0));
            var braceLine = new Line(new Point(sectionLength, 0, 0),
                new Vector(-Math.Cos(braceBeamAngle), Math.Sin(braceBeamAngle), 0));

            var p = IntersectionExtension.LineToLine(
                    beamLine.Offset(beamWidth * 0.5 + clearance, LineExtension.OffsetDirectionEnum.LEFT),
                    braceLine.Offset(braceWidth * 0.5, LineExtension.OffsetDirectionEnum.LEFT))
                .StartPoint;

            var axisZ = new Vector(0, 0, 1);
            var endLine = new Line(p + braceLine.Direction * coincidentLength, braceLine.Direction.Cross(axisZ));
            var p0 = IntersectionExtension.LineToLine(
                    endLine,
                    braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirectionEnum.LEFT))
                .StartPoint;

            var radians2 = Math.PI - braceBeamAngle - 30.0 / 180.0 * Math.PI;
            var hypotenuseLine = new Line(p0, new Vector(Math.Cos(radians2), Math.Sin(radians2), 0));
            var p1 = IntersectionExtension.LineToLine(
                    hypotenuseLine,
                    beamLine.Offset(beamWidth * 0.5, LineExtension.OffsetDirectionEnum.LEFT))
                .StartPoint;

            var p2 = new Point(p1.X, beamWebThickness * 0.5, 0);

            var p4 = IntersectionExtension.LineToLine(
                    beamLine.Offset(beamWidth * 0.5, LineExtension.OffsetDirectionEnum.LEFT),
                    braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirectionEnum.RIGHT))
                .StartPoint;
            var p3 = new Point(p4.X, beamWebThickness * 0.5, 0);

            var p5 = IntersectionExtension.LineToLine(
                    endLine,
                    braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirectionEnum.RIGHT))
                .StartPoint;

            if (braceOnRight) {
                p0.Y *= -1;
                p1.Y *= -1;
                p2.Y *= -1;
                p3.Y *= -1;
                p4.Y *= -1;
                p5.Y *= -1;
            }

            return new[] { p0, p1, p2, p3, p4, p5 };
        }

        /// <summary>
        /// 在梁上创建加劲板。
        /// </summary>
        /// <param name="beamID"></param>
        /// <param name="reverseDirection"></param>
        /// <param name="position">内侧加劲板的外侧（靠近节点为内侧，反之为外侧）</param>
        /// <param name="thickness"></param>
        /// <param name="distanceNet"></param>
        /// <param name="material"></param>
        /// <returns>成功创建的加劲板的 Identifier 集合。
        /// 第一个加劲板在梁右内侧，第二个加劲板在梁右外侧，
        /// 第三个加劲板在梁左内侧，第四个加劲板在梁左外侧。</returns>
        private IEnumerable<Identifier> CreatStiffeners(Identifier beamID, bool reverseDirection, Point position,
            double thickness, double distanceNet, string material) {

            var beam = model.SelectModelObject(beamID) as Part;
            var centerLine = beam.GetCenterLine(false).Cast<Point>();
            var direction = new Vector(centerLine.Last() - centerLine.First()).GetNormal();
            direction *= reverseDirection ? -1 : 1;

            var stifs1 = ModelOperation.CreatStiffeners(beam, position - direction * thickness * 0.5, thickness, material);
            var stifs2 = ModelOperation.CreatStiffeners(beam, position + direction * (distanceNet + thickness * 0.5), thickness, material);

            var tmpTP = new TransformationPlane(centerLine.First(), direction, new Vector(0, 0, 1).Cross(direction));

            var p = stifs1.First().Contour.ContourPoints[0] as Point;
            p = p.TransformTo(tmpTP);
            ContourPlate[] stifs;
            if (p.Y <= 0) {
                stifs = new ContourPlate[] { stifs1.ElementAt(0), stifs2.ElementAt(0), stifs1.ElementAt(1), stifs2.ElementAt(1) };
            } else {
                stifs = new ContourPlate[] { stifs1.ElementAt(1), stifs2.ElementAt(1), stifs1.ElementAt(0), stifs2.ElementAt(0) };
            }

            var currentTP = model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(tmpTP);

            foreach (var stif in stifs) {
                ModelOperation.CreatWeld(beam, stif, false,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
                ModelOperation.CreatWeld(beam, stif, false,
                    position: Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
                ModelOperation.CreatWeld(beam, stif, false,
                    position: Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
            }

            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTP);

            return stifs.Select(stif => stif.Identifier);
        }

        /*/// <summary>
        /// 在梁上创建加劲板。
        /// </summary>
        /// <remarks>
        /// 使用系统细部 Stiffeners(1034) 创建，插入点为内侧加劲板的外侧（靠近节点为内，反之为外侧）。
        /// </remarks>
        /// <param name="beamID"></param>
        /// <param name="reverseDirection"></param>
        /// <param name="position"></param>
        /// <param name="thickness"></param>
        /// <param name="distanceNet"></param>
        /// <param name="material"></param>
        /// <returns>成功创建的系统细部 Stiffeners(1034) 的 Identifier。</returns>
        private Identifier CreatStiffeners(Identifier beamID, bool reverseDirection, Point position,
            double thickness, double distanceNet, string material) {

            var beam = model.SelectModelObject(beamID) as Beam;

            //  系统细部 Stiffeners(1034)
            //  adist4          距离列
            //  adist5          数量
            //  atab1           对齐
            //  hpl2            前面加劲板高度
            //  bpl2            前面加劲板宽度
            //  tpl2            前面加劲板厚度
            //  hpl3            后面加劲板高度
            //  bpl3            后面加劲板宽度
            //  tpl3            后面加劲板厚度
            //  partname10      零件名称
            //  mat10           材质
            //  prefix_pos10    零件前缀
            //  startno_pos10   零件起始号
            //  partname15      抛光
            //  w1_type         腹板焊缝上类型
            //  w1_type2        腹板焊缝下类型
            //  w1_size         腹板焊缝上尺寸
            //  w1_size2        腹板焊缝下尺寸
            //  w2_type         下翼缘焊缝上类型
            //  w2_type2        下翼缘焊缝下类型
            //  w2_size         下翼缘焊缝上尺寸
            //  w2_size2        下翼缘焊缝下尺寸
            //  w3_type         上翼缘焊缝上类型
            //  w3_type2        上翼缘焊缝下类型
            //  w3_size         上翼缘焊缝上尺寸
            //  w3_size2        上翼缘焊缝下尺寸
            //  group_no        等级
            var detail = new Detail {
                Name = "Stiffeners",
                Number = 310001034,
                Class = 99
            };

            detail.SetPrimaryObject(beam);
            detail.SetReferencePoint(position);

            detail.LoadAttributesFromFile("standard");

            detail.SetAttribute("adist4", reverseDirection ? -distanceNet - thickness : distanceNet + thickness);
            detail.SetAttribute("adist5", 2);
            detail.SetAttribute("atab1", reverseDirection ? 2 : 1);
            detail.SetAttribute("tpl2", thickness);
            detail.SetAttribute("tpl3", thickness);
            detail.SetAttribute("partname10", "STIF");
            detail.SetAttribute("mat10", material ?? "Q235");
            detail.SetAttribute("w1_type", 10);
            detail.SetAttribute("w1_type2", 10);
            detail.SetAttribute("w1_size", 6.0);
            detail.SetAttribute("w1_size2", 6.0);
            detail.SetAttribute("w2_type", 10);
            detail.SetAttribute("w2_type2", 10);
            detail.SetAttribute("w2_size", 6.0);
            detail.SetAttribute("w2_size2", 6.0);
            detail.SetAttribute("w3_type", 10);
            detail.SetAttribute("w3_type2", 10);
            detail.SetAttribute("w3_size", 6.0);
            detail.SetAttribute("w3_size2", 6.0);
            //detail.SetAttribute("group_no", 99); // 不起作用

            if (!detail.Insert()) {
                throw new Exception($"Failed to insert detail \"Stiffeners (1034)\".");
            }

            return detail.Identifier;
        }*/

        /*private void FixStiffenerWeldsPosition(IEnumerable<Identifier> weldIDs, Identifier modelObjectID) {
            if (weldIDs is null) {
                throw new ArgumentNullException(nameof(weldIDs));
            }

            if (modelObjectID is null) {
                throw new ArgumentNullException(nameof(modelObjectID));
            }

            if (!(model.SelectModelObject(modelObjectID) is ModelObject obj)) {
                throw new ArgumentException($"Object with identifier {modelObjectID} is not a ModelObject.", nameof(modelObjectID));
            }

            var cs = obj.GetCoordinateSystem();
            var currentTP = model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(cs));
            foreach (var id in weldIDs) {
                var weld = model.SelectModelObject(id) as Weld ??
                    throw new ArgumentException($"Object with identifier {id} is not a Weld.", nameof(weldIDs));

                if (weld.Position == Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z || weld.Position == Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z) {
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                    weld.Modify();
                }
            }
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTP);
        }*/

        /// <summary>
        /// 获取一个 <see cref="Part"/> 的标高。
        /// </summary>
        /// <remarks>
        /// 亦可使用 <see cref="ModelObject.GetReportProperty(string, ref string)"/> 方法获取 "BOTTOM_LEVEL" 和 "TOP_LEVEL" 属性的值，
        /// 但需再用 <see cref="double.Parse(string)"/> 或 <see cref="double.TryParse(string, out double)"/> 方法进行转换，并且要注意单位数量级。
        /// </remarks>
        /// <param name="partIdentifier"><see cref="Part"/> 的 Identifier 属性</param>
        /// <param name="localTransformationPlane">当前变换平面 或 全局变换平面</param>
        /// <param name="bottom_level">底标高 或 顶标高</param>
        /// <returns><see cref="Part"/> 的标高。</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="partIdentifier"/> 对应的 <see cref="ModelObject"/> 不是 <see cref="Part"/> 时引发。
        /// </exception>
        private double GetLevel(Identifier partIdentifier, bool localTransformationPlane, bool bottom_level) {
            var part = model.SelectModelObject(partIdentifier) as Part
                ?? throw new ArgumentException($"Object with identifier {partIdentifier} is not a part.", nameof(partIdentifier));

            TransformationPlane localTP = null;
            if (!localTransformationPlane) {
                localTP = model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            }

            var solid = part.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var level = bottom_level ? solid.MinimumPoint.Z : solid.MaximumPoint.Z;

            if (!localTransformationPlane) {
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localTP);
            }

            return level;
        }

        /// <summary>
        /// 创建连接板。
        /// </summary>
        /// <param name="beamID"></param>
        /// <param name="stifIDs"></param>
        /// <param name="targetBeamID"></param>
        /// <param name="contour"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <param name="isPre"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        private Identifier CreatGusset(Identifier beamID, IEnumerable<Identifier> stifIDs, Identifier targetBeamID,
            ref IEnumerable<Point> contour, double thickness, string material, bool isPre, bool isLower) {

            var beam = model.SelectModelObject(beamID);
            var targetBeam = model.SelectModelObject(targetBeamID) as Beam;

            /*var propertyName = isLower ? "BOTTOM_LEVEL" : "TOP_LEVEL";
            var beamLvlStr = string.Empty;
            var targetBeamLvlStr = string.Empty;
            if (!beam.GetReportProperty(propertyName, ref beamLvlStr)) {
                throw new Exception($"Can't get {propertyName} for {beamID}.");
            }

            if (!targetBeam.GetReportProperty(propertyName, ref targetBeamLvlStr)) {
                throw new Exception($"Can't get {propertyName} for {targetBeamID}.");
            }

            var beamLvl = double.Parse(beamLvlStr) * 1000;
            var targetBeamLvl = double.Parse(targetBeamLvlStr) * 1000;*/

            var beamLvl = GetLevel(beamID, true, isLower);
            var targetBeamLvl = GetLevel(targetBeamID, true, isLower);

            if (isLower && beamLvl >= targetBeamLvl || !isLower && beamLvl <= targetBeamLvl) {
                contour = contour.Take(2).Concat(contour.Skip(contour.Count() - 2));
            }

            var gussetLvl = isLower ? Math.Max(beamLvl, targetBeamLvl) : Math.Min(beamLvl, targetBeamLvl);
            contour = contour.Select(p => { p.Z = gussetLvl; return p; });

            //  ContourPlate 的 Position.DepthEnum 与其 Contour.ContourPoints 顺序有关，根据右手螺旋法则确定
            var plate = ModelOperation.CreatContourPlate(contour, "GUSSET", $"PL{thickness}", material,
                depthEnum: (isLower && !isPre || !isLower && isPre)
                    ? Position.DepthEnum.FRONT
                    : Position.DepthEnum.BEHIND);

            ModelOperation.CreatWeld(beam, plate, false, position: Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z,
                typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);

            if (isLower && beamLvl >= targetBeamLvl || !isLower && beamLvl <= targetBeamLvl) {
                return plate.Identifier;
            }

            if (stifIDs != null) {
                foreach (var stifID in stifIDs) {
                    var stif = model.SelectModelObject(stifID);
                    ModelOperation.CreatWeld(stif, plate, false, position: Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z,
                        typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
                }
            }

            return plate.Identifier;
        }

        /// <summary>
        /// 创建支撑并安装螺栓。
        /// </summary>
        /// <param name="currentGussetID"></param>
        /// <param name="nextGussetID"></param>
        /// <param name="profile"></param>
        /// <param name="material"></param>
        /// <param name="boltPositions"></param>
        /// <param name="boltStd"></param>
        /// <param name="boltSize"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Identifier CreatBraceAndBolt(Identifier currentGussetID, Identifier nextGussetID, string profile, string material,
            DistanceList boltPositions, string boltStd, double boltSize, bool isLower) {
            var curGusset = model.SelectModelObject(currentGussetID) as ContourPlate;
            var nxtGusset = model.SelectModelObject(nextGussetID) as ContourPlate;

            var curContour = curGusset.Contour.ContourPoints.Cast<Point>();
            var nxtContour = nxtGusset.Contour.ContourPoints.Cast<Point>();

            var p1 = curContour.Take(1).Append(curContour.Last()).Aggregate((p1, p2) => p1 + p2).Multiply(0.5);
            var p2 = nxtContour.Take(1).Append(nxtContour.Last()).Aggregate((p1, p2) => p1 + p2).Multiply(0.5);
            var direction = new Vector(p2 - p1).GetNormal();

            var coincidentLength = boltPositions.Sum(dis => dis.Value);
            p1 -= direction * coincidentLength;
            p2 += direction * coincidentLength;

            var brace = ModelOperation.CreatBeam(
                isLower ? p2 : p1, isLower ? p1 : p2, "BRACE", profile, material,
                depthEnum: isLower ? Position.DepthEnum.FRONT : Position.DepthEnum.BEHIND,
                depthOffset: gussetThickness,
                rotationEnum: isLower ? Position.RotationEnum.FRONT : Position.RotationEnum.BACK);

            var boltPositionNum = boltPositions.Count();
            var curBolt = ModelOperation.CreatBoltArray(curGusset, brace, null, p1, p2,
                boltPositions.Take(boltPositionNum - 1).Skip(1), new TSD.Distance[1],
                new Position { Rotation = Position.RotationEnum.FRONT }, new Offset { Dx = boltPositions.First().Value },
                null, boltStd, boltSize);
            var nxtBolt = ModelOperation.CreatBoltArray(nxtGusset, brace, null, p2, p1,
                boltPositions.Take(boltPositionNum - 1).Skip(1), new TSD.Distance[1],
                new Position { Rotation = Position.RotationEnum.FRONT }, new Offset { Dx = boltPositions.First().Value },
                null, boltStd, boltSize);

            return brace.Identifier;
        }

        private void CreatSplices(IEnumerable<Identifier> beamIDs, IEnumerable<bool> reverseDirection) {

            var totalNum = beamIDs.Count();

            var beams = beamIDs.Select(id => model.SelectModelObject(id) as Beam);
            var centerLines = beams.Zip(reverseDirection, (beam, reverse) => {
                var centerLine = beam.GetCenterLine(false).Cast<Point>();
                var line = new Line(centerLine.First(), centerLine.Last());
                if (reverse) line.Direction *= -1;
                return line;
            });

            var gussetIDs = new GussetIdentifiers[totalNum];
            for (int i = 0; i < totalNum; i++) {
                gussetIDs[i] = new GussetIdentifiers();
            }

            var origin = new Point();
            var axisX = new Vector(100, 0, 0);
            var axisZ = new Vector(0, 0, 100);
            var angleMax = 170.0 / 180.0 * Math.PI;
            var braceProfile = new ProfileL(braceProfileStr);
            var coincidentLength = boltPositions.Sum(dis => dis.Value);
            var index = -1;
            foreach (var id in beamIDs) {
                ++index;
                var preIndex = index == 0 ? totalNum - 1 : index - 1;
                var nxtIndex = index == totalNum - 1 ? 0 : index + 1;

                var curBeam = beams.ElementAt(index);
                var preLine = centerLines.ElementAt(preIndex);
                var curLine = centerLines.ElementAt(index);
                var nxtLine = centerLines.ElementAt(nxtIndex);

                var angle_pre = preLine.Direction.GetAngleBetween_WithDirection(curLine.Direction, axisZ);
                var angle_nxt = curLine.Direction.GetAngleBetween_WithDirection(nxtLine.Direction, axisZ);

                double beamWidth = 0.0, beamWebThickness = 0.0;
                if (!curBeam.GetReportProperty("WIDTH", ref beamWidth)) {
                    if (!curBeam.GetReportProperty("PART.WIDTH", ref beamWidth)) {
                        if (!curBeam.GetReportProperty("PART.PROFILE.WIDTH", ref beamWidth)) {
                            throw new Exception($"Can't get beam width for {id}.");
                        }
                    }
                }
                if (!curBeam.GetReportProperty("WEB_THICKNESS", ref beamWebThickness)) {
                    if (!curBeam.GetReportProperty("PROFILE.WEB_THICKNESS", ref beamWebThickness)) {
                        if (!curBeam.GetReportProperty("PART.PROFILE.WEB_THICKNESS", ref beamWebThickness)) {
                            throw new Exception($"Can't get beam web thickness for {id}.");
                        }
                    }
                }

                //  计算连接板角点
                //  确保前后加劲板位置一致
                IEnumerable<Point> gussetPoints_pre = null, gussetPoints_nxt = null;
                double p2_X = double.MaxValue, p3_X = double.MinValue;
                if (angle_pre <= angleMax) {
                    gussetPoints_pre = CalculateGussetPoints(
                        beamWidth, beamWebThickness, braceProfile.h, sectionLength, (Math.PI - angle_pre) * 0.5,
                        extendedDistance, coincidentLength, clearance, true);
                    p2_X = gussetPoints_pre.ElementAt(2).X;
                    p3_X = gussetPoints_pre.ElementAt(3).X;
                }

                if (angle_nxt <= angleMax) {
                    gussetPoints_nxt = CalculateGussetPoints(
                        beamWidth, beamWebThickness, braceProfile.h, sectionLength, (Math.PI - angle_nxt) * 0.5,
                        extendedDistance, coincidentLength, clearance, false);
                    p2_X = Math.Min(p2_X, gussetPoints_nxt.ElementAt(2).X);
                    p3_X = Math.Max(p3_X, gussetPoints_nxt.ElementAt(3).X);
                }

                if (angle_pre <= angleMax) {
                    var tmpArray = gussetPoints_pre.ToArray();
                    tmpArray[1].X = p2_X;
                    tmpArray[2].X = p2_X;
                    tmpArray[3].X = p3_X;
                    tmpArray[4].X = p3_X;
                    gussetPoints_pre = tmpArray;
                }

                if (angle_nxt <= angleMax) {
                    var tmpArray = gussetPoints_nxt.ToArray();
                    tmpArray[1].X = p2_X;
                    tmpArray[2].X = p2_X;
                    tmpArray[3].X = p3_X;
                    tmpArray[4].X = p3_X;
                    gussetPoints_nxt = tmpArray;
                }

                //  X轴到当前梁中心线方向的旋转矩阵
                var matrix = MatrixFactory.Rotate(-axisX.GetAngleBetween_WithDirection(curLine.Direction, axisZ), axisZ);
                var matrixTranspose = matrix.GetTranspose();

                //  创建加劲板
                var position = matrix.Transform(new Point(p2_X, 0, 0));
                var distanceNet = p3_X - p2_X;
                var stifIDs = CreatStiffeners(id, reverseDirection.ElementAt(index), position, stiffenerThickness, distanceNet, materialStr);
                var stiffeners = stifIDs.Select(id => model.SelectModelObject(id) as ContourPlate);

                //  创建连接板
                ContourPlate gusset;
                Identifier gussetID;
                IEnumerable<Point> gussetPoints;

                if (angle_pre >= angleMax)
                    goto SKIP_GUSSET_PRE;

                gussetPoints = gussetPoints_pre.Select(p => matrix.Transform(p));
                gussetID = CreatGusset(id, stifIDs.Take(2), beamIDs.ElementAt(preIndex),
                    ref gussetPoints, gussetThickness, materialStr, true, true);
                gussetIDs[index].pre_lower = gussetID;
                //  修复轮廓点顺序错误
                gusset = model.SelectModelObject(gussetID) as ContourPlate;
                gusset.Contour.ContourPoints = new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                gusset.Modify();

                if (creatUpperSplices == 1) {
                    gussetPoints = gussetPoints_pre.Select(p => matrix.Transform(p));
                    gussetID = CreatGusset(id, stifIDs.Take(2), beamIDs.ElementAt(preIndex),
                        ref gussetPoints, gussetThickness, materialStr, true, false);
                    gussetIDs[index].pre_upper = gussetID;
                    //  修复轮廓点顺序错误
                    gusset = model.SelectModelObject(gussetID) as ContourPlate;
                    gusset.Contour.ContourPoints = new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                    gusset.Modify();
                }

            SKIP_GUSSET_PRE:;

                if (angle_nxt >= angleMax)
                    goto SKIP_GUSSET_NXT;

                gussetPoints = gussetPoints_nxt.Select(p => matrix.Transform(p));
                gussetID = CreatGusset(id, stifIDs.Skip(2), beamIDs.ElementAt(nxtIndex),
                    ref gussetPoints, gussetThickness, materialStr, false, true);
                gussetIDs[index].nxt_lower = gussetID;
                //  修复轮廓点顺序错误
                gusset = model.SelectModelObject(gussetID) as ContourPlate;
                gusset.Contour.ContourPoints = new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                gusset.Modify();

                if (creatUpperSplices == 1) {
                    gussetPoints = gussetPoints_nxt.Select(p => matrix.Transform(p));
                    gussetID = CreatGusset(id, stifIDs.Skip(2), beamIDs.ElementAt(nxtIndex),
                        ref gussetPoints, gussetThickness, materialStr, false, false);
                    gussetIDs[index].nxt_upper = gussetID;
                    //  修复轮廓点顺序错误
                    gusset = model.SelectModelObject(gussetID) as ContourPlate;
                    gusset.Contour.ContourPoints = new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                    gusset.Modify();
                }

            SKIP_GUSSET_NXT:;
            }

            //  创建支撑
            index = -1;
            foreach (var id in beamIDs) {
                ++index;
                var nxtIndex = index == totalNum - 1 ? 0 : index + 1;

                var curLine = centerLines.ElementAt(index);
                var nxtLine = centerLines.ElementAt(nxtIndex);

                var radian_nxt = curLine.Direction.GetAngleBetween_WithDirection(nxtLine.Direction, axisZ);

                if (radian_nxt >= angleMax)
                    goto SKIP_BRACE_NXT;

                CreatBraceAndBolt(gussetIDs[index].nxt_lower, gussetIDs[nxtIndex].pre_lower, braceProfileStr, materialStr,
                    boltPositions, boltStandard, boltSize, true);

                if (creatUpperSplices == 1) {
                    CreatBraceAndBolt(gussetIDs[index].nxt_upper, gussetIDs[nxtIndex].pre_upper, braceProfileStr, materialStr,
                        boltPositions, boltStandard, boltSize, false);
                }

            SKIP_BRACE_NXT:;
            }
        }

        #endregion
    }
}