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
 *  MG1001.cs: "MG1001" connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using static Tekla.Structures.Geometry3d.GeometryConstants;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using TSDatatype = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MG1001;

[PlateFields("top", "diagonal", "horizontal", "thickened", "eave", "end1", "end2", "flangeStif", "webStif")]
[ChamferFields("diagonal", "horizontal", "stifInside", "stifOutside")]
[BoltFields(1)]
[WeldFields(1, 2, 3, 4, 5, 6, 7, 8, 9)]
[GeneralFields(typeof(int), "isTopHorizontal", "flangeStifKind", "webStifKind")]
[GeneralFields(typeof(double), "diagPosition1", "diagPosition2", "thickenedPosition", "endPosition",
    "difference", "grooveSlope", "boltOffset")]
[GeneralFields(typeof(string), "webStifDistList")]
public partial class MG1001Data;

[Plugin("MG1001")]
[PluginUserInterface("Muggle.TsExtensions.MG1001.Views.MainWindow")]
[SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
[AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_BASIC)]
[FieldsFrom(typeof(MG1001Data))]
public partial class MG1001 : ConnectionBase {

    private Model Model { get; set; }

    [PlateFieldDefaultValues("top", name: "Top")]
    [PlateFieldDefaultValues("diagonal", name: "Diagonal")]
    [PlateFieldDefaultValues("horizontal", name: "Horizontal")]
    [PlateFieldDefaultValues("thickened", name: "Thickened", @class: 4)]
    [PlateFieldDefaultValues("eave", name: "Eave")]
    [PlateFieldDefaultValues("end1", name: "End1", thickness: 28, breadth: 350, height: 1355)]
    [PlateFieldDefaultValues("end2", name: "End2", thickness: 28, breadth: 350, height: 1255)]
    [PlateFieldDefaultValues("flangeStif", name: "FlangeStif", thickness: 10, breadth: 150, height: 150)]
    [PlateFieldDefaultValues("webStif", name: "WebStif", thickness: 10, breadth: 150, height: 150)]
    [ChamferFieldDefaultValues("diagonal", type: 1, x: 25, y: 25)]
    [ChamferFieldDefaultValues("horizontal", type: 1, x: 25, y: 25)]
    [ChamferFieldDefaultValues("stifInside", type: 1, x: 25, y: 25)]
    [ChamferFieldDefaultValues("stifOutside", type: 1, x: 25, y: 25)]
    [BoltFieldDefaultValues(1, size: 20, standard: "HS10.9", distXText: "80 6*150 120", distYText: "70")]
    [WeldFieldDefaultValues(1, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(2, shop: 1, typeAbove: 5, angleAbove: 15, rootFaceAbove: 4,
        rootOpeningAbove: 2, preparation: 1)]
    [WeldFieldDefaultValues(3, shop: 1, typeAbove: 5, sizeAbove: 8, angleAbove: 15, rootFaceAbove: 4,
        rootOpeningAbove: 2, preparation: 1)]
    [WeldFieldDefaultValues(4, shop: 1, typeAbove: 10, typeBelow: 10, sizeAbove: 8, sizeBelow: 8)]
    [WeldFieldDefaultValues(5, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(6, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(7, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(8, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(9, shop: 1, typeAbove: 10, sizeAbove: 8)]
    [GeneralFieldDefaultValues("isTopHorizontal", 0, "flangeStifKind", 0, "webStifKind", 0,
        "diagPosition1", 80, "diagPosition2", 80, "thickenedPosition", 100, "endPosition", 160,
        "difference", 4, "grooveSlope", 2.5, "boltOffset", 70, "webStifDistList", "190 4*150")]
    private MG1001Data Data { get; set; }

    #region Constructor

    public MG1001(MG1001Data data) {
        Model = new Model();
        Data = data;
    }

    #endregion

    #region Overrides

    public override bool Run() {
        var result = false;

        try {
            var primPart = Model.SelectModelObject(Primary) as Beam;
            var secPart = Model.SelectModelObject(Secondaries[0]) as Beam;

            CheckIfAcceptableProfile(primPart, secPart, out var prfPrim, out var prfSec);

            SetDataToDefaultIfUnset();

            if (Data.PlateTopThickness <= 0) Data.PlateTopThickness = prfSec.t1;
            if (Data.PlateTopBreadth <= 0) Data.PlateTopBreadth = prfSec.b1;

            if (Data.PlateHorizontalThickness <= 0) Data.PlateHorizontalThickness = prfSec.t1;
            if (Data.PlateHorizontalBreadth <= 0)
                Data.PlateHorizontalBreadth =
                    (Math.Min(prfPrim.b1, prfSec.b1) - Math.Max(prfPrim.s, Data.PlateThickenedThickness)) * 0.5;

            if (Data.PlateDiagonalThickness > 0 && Data.PlateDiagonalBreadth <= 0) {
                Data.PlateDiagonalBreadth =
                    (Math.Min(prfPrim.b1, prfSec.b1) - Math.Max(prfPrim.s, Data.PlateThickenedThickness)) * 0.5;
            }

            if (Data.ThickenedPosition < 0) Data.ThickenedPosition = 0;

            GetFieldValuesFrom(Data);

            var workTp = GetWorkTransformationPlane(prfPrim, prfSec, out var roofSlope);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            result = CreatConnection(roofSlope);
        } catch (Exception e) {
            MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// 仅支持等截面和楔形变截面，不支持对称变截面
    /// </summary>
    /// <param name="primPart"></param>
    /// <param name="secPart"></param>
    /// <param name="prfPrim"></param>
    /// <param name="prfSec"></param>
    private static void CheckIfAcceptableProfile(Beam primPart, Beam secPart,
        out ProfileH prfPrim, out ProfileH prfSec) {

        prfPrim = new ProfileH(primPart.Profile.ProfileString);
        prfSec = new ProfileH(secPart.Profile.ProfileString);

        if (prfPrim.ProfileText.Contains("I_VAR_A") && Math.Abs(prfPrim.h2 - prfPrim.h1) > DISTANCE_EPSILON ||
            Math.Abs(prfPrim.b2 - prfPrim.b1) > DISTANCE_EPSILON)
            throw new UnAcceptableProfileException(prfPrim.ProfileText);
        if (prfSec.ProfileText.Contains("I_VAR_A") && Math.Abs(prfSec.h2 - prfSec.h1) > DISTANCE_EPSILON ||
            Math.Abs(prfSec.b2 - prfSec.b1) > DISTANCE_EPSILON)
            throw new UnAcceptableProfileException(prfSec.ProfileText);
    }

    private TransformationPlane GetWorkTransformationPlane(ProfileH prfPrim, ProfileH prfSec, out double slope) {
        var primPart = (Beam)Model.SelectModelObject(Primary);
        var secPart = (Beam)Model.SelectModelObject(Secondaries[0]);

        var primCs = primPart.GetCoordinateSystem();
        var secCs = secPart.GetCoordinateSystem();

        var primCenterLine = primPart.GetCenterLine(false);
        var secCenterLine = secPart.GetCenterLine(false);
        var p1 = primCenterLine[0] as Point;
        var p2 = primCenterLine[primCenterLine.Count - 1] as Point;
        var p3 = secCenterLine[0] as Point;
        var p4 = secCenterLine[secCenterLine.Count - 1] as Point;

        // 大致从节点指向远点
        var primApproximateDirection = new Vector(p2).GetLength() > new Vector(p1).GetLength()
            ? new Vector(p2 - p1)
            : new Vector(p1 - p2);
        var secApproximateDirection = new Vector(p4).GetLength() > new Vector(p3).GetLength()
            ? new Vector(p4 - p3)
            : new Vector(p3 - p4);

        var primAxisY = primCs.AxisY.GetNormal();
        var secAxisY = secCs.AxisY.GetNormal();

        var primLeftLine = primAxisY.Dot(secApproximateDirection) >= 0
            ? new Line(p1 - primAxisY * (prfPrim.h2 * 0.5), p2 - primAxisY * (prfPrim.h2 * 0.5))
            : new Line(p1 + primAxisY * (prfPrim.h1 - prfPrim.h2 * 0.5), p2 + primAxisY * (prfPrim.h2 * 0.5));
        var secTopLine = secAxisY.Dot(primApproximateDirection) >= 0
            ? new Line(p3 - secAxisY * (prfSec.h2 * 0.5), p4 - secAxisY * (prfSec.h2 * 0.5))
            : new Line(p3 + secAxisY * (prfSec.h1 - prfSec.h2 * 0.5), p4 + secAxisY * (prfSec.h2 * 0.5));

        var origin = IntersectionExtension.LineToLine(primLeftLine, secTopLine).StartPoint;
        var axisY = primApproximateDirection * -1;
        var axisZ = primApproximateDirection.Cross(secApproximateDirection);
        var axisX = axisY.Cross(axisZ);

        var workTp = new TransformationPlane(origin, axisX, axisY);

        slope = Math.Abs(Math.Tan(axisX.GetAngleBetween(secTopLine.Direction)));

        return workTp;
    }

    private void FixPrim(ProfileH prfPrim, double endPlateEndPointY) {
        if (Math.Abs(prfPrim.h1 - prfPrim.h2) < DISTANCE_EPSILON) return;

        var primPart = (Beam)Model.SelectModelObject(Primary);

        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);

        var origin = new Point();
        var ps = primPart.StartPoint;
        var pe = primPart.EndPoint;

        var reverseDirection = Distance.PointToPoint(origin, ps) <=
                               Distance.PointToPoint(origin, pe);

        var p1 = reverseDirection
            ? new Point(pe.X + prfPrim.h2, pe.Y - primPart.EndPointOffset.Dx, pe.Z)
            : new Point(ps.X + prfPrim.h1, ps.Y + primPart.StartPointOffset.Dx, ps.Z);
        var p2 = new Point((reverseDirection ? prfPrim.h1 : prfPrim.h2) - prfPrim.t1, endPlateEndPointY, 0);

        var vector = new Vector(p2 - p1);
        var angle = vector.GetAngleBetween(axisX);
        var distance = vector.GetLength();
        angle -= Math.Asin(prfPrim.t1 / distance);
        var line = new Line(p1, Geometry3dOperation.GetDirectionByRadian(angle));

        var point3 = Intersection.LineToLine(
                line,
                new Line(new Point(reverseDirection ? prfPrim.h1 : prfPrim.h2, 0, 0), axisY))
            .StartPoint;

        var d = reverseDirection ? ps.Y - point3.Y : point3.Y - pe.Y;
        if (Math.Abs((reverseDirection ? primPart.StartPointOffset.Dx : primPart.EndPointOffset.Dx) - d)
            < DISTANCE_EPSILON)
            return;

        if (reverseDirection) {
            primPart.StartPointOffset.Dx = d;
        } else {
            primPart.EndPointOffset.Dx = d;
        }

        primPart.Modify();
    }

    private bool CreatConnection(double slope) {

        #region 初始信息、工作平面

        var origin = new Point();
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        var primPart = (Beam)Model.SelectModelObject(Primary);
        var secPart = (Beam)Model.SelectModelObject(Secondaries[0]);

        var prfPrim = new ProfileH(primPart.Profile.ProfileString);
        var prfSec = new ProfileH(secPart.Profile.ProfileString);

        #endregion

        #region 求主零件边线

        var centerLine = primPart.GetCenterLine(false);
        var p1 = centerLine[0] as Point;
        var p2 = centerLine[centerLine.Count - 1] as Point;

        var primCs = primPart.GetCoordinateSystem();
        var primLeftLine = new Line(
            p1 - primCs.AxisY.GetNormal(prfPrim.h2 * 0.5),
            p2 - primCs.AxisY.GetNormal(prfPrim.h2 * 0.5));
        var primRightLine = new Line(
            p1 + primCs.AxisY.GetNormal(prfPrim.h1 - prfPrim.h2 * 0.5),
            p2 + primCs.AxisY.GetNormal(prfPrim.h2 * 0.5));

        if (primCs.AxisY.Dot(axisX) < 0) {
            (primLeftLine, primRightLine) = (primRightLine, primLeftLine);
        }

        var reversePrim = Distance.PointToPoint(origin, p2) > Distance.PointToPoint(origin, p1);
        // 统一为从远端指向近端，关系到左右偏移
        if (reversePrim) {
            primLeftLine.Direction *= -1;
            primRightLine.Direction *= -1;
        }

        #endregion

        #region 求次零件边线

        centerLine = secPart.GetCenterLine(false);
        p1 = centerLine[0] as Point;
        p2 = centerLine[centerLine.Count - 1] as Point;

        var secCs = secPart.GetCoordinateSystem();
        var secTopLine = new Line(
            p1 - secCs.AxisY.GetNormal(prfSec.h2 * 0.5),
            p2 - secCs.AxisY.GetNormal(prfSec.h2 * 0.5));
        var secBotLine = new Line(
            p1 + secCs.AxisY.GetNormal(prfSec.h1 - prfSec.h2 * 0.5),
            p2 + secCs.AxisY.GetNormal(prfSec.h2 * 0.5));

        if (secCs.AxisY.Dot(axisY) > 0) {
            (secTopLine, secBotLine) = (secBotLine, secTopLine);
        }

        var reverseSec = Distance.PointToPoint(origin, p2) > Distance.PointToPoint(origin, p1);
        // 统一为从远端指向近端，关系到左右偏移
        if (reverseSec) {
            secTopLine.Direction *= -1;
            secBotLine.Direction *= -1;
        }

        #endregion

        #region 创建端板

        p1 = new Point { X = Math.Max(prfPrim.h1, prfPrim.h2) - prfPrim.t1 + _plateEnd1Thickness };
        p1.Y = (p1.X + _plateEnd2Thickness) * slope + _endPosition;
        p2 = p1 - axisY * _plateEnd1Height;

        var endPlate1 = CreatPlateEnd1<Beam>();
        endPlate1.StartPoint = p1;
        endPlate1.EndPoint = p2;
        endPlate1.Position.Rotation = Position.RotationEnum.TOP;
        endPlate1.Position.Plane = Position.PlaneEnum.RIGHT;
        endPlate1.Position.Depth = Position.DepthEnum.MIDDLE;
        endPlate1.Insert();

        p2 = p1 - axisY * _plateEnd2Height;
        var endPlate2 = CreatPlateEnd2<Beam>();
        endPlate2.StartPoint = p1;
        endPlate2.EndPoint = p2;
        endPlate2.Position.Rotation = Position.RotationEnum.TOP;
        endPlate2.Position.Plane = Position.PlaneEnum.LEFT;
        endPlate2.Position.Depth = Position.DepthEnum.MIDDLE;
        endPlate2.Insert();

        var endPlateLine = new Line(p1, p2);

        #endregion

        #region 创建柱顶板

        p1 = new Point();
        if (_plateEaveThickness > 0 && _plateEaveBreadth > 0 && _plateEaveHeight > 0) {
            p1.X -= _plateEaveBreadth;
        }

        p2 = new Point { X = Math.Max(prfPrim.h1, prfPrim.h2) - prfPrim.t1 };

        if (_isTopHorizontal != 0) {
            p2.Y = endPlate1.StartPoint.Y - _endPosition;
            p1.Y = p2.Y;
        } else {
            p2.Y = p2.X * slope;
            p1.Y = p1.X * slope;
        }

        var topPlate = CreatPlateTop<Beam>();
        topPlate.Profile.ProfileString = $"PL{_plateTopThickness}*{_plateTopBreadth}";
        topPlate.StartPoint = p1;
        topPlate.EndPoint = p2;
        topPlate.Position.Rotation = Position.RotationEnum.TOP;
        topPlate.Position.Plane = Position.PlaneEnum.RIGHT;
        topPlate.Position.Depth = Position.DepthEnum.MIDDLE;
        topPlate.Insert();

        Fitting fit;
        if (_isTopHorizontal != 0) {
            fit = new Fitting {
                Father = topPlate,
                Plane = new Plane { Origin = p1, AxisX = -1 * axisY, AxisY = axisZ }
            };
            fit.Insert();

            fit = new Fitting {
                Father = topPlate,
                Plane = new Plane { Origin = p2, AxisX = -1 * axisY, AxisY = axisZ }
            };
            fit.Insert();
        }

        var topPlateLine = new Line(p1, p2);

        #endregion

        #region 创建檐口竖板

        ContourPlate eavePlate = null;
        var chamferNone = new Chamfer();

        if (_plateEaveThickness <= 0 || _plateEaveBreadth <= 0 || _plateEaveHeight <= 0) goto SkipEavePlate;

        p1 = Intersection.LineToLine(primLeftLine,
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = Intersection.LineToLine(
                primLeftLine.Offset(_plateEaveBreadth, LineExtension.OffsetDirectionEnum.LEFT),
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        var p3 = p2 - axisY * _plateEaveHeight;
        var p4 = p3 + axisX * _plateEaveBreadth;
        var cp1 = new ContourPoint(p1, chamferNone);
        var cp2 = new ContourPoint(p2, chamferNone);
        var cp3 = new ContourPoint(p3, chamferNone);
        var cp4 = new ContourPoint(p4, chamferNone);

        eavePlate = CreatPlateEave<ContourPlate>();
        eavePlate.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        eavePlate.Insert();

    SkipEavePlate:;

        #endregion

        #region 创建水平板

        p2 = Intersection.LineToLine(
                endPlateLine.Offset(_plateEnd2Thickness, LineExtension.OffsetDirectionEnum.LEFT),
                secBotLine)
            .StartPoint;
        p2.X -= _plateEnd1Thickness + _plateEnd2Thickness;
        p2.Z = Math.Max(prfPrim.s, _plateThickenedThickness) * 0.5;
        p1 = p2 - axisX * (Math.Max(prfPrim.h1, prfPrim.h2) - prfPrim.t1 * 2);
        p3 = p2 + axisZ * _plateHorizontalBreadth;
        p4 = p1 + axisZ * _plateHorizontalBreadth;

        cp1 = new ContourPoint(p1, CreatChamferHorizontal());
        cp2 = new ContourPoint(p2, CreatChamferHorizontal());
        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        var horPlateFront = CreatPlateHorizontal<ContourPlate>();
        horPlateFront.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        horPlateFront.Position.Depth = Position.DepthEnum.BEHIND;
        horPlateFront.Insert();

        cp1.Z *= -1;
        cp2.Z *= -1;
        cp3.Z *= -1;
        cp4.Z *= -1;

        var horPlateBehind = CreatPlateHorizontal<ContourPlate>();
        horPlateBehind.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        horPlateBehind.Position.Depth = Position.DepthEnum.FRONT;
        horPlateBehind.Insert();

        var horPlateLine = new Line(new Point(p1) { Z = 0 }, new Point(p2) { Z = 0 });

        #endregion

        #region 创建柱加厚板

        ContourPlate thkedPlate = null;
        if (_plateThickenedThickness <= prfPrim.s) goto SkipThickened;

        p1 = Intersection.LineToLine(
                primLeftLine.Offset(prfPrim.t1, LineExtension.OffsetDirectionEnum.RIGHT),
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = Intersection.LineToLine(
                primLeftLine.Offset(prfPrim.t1, LineExtension.OffsetDirectionEnum.RIGHT),
                horPlateLine.Offset(_thickenedPosition, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        var p5 = Intersection.LineToLine(
                endPlateLine.Offset(_plateEnd1Thickness, LineExtension.OffsetDirectionEnum.RIGHT),
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;

        cp1 = new ContourPoint(p1, chamferNone);
        cp2 = new ContourPoint(p2, chamferNone);
        var cp5 = new ContourPoint(p5, chamferNone);

        ArrayList contourPoints;
        if (p2.Y < endPlate1.EndPoint.Y) {
            p3 = Intersection.LineToLine(
                    primRightLine.Offset(prfPrim.t1, LineExtension.OffsetDirectionEnum.LEFT),
                    horPlateLine.Offset(_thickenedPosition, LineExtension.OffsetDirectionEnum.RIGHT))
                .StartPoint;
            p4 = new Point(endPlate1.EndPoint);
            p4.X -= _plateEnd1Thickness;

            cp3 = new ContourPoint(p3, chamferNone);
            cp4 = new ContourPoint(p4, chamferNone);

            contourPoints = new ArrayList {
                cp1,
                cp2,
                cp3,
                cp4,
                cp5
            };
        } else {
            p4 = Intersection.LineToLine(
                    endPlateLine.Offset(_plateEnd1Thickness, LineExtension.OffsetDirectionEnum.RIGHT),
                    horPlateLine.Offset(_thickenedPosition, LineExtension.OffsetDirectionEnum.RIGHT))
                .StartPoint;

            cp4 = new ContourPoint(p4, chamferNone);

            contourPoints = new ArrayList { cp1, cp2, cp4, cp5 };
        }

        thkedPlate = CreatPlateThickened<ContourPlate>();
        thkedPlate.Contour.ContourPoints = contourPoints;
        thkedPlate.Insert();

    SkipThickened:;

        #endregion

        #region 创建对角板

        ContourPlate diagPlateFront = null, diagPlateBehind = null;
        if (_plateDiagonalThickness <= 0) goto SkipDiag;

        p1 = Intersection.LineToLine(
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT),
                primLeftLine.Offset(prfPrim.t1 + _diagPosition1, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = Intersection.LineToLine(
                horPlateLine.Offset(_plateHorizontalThickness, LineExtension.OffsetDirectionEnum.LEFT),
                endPlateLine.Offset(_plateEnd1Thickness + _diagPosition2, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;

        var diagPlateLine = new Line(p1, p2);

        p1 = Intersection.LineToLine(
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT),
                diagPlateLine.Offset(_plateDiagonalThickness * 0.5, LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;
        p1 = Projection.PointToLine(p1, diagPlateLine);
        p2 = Intersection.LineToLine(
                horPlateLine.Offset(_plateHorizontalThickness, LineExtension.OffsetDirectionEnum.LEFT),
                diagPlateLine.Offset(_plateDiagonalThickness * 0.5, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = Projection.PointToLine(p2, diagPlateLine);
        p1.Z = Math.Max(prfPrim.s, _plateThickenedThickness) * 0.5;
        p2.Z = p1.Z;

        p3 = p2 + axisZ * _plateDiagonalBreadth;
        p4 = p1 + axisZ * _plateDiagonalBreadth;

        cp1 = new ContourPoint(p1, CreatChamferDiagonal());
        cp2 = new ContourPoint(p2, CreatChamferDiagonal());
        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        diagPlateFront = CreatPlateDiagonal<ContourPlate>();
        diagPlateFront.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        diagPlateFront.Insert();

        cp1.Z *= -1;
        cp2.Z *= -1;
        cp3.Z *= -1;
        cp4.Z *= -1;

        diagPlateBehind = CreatPlateDiagonal<ContourPlate>();
        diagPlateBehind.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        diagPlateBehind.Insert();

    SkipDiag:

        #endregion

        #region 创建翼缘加劲板

        p1 = Intersection.LineToLine(topPlateLine,
                endPlateLine.Offset(_plateEnd1Thickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = p1 + axisY * _plateFlangeStifHeight;
        p4 = Intersection.LineToLine(topPlateLine,
                endPlateLine.Offset(_plateEnd1Thickness + _plateFlangeStifBreadth,
                    LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;

        cp1 = new ContourPoint(p1, CreatChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);
        contourPoints = new ArrayList { cp1, cp2, cp4 };

        if (_flangeStifKind == 1) {
            p3 = p2 - axisX * _plateFlangeStifBreadth;
            cp3 = new ContourPoint(p3, CreatChamferStifOutside());

            contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        }

        var primTopFlangeStif = CreatPlateFlangeStif<ContourPlate>();
        primTopFlangeStif.Contour.ContourPoints = contourPoints;
        primTopFlangeStif.Insert();

        p1 = Intersection.LineToLine(secTopLine,
                endPlateLine.Offset(_plateEnd2Thickness, LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;
        p2 = p1 + axisY * _plateFlangeStifHeight;
        p4 = Intersection.LineToLine(secTopLine,
                endPlateLine.Offset(_plateEnd2Thickness + _plateFlangeStifBreadth,
                    LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;

        cp1 = new ContourPoint(p1, CreatChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);
        contourPoints = new ArrayList { cp1, cp2, cp4 };

        if (_flangeStifKind == 1) {
            p3 = p2 + axisX * _plateFlangeStifBreadth;
            cp3 = new ContourPoint(p3, CreatChamferStifOutside());

            contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        }

        var secTopFlangeStif = CreatPlateFlangeStif<ContourPlate>();
        secTopFlangeStif.Contour.ContourPoints = contourPoints;
        secTopFlangeStif.Insert();

        p1 = Intersection.LineToLine(secBotLine,
                endPlateLine.Offset(_plateEnd2Thickness, LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;
        p2 = p1 - axisY * _plateFlangeStifHeight;
        p4 = Intersection.LineToLine(secBotLine,
                endPlateLine.Offset(_plateEnd2Thickness + _plateFlangeStifBreadth,
                    LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;

        cp1 = new ContourPoint(p1, CreatChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);
        contourPoints = new ArrayList { cp1, cp2, cp4 };

        if (_flangeStifKind == 1) {
            p3 = p2 + axisX * _plateFlangeStifBreadth;
            cp3 = new ContourPoint(p3, CreatChamferStifOutside());

            contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        }

        var secBotFlangeStif = CreatPlateFlangeStif<ContourPlate>();
        secBotFlangeStif.Contour.ContourPoints = contourPoints;
        secBotFlangeStif.Insert();

        #endregion

        #region 创建腹板加劲板

        var primWebStifs = new List<ContourPlate>();

        p1 = Intersection.LineToLine(secTopLine,
                endPlateLine.Offset(_plateEnd2Thickness, LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;
        p1 = p1 - axisX * (_plateEnd1Thickness + _plateEnd2Thickness) +
             axisZ * Math.Max(prfPrim.s, _plateThickenedThickness) * 0.5;
        p2 = p1 - axisX * _plateWebStifBreadth;
        p4 = p1 + axisZ * _plateWebStifHeight;
        cp1 = new ContourPoint(p1, CreatChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        ContourPoint[] cps;
        if (_webStifKind == 1) {
            p3 = p2 + axisZ * _plateWebStifHeight;
            cp3 = new ContourPoint(p3, CreatChamferStifOutside());

            cps = [cp1, cp2, cp3, cp4];
        } else {
            cps = [cp1, cp2, cp4];
        }

        var webStifDistList = TSDatatype.DistanceList.Parse(_webStifDistList,
            CultureInfo.CurrentCulture, TSDatatype.Distance.CurrentUnitType);

        foreach (var distance in webStifDistList) {
            foreach (var cp in cps) {
                cp.Y -= distance.Value;
            }

            var webStif = CreatPlateWebStif<ContourPlate>();
            webStif.Contour.ContourPoints = new ArrayList(cps);
            webStif.Insert();

            primWebStifs.Add(webStif);

            foreach (var cp in cps) {
                cp.Z *= -1;
            }

            webStif = CreatPlateWebStif<ContourPlate>();
            webStif.Contour.ContourPoints = new ArrayList(cps);
            webStif.Insert();

            primWebStifs.Add(webStif);

            foreach (var cp in cps) {
                cp.Z *= -1;
            }
        }

        var secWebStifs = new List<ContourPlate>();

        p1 = Intersection.LineToLine(secTopLine,
                endPlateLine.Offset(_plateEnd2Thickness, LineExtension.OffsetDirectionEnum.LEFT))
            .StartPoint;
        p1.Z = prfSec.s * 0.5;
        p2 = p1 + axisX * _plateWebStifBreadth;
        p4 = p1 + axisZ * _plateWebStifHeight;
        cp1 = new ContourPoint(p1, CreatChamferStifInside());
        cp2 = new ContourPoint(p2, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        if (_webStifKind == 1) {
            p3 = p2 + axisZ * _plateWebStifHeight;
            cp3 = new ContourPoint(p3, CreatChamferStifOutside());

            cps = [cp1, cp2, cp3, cp4];
        } else {
            cps = [cp1, cp2, cp4];
        }

        foreach (var distance in webStifDistList) {
            foreach (var cp in cps) {
                cp.Y -= distance.Value;
            }

            var webStif = CreatPlateWebStif<ContourPlate>();
            webStif.Contour.ContourPoints = new ArrayList(cps);
            webStif.Insert();

            secWebStifs.Add(webStif);

            foreach (var cp in cps) {
                cp.Z *= -1;
            }

            webStif = CreatPlateWebStif<ContourPlate>();
            webStif.Contour.ContourPoints = new ArrayList(cps);
            webStif.Insert();

            secWebStifs.Add(webStif);

            foreach (var cp in cps) {
                cp.Z *= -1;
            }
        }

        #endregion

        #region 端板开坡口

        ContourPlate cutpart;
        if (_plateEnd1Thickness - prfPrim.t1 <= _difference) goto SkipGroove;

        p1 = Intersection.LineToLine(primRightLine, new Line(endPlate1.EndPoint, axisX)).StartPoint;

        var p0 = endPlate1.EndPoint - axisX * (_plateEnd1Thickness - (prfPrim.t1 - _weld2SizeAbove));

        p2 = Intersection.LineToLine(new Line(p0, primRightLine.Direction),
                new Line(endPlate1.EndPoint + axisY * (_weld2RootOpeningAbove * 0.5), axisX))
            .StartPoint;
        p2 = Intersection.LineToLine(primRightLine,
                new Line(p2, Geometry3dOperation.GetDirectionByRadian(_weld2AngleAbove * 0.5 / 180.0 * Math.PI)))
            .StartPoint;

        p3 = Intersection.LineToLine(new Line(p0, primRightLine.Direction),
                new Line(endPlate1.EndPoint - axisY * (_weld2RootOpeningAbove * 0.5), axisX))
            .StartPoint;
        p3 = Intersection.LineToLine(primRightLine,
                new Line(p3, Geometry3dOperation.GetDirectionByRadian(_weld2AngleAbove * -0.5 / 180.0 * Math.PI)))
            .StartPoint;

        var grooveLine = new Line(p3, Geometry3dOperation.GetDirectionByRadian(Math.Atan(_grooveSlope)));
        p3 = Intersection.LineToLine(grooveLine,
                new Line(p2, Geometry3dOperation.GetDirectionByRadian(_weld2AngleAbove * 0.5 / 180.0 * Math.PI)))
            .StartPoint;
        p4 = Intersection.LineToLine(grooveLine, endPlateLine).StartPoint;
        p5 = new Point(endPlate1.EndPoint);

        cutpart = ModelOperation.CreatBooleanOperationPolygon([p1, p2, p3, p4, p5], _plateEnd1Breadth);
        ModelOperation.ApplyBooleanOperation(endPlate1, cutpart);
        cutpart.Delete();

    SkipGroove:;

        #endregion

        #region 主零件对齐修剪

        fit = new Fitting {
            Father = primPart,
            Plane = new Plane {
                Origin = topPlateLine.Origin + topPlateLine.Direction.GetNormal().Cross(axisZ) * _plateTopThickness,
                AxisX = topPlateLine.Direction.GetNormal(10),
                AxisY = axisZ,
            }
        };
        fit.Insert();

        if (_plateThickenedThickness > prfPrim.s) {
            ModelOperation.ApplyBooleanOperation(primPart, thkedPlate);
        }

        p1 = Intersection.LineToLine(
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT),
                endPlateLine.Offset(_plateEnd1Thickness, LineExtension.OffsetDirectionEnum.RIGHT))
            .StartPoint;
        p2 = endPlate1.EndPoint - axisX * _plateEnd1Thickness;
        p3 = Intersection.LineToLine(new Line(p2, axisX), primRightLine).StartPoint;
        p4 = Intersection.LineToLine(
                topPlateLine.Offset(_plateTopThickness, LineExtension.OffsetDirectionEnum.RIGHT),
                primRightLine)
            .StartPoint;

        cutpart = ModelOperation.CreatBooleanOperationPolygon([p1, p2, p3, p4], prfPrim.b1);

        ModelOperation.ApplyBooleanOperation(primPart, cutpart);
        cutpart.Delete();

        #endregion

        #region 次零件对齐

        fit = new Fitting {
            Father = secPart,
            Plane = new Plane {
                Origin = endPlate1.StartPoint + axisX * _plateEnd2Thickness,
                AxisX = axisY,
                AxisY = axisZ
            }
        };
        fit.Insert();

        #endregion

        #region 焊接

        var weld = CreatWeld1<Weld>();
        weld.MainObject = topPlate;
        weld.SecondaryObject = endPlate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        if (_plateThickenedThickness <= prfPrim.s) {
            weld.MainObject = primPart;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();
        } else {
            if (thkedPlate.Contour.ContourPoints.Count == 4) {
                weld.MainObject = primPart;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();
            }

            weld.MainObject = thkedPlate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Insert();

            weld.MainObject = primPart;
            weld.SecondaryObject = thkedPlate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Insert();
        }

        weld.MainObject = secPart;
        weld.SecondaryObject = endPlate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        if (_weld1Around == 0) {
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
            weld.Insert();
        }

        weld = CreatWeld2<Weld>();
        weld.MainObject = primPart;
        weld.SecondaryObject = endPlate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        if (_plateThickenedThickness > prfPrim.s) {
            weld = CreatWeld3<Weld>();
            weld.MainObject = primPart;
            weld.SecondaryObject = thkedPlate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
            weld.Insert();
        }

        weld = CreatWeld4<Weld>();
        if (_plateThickenedThickness > prfPrim.s) {
            weld.MainObject = thkedPlate;
        } else {
            weld.MainObject = primPart;
        }

        weld.SecondaryObject = topPlate;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        weld.MainObject = primPart;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        if (_plateDiagonalThickness > 0) {
            weld = CreatWeld5<Weld>();

            if (_plateThickenedThickness > prfPrim.s) {
                weld.MainObject = thkedPlate;
            } else {
                weld.MainObject = primPart;
            }

            weld.SecondaryObject = diagPlateFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld.Insert();

            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();

            weld.MainObject = topPlate;
            weld.SecondaryObject = diagPlateFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();
            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();

            weld.MainObject = horPlateFront;
            weld.SecondaryObject = diagPlateFront;
            weld.Insert();

            weld.MainObject = horPlateBehind;
            weld.SecondaryObject = diagPlateBehind;
            weld.Insert();
        }

        weld = CreatWeld6<Weld>();
        if (_plateThickenedThickness > prfPrim.s) {
            weld.MainObject = thkedPlate;
        } else {
            weld.MainObject = primPart;
        }

        weld.SecondaryObject = horPlateFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();
        weld.SecondaryObject = horPlateBehind;
        weld.Insert();

        weld.MainObject = primPart;
        weld.SecondaryObject = horPlateFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();
        weld.SecondaryObject = horPlateBehind;
        weld.Insert();

        weld.MainObject = endPlate1;
        weld.SecondaryObject = horPlateFront;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();
        weld.SecondaryObject = horPlateBehind;
        weld.Insert();

        if (_plateEaveThickness > 0 && _plateEaveBreadth > 0 && _plateEaveHeight > 0) {
            weld = CreatWeld7<Weld>();
            weld.MainObject = primPart;
            weld.SecondaryObject = eavePlate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Insert();

            weld.MainObject = topPlate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
            weld.Insert();
        }

        weld = CreatWeld8<Weld>();
        weld.MainObject = topPlate;
        weld.SecondaryObject = primTopFlangeStif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.MainObject = endPlate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        weld.MainObject = secPart;
        weld.SecondaryObject = secTopFlangeStif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.MainObject = endPlate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.MainObject = secPart;
        weld.SecondaryObject = secBotFlangeStif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.MainObject = endPlate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld = CreatWeld9<Weld>();
        for (int i = 0; i < primWebStifs.Count; i++) {
            if (_plateThickenedThickness > prfPrim.s) {
                weld.MainObject = thkedPlate;
            } else {
                weld.MainObject = primPart;
            }

            weld.SecondaryObject = primWebStifs[i];
            weld.Position = i % 2 == 0
                ? Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z
                : Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();

            weld.MainObject = endPlate1;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
            weld.Insert();
        }

        for (int i = 0; i < secWebStifs.Count; i++) {
            weld.MainObject = secPart;
            weld.SecondaryObject = secWebStifs[i];
            weld.Position = i % 2 == 0
                ? Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z
                : Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();

            weld.MainObject = endPlate2;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();
        }

        #endregion

        #region 创建螺栓

        var bolt = CreatBolt1<BoltArray>();
        bolt.FirstPosition = endPlate1.StartPoint;
        bolt.SecondPosition = endPlate1.EndPoint;
        bolt.StartPointOffset.Dx = _boltOffset;
        bolt.PartToBoltTo = endPlate1;
        bolt.PartToBeBolted = endPlate2;
        bolt.Position.Rotation = Position.RotationEnum.TOP;
        bolt.Insert();

        #endregion

        #region FixPrim

        FixPrim(prfPrim, endPlate1.EndPoint.Y);

        #endregion

        return true;
    }

    #endregion

}