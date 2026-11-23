/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2024-2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  MG1002.cs: horizontal connection between middle column and two beams
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
using Tekla.Structures.Dialog;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Plugins;
using Distance = Tekla.Structures.Geometry3d.Distance;
using TsDatatype = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MG1002;

[Plugin("MG1002")]
[PluginUserInterface("Muggle.TsExtensions.MG1002.View")]
[SecondaryType(SecondaryType.SECONDARYTYPE_TWO)]
[AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_BASIC)]
[FieldsFromData]
public partial class MG1002 : ConnectionBase {

    private Localization Localization { get; }

    private Model Model { get; }

    private PluginData Data { get; }

    public MG1002(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(MG1002)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            if (Primary == null || Secondaries is not { Count: 2 })
                throw new Exception(Localization.GetText("exception_need_one_primary_two_secondaries"));

            VerifyProfile(out var prfPrim, out var prfSec1, out var prfSec2);

            if (Data.PlateSplicingWebThickness <= 0) {
                Data.PlateSplicingWebThickness = Math.Max(prfSec1.s, prfSec2.s);
            }

            if (Data.PlateVerticalBreadth <= 0) {
                Data.PlateVerticalBreadth =
                    (new[] { prfPrim.b1, prfSec1.b1, prfSec2.b1 }.Min() - Data.PlateSplicingWebThickness) * 0.5;
            }

            if (Data.PlateDiagonalBreadth <= 0) {
                Data.PlateDiagonalBreadth =
                    (new[] { prfPrim.b1, prfSec1.b1, prfSec2.b1 }.Min() - Data.PlateSplicingWebThickness) * 0.5;
            }

            Data.SetFieldsToDefaultIfUnset();
            GetFieldValuesFrom(Data);

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetWorkTransformationPlane());

            CreateConnection(prfPrim, prfSec1, prfSec2);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }

    }

    /// <summary>
    /// 主零件仅支持等截面和对称变截面H型钢，次零件仅支持等截面和楔形H型钢
    /// </summary>
    /// <exception cref="Exception">当选择的零件不支持时抛出。</exception>
    private void VerifyProfile(out ProfileH_Symmetrical prfPrim,
        out ProfileH prfSec1, out ProfileH prfSec2) {

        var primPart = (Beam)Model.SelectModelObject(Primary);
        var secPart1 = (Beam)Model.SelectModelObject(Secondaries[0]);
        var secPart2 = (Beam)Model.SelectModelObject(Secondaries[1]);

        try {
            prfPrim = new ProfileH_Symmetrical(primPart.Profile.ProfileString);
            if (Math.Abs(prfPrim.b2 - prfPrim.b1) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_primary_profile_not_supported"));
        }

        try {
            prfSec1 = new ProfileH(secPart1.Profile.ProfileString);
            if (prfSec1.ProfileText.Contains("I_VAR_A") &&
                Math.Abs(prfSec1.h2 - prfSec1.h1) > GeometryConstants.DISTANCE_EPSILON ||
                Math.Abs(prfSec1.b2 - prfSec1.b1) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_secondary_profile_not_supported"));
        }

        try {
            prfSec2 = new ProfileH(secPart2.Profile.ProfileString);
            if (prfSec2.ProfileText.Contains("I_VAR_A") &&
                Math.Abs(prfSec2.h2 - prfSec2.h1) > GeometryConstants.DISTANCE_EPSILON ||
                Math.Abs(prfSec2.b2 - prfSec2.b1) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_secondary_profile_not_supported"));
        }
    }

    /// <summary>
    /// 获取工作平面。
    /// </summary>
    /// <returns>原点为柱顶点，X轴为柱的零件坐标系Y轴，Y轴为柱的零件坐标系X轴，若柱是从上往下绘制则Y轴反向。</returns>
    private TransformationPlane GetWorkTransformationPlane() {
        var primPart = (Beam)Model.SelectModelObject(Primary);

        var zeroPoint = new Point();
        var disStart = Distance.PointToPoint(zeroPoint, primPart.StartPoint);
        var disEnd = Distance.PointToPoint(zeroPoint, primPart.EndPoint);

        var origin = new Point(disStart > disEnd ? primPart.EndPoint : primPart.StartPoint);
        var axisX = new Vector(primPart.GetCoordinateSystem().AxisY);
        var axisY = new Vector(primPart.GetCoordinateSystem().AxisX);
        axisY *= disStart > disEnd ? 1 : -1;

        return new TransformationPlane(origin, axisX, axisY);
    }

    private void CreateConnection(ProfileH_Symmetrical prfPrim, ProfileH prfSecL, ProfileH prfSecR) {

        var origin = new Point();
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        var primPart = (Beam)Model.SelectModelObject(Primary);
        var secPartL = (Beam)Model.SelectModelObject(Secondaries[0]);
        var secPartR = (Beam)Model.SelectModelObject(Secondaries[1]);

        #region 定义左、右梁

        //  与柱零件坐标系Y轴(即工作变换平面X轴)同向为右梁，反向为左梁
        var disStart = Distance.PointToPoint(origin, secPartL.StartPoint);
        var disEnd = Distance.PointToPoint(origin, secPartL.EndPoint);
        if (new Vector(disStart > disEnd ? secPartL.StartPoint : secPartL.EndPoint).Dot(axisX) > 0) {
            (secPartL, secPartR) = (secPartR, secPartL);
            (prfSecL, prfSecR) = (prfSecR, prfSecL);
        }

        #endregion

        #region 主次零件规格、边线

        var primCs = primPart.GetCoordinateSystem();
        var secLCs = secPartL.GetCoordinateSystem();
        var secRCs = secPartR.GetCoordinateSystem();

        var centerline = primPart.GetCenterLine(false);

        var p1 = (Point)centerline[0];
        var p2 = (Point)centerline[centerline.Count - 1];
        //  不清楚什么原因，有时候Z坐标会是一个很小的数，这里要消除误差
        p1.Z = 0;
        p2.Z = 0;

        var primCenterLine = new Line(p1, p2);

        var primAxisY = primCs.AxisY.GetNormal();
        p1 -= primAxisY * prfPrim.h1 * 0.5;
        p2 -= primAxisY * prfPrim.h2 * 0.5;
        var primLeftLine = new Line(p1, p2);

        p1 += primAxisY * prfPrim.h1;
        p2 += primAxisY * prfPrim.h2;
        var primRightLine = new Line(p1, p2);

        if (axisY.Dot(primCs.AxisX) < 0) {
            primCenterLine.Direction *= -1;
            primLeftLine.Direction *= -1;
            primRightLine.Direction *= -1;
        } //  方向统一成从底指向顶

        centerline = secPartL.GetCenterLine(false);

        p1 = (Point)centerline[0];
        p2 = (Point)centerline[centerline.Count - 1];
        p1.Z = 0;
        p2.Z = 0;

        var secLAxisY = secLCs.AxisY.GetNormal();
        p1 -= secLAxisY * prfSecL.h2 * 0.5;
        p2 -= secLAxisY * prfSecL.h2 * 0.5;
        var secLTopLine = new Line(p1, p2);

        p1 += secLAxisY * prfSecL.h1;
        p2 += secLAxisY * prfSecL.h2;
        var secLBotLine = new Line(p1, p2);

        if (axisX.Dot(secLCs.AxisX) < 0) {
            secLTopLine.Direction *= -1;
            secLBotLine.Direction *= -1;
        } //  方向统一成从远端指向近端

        centerline = secPartR.GetCenterLine(false);

        p1 = (Point)centerline[0];
        p2 = (Point)centerline[1];
        p1.Z = 0;
        p2.Z = 0;

        var secRAxisY = secRCs.AxisY.GetNormal();
        p1 -= secRAxisY * prfSecR.h2 * 0.5;
        p2 -= secRAxisY * prfSecR.h2 * 0.5;
        var secRTopLine = new Line(p1, p2);

        p1 += secRAxisY * prfSecR.h1;
        p2 += secRAxisY * prfSecR.h2;
        var secRBotLine = new Line(p1, p2);

        if (axisX.Dot(secRCs.AxisX) > 0) {
            secRTopLine.Direction *= -1;
            secRBotLine.Direction *= -1;
        } //  方向统一成从远端指向近端

        var parallelTopLine = secLTopLine.Direction.Cross(secRTopLine.Direction)
            .IsZero(GeometryConstants.DISTANCE_EPSILON);

        #endregion

        #region 创建端板、柱末端对齐

        var e1 = Math.Sqrt(Math.Pow(_plateEndThickness, 2) + Math.Pow(_plateEndHeight * 0.5, 2));
        var secLBotLineInside = secLBotLine.Offset(prfSecL.t2, LineExtension.OffsetDirection.Left);
        var secRBotLineInside = secRBotLine.Offset(prfSecR.t2, LineExtension.OffsetDirection.Right);
        var positionOfTriangle = Geometry3dOperation.PositionOfTriangleOnLines(
            (secLBotLineInside, secRBotLineInside, primCenterLine),
            (e1, e1, _plateEndHeight));
        if (positionOfTriangle.Count == 0)
            throw new Exception(Localization.GetText("exception_cannot_place_end_plate"));

        Vector translateVector = null;
        Point p3 = null;
        foreach (var (P1, P2, P3) in positionOfTriangle) {

            //  抛弃P3在上方的组合
            translateVector = new Vector(P3 - (P1 + P2).Multiply(0.5));
            if (translateVector.Y >= 0) continue;

            //  抛弃P1或P2在延长线上的组合
            if (P1.X >= P2.X) continue;

            //  最多可能仍有2组解，不做区分，直接使用第1组解
            p1 = P1;
            p2 = P2;
            p3 = P3;

            break; //  跳过可能存在的第2组解
        }

        p1.Translate(translateVector);
        p2.Translate(translateVector);
        p3.Translate(translateVector);

        //  端板线从左指向右
        var endPlateLine = new Line(p1, p2);
        var endPlateTopLine = endPlateLine.Offset(-1 * translateVector);
        var endPlateBotLine = endPlateLine.Offset(translateVector);

        var endPlateLower = Data.CreatePlateEnd<Beam>();
        endPlateLower.StartPoint = p1;
        endPlateLower.EndPoint = p2;
        endPlateLower.Position.Rotation = Position.RotationEnum.TOP;
        endPlateLower.Position.Plane = Position.PlaneEnum.RIGHT;
        endPlateLower.Position.Depth = Position.DepthEnum.MIDDLE;
        endPlateLower.Insert();

        var endPlateUpper = Data.CreatePlateEnd<Beam>();
        endPlateUpper.StartPoint = p1;
        endPlateUpper.EndPoint = p2;
        endPlateUpper.Position.Rotation = Position.RotationEnum.TOP;
        endPlateUpper.Position.Plane = Position.PlaneEnum.LEFT;
        endPlateUpper.Position.Depth = Position.DepthEnum.MIDDLE;
        endPlateUpper.Insert();

        var primFittingPlane = new Plane {
            Origin = endPlateBotLine.Origin, AxisX = endPlateBotLine.Direction.GetNormal(), AxisY = axisZ
        };
        var fitting = new Fitting { Father = primPart, Plane = primFittingPlane, };
        fitting.Insert();

        #endregion

        #region 创建拼接腹板、梁末端对齐、切割

        var secLTopLineInside = secLTopLine.Offset(prfSecL.t1, LineExtension.OffsetDirection.Right);
        var secRTopLineInside = secRTopLine.Offset(prfSecR.t1, LineExtension.OffsetDirection.Left);

        p2 = endPlateTopLine.Origin + secLBotLineInside.Direction.GetNormal(-_distWebFlangeSeam);
        p1 = Projection.PointToLine(p2, secLTopLineInside);
        p3 = new Point(endPlateTopLine.Origin);
        var p4 = p3 + endPlateLine.Direction.GetNormal(_plateEndHeight);
        var p5 = p4 + secRBotLineInside.Direction.GetNormal(-_distWebFlangeSeam);
        var p6 = Projection.PointToLine(p5, secRTopLineInside);
        var p7 = parallelTopLine
            ? IntersectionExtension.LineToLine(secLTopLineInside, primCenterLine).StartPoint
            : IntersectionExtension.LineToLine(secLTopLineInside, secRTopLineInside).StartPoint;

        var chamferNone = new Chamfer();

        var splicingWeb = Data.CreatePlateSplicingWeb<ContourPlate>();
        splicingWeb.Contour.ContourPoints = new ArrayList(
            new[] { p1, p2, p3, p4, p5, p6, p7 }
                .Select(p => new ContourPoint(p, chamferNone))
                .ToArray());
        splicingWeb.Insert();

        var secFittingPlane = new Plane { Origin = p7, AxisX = axisY, AxisY = axisZ };
        fitting = new Fitting { Father = secPartL, Plane = secFittingPlane, };
        fitting.Insert();
        fitting = new Fitting { Father = secPartR, Plane = secFittingPlane, };
        fitting.Insert();

        var p3_1 = IntersectionExtension.LineToLine(secLBotLine, new Line(p3, translateVector)).StartPoint;
        var p4_1 = IntersectionExtension.LineToLine(secRBotLine, new Line(p4, translateVector)).StartPoint;
        var p7_1 = new[] {
            IntersectionExtension.LineToLine(secLBotLine, primCenterLine).StartPoint,
            IntersectionExtension.LineToLine(secRBotLine, primCenterLine).StartPoint
        }.OrderBy(p => p.Y).First();
        var cuttingPart = ModelOperation.CreateBooleanOperationPolygon(
            [p1, p2, p3, p3_1, p7_1, p4_1, p4, p5, p6, p7],
            Math.Max(prfSecL.b1, prfSecR.b1));
        cuttingPart.Insert();
        _ = ModelOperation.ApplyBooleanOperation(secPartL, cuttingPart);
        _ = ModelOperation.ApplyBooleanOperation(secPartR, cuttingPart);
        cuttingPart.Delete();

        #endregion

        #region 创建竖板

        var vertLeftLine = new Line(IntersectionExtension.LineToLine(primLeftLine, endPlateBotLine).StartPoint, axisY);
        var vertLeftLineOffset = vertLeftLine
            .Offset(_plateVerticalThickness, LineExtension.OffsetDirection.Right);

        p1 = IntersectionExtension.LineToLine(vertLeftLine, secLTopLineInside).StartPoint;
        p2 = IntersectionExtension.LineToLine(vertLeftLine, endPlateTopLine).StartPoint;
        p3 = IntersectionExtension.LineToLine(vertLeftLineOffset, secLTopLineInside).StartPoint;
        p4 = IntersectionExtension.LineToLine(vertLeftLineOffset, endPlateTopLine).StartPoint;

        p1.Y = p1.Y < p3.Y ? p1.Y : p3.Y;
        p2.Y = p2.Y > p4.Y ? p2.Y : p4.Y;

        p1.Z = _plateSplicingWebThickness * 0.5;
        p2.Z = _plateSplicingWebThickness * 0.5;

        p3 = p2 + axisZ * _plateVerticalBreadth;
        p4 = p1 + axisZ * _plateVerticalBreadth;

        var chamfer = Data.CreateChamferVertical();
        var cp1 = new ContourPoint(p1, chamfer);

        chamfer = Data.CreateChamferVertical().GetReversed();
        var cp2 = new ContourPoint(p2, chamfer);

        var cp3 = new ContourPoint(p3, chamferNone);
        var cp4 = new ContourPoint(p4, chamferNone);

        var vertPlateLeftFront = Data.CreatePlateVertical<ContourPlate>();
        vertPlateLeftFront.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        vertPlateLeftFront.Position.Depth = Position.DepthEnum.BEHIND;
        vertPlateLeftFront.Insert();

        cp1.Z *= -1;
        cp2.Z *= -1;
        cp3.Z *= -1;
        cp4.Z *= -1;

        var vertPlateLeftBehind = Data.CreatePlateVertical<ContourPlate>();
        vertPlateLeftBehind.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        vertPlateLeftBehind.Position.Depth = Position.DepthEnum.FRONT;
        vertPlateLeftBehind.Insert();

        var vertRightLine =
            new Line(IntersectionExtension.LineToLine(primRightLine, endPlateBotLine).StartPoint, axisY);
        var vertRightLineOffset = vertRightLine
            .Offset(_plateVerticalThickness, LineExtension.OffsetDirection.Left);

        p1 = IntersectionExtension.LineToLine(vertRightLine, secRTopLineInside).StartPoint;
        p2 = IntersectionExtension.LineToLine(vertRightLine, endPlateTopLine).StartPoint;
        p3 = IntersectionExtension.LineToLine(vertRightLineOffset, secRTopLineInside).StartPoint;
        p4 = IntersectionExtension.LineToLine(vertRightLineOffset, endPlateTopLine).StartPoint;

        p1.Y = p1.Y < p3.Y ? p1.Y : p3.Y;
        p2.Y = p2.Y > p4.Y ? p2.Y : p4.Y;

        p1.Z = _plateSplicingWebThickness * 0.5;
        p2.Z = _plateSplicingWebThickness * 0.5;

        p3 = p2 + axisZ * _plateVerticalBreadth;
        p4 = p1 + axisZ * _plateVerticalBreadth;

        chamfer = Data.CreateChamferVertical();
        cp1 = new ContourPoint(p1, chamfer);

        chamfer = chamfer.GetReversed();
        cp2 = new ContourPoint(p2, chamfer);

        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        var vertPlateRightFront = Data.CreatePlateVertical<ContourPlate>();
        vertPlateRightFront.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        vertPlateRightFront.Position.Depth = Position.DepthEnum.FRONT;
        vertPlateRightFront.Insert();

        cp1.Z *= -1;
        cp2.Z *= -1;
        cp3.Z *= -1;
        cp4.Z *= -1;

        var vertPlateRightBehind = Data.CreatePlateVertical<ContourPlate>();
        vertPlateRightBehind.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        vertPlateRightBehind.Position.Depth = Position.DepthEnum.BEHIND;
        vertPlateRightBehind.Insert();

        #endregion

        #region 创建对角板

        ContourPlate diagPlateFront = null, diagPlateBehind = null;
        var diagIntersectSecL = false;

        if (_plateDiagonalThickness <= 0) goto SkipDiagPlate;

        var secsIntersectPoint = parallelTopLine
            ? IntersectionExtension.LineToLine(secLTopLine, primCenterLine).StartPoint
            : IntersectionExtension.LineToLine(secLTopLine, secRTopLine).StartPoint;
        diagIntersectSecL = _diagonalPos1 + vertLeftLine.Origin.X < secsIntersectPoint.X;

        var diagLine = new Line(
            IntersectionExtension.LineToLine(diagIntersectSecL ? secLTopLineInside : secRTopLineInside,
                    vertLeftLine.Offset(_diagonalPos1, LineExtension.OffsetDirection.Right))
                .StartPoint,
            IntersectionExtension.LineToLine(endPlateTopLine,
                    vertRightLine.Offset(_diagonalPos2, LineExtension.OffsetDirection.Left))
                .StartPoint);
        p1 = diagIntersectSecL
            ? IntersectionExtension.LineToLine(secLTopLineInside,
                    diagLine.Offset(_plateDiagonalThickness * 0.5, LineExtension.OffsetDirection.Left))
                .StartPoint
            : IntersectionExtension.LineToLine(secRTopLineInside,
                    diagLine.Offset(_plateDiagonalThickness * 0.5, LineExtension.OffsetDirection.Right))
                .StartPoint;
        p2 = IntersectionExtension.LineToLine(endPlateTopLine,
                diagLine.Offset(_plateDiagonalThickness * 0.5,
                    diagIntersectSecL
                        ? LineExtension.OffsetDirection.Right
                        : LineExtension.OffsetDirection.Left))
            .StartPoint;
        p1 = Projection.PointToLine(p1, diagLine);
        p2 = Projection.PointToLine(p2, diagLine);

        p1.Z = _plateSplicingWebThickness * 0.5;
        p2.Z = _plateSplicingWebThickness * 0.5;

        p3 = p2 + axisZ * _plateDiagonalBreadth;
        p4 = p1 + axisZ * _plateDiagonalBreadth;

        chamfer = Data.CreateChamferDiagonal();
        cp1 = new ContourPoint(p1, chamfer);

        chamfer = chamfer.GetReversed();
        cp2 = new ContourPoint(p2, chamfer);

        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        diagPlateFront = Data.CreatePlateDiagonal<ContourPlate>();
        diagPlateFront.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        diagPlateFront.Insert();

        cp1.Z *= -1;
        cp2.Z *= -1;
        cp3.Z *= -1;
        cp4.Z *= -1;

        diagPlateBehind = Data.CreatePlateDiagonal<ContourPlate>();
        diagPlateBehind.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        diagPlateBehind.Insert();

        SkipDiagPlate: ;

        #endregion

        #region 创建翼缘加劲板

        p1 = IntersectionExtension.LineToLine(endPlateBotLine, primLeftLine).StartPoint;
        p2 = IntersectionExtension.LineToLine(endPlateBotLine,
                vertLeftLine.Offset(_plateFlangeStifBreadth, LineExtension.OffsetDirection.Left))
            .StartPoint;
        p4 = IntersectionExtension.LineToLine(primLeftLine,
                endPlateBotLine.Offset(_plateFlangeStifHeight, LineExtension.OffsetDirection.Right))
            .StartPoint;

        cp1 = new ContourPoint(p1, Data.CreateChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        ArrayList contours;
        if (_flangeStifKind != 0) {
            p3 = IntersectionExtension.LineToLine(
                    vertLeftLine.Offset(_plateFlangeStifBreadth, LineExtension.OffsetDirection.Left),
                    endPlateBotLine.Offset(_plateFlangeStifHeight, LineExtension.OffsetDirection.Right))
                .StartPoint;
            cp3 = new ContourPoint(p3, Data.CreateChamferStifOutside());

            contours = new ArrayList { cp1, cp2, cp3, cp4 };
        } else {
            contours = new ArrayList { cp1, cp2, cp4 };
        }

        var leftFlangeStif = Data.CreatePlateFlangeStif<ContourPlate>();
        leftFlangeStif.Contour.ContourPoints = contours;
        leftFlangeStif.Insert();

        p1 = IntersectionExtension.LineToLine(endPlateBotLine, primRightLine).StartPoint;
        p2 = IntersectionExtension.LineToLine(endPlateBotLine,
                vertRightLine.Offset(_plateFlangeStifBreadth, LineExtension.OffsetDirection.Right))
            .StartPoint;
        p4 = IntersectionExtension.LineToLine(primRightLine,
                endPlateBotLine.Offset(_plateFlangeStifHeight, LineExtension.OffsetDirection.Right))
            .StartPoint;

        cp1 = new ContourPoint(p1, Data.CreateChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        if (_flangeStifKind != 0) {
            p3 = IntersectionExtension.LineToLine(
                    vertRightLine.Offset(_plateFlangeStifBreadth, LineExtension.OffsetDirection.Right),
                    endPlateBotLine.Offset(_plateFlangeStifHeight, LineExtension.OffsetDirection.Right))
                .StartPoint;
            cp3 = new ContourPoint(p3, Data.CreateChamferStifOutside());

            contours = new ArrayList { cp1, cp2, cp3, cp4 };
        } else {
            contours = new ArrayList { cp1, cp2, cp4 };
        }

        var rightFlangeStif = Data.CreatePlateFlangeStif<ContourPlate>();
        rightFlangeStif.Contour.ContourPoints = contours;
        rightFlangeStif.Insert();

        #endregion

        #region 创建腹板加劲板

        var endPlateLineAngle = endPlateLine.Direction.GetAngleBetweenPrecisely(axisX);
        var offset = Math.Tan(endPlateLineAngle) * _plateWebStifThickness * 0.5;

        p1 = IntersectionExtension.LineToLine(vertLeftLine, endPlateTopLine).StartPoint;
        p1.Z = _plateSplicingWebThickness * 0.5;
        p1.Y += offset;
        p2 = p1 + axisY * _plateWebStifHeight;
        p4 = p1 + axisZ * _plateWebStifBreadth;

        chamfer = Data.CreateChamferStifInside();
        cp1 = new ContourPoint(p1, chamfer);
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        p5 = IntersectionExtension.LineToLine(vertLeftLine, endPlateBotLine).StartPoint;
        p5.Z = _plateSplicingWebThickness * 0.5;
        p5.Y -= offset;
        p6 = p5 - axisY * _plateWebStifHeight;
        var p8 = p5 + axisZ * _plateWebStifBreadth;

        var cp5 = new ContourPoint(p5, chamfer);
        var cp6 = new ContourPoint(p6, chamferNone);
        var cp8 = new ContourPoint(p8, chamferNone);

        ContourPoint[] contoursUpper, contoursLower;
        if (_webStifKind != 0) {
            p3 = p2 + axisZ * _plateWebStifBreadth;
            p7 = p6 + axisZ * _plateWebStifBreadth;

            chamfer = Data.CreateChamferStifOutside();

            cp3 = new ContourPoint(p3, chamfer);
            var cp7 = new ContourPoint(p7, chamfer);

            contoursUpper = [cp1, cp2, cp3, cp4];
            contoursLower = [cp5, cp6, cp7, cp8];
        } else {
            contoursUpper = [cp1, cp2, cp4];
            contoursLower = [cp5, cp6, cp8];
        }

        var webStifDistList = TsDatatype.DistanceList.Parse(_webStifDistList, CultureInfo.CurrentCulture,
            TsDatatype.Distance.CurrentUnitType);
        var transX = endPlateLine.Direction.GetNormal();

        var upWebStifs = new List<ContourPlate>();
        var lowWebStifs = new List<ContourPlate>();
        foreach (var dist in webStifDistList) {
            for (var i = 0; i < contoursUpper.Length; i++) {
                contoursUpper[i] = contoursUpper[i].Addition(transX * dist.Value);
            }

            var plate = Data.CreatePlateWebStif<ContourPlate>();
            plate.Contour.ContourPoints = new ArrayList(contoursUpper);
            plate.Insert();
            upWebStifs.Add(plate);

            foreach (var cp in contoursUpper) {
                cp.Z *= -1;
            }

            plate = Data.CreatePlateWebStif<ContourPlate>();
            plate.Contour.ContourPoints = new ArrayList(contoursUpper);
            plate.Insert();
            upWebStifs.Add(plate);

            foreach (var cp in contoursUpper) {
                cp.Z *= -1;
            }

            for (var i = 0; i < contoursLower.Length; i++) {
                contoursLower[i] = contoursLower[i].Addition(transX * dist.Value);
            }

            plate = Data.CreatePlateWebStif<ContourPlate>();
            plate.Contour.ContourPoints = new ArrayList(contoursLower);
            plate.Insert();
            lowWebStifs.Add(plate);

            foreach (var cp in contoursLower) {
                cp.Z *= -1;
            }

            plate = Data.CreatePlateWebStif<ContourPlate>();
            plate.Contour.ContourPoints = new ArrayList(contoursLower);
            plate.Insert();
            lowWebStifs.Add(plate);

            foreach (var cp in contoursLower) {
                cp.Z *= -1;
            }
        }

        #endregion

        #region 创建螺栓

        var bolt = Data.CreateBolt1<BoltArray>();
        bolt.FirstPosition = endPlateLower.StartPoint;
        bolt.SecondPosition = endPlateLower.EndPoint;
        bolt.PartToBoltTo = endPlateLower;
        bolt.PartToBeBolted = endPlateUpper;
        bolt.Position.Rotation = Position.RotationEnum.BELOW;
        bolt.StartPointOffset.Dx = _boltStartOffsetX;
        bolt.Insert();

        #endregion

        #region 焊接

        var weld = Data.CreateWeld1<Weld>();
        weld.MainObject = primPart;
        weld.SecondaryObject = endPlateLower;

        if (_weld1Around != 0) {
            weld.Insert();
        } else {
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Insert();
        }

        weld = Data.CreateWeld2<Weld>();
        weld.MainObject = splicingWeb;
        weld.SecondaryObject = endPlateUpper;
        weld.Insert();

        weld.MainObject = secPartL;
        weld.SecondaryObject = splicingWeb;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.MainObject = secPartR;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld = Data.CreateWeld3<Weld>();
        weld.MainObject = secPartL;
        weld.SecondaryObject = endPlateUpper;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.MainObject = secPartR;
        weld.Insert();

        weld = Data.CreateWeld4<Weld>();
        weld.MainObject = secPartL;
        weld.SecondaryObject = splicingWeb;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        weld.MainObject = secPartR;
        weld.Insert();

        weld = Data.CreateWeld5<Weld>();
        weld.MainObject = secPartL;
        weld.SecondaryObject = secPartR;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld = Data.CreateWeld6<Weld>();
        weld.MainObject = secPartL;
        weld.SecondaryObject = vertPlateLeftFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.SecondaryObject = vertPlateLeftBehind;
        weld.Insert();

        weld.MainObject = splicingWeb;
        weld.SecondaryObject = vertPlateLeftFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.SecondaryObject = vertPlateLeftBehind;
        weld.Insert();

        weld.MainObject = endPlateUpper;
        weld.SecondaryObject = vertPlateLeftFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.SecondaryObject = vertPlateLeftBehind;
        weld.Insert();

        weld.MainObject = secPartR;
        weld.SecondaryObject = vertPlateRightFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.SecondaryObject = vertPlateRightBehind;
        weld.Insert();

        weld.MainObject = splicingWeb;
        weld.SecondaryObject = vertPlateRightFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        weld.SecondaryObject = vertPlateRightBehind;
        weld.Insert();

        weld.MainObject = endPlateUpper;
        weld.SecondaryObject = vertPlateRightFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.SecondaryObject = vertPlateRightBehind;
        weld.Insert();

        if (_plateDiagonalThickness > 0) {
            weld = Data.CreateWeld7<Weld>();
            weld.MainObject = splicingWeb;
            weld.SecondaryObject = diagPlateFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();

            weld.MainObject = endPlateUpper;
            weld.SecondaryObject = diagPlateFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();

            weld.MainObject = diagIntersectSecL ? secPartL : secPartR;
            weld.SecondaryObject = diagPlateFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();
        }

        weld = Data.CreateWeld8<Weld>();
        weld.MainObject = primPart;
        weld.SecondaryObject = leftFlangeStif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        weld.MainObject = endPlateLower;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.MainObject = primPart;
        weld.SecondaryObject = rightFlangeStif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.MainObject = endPlateLower;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        foreach (var stif in upWebStifs) {
            weld.MainObject = endPlateUpper;
            weld.SecondaryObject = stif;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld.Insert();

            weld.MainObject = splicingWeb;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();
        }

        foreach (var stif in lowWebStifs) {
            weld.MainObject = primPart;
            weld.SecondaryObject = stif;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld.MainObject = endPlateLower;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
            weld.Insert();
        }

        #endregion

    }

}