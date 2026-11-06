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
 *  KJ1002.cs: sideway restraint of beam flange
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Muggle.TsExtensions.Common.Resources;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Plugins;
using Distance = Tekla.Structures.Geometry3d.Distance;
using Localization = Tekla.Structures.Dialog.Localization;
using TsData = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1002;

[Plugin("KJ1002")]
[PluginUserInterface("Muggle.TsExtensions.KJ1002.View")]
[SecondaryType(SecondaryType.SECONDARYTYPE_MULTIPLE)]
[AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z)]
[FieldsFrom(typeof(PluginData))]
public partial class KJ1002 : ConnectionBase {

    private class GussetIdentifiers {
        public Identifier PreLower;
        public Identifier PreUpper;
        public Identifier NextLower;
        public Identifier NextUpper;
    }

    private TsData.DistanceList _boltDistListX;

    private Localization Localization { get; }

    private Model Model { get; }

    [PartFieldDefaultValues("Brace", profile: "L70*5.0", material: "Q235B")]
    [PlateFieldDefaultValues("Stiffener", thickness: 8, material: "Q235B")]
    [PlateFieldDefaultValues("Gusset", thickness: 8, material: "Q235B")]
    [BoltFieldDefaultValues(1, standard: "HS10.9", size: 16, distXText: "70")]
    [WeldFieldDefaultValues(1, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
    [WeldFieldDefaultValues(2, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
    [GeneralFieldDefaultValues("CreateUpperSplices", 0, "SectionLength", 1000, "Clearance", 50, "ExtendedDistance", 30,
        "BoltOffsetX", 50)]
    private PluginData Data { get; }

    public KJ1002(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(KJ1002)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            var beamIDs = GetBeamIdentifiers();
            var reverseDirection = DetermineIfNeedToReverseDirection(beamIDs);

            Verify(beamIDs, reverseDirection);

            SetDataToDefaultIfUnset();

            _ = new ProfileL(Data.PartBraceProfile);

            GetFieldValuesFrom(Data);

            _boltDistListX = TsData.DistanceList.Parse(_bolt1DistXText, CultureInfo.CurrentCulture,
                TsData.Distance.CurrentUnitType);

            var workTp = GetWorkTransformationPlane(beamIDs, reverseDirection);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            beamIDs = OrderBeams(beamIDs, ref reverseDirection);

            if (IsDefaultValue(Data.SectionLength) || Data.SectionLength <= 0.0) {
                Data.SectionLength = CalculateSectionLength(beamIDs, reverseDirection);
                _sectionLength = Data.SectionLength;
            }

            CreateSplices(beamIDs, reverseDirection);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }

    /// <summary>
    /// 在一个集合里统一管理所有梁ID。
    /// </summary>
    /// <remarks>
    /// 第一个元素是主梁ID，后续元素依次是按顺序输入的梁ID。
    /// </remarks>
    /// <returns>所有梁ID。</returns>
    private List<Identifier> GetBeamIdentifiers() {
        var ids = new List<Identifier> { Primary };
        ids.AddRange(Secondaries);
        return ids;
    }

    /// <summary>
    /// 判断梁方向是否需要反转。
    /// </summary>
    /// <remarks>
    /// 以远离节点的方向为正向，否则需要反转。
    /// </remarks>
    /// <param name="beamIDs"></param>
    /// <returns>与 <paramref name="beamIDs"/> 各元素对应，是否需反转方向。</returns>
    private List<bool> DetermineIfNeedToReverseDirection(List<Identifier> beamIDs) {
        var reverse = new bool[beamIDs.Count];

        var segments = beamIDs
            .Select(id => {
                var beam = (Beam)Model.SelectModelObject(id);
                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                return new LineSegment(centerLine.First(), centerLine.Last());
            })
            .ToArray();

        var primSeg = segments[0];
        var secdSeg = segments[1];

        var d1 = Distance.PointToPoint(primSeg.StartPoint, secdSeg.StartPoint);
        var d2 = Distance.PointToPoint(primSeg.StartPoint, secdSeg.EndPoint);
        var d3 = Distance.PointToPoint(primSeg.EndPoint, secdSeg.StartPoint);
        var d4 = Distance.PointToPoint(primSeg.EndPoint, secdSeg.EndPoint);

        if (Math.Min(d1, d2) > Math.Min(d3, d4)) reverse[0] = true;
        if (Math.Min(d1, d3) > Math.Min(d2, d4)) reverse[1] = true;

        var basePoint = reverse[0] ? primSeg.EndPoint : primSeg.StartPoint;

        var otherSegs = segments.Skip(2).ToArray();

        for (int i = 0; i < otherSegs.Length; i++) {
            var seg = otherSegs[i];
            if (Distance.PointToPoint(seg.StartPoint, basePoint) >
                Distance.PointToPoint(seg.EndPoint, basePoint)) {
                reverse[i + 2] = true;
            }
        }

        return [.. reverse];
    }

    /// <summary>
    /// 验证输入的梁是否满足正常连接节点。
    /// </summary>
    /// <param name="beamIDs"></param>
    /// <param name="reverseDirection"></param>
    /// <exception cref="Exception"></exception>
    private void Verify(List<Identifier> beamIDs, List<bool> reverseDirection) {
        var beams = beamIDs.Select(id => Model.SelectModelObject(id) as Beam);
        var centerLines = beams
            .Zip(reverseDirection, (beam, reverse) => {
                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                var line = new Line(centerLine.First(), centerLine.Last());
                if (reverse) line.Direction *= -1;
                return line;
            })
            .ToArray();

        var primLine = centerLines[0];

        //  不能全部平行
        if (centerLines.Skip(1)
            .All(line => Parallel.VectorToVector(line.Direction, primLine.Direction))) {
            throw new Exception(Localization.GetText("exception_cannot_all_beams_parallel"));
        }

        //  所有向量必须共面
        var firstNotParallelLineIndex = centerLines.Skip(1)
            .Select((line, index) => (line, index))
            .First(item => !Parallel.VectorToVector(item.line.Direction, primLine.Direction))
            .index + 1;
        var plane = new GeometricPlane(
            new Point(),
            primLine.Direction,
            centerLines[firstNotParallelLineIndex].Direction);
        if (centerLines.Skip(1)
            .Take(firstNotParallelLineIndex - 1)
            .Concat(centerLines.Skip(firstNotParallelLineIndex + 1))
            .Any(line => Math.Abs(plane.Normal.GetAngleBetweenPrecisely(line.Direction) - Math.PI * 0.5) >
                         GeometryConstants.ANGULAR_EPSILON)) {
            throw new Exception(Localization.GetText("exception_all_beam_vectors_must_be_coplanar"));
        }

        //  必须交汇于同一点
        var projectionLines = centerLines.Select(line => Projection.LineToPlane(line, plane)).ToArray();
        var p = IntersectionExtension
            .LineToLine(projectionLines[0], projectionLines[firstNotParallelLineIndex]).StartPoint;
        if (projectionLines.Skip(1)
            .Take(firstNotParallelLineIndex - 1)
            .Concat(projectionLines.Skip(firstNotParallelLineIndex + 1))
            .Any(line => Distance.PointToLine(p, line) >= GeometryConstants.DISTANCE_EPSILON)) {
            throw new Exception(
                Localization.GetText("exception_all_beam_projection_lines_must_intersect_at_the_same_point"));
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
    private TransformationPlane GetWorkTransformationPlane(List<Identifier> beamIDs, List<bool> reverseDirection) {
        var lines = beamIDs
            .Select(id => {
                var beam = (Beam)Model.SelectModelObject(id);
                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                return new Line(centerLine.First(), centerLine.Last());
            })
            .ToArray();

        var primLine = lines[0];
        if (reverseDirection[0]) primLine.Direction *= -1;

        var axisX = primLine.Direction;

        var firstNotParallelLineIndex = lines.Skip(1)
            .Select((line, index) => (line, index))
            .First(item => !Parallel.VectorToVector(item.line.Direction, axisX))
            .index + 1;
        var firstNotParallelLine = lines[firstNotParallelLineIndex];
        if (reverseDirection[firstNotParallelLineIndex]) firstNotParallelLine.Direction *= -1;

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
    private List<Identifier> OrderBeams(List<Identifier> beamIDs, ref List<bool> reverseDirection) {
        var zip = beamIDs
            .Zip(reverseDirection, (id, r) => (id, r))
            .Select(z => {
                var beam = (Beam)Model.SelectModelObject(z.id);
                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                var vector = new Vector(centerLine.Last() - centerLine.First());
                if (z.r) vector *= -1;

                return (z.id, vector, reverse: z.r);
            })
            .ToList();

        var primVector = zip[0].vector;
        var axisZ = new Vector(0, 0, 100);

        var ordered = zip.Take(1)
            .Concat(zip.Skip(1).OrderBy(z =>
                primVector.GetAngleBetweenWithDirection(z.vector, axisZ)))
            .ToArray();

        reverseDirection = [.. ordered.Select(o => o.reverse)];
        return [.. ordered.Select(o => o.id)];
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

        var beam = (Beam)Model.SelectModelObject(identifier);
        var beamLine = beam.GetCenterLine(false);
        var p = (Point)(reverseDirection ? beamLine[0] : beamLine[beamLine.Count - 1]);

        var minPoint = new Point(p.X - 10, p.Y - 10, p.Z - 10);
        var maxPoint = new Point(p.X + 10, p.Y + 10, p.Z + 10);
        var enumerator = Model.GetModelObjectSelector().GetObjectsByBoundingBox(minPoint, maxPoint);

        Beam column = null;
        foreach (ModelObject obj in enumerator) {
            if (obj is not Beam col) continue;

            var colName = col.Name.ToLower();
            if (col.Type != Beam.BeamTypeEnum.COLUMN && !names.Any(colName.Contains)) continue;

            column = col;
            break;
        }

        if (column == null)
            return new Vector(p).GetLength(); //完整应当计算两头距离，误差不大

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
    private double CalculateSectionLength(List<Identifier> beamIDs, List<bool> reverseDirection) {
        var maxSpan = double.MinValue;
        var maxHeight = double.MinValue;
        var cnt = -1;
        foreach (var id in beamIDs) {
            ++cnt;
            var span = CalculateBeamSpan(id, reverseDirection[cnt]);
            maxSpan = Math.Max(maxSpan, span);

            var beam = (Beam)Model.SelectModelObject(id);
            var height = 0.0;
            if (!beam.GetReportProperty("HEIGHT", ref height)) {
                if (!beam.GetReportProperty("PART.HEIGHT", ref height)) {
                    if (!beam.GetReportProperty("PART.PROFILE.HEIGHT", ref height)) {
                        throw new Exception(string.Format(Localization.GetText("exception_cannot_get_value_for"),
                            "HEIGHT", beam.Identifier.GUID));
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
    private List<Point> CalculateGussetPoints(double beamWidth, double beamWebThickness, double braceWidth,
        double sectionLength, double braceBeamAngle, double extendedDistance, double coincidentLength,
        double clearance, bool braceOnRight) {
        if (braceBeamAngle is <= 0 or >= Math.PI * 0.5) {
            throw new ArgumentOutOfRangeException(nameof(braceBeamAngle),
                Localization.GetText("exception_angle_between_beam_and_brace_must_in_range"));
        }

        var beamLine = new Line(new Point(), new Vector(1, 0, 0));
        var braceLine = new Line(new Point(sectionLength, 0, 0),
            new Vector(-Math.Cos(braceBeamAngle), Math.Sin(braceBeamAngle), 0));

        var p = IntersectionExtension.LineToLine(
                beamLine.Offset(beamWidth * 0.5 + clearance, LineExtension.OffsetDirection.Left),
                braceLine.Offset(braceWidth * 0.5, LineExtension.OffsetDirection.Left))
            .StartPoint;

        var axisZ = new Vector(0, 0, 1);
        var endLine = new Line(p + braceLine.Direction * coincidentLength, braceLine.Direction.Cross(axisZ));
        var p0 = IntersectionExtension.LineToLine(
                endLine,
                braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirection.Left))
            .StartPoint;

        var radians2 = Math.PI - braceBeamAngle - 30.0 / 180.0 * Math.PI;
        var hypotenuseLine = new Line(p0, new Vector(Math.Cos(radians2), Math.Sin(radians2), 0));
        var p1 = IntersectionExtension.LineToLine(
                hypotenuseLine,
                beamLine.Offset(beamWidth * 0.5, LineExtension.OffsetDirection.Left))
            .StartPoint;

        var p2 = new Point(p1.X, beamWebThickness * 0.5, 0);

        var p4 = IntersectionExtension.LineToLine(
                beamLine.Offset(beamWidth * 0.5, LineExtension.OffsetDirection.Left),
                braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirection.Right))
            .StartPoint;
        var p3 = new Point(p4.X, beamWebThickness * 0.5, 0);

        var p5 = IntersectionExtension.LineToLine(
                endLine,
                braceLine.Offset(braceWidth * 0.5 + extendedDistance, LineExtension.OffsetDirection.Right))
            .StartPoint;

        if (braceOnRight) {
            p0.Y *= -1;
            p1.Y *= -1;
            p2.Y *= -1;
            p3.Y *= -1;
            p4.Y *= -1;
            p5.Y *= -1;
        }

        return [p0, p1, p2, p3, p4, p5];
    }

    /// <summary>
    /// 在梁上创建加劲板。
    /// </summary>
    /// <param name="beamId"></param>
    /// <param name="reverseDirection"></param>
    /// <param name="position">内侧加劲板的外侧（靠近节点为内侧，反之为外侧）</param>
    /// <param name="distanceNet"></param>
    /// <returns>成功创建的加劲板的 Identifier 集合。
    /// 第一个加劲板在梁右内侧，第二个加劲板在梁右外侧，
    /// 第三个加劲板在梁左内侧，第四个加劲板在梁左外侧。</returns>
    private List<Identifier> InsertStiffeners(Identifier beamId, bool reverseDirection, Point position,
        double distanceNet) {
        var beam = (Part)Model.SelectModelObject(beamId);
        var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
        var direction = new Vector(centerLine.Last() - centerLine.First()).GetNormal();
        direction *= reverseDirection ? -1 : 1;

        ContourPlate InsertContourPlate(ArrayList arr) {
            var contourPlate = CreatePlateStiffener<ContourPlate>();
            contourPlate.Contour.ContourPoints = arr;
            contourPlate.Position.Depth = Position.DepthEnum.MIDDLE;
            contourPlate.Insert();
            return contourPlate;
        }

        var stifs1 = ModelOperation
            .CalculateStiffeners(beam, position - direction * _plateStiffenerThickness * 0.5, _plateStiffenerThickness)
            .Select(InsertContourPlate)
            .ToList();
        var stifs2 = ModelOperation
            .CalculateStiffeners(beam, position + direction * (distanceNet + _plateStiffenerThickness * 0.5),
                _plateStiffenerThickness)
            .Select(InsertContourPlate)
            .ToList();

        var tmpTp = new TransformationPlane(centerLine[0], direction, new Vector(0, 0, 1).Cross(direction));

        var p = stifs1[0].Contour.ContourPoints[0] as Point;
        p = p.TransformTo(tmpTp);
        ContourPlate[] stifs;
        if (p.Y <= 0) {
            stifs = [stifs1[0], stifs2[0], stifs1[1], stifs2[1]];
        } else {
            stifs = [stifs1[1], stifs2[1], stifs1[0], stifs2[0]];
        }

        var currentTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(tmpTp);

        foreach (var stif in stifs) {
            var weld = CreateWeld1<Weld>();
            weld.MainObject = beam;
            weld.SecondaryObject = stif;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            if (_weld1Around != 0) {
                continue;
            }

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();
        }

        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTp);

        return [.. stifs.Select(stif => stif.Identifier)];
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
    private Identifier CreateStiffeners(Identifier beamID, bool reverseDirection, Point position,
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
    /// <param name="bottomLevel">底标高 或 顶标高</param>
    /// <returns><see cref="Part"/> 的标高。</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="partIdentifier"/> 对应的 <see cref="ModelObject"/> 不是 <see cref="Part"/> 时引发。
    /// </exception>
    private double GetLevel(Identifier partIdentifier, bool localTransformationPlane, bool bottomLevel) {
        var part = Model.SelectModelObject(partIdentifier) as Part
                   ?? throw new ArgumentException(
                       string.Format(Localization.GetText("exception_is_not"), partIdentifier, "Part"),
                       nameof(partIdentifier));

        TransformationPlane localTp = null;
        if (!localTransformationPlane) {
            localTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
        }

        var solid = part.GetSolid(Solid.SolidCreationTypeEnum.RAW);
        var level = bottomLevel ? solid.MinimumPoint.Z : solid.MaximumPoint.Z;

        if (!localTransformationPlane) {
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localTp);
        }

        return level;
    }

    /// <summary>
    /// 创建连接板。
    /// </summary>
    /// <param name="beamId"></param>
    /// <param name="targetBeamId"></param>
    /// <param name="isPre"></param>
    /// <param name="isLower"></param>
    /// <param name="contour"></param>
    /// <param name="stifIds"></param>
    /// <returns></returns>
    private Identifier CreateGusset(Identifier beamId, Identifier targetBeamId, bool isPre, bool isLower,
        ref List<Point> contour, IEnumerable<Identifier> stifIds) {
        var beam = Model.SelectModelObject(beamId);

        /*var propertyName = isLower ? "BOTTOM_LEVEL" : "TOP_LEVEL";
        var beamLvlStr = string.Empty;
        var targetBeamLvlStr = string.Empty;
        if (!beam.GetReportProperty(propertyName, ref beamLvlStr)) {
            throw new Exception($"Can't get {propertyName} for {beamId}.");
        }

        if (!targetBeam.GetReportProperty(propertyName, ref targetBeamLvlStr)) {
            throw new Exception($"Can't get {propertyName} for {targetBeamId}.");
        }

        var beamLvl = double.Parse(beamLvlStr) * 1000;
        var targetBeamLvl = double.Parse(targetBeamLvlStr) * 1000;*/

        var beamLvl = GetLevel(beamId, true, isLower);
        var targetBeamLvl = GetLevel(targetBeamId, true, isLower);

        if (isLower && beamLvl >= targetBeamLvl || !isLower && beamLvl <= targetBeamLvl) {
            contour = [.. contour.Take(2), .. contour.Skip(contour.Count - 2)];
        }

        var gussetLvl = isLower ? Math.Max(beamLvl, targetBeamLvl) : Math.Min(beamLvl, targetBeamLvl);
        contour = [
            .. contour.Select(p => {
                p.Z = gussetLvl;
                return p;
            })
        ];

        //  ContourPlate 的 Position.DepthEnum 与其 Contour.ContourPoints 顺序有关，根据右手螺旋法则确定
        var plate = CreatePlateGusset<ContourPlate>();
        plate.Contour.ContourPoints = new ArrayList(contour.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
        plate.Position.Depth = isLower && !isPre || !isLower && isPre
            ? Position.DepthEnum.FRONT
            : Position.DepthEnum.BEHIND;
        plate.Insert();

        var weld = CreateWeld2<Weld>();
        weld.MainObject = beam;
        weld.SecondaryObject = plate;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        if (isLower && beamLvl >= targetBeamLvl || !isLower && beamLvl <= targetBeamLvl || stifIds == null) {
            return plate.Identifier;
        }

        foreach (var stifId in stifIds) {
            var stif = Model.SelectModelObject(stifId);
            weld.MainObject = stif;
            weld.Insert();
        }

        return plate.Identifier;
    }

    /// <summary>
    /// 创建支撑并安装螺栓。
    /// </summary>
    /// <param name="currentGussetId"></param>
    /// <param name="nextGussetId"></param>
    /// <param name="isLower"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private Identifier CreateBraceAndBolt(Identifier currentGussetId, Identifier nextGussetId, bool isLower) {
        var curGusset = (ContourPlate)Model.SelectModelObject(currentGussetId);
        var nxtGusset = (ContourPlate)Model.SelectModelObject(nextGussetId);

        var curContour = curGusset.Contour.ContourPoints.Cast<Point>().ToArray();
        var nxtContour = nxtGusset.Contour.ContourPoints.Cast<Point>().ToArray();

        var p1 = (curContour.First() + curContour.Last()).Multiply(0.5);
        var p2 = (nxtContour.First() + nxtContour.Last()).Multiply(0.5);
        var direction = new Vector(p2 - p1).GetNormal();

        var coincidentLength = _boltDistListX.Sum(dis => dis.Value) + _boltOffsetX * 2;
        p1 -= direction * coincidentLength;
        p2 += direction * coincidentLength;

        var brace = CreatePartBrace<Beam>();
        brace.StartPoint = isLower ? p2 : p1;
        brace.EndPoint = isLower ? p1 : p2;
        brace.Position.Rotation = isLower ? Position.RotationEnum.FRONT : Position.RotationEnum.BACK;
        brace.Position.Depth = isLower ? Position.DepthEnum.FRONT : Position.DepthEnum.BEHIND;
        brace.Position.DepthOffset = _plateGussetThickness;
        brace.Insert();

        var curBolt = CreateBolt1<BoltArray>();
        curBolt.PartToBoltTo = curGusset;
        curBolt.PartToBeBolted = brace;
        curBolt.FirstPosition = p1;
        curBolt.SecondPosition = p2;
        curBolt.Position.Rotation = Position.RotationEnum.FRONT;
        curBolt.StartPointOffset.Dx = _boltOffsetX;
        curBolt.Insert();

        var nxtBolt = CreateBolt1<BoltArray>();
        nxtBolt.PartToBoltTo = nxtGusset;
        nxtBolt.PartToBeBolted = brace;
        nxtBolt.FirstPosition = p2;
        nxtBolt.SecondPosition = p1;
        nxtBolt.Position.Rotation = Position.RotationEnum.FRONT;
        nxtBolt.StartPointOffset.Dx = _boltOffsetX;
        nxtBolt.Insert();

        return brace.Identifier;
    }

    private void CreateSplices(List<Identifier> beamIDs, List<bool> reverseDirection) {
        var totalNum = beamIDs.Count;

        var beams = beamIDs.Select(id => Model.SelectModelObject(id) as Beam).ToArray();
        var centerLines = beams
            .Zip(reverseDirection, (beam, reverse) => {
                var centerLine = beam.GetCenterLine(false).Cast<Point>().ToArray();
                var line = new Line(centerLine.First(), centerLine.Last());
                if (reverse) line.Direction *= -1;
                return line;
            })
            .ToArray();

        var gussetIDs = new GussetIdentifiers[totalNum];
        for (int i = 0; i < totalNum; i++) {
            gussetIDs[i] = new GussetIdentifiers();
        }

        var axisX = new Vector(100, 0, 0);
        var axisZ = new Vector(0, 0, 100);
        const double angleMax = 170.0 / 180.0 * Math.PI;
        var braceProfile = new ProfileL(_partBraceProfile);
        var coincidentLength = _boltDistListX.Sum(dis => dis.Value) + _boltOffsetX * 2;
        var index = -1;
        foreach (var id in beamIDs) {
            ++index;
            var preIndex = index == 0 ? totalNum - 1 : index - 1;
            var nxtIndex = index == totalNum - 1 ? 0 : index + 1;

            var curBeam = beams[index];
            var preLine = centerLines[preIndex];
            var curLine = centerLines[index];
            var nxtLine = centerLines[nxtIndex];

            var anglePre = preLine.Direction.GetAngleBetweenWithDirection(curLine.Direction, axisZ);
            var angleNxt = curLine.Direction.GetAngleBetweenWithDirection(nxtLine.Direction, axisZ);

            double beamWidth = 0.0, beamWebThickness = 0.0;
            if (!curBeam.GetReportProperty("WIDTH", ref beamWidth)) {
                if (!curBeam.GetReportProperty("PART.WIDTH", ref beamWidth)) {
                    if (!curBeam.GetReportProperty("PART.PROFILE.WIDTH", ref beamWidth)) {
                        throw new Exception(string.Format(Localization.GetText("exception_cannot_get_value_for"),
                            "WIDTH", id.GUID));
                    }
                }
            }

            if (!curBeam.GetReportProperty("WEB_THICKNESS", ref beamWebThickness)) {
                if (!curBeam.GetReportProperty("PROFILE.WEB_THICKNESS", ref beamWebThickness)) {
                    if (!curBeam.GetReportProperty("PART.PROFILE.WEB_THICKNESS", ref beamWebThickness)) {
                        throw new Exception(string.Format(Localization.GetText("exception_cannot_get_value_for"),
                            "WEB_THICKNESS", id.GUID));
                    }
                }
            }

            //  计算连接板角点
            //  确保前后加劲板位置一致
            List<Point> gussetPointsPre = null, gussetPointsNxt = null;
            double p2X = double.MaxValue, p3X = double.MinValue;
            if (anglePre <= angleMax) {
                gussetPointsPre = CalculateGussetPoints(
                    beamWidth, beamWebThickness, braceProfile.h, _sectionLength, (Math.PI - anglePre) * 0.5,
                    _extendedDistance, coincidentLength, _clearance, true);
                p2X = gussetPointsPre[2].X;
                p3X = gussetPointsPre[3].X;
            }

            if (angleNxt <= angleMax) {
                gussetPointsNxt = CalculateGussetPoints(
                    beamWidth, beamWebThickness, braceProfile.h, _sectionLength, (Math.PI - angleNxt) * 0.5,
                    _extendedDistance, coincidentLength, _clearance, false);
                p2X = Math.Min(p2X, gussetPointsNxt[2].X);
                p3X = Math.Max(p3X, gussetPointsNxt[3].X);
            }

            if (anglePre <= angleMax) {
                gussetPointsPre![1].X = p2X;
                gussetPointsPre[2].X = p2X;
                gussetPointsPre[3].X = p3X;
                gussetPointsPre[4].X = p3X;
            }

            if (angleNxt <= angleMax) {
                gussetPointsNxt![1].X = p2X;
                gussetPointsNxt[2].X = p2X;
                gussetPointsNxt[3].X = p3X;
                gussetPointsNxt[4].X = p3X;
            }

            //  X轴到当前梁中心线方向的旋转矩阵
            var matrix = MatrixFactory.Rotate(-axisX.GetAngleBetweenWithDirection(curLine.Direction, axisZ), axisZ);

            //  创建加劲板
            var position = matrix.Transform(new Point(p2X, 0, 0));
            var distanceNet = p3X - p2X;
            var stifIDs = InsertStiffeners(id, reverseDirection[index], position, distanceNet);

            //  创建连接板
            ContourPlate gusset;
            Identifier gussetId;
            List<Point> gussetPoints;

            if (anglePre > angleMax)
                goto SKIP_GUSSET_PRE;

            gussetPoints = [.. gussetPointsPre!.Select(matrix.Transform)];
            gussetId = CreateGusset(id, beamIDs[preIndex], true, true, ref gussetPoints, stifIDs.Take(2));
            gussetIDs[index].PreLower = gussetId;
            //  修复轮廓点顺序错误
            gusset = (ContourPlate)Model.SelectModelObject(gussetId);
            gusset.Contour.ContourPoints =
                new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
            gusset.Modify();

            if (_createUpperSplices == 1) {
                gussetPoints = [.. gussetPointsPre.Select(matrix.Transform)];
                gussetId = CreateGusset(id, beamIDs[preIndex], true, false, ref gussetPoints, stifIDs.Take(2));
                gussetIDs[index].PreUpper = gussetId;
                //  修复轮廓点顺序错误
                gusset = (ContourPlate)Model.SelectModelObject(gussetId);
                gusset.Contour.ContourPoints =
                    new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                gusset.Modify();
            }

            SKIP_GUSSET_PRE: ;

            if (angleNxt > angleMax)
                goto SKIP_GUSSET_NXT;

            gussetPoints = [.. gussetPointsNxt!.Select(matrix.Transform)];
            gussetId = CreateGusset(id, beamIDs[nxtIndex], false, true, ref gussetPoints, stifIDs.Skip(2));
            gussetIDs[index].NextLower = gussetId;
            //  修复轮廓点顺序错误
            gusset = (ContourPlate)Model.SelectModelObject(gussetId);
            gusset.Contour.ContourPoints =
                new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
            gusset.Modify();

            if (_createUpperSplices == 1) {
                gussetPoints = [.. gussetPointsNxt.Select(matrix.Transform)];
                gussetId = CreateGusset(id, beamIDs[nxtIndex], false, false, ref gussetPoints,
                    stifIDs.Skip(2));
                gussetIDs[index].NextUpper = gussetId;
                //  修复轮廓点顺序错误
                gusset = (ContourPlate)Model.SelectModelObject(gussetId);
                gusset.Contour.ContourPoints =
                    new ArrayList(gussetPoints.Select(p => new ContourPoint(p, new Chamfer())).ToArray());
                gusset.Modify();
            }

            SKIP_GUSSET_NXT: ;
        }

        //  创建支撑
        for (var i = 0; i < beamIDs.Count; i++) {
            index = i;
            var nxtIndex = index == totalNum - 1 ? 0 : index + 1;

            var curLine = centerLines[index];
            var nxtLine = centerLines[nxtIndex];

            var radianNxt = curLine.Direction.GetAngleBetweenWithDirection(nxtLine.Direction, axisZ);

            if (radianNxt > angleMax)
                goto SKIP_BRACE_NXT;

            _ = CreateBraceAndBolt(gussetIDs[index].NextLower, gussetIDs[nxtIndex].PreLower, true);

            if (_createUpperSplices == 1) {
                _ = CreateBraceAndBolt(gussetIDs[index].NextUpper, gussetIDs[nxtIndex].PreUpper, false);
            }

            SKIP_BRACE_NXT: ;
        }
    }

}