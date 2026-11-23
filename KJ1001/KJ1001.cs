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
 *  KJ1001.cs: connection between H or box column and H-beam
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
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
using TsData = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1001;

[Plugin("KJ1001")]
[PluginUserInterface("Muggle.TsExtensions.KJ1001.View")]
[SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
[PositionType(PositionTypeEnum.MIDDLE_PLANE)]
[FieldsFromData]
public partial class KJ1001 : ConnectionBase {

    private readonly record struct Profile(char Type, double H, double B, double S, double T) {
        public readonly char Type = Type;
        public readonly double H = H;
        public readonly double B = B;
        public readonly double S = S;
        public readonly double T = T;
    }

    private Localization Localization { get; }

    private Model Model { get; }

    private PluginData Data { get; }

    public KJ1001(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(KJ1001)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            var prim = (Part)Model.SelectModelObject(Primary);

            VerifyProfile(out var prfPrim, out var prfSec);

            if (string.IsNullOrEmpty(Data.PlateThickenedMaterial))
                Data.PlateThickenedMaterial = prim.Material.MaterialString;
            if (Data.PlatePrimStifThickness <= 0) Data.PlatePrimStifThickness = prfSec.t1;

            Data.SetFieldsToDefaultIfUnset();

            if (Data.ChamferTopCoverX >= Data.PlateTopCoverBreadth)
                throw new ArgumentOutOfRangeException(nameof(Data.ChamferTopCoverX));
            if (Data.ChamferTopCoverY >= Data.PlateTopCoverHeight * 0.5)
                throw new ArgumentOutOfRangeException(nameof(Data.ChamferTopCoverY));
            if (Data.ChamferBotCoverX >= Data.PlateBotCoverBreadth)
                throw new ArgumentOutOfRangeException(nameof(Data.ChamferBotCoverX));
            if (Data.ChamferBotCoverY >= Data.PlateBotCoverHeight * 0.5)
                throw new ArgumentOutOfRangeException(nameof(Data.ChamferBotCoverY));

            GetFieldValuesFrom(Data);

            Model.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(GetWorkTransformationPlane(prfPrim, out var secWorkTp));

            CreateConnection(prfPrim, prfSec, secWorkTp);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }

    private void VerifyProfile(out Profile prfPrim, out ProfileH_Symmetrical prfSec) {
        var prim = (Part)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        prfPrim = new Profile();
        try {
            var prf = new ProfileRect_Invariant(prim.Profile.ProfileString);
            prfPrim = new Profile('R', prf.h1, prf.b1, prf.s, prf.t);
        } catch {
            // ignore
        }

        try {
            var prf = new ProfileH_Symmetrical(prim.Profile.ProfileString);
            if (Math.Abs(prf.h2 - prf.h1) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
            prfPrim = new Profile('H', prf.h1, prf.b1, prf.s, prf.t1);
        } catch {
            // ignore
        }

        if (prfPrim.Type == '\0') throw new Exception(Localization.GetText("exception_primary_profile_not_supported"));

        try {
            prfSec = new ProfileH_Symmetrical(sec.Profile.ProfileString);
            if (Math.Abs(prfSec.h1 - prfSec.h2) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_secondary_profile_not_supported"));
        }
    }

    /// <summary>
    /// 获取主零件工作平面，
    /// 原点为次零件中心线与主零件中心线交点，
    /// X轴为主零件坐标系 +y, -y, +z, -z 中与次零件最接近的轴，
    /// Y轴为主零件坐标系 +x, -x 中与全局坐标系 +z 最接近的轴
    /// </summary>
    /// <param name="prfPrim">主零件截面参数</param>
    /// <param name="secWorkTp">输出次零件工作平面，
    /// 原点为次零件中心线与主零件边界框交点，
    /// X轴为次零件坐标系 +x, -x 中从节点指向远处的轴，
    /// Y轴为次零件坐标系 +y, -y 中与全局坐标系 +z 最接近的轴</param>
    /// <returns>主零件工作平面。</returns>
    private TransformationPlane GetWorkTransformationPlane(Profile prfPrim, out TransformationPlane secWorkTp) {
        var prim = (Beam)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        var origin = new Point(0, 0, 0);
        var globalZ = new Vector(0, 0, 1).TransformFrom(new TransformationPlane());
        const double halfPi = Math.PI * 0.5;

        var secCenterLine = sec.GetCenterLine(false);
        var secFirstPoint = secCenterLine[0] as Point;
        var secLastPoint = secCenterLine[secCenterLine.Count - 1] as Point;

        var reverse = Distance.PointToPoint(secFirstPoint, origin) > Distance.PointToPoint(secLastPoint, origin);

        var index = reverse ? secCenterLine.Count - 1 : 0;
        var step = reverse ? -1 : 1;

        var secLine = new Line(secCenterLine[index] as Point, secCenterLine[index + step] as Point);
        var secDirection = secLine.Direction.GetNormal();

        var primCs = prim.GetCoordinateSystem();
        var primCsAxisX = primCs.AxisX.GetNormal();
        var primCsAxisY = primCs.AxisY.GetNormal();
        var primCsAxisZ = primCsAxisX.Cross(primCsAxisY).GetNormal();

        var primCsYzPlane = new GeometricPlane(primCs.Origin, primCsAxisY, primCsAxisZ);
        var secDirectionProjection = ProjectionExtension.VectorToPlane(secDirection, primCsYzPlane).GetNormal();

        var angleY = Math.Acos(primCsAxisY.Dot(secDirectionProjection));
        var angleZ = Math.Acos(primCsAxisZ.Dot(secDirectionProjection));
        if (angleY > halfPi) angleY = Math.PI - angleY;
        if (angleZ > halfPi) angleZ = Math.PI - angleZ;

        var primTpAxisX = angleY < angleZ ? primCsAxisY : primCsAxisZ;
        primTpAxisX *= primTpAxisX.Dot(secDirection) < 0 ? -1 : 1;
        var primTpAxisY = primCsAxisX;
        primTpAxisY *= primTpAxisY.Dot(globalZ) < 0 ? -1 : 1;

        var primTpXyPlane = new GeometricPlane(primCs.Origin, primTpAxisX.Cross(primTpAxisY));
        var secLineProjection = Projection.LineToPlane(secLine, primTpXyPlane);
        var primTpOrigin = IntersectionExtension.LineToLine(new Line(primCs.Origin, primTpAxisY), secLineProjection)
            .StartPoint;
        var primWorkTp = new TransformationPlane(primTpOrigin, primTpAxisX, primTpAxisY);

        var outlineYzPlane = new GeometricPlane(
            primTpOrigin + primTpAxisX * (angleY < angleZ
                ? prfPrim.H * 0.5
                : prfPrim.Type is 'R'
                    ? prfPrim.B * 0.5
                    : prfPrim.S * 0.5),
            primTpAxisX);

        CoordinateSystem secCs;
        switch (sec) {
        case PolyBeam polyBeam:
            var polyBeamCss = polyBeam.GetPolybeamCoordinateSystemsEx();
            secCs = polyBeamCss[reverse ? polyBeamCss.Count - 1 : 0];
            break;
        default:
            secCs = sec.GetCoordinateSystem();
            break;
        }

        var secOrigin = Intersection.LineToPlane(secLine, outlineYzPlane);
        var secAxisX = secCs.AxisX.GetNormal();
        secAxisX *= reverse ? -1 : 1;
        var secAxisY = secCs.AxisY.GetNormal();
        secAxisY *= secAxisY.Dot(globalZ) < 0 ? -1 : 1;
        secWorkTp = new TransformationPlane(secOrigin, secAxisX, secAxisY);

        return primWorkTp;
    }

    private void CreateConnection(Profile prfPrim, ProfileH_Symmetrical prfSec,
        TransformationPlane secWorkTp) {

        #region 共用变量、方法

        var prim = (Part)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        var origin = new Point(0, 0, 0);
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        var primWorkTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

        var secOrigin = origin.Transform(secWorkTp, primWorkTp);
        var secAxisX = axisX.Transform(secWorkTp, primWorkTp).GetNormal();
        var secAxisY = axisY.Transform(secWorkTp, primWorkTp).GetNormal();
        var secAxisZ = axisZ.Transform(secWorkTp, primWorkTp).GetNormal();

        var primCs = prim.GetCoordinateSystem();
        var isFlangeSide = primCs.AxisY.GetNormal().Dot(axisX) != 0;
        var primRightOutlinePlane = new GeometricPlane(secOrigin, axisX);

        var secCenterLine = new Line(origin.Transform(secWorkTp, primWorkTp),
            axisX.Transform(secWorkTp, primWorkTp));
        var secTopMidLine = secCenterLine.Offset(secAxisY * (prfSec.h1 * 0.5));
        var secTopFrontLine = secTopMidLine.Offset(secAxisZ * (prfSec.b1 * 0.5));
        var secTopBehindLine = secTopMidLine.Offset(secAxisZ * (prfSec.b1 * -0.5));
        var secBottomMidLine = secCenterLine.Offset(secAxisY * (prfSec.h1 * -0.5));
        var secBottomFrontLine = secBottomMidLine.Offset(secAxisZ * (prfSec.b1 * 0.5));
        var secBottomBehindLine = secBottomMidLine.Offset(secAxisZ * (prfSec.b1 * -0.5));
        var secTFxPrim = Intersection.LineToPlane(secTopFrontLine, primRightOutlinePlane);
        var secTBxPrim = Intersection.LineToPlane(secTopBehindLine, primRightOutlinePlane);
        var secBFxPrim = Intersection.LineToPlane(secBottomFrontLine, primRightOutlinePlane);
        var secBBxPrim = Intersection.LineToPlane(secBottomBehindLine, primRightOutlinePlane);
        var secTxPrim = (secTFxPrim + secTBxPrim).Multiply(0.5);
        var secBxPrim = (secBFxPrim + secBBxPrim).Multiply(0.5);

        var v = secAxisY * prfSec.t1;
        var secTFIxPrim = Intersection.LineToPlane(secTopFrontLine.Offset(v * -1), primRightOutlinePlane);
        var secTBIxPrim = Intersection.LineToPlane(secTopBehindLine.Offset(v * -1), primRightOutlinePlane);
        var secBFIxPrim = Intersection.LineToPlane(secBottomFrontLine.Offset(v), primRightOutlinePlane);
        var secBBIxPrim = Intersection.LineToPlane(secBottomBehindLine.Offset(v), primRightOutlinePlane);
        var secTIxPrim = (secTFIxPrim + secTBIxPrim).Multiply(0.5);
        var secBIxPrim = (secBFIxPrim + secBBIxPrim).Multiply(0.5);

        var isSloping = axisY.GetAngleBetweenPrecisely(secAxisY) > GeometryConstants.ANGULAR_EPSILON;
        var slopingAngle = axisY.GetAngleBetweenPrecisely(secAxisY);
        var isSkew = axisZ.GetAngleBetweenPrecisely(secAxisZ) > GeometryConstants.ANGULAR_EPSILON;
        var skewAngle = axisZ.GetAngleBetweenPrecisely(secAxisZ);
        var shearYMatrix = MatrixFactoryExtension.Shear(0, Math.Sin(slopingAngle), Math.Sin(skewAngle));
        var transMatrix = MatrixFactoryExtension.Translate(new Vector(secOrigin) * -1);
        var matrix = transMatrix * shearYMatrix;

        //  正常做法是矩阵连乘进行组合
        //  由于Tekla内部实现的问题，切变矩阵不能与其他矩阵组合，否则变换后的点会有偏差
        static void TransformPoint(Matrix[] matrices, params Point[] points) {
            foreach (var m in matrices) {
                foreach (var p in points) {
                    var point = m.Transform(p);
                    p.X = point.X;
                    p.Y = point.Y;
                    p.Z = point.Z;
                }
            }
        }

        var webBoltDisListX =
            TsData.DistanceList.Parse(_boltWebDistX, CultureInfo.CurrentCulture, TsData.Distance.CurrentUnitType);
        var webBoltDisListY =
            TsData.DistanceList.Parse(_boltWebDistY, CultureInfo.CurrentCulture, TsData.Distance.CurrentUnitType);
        var webBoltTotalDisX = webBoltDisListX.Sum(d => d.Value);
        var webBoltTotalDisY = webBoltDisListY.Sum(d => d.Value);

        var chamferNone = new Chamfer();

        #endregion

        #region 柱加厚补强板

        Beam thkdPlate1 = null, thkdPlate2 = null;

        if (prfPrim.Type != 'R' || _plateThickenedThickness <= (isFlangeSide ? prfPrim.S : prfPrim.T))
            goto SkipThickened;

        Data.PlateThickenedBreadth = isFlangeSide ? prfPrim.H : prfPrim.B;
        _plateThickenedBreadth = Data.PlateThickenedBreadth;

        var p1 = new Point(secTxPrim);
        var p2 = new Point(secBxPrim);
        p1.X -= isFlangeSide ? prfPrim.H * 0.5 : prfPrim.B * 0.5;
        p2.X = p1.X;
        p1.Y += _extraLen;
        p2.Y -= _extraLen;
        p1.Z = isFlangeSide ? prfPrim.B * 0.5 : prfPrim.H * 0.5;
        p2.Z = p1.Z;
        thkdPlate1 = Data.CreatePlateThickened<Beam>();
        thkdPlate1.StartPoint = p1;
        thkdPlate1.EndPoint = p2;
        thkdPlate1.Position.Rotation = Position.RotationEnum.FRONT;
        thkdPlate1.Position.Plane = Position.PlaneEnum.MIDDLE;
        thkdPlate1.Position.Depth = Position.DepthEnum.BEHIND;
        thkdPlate1.Insert();

        p1.Z *= -1;
        p2.Z *= -1;
        thkdPlate2 = Data.CreatePlateThickened<Beam>();
        thkdPlate2.StartPoint = p1;
        thkdPlate2.EndPoint = p2;
        thkdPlate2.Position.Rotation = Position.RotationEnum.BACK;
        thkdPlate2.Position.Plane = Position.PlaneEnum.MIDDLE;
        thkdPlate2.Position.Depth = Position.DepthEnum.FRONT;
        thkdPlate2.Insert();

        _ = ModelOperation.ApplyBooleanOperation(prim, thkdPlate1);
        _ = ModelOperation.ApplyBooleanOperation(prim, thkdPlate2);

        var weld = Data.CreateWeld4<Weld>();
        weld.MainObject = prim;
        weld.SecondaryObject = thkdPlate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.SecondaryObject = thkdPlate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld = Data.CreateWeld5<Weld>();
        weld.MainObject = prim;
        weld.SecondaryObject = thkdPlate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        weld.SecondaryObject = thkdPlate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();

        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        SkipThickened: ;

        #endregion

        #region 柱内部加强板

        if (_platePrimStifThickness <= 0.0) goto SkipPrimStif;

        CreateAndWeldPrimStif(secTxPrim.Y, isFlangeSide, Position.DepthEnum.BEHIND, prfPrim, prim, thkdPlate1,
            thkdPlate2);
        CreateAndWeldPrimStif(secBxPrim.Y, isFlangeSide, Position.DepthEnum.FRONT, prfPrim, prim, thkdPlate1,
            thkdPlate2);

        if (!string.IsNullOrEmpty(_moreStifDisList)) {
            var y = secTxPrim.Y;
            var stifDisList = TsData.DistanceList.Parse(_moreStifDisList, CultureInfo.CurrentCulture,
                TsData.Distance.CurrentUnitType);

            foreach (var dist in stifDisList) {
                y -= dist.Value;
                CreateAndWeldPrimStif(y, isFlangeSide, Position.DepthEnum.MIDDLE, prfPrim, prim, thkdPlate1,
                    thkdPlate2);
            }
        }

        SkipPrimStif: ;

        #endregion

        #region 梁对齐切割

        var fittingPlane = new Plane {
            Origin = secOrigin + axisX * (_weld2TypeAbove == 0 && _weld2TypeBelow == 0 ? _gap : 0),
            AxisX = axisY,
            AxisY = axisZ
        };
        var fitting = new Fitting { Father = sec, Plane = fittingPlane };
        fitting.Insert();

        if (_weld2TypeAbove == 0 && _weld2TypeBelow == 0 || _chamferBeamType == 0) goto SkipCut1;

        // 此做法仅在创建节点时操作正确
        // 对节点实例进行 Modify 操作后，sec 被多切割了一部分
        /*p1 = new Point(secTIxPrim);
        p2 = p1 + secAxisX * (Math.Max(_chamferBeamX, _chamferBeamY) + 100);
        var p4 = new Point(secBIxPrim);
        var p3 = p4 + secAxisX * (Math.Max(_chamferBeamX, _chamferBeamY) + 100);

        var chamfer = CreateChamferBeam();
        var chamferReversed = chamfer.GetReversed();

        var cp1 = new ContourPoint(p1, chamfer);
        var cp2 = new ContourPoint(p2, chamferNone);
        var cp3 = new ContourPoint(p3, chamferNone);
        var cp4 = new ContourPoint(p4, chamferReversed);

        var boolean1 = ModelOperation.CreateBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3, cp4 }, prfSec.s + 10);

        p1 -= secAxisX * 100;
        p2 -= secAxisX * 50;
        p3 -= secAxisX * 50;
        p4 -= secAxisX * 100;

        cp1 = new ContourPoint(p1, chamferNone);
        cp2 = new ContourPoint(p2, chamferNone);
        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);

        var boolean2 = ModelOperation.CreateBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3, cp4 }, prfSec.s + 10);

        ModelOperation.ApplyBooleanOperation(boolean2, boolean1);
        ModelOperation.ApplyBooleanOperation(sec, boolean2);

        boolean2.Delete();
        boolean1.Delete();*/

        var gapPlane = new GeometricPlane { Origin = secOrigin + axisX * _gap, Normal = axisX };

        p1 = secBIxPrim - secAxisX * 100;
        p2 = Intersection.LineToPlane(new Line(secBIxPrim, secAxisX), gapPlane);
        var p3 = p2 + secAxisX * 1000;
        var p5 = Intersection.LineToPlane(new Line(secTIxPrim, secAxisX), gapPlane);
        var p4 = p5 + secAxisX * 1000;
        var p6 = secTIxPrim - secAxisX * 100;

        var chamfer = Data.CreateChamferBeam();
        var chamferReversed = chamfer.GetReversed();
        var cp1 = new ContourPoint(p1, chamferNone);
        var cp2 = new ContourPoint(p2, chamfer);
        var cp5 = new ContourPoint(p5, chamferReversed);
        var cp6 = new ContourPoint(p6, chamferNone);

        var cps = new ArrayList();
        cps.Add(cp1);
        var simulated = cp2.GetSimulatedContourPoints(p3, p5);
        cps.AddRange(simulated);
        simulated = cp5.GetSimulatedContourPoints(p2, p4);
        cps.AddRange(simulated);
        cps.Add(cp6);

        var boolean = ModelOperation.CreateBooleanOperationPolygon(cps, prfSec.s + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(sec, boolean);
        boolean.Delete();

        SkipCut1: ;

        if (_weld2TypeAbove == 0 && _weld2TypeBelow == 0) goto SkipWeld;

        var polyWeld = Data.CreateWeld2<PolygonWeld>();
        polyWeld.MainObject = prim;
        polyWeld.SecondaryObject = sec;

        var polygon = new Polygon { Points = new ArrayList { secTFxPrim, secTBxPrim } };
        polyWeld.Polygon = polygon;
        polyWeld.Insert();

        polygon = new Polygon { Points = new ArrayList { secBFIxPrim, secBBIxPrim } };
        polyWeld.Polygon = polygon;
        polyWeld.Insert();

        SkipWeld: ;

        #endregion

        #region 腹板连接板及栓接

        Part webSpliceFront = null, webSpliceBehind = null;

        p1 = new Point(isSkew ? -50 : 0, _plateWebSpliceHeight * -0.5, 0);
        p2 = new Point(_plateWebSpliceBreadth, _plateWebSpliceHeight * -0.5, 0);
        p3 = p2 + axisY * _plateWebSpliceHeight;
        p4 = p1 + axisY * _plateWebSpliceHeight;
        TransformPoint([matrix], [p1, p2, p3, p4]);

        var contours = new ArrayList {
            new ContourPoint(p1, chamferNone),
            new ContourPoint(p2, chamferNone),
            new ContourPoint(p3, chamferNone),
            new ContourPoint(p4, chamferNone)
        };

        // 当 isSkew 时，切割连接板
        // 当 isSloping 时，切割盖板
        boolean = isSkew || isSloping
            ? ModelOperation.CreateBooleanOperationPolygon(
                [
                    new ContourPoint(new Point(secBxPrim.X - 100, secBxPrim.Y - 100, 0), chamferNone),
                    new ContourPoint(new Point(secBxPrim.X, secBxPrim.Y - 100, 0), chamferNone),
                    new ContourPoint(new Point(secTxPrim.X, secTxPrim.Y + 100, 0), chamferNone),
                    new ContourPoint(new Point(secTxPrim.X - 100, secTxPrim.Y + 100, 0), chamferNone),
                ],
                prfPrim.B + 10)
            : null;
        boolean.Insert();

        if (_spliceCreation is 0 or 2) {
            webSpliceFront = Data.CreatePlateWebSplice<ContourPlate>();
            ((ContourPlate)webSpliceFront).Contour.ContourPoints = contours;
            webSpliceFront.Position.Depth = Position.DepthEnum.FRONT;
            webSpliceFront.Position.DepthOffset = prfSec.s * 0.5;
            webSpliceFront.Insert();

            if (isSkew) {
                _ = ModelOperation.ApplyBooleanOperation(webSpliceFront, boolean);
            }

            weld = Data.CreateWeld6<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = webSpliceFront;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();
        }

        if (_spliceCreation is 1 or 2) {
            webSpliceBehind = Data.CreatePlateWebSplice<ContourPlate>();
            ((ContourPlate)webSpliceBehind).Contour.ContourPoints = contours;
            webSpliceBehind.Position.Depth = Position.DepthEnum.BEHIND;
            webSpliceBehind.Position.DepthOffset = prfSec.s * 0.5;
            webSpliceBehind.Insert();

            if (isSkew) {
                _ = ModelOperation.ApplyBooleanOperation(webSpliceBehind, boolean);
            }

            weld = Data.CreateWeld6<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = webSpliceBehind;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();
        }

        // 切割完盖板再删除
        // boolean?.Delete();

        var bolt = Data.CreateBoltWeb<BoltArray>();
        bolt.Position.Rotation = Position.RotationEnum.FRONT;
        bolt.StartPointOffset.Dx = (_plateWebSpliceHeight - webBoltTotalDisX) * 0.5;
        bolt.PartToBoltTo = sec;
        switch (_spliceCreation) {
        case 0:
            bolt.PartToBeBolted = webSpliceFront;
            break;
        case 1:
            bolt.PartToBeBolted = webSpliceBehind;
            break;
        case 2:
            bolt.PartToBeBolted = webSpliceFront;
            bolt.AddOtherPartToBolt(webSpliceBehind);
            break;
        }

        if (isSloping && webBoltDisListY.Count > 1) {
            for (int i = webBoltDisListY.Count - 1; i >= 0; i--) {
                bolt.RemoveBoltDistY(i);
            }

            bolt.AddBoltDistY(0.0);

            p1 = new Point(_gap + (_plateWebSpliceBreadth - _gap) * 0.5 - webBoltTotalDisY * 0.5,
                _plateWebSpliceHeight * 0.5);
            p2 = p1 - axisY * _plateWebSpliceHeight;
            bolt.FirstPosition = matrix.Transform(p1);
            bolt.SecondPosition = matrix.Transform(p2);
            bolt.Insert();

            if (isSkew) FixRotationOffset();

            foreach (var dis in webBoltDisListY) {
                p1.X += dis.Value;
                p2.X += dis.Value;

                bolt.FirstPosition = matrix.Transform(p1);
                bolt.SecondPosition = matrix.Transform(p2);
                bolt.Insert();

                if (isSkew) FixRotationOffset();
            }
        } else {
            p1 = new Point(_gap + (_plateWebSpliceBreadth - _gap) * 0.5, _plateWebSpliceHeight * 0.5);
            p2 = p1 - axisY * _plateWebSpliceHeight;

            bolt.FirstPosition = matrix.Transform(p1);
            bolt.SecondPosition = matrix.Transform(p2);
            bolt.Insert();

            if (isSkew) FixRotationOffset();
        }

        void FixRotationOffset() {
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(secWorkTp);
            bolt = (BoltArray)Model.SelectModelObject(bolt.Identifier);
            bolt.Position.RotationOffset = 0.0;
            bolt.Modify();
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(primWorkTp);
        }

        #endregion

        #region 上盖板

        var lineFront = new Line(secTxPrim + secAxisZ * (_plateTopCoverBreadth * 0.5), secAxisX);
        var lineBehind = new Line(secTxPrim - secAxisZ * (_plateTopCoverBreadth * 0.5), secAxisX);

        p1 = Intersection.LineToPlane(lineFront, primRightOutlinePlane);
        p6 = Intersection.LineToPlane(lineBehind, primRightOutlinePlane);
        if (isSloping) {
            p1 -= secAxisX * 50;
            p6 -= secAxisX * 50;
        }

        var p0 = secTxPrim + secAxisX * (_plateTopCoverHeight - _chamferTopCoverX);
        var secAxisXRealAngle = 0.0;
        if (isSkew) {
            v = new Vector(Math.Cos(slopingAngle), Math.Sin(slopingAngle), 0);
            secAxisXRealAngle = secAxisX.GetAngleBetweenPrecisely(v);
            p0 += secAxisX * (_plateTopCoverBreadth * 0.5 * Math.Tan(secAxisXRealAngle));
        }

        p2 = Projection.PointToLine(p0, lineFront);
        p3 = p2 + secAxisX * _chamferTopCoverX - secAxisZ * _chamferTopCoverY;
        p5 = Projection.PointToLine(p0, lineBehind);
        p4 = p5 + secAxisX * _chamferTopCoverX + secAxisZ * _chamferTopCoverY;

        cp1 = new ContourPoint(p1, chamferNone);
        cp2 = new ContourPoint(p2, chamferNone);
        var cp3 = new ContourPoint(p3, chamferNone);
        var cp4 = new ContourPoint(p4, chamferNone);
        cp5 = new ContourPoint(p5, chamferNone);
        cp6 = new ContourPoint(p6, chamferNone);

        cps = new ArrayList(new[] { cp1, cp2, cp3, cp4, cp5, cp6 });

        var topCover = Data.CreatePlateTopCover<ContourPlate>();
        topCover.Contour.ContourPoints = cps;
        topCover.Position.Depth = Position.DepthEnum.FRONT;
        topCover.Insert();
        // 修复轮廓点顺序错乱
        topCover.Contour.ContourPoints = cps;
        topCover.Modify();

        if (isSloping) {
            _ = ModelOperation.ApplyBooleanOperation(topCover, boolean);
            // 创建完下盖板再删除 boolean
        }

        weld = Data.CreateWeld8<Weld>();
        weld.MainObject = prim;
        weld.SecondaryObject = topCover;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        if (_plateTopCoverBreadth >= prfSec.b1) {
            if (_plateTopCoverBreadth - _chamferTopCoverY * 2 >= prfSec.b1) {
                polygon = new Polygon {
                    Points = new ArrayList {
                        secTFxPrim,
                        p0 + secAxisX * _chamferTopCoverX + secAxisZ * (prfSec.b1 * 0.5),
                        p0 + secAxisX * _chamferTopCoverX - secAxisZ * (prfSec.b1 * 0.5),
                        secTBxPrim
                    }
                };
            } else {
                p1 = secTFxPrim;
                p2 = IntersectionExtension.LineToLine(secTopFrontLine, new Line(cp2, cp3)).StartPoint;
                p3 = cp3;
                p4 = cp4;
                p5 = IntersectionExtension.LineToLine(secTopBehindLine, new Line(cp5, cp4)).StartPoint;
                p6 = secTBxPrim;
                polygon = new Polygon { Points = new ArrayList(new[] { p1, p2, p3, p4, p5, p6 }) };
            }
        } else {
            polygon = new Polygon {
                Points = new ArrayList {
                    Intersection.LineToPlane(lineFront, primRightOutlinePlane),
                    cp2,
                    cp3,
                    cp4,
                    cp5,
                    Intersection.LineToPlane(lineBehind, primRightOutlinePlane)
                }
            };
        }

        polyWeld = Data.CreateWeld7<PolygonWeld>();
        polyWeld.MainObject = sec;
        polyWeld.SecondaryObject = topCover;
        polyWeld.Polygon = polygon;
        polyWeld.Insert();

        #endregion

        #region 下盖板

        lineFront = new Line(secBxPrim + secAxisZ * (_plateBotCoverBreadth * 0.5), secAxisX);
        lineBehind = new Line(secBxPrim - secAxisZ * (_plateBotCoverBreadth * 0.5), secAxisX);

        p1 = Intersection.LineToPlane(lineFront, primRightOutlinePlane);
        p6 = Intersection.LineToPlane(lineBehind, primRightOutlinePlane);
        if (isSloping) {
            p1 -= secAxisX * 50;
            p6 -= secAxisX * 50;
        }

        p0 = secBxPrim + secAxisX * (_plateBotCoverHeight - _chamferBotCoverX);
        if (isSkew) {
            p0 += secAxisX * (_plateBotCoverBreadth * 0.5 * Math.Tan(secAxisXRealAngle));
        }

        p2 = Projection.PointToLine(p0, lineFront);
        p3 = p2 + secAxisX * _chamferBotCoverX - secAxisZ * _chamferBotCoverY;
        p5 = Projection.PointToLine(p0, lineBehind);
        p4 = p5 + secAxisX * _chamferBotCoverX + secAxisZ * _chamferBotCoverY;

        cp1 = new ContourPoint(p1, chamferNone);
        cp2 = new ContourPoint(p2, chamferNone);
        cp3 = new ContourPoint(p3, chamferNone);
        cp4 = new ContourPoint(p4, chamferNone);
        cp5 = new ContourPoint(p5, chamferNone);
        cp6 = new ContourPoint(p6, chamferNone);

        cps = new ArrayList(new[] { cp1, cp2, cp3, cp4, cp5, cp6 });

        var botCover = Data.CreatePlateBotCover<ContourPlate>();
        botCover.Contour.ContourPoints = cps;
        botCover.Position.Depth = Position.DepthEnum.BEHIND;
        botCover.Insert();
        // 修复轮廓点顺序错乱
        botCover.Contour.ContourPoints = cps;
        botCover.Modify();

        if (isSloping) {
            _ = ModelOperation.ApplyBooleanOperation(botCover, boolean);
        }

        boolean?.Delete();

        weld = Data.CreateWeld8<Weld>();
        weld.MainObject = prim;
        weld.SecondaryObject = botCover;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();

        if (_plateBotCoverBreadth >= prfSec.b1) {
            if (_plateBotCoverBreadth - _chamferBotCoverY * 2 >= prfSec.b1) {
                polygon = new Polygon {
                    Points = new ArrayList {
                        secBFxPrim,
                        p0 + secAxisX * _chamferBotCoverX + secAxisZ * (prfSec.b1 * 0.5),
                        p0 + secAxisX * _chamferBotCoverX - secAxisZ * (prfSec.b1 * 0.5),
                        secBBxPrim
                    }
                };
            } else {
                p1 = secBFxPrim;
                p2 = IntersectionExtension.LineToLine(secBottomFrontLine, new Line(cp2, cp3)).StartPoint;
                p3 = cp3;
                p4 = cp4;
                p5 = IntersectionExtension.LineToLine(secBottomBehindLine, new Line(cp5, cp4)).StartPoint;
                p6 = secBBxPrim;
                polygon = new Polygon { Points = new ArrayList(new[] { p1, p2, p3, p4, p5, p6 }) };
            }
        } else {
            polygon = new Polygon {
                Points = new ArrayList {
                    Intersection.LineToPlane(lineFront, primRightOutlinePlane),
                    cp2,
                    cp3,
                    cp4,
                    cp5,
                    Intersection.LineToPlane(lineBehind, primRightOutlinePlane)
                }
            };
        }

        polyWeld = Data.CreateWeld7<PolygonWeld>();
        polyWeld.MainObject = sec;
        polyWeld.SecondaryObject = botCover;
        polyWeld.Polygon = polygon;
        polyWeld.Insert();

        #endregion

    }

    private void CreateAndWeldPrimStif(double y, bool isFlangeSide, Position.DepthEnum depthEnum, Profile prfPrim,
        Part prim, Part thickenedPlate1, Part thickenedPlate2) {
        y = depthEnum switch {
            Position.DepthEnum.FRONT => y + _platePrimStifThickness * 0.5,
            Position.DepthEnum.BEHIND => y - _platePrimStifThickness * 0.5,
            _ => y,
        };

        var chamfer = Data.CreateChamferPrimStif();
        var chamferReversed = chamfer.GetReversed();

        ContourPlate stif;
        Weld weld;
        switch (prfPrim.Type) {
        case 'R':
            var x = isFlangeSide ? prfPrim.H * 0.5 - prfPrim.T : prfPrim.B * 0.5 - prfPrim.S;
            var z = isFlangeSide ? prfPrim.B * 0.5 - prfPrim.S : prfPrim.H * 0.5 - prfPrim.T;
            var fix = _plateThickenedThickness - (isFlangeSide ? prfPrim.S : prfPrim.T);
            if (fix > 0) z -= fix;

            var cps = new ArrayList {
                new ContourPoint(new Point(x, y, z), chamfer),
                new ContourPoint(new Point(-x, y, z), chamferReversed),
                new ContourPoint(new Point(-x, y, -z), chamfer),
                new ContourPoint(new Point(x, y, -z), chamferReversed),
            };
            stif = Data.CreatePlatePrimStif<ContourPlate>();
            stif.Contour.ContourPoints = cps;
            stif.Insert();

            weld = Data.CreateWeld3<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = stif;
            if (_weld3Around != 0 && thickenedPlate1 is null) {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();
            } else {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                if (thickenedPlate1 is null) {
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                    weld.Insert();

                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                    weld.Insert();
                } else {
                    weld.MainObject = thickenedPlate1;
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                    weld.Insert();

                    weld.MainObject = thickenedPlate2;
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                    weld.Insert();
                }
            }

            break;
        case 'H':
            var contours = ModelOperation.CalculateStiffeners(prim, new Point(0, y), _platePrimStifThickness,
                clearance: 0, chamfer: chamfer);

            if (isFlangeSide) {
                foreach (var arr in contours) {
                    stif = Data.CreatePlatePrimStif<ContourPlate>();
                    stif.Contour.ContourPoints = arr;
                    stif.Insert();

                    weld = Data.CreateWeld3<Weld>();
                    weld.MainObject = prim;
                    weld.SecondaryObject = stif;
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                    weld.Insert();
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                    weld.Insert();
                    weld.Position = ((Point)arr[0]).Z > 0
                        ? Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z
                        : Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                    weld.Insert();
                }
            } else {
                stif = Data.CreatePlatePrimStif<ContourPlate>();
                stif.Contour.ContourPoints = contours.First(arr => ((Point)arr[0]).X < 0);
                stif.Insert();

                weld = Data.CreateWeld3<Weld>();
                weld.MainObject = prim;
                weld.SecondaryObject = stif;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();
            }

            break;
        }
    }
}