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
 *  KJ1004.cs: connection between H or box column and H-beam
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
using static TeklaStructuresAttributes.Attributes;
using Distance = Tekla.Structures.Geometry3d.Distance;
using TsData = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1004;

[Plugin("KJ1004")]
[PluginUserInterface("Muggle.TsExtensions.KJ1004.View")]
[SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
[PositionType(PositionTypeEnum.MIDDLE_PLANE)]
[FieldsFrom(typeof(PluginData))]
public partial class KJ1004 : ConnectionBase {

    private readonly record struct Profile(char Type, double H, double B, double S, double T) {
        public readonly char Type = Type;
        public readonly double H = H;
        public readonly double B = B;
        public readonly double S = S;
        public readonly double T = T;
    }

    private Localization Localization { get; }

    private Model Model { get; }

    #region DefaultValues

    [PartFieldDefaultValues("ShortBeam")]
    [PlateFieldDefaultValues("PrimStif", thickness: 10, material: "Q235B")]
    [PlateFieldDefaultValues("Thickened")]
    [PlateFieldDefaultValues("WebSplice", thickness: 16, breadth: 350, height: 300, material: "Q235B")]
    [ChamferFieldDefaultValues("PrimStif", type: 1, x: 25, y: 25)]
    [ChamferFieldDefaultValues("Beam", type: 3, x: 25, y: 25)]
    [BoltFieldDefaultValues("Web", size: 20, standard: "HS10.9", distXText: "3*70", distYText: "70 100 70")]
    [WeldFieldDefaultValues(1, typeAbove: 10, sizeAbove: 8, around: 1)]
    [WeldFieldDefaultValues(2, typeAbove: 0, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
    [WeldFieldDefaultValues(3, typeAbove: 10, sizeAbove: 8)]
    [WeldFieldDefaultValues(4, typeAbove: 6, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
    [WeldFieldDefaultValues(5, typeAbove: 6, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2,
        placement: 1)]
    [WeldFieldDefaultValues(6, typeAbove: 5, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
    [GeneralFieldDefaultValues("ExtraLen", 150, "Gap", 15, "ShortBeamLen", 1200, "MoreStifDisList", "")]

    #endregion

    private PluginData Data { get; }

    public KJ1004(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(KJ1004)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            var prim = (Part)Model.SelectModelObject(Primary);
            var sec = (Part)Model.SelectModelObject(Secondaries[0]);

            VerifyProfile(out var prfPrim, out var prfSec);

            if (string.IsNullOrEmpty(Data.PlateThickenedMaterial))
                Data.PlateThickenedMaterial = prim.Material.MaterialString;
            Data.PartShortBeamProfile = sec.Profile.ProfileString;
            if (string.IsNullOrEmpty(Data.PartShortBeamMaterial))
                Data.PartShortBeamMaterial = sec.Material.MaterialString;
            if (Data.PlatePrimStifThickness <= 0) Data.PlatePrimStifThickness = prfSec.t1;

            SetDataToDefaultIfUnset();

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

        var isSkew = axisZ.GetAngleBetweenPrecisely(secAxisZ) > GeometryConstants.ANGULAR_EPSILON;

        var webBoltDisListX =
            TsData.DistanceList.Parse(_boltWebDistXText, CultureInfo.CurrentCulture, TsData.Distance.CurrentUnitType);
        var webBoltTotalDisX = webBoltDisListX.Sum(d => d.Value);

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
        thkdPlate1 = CreatePlateThickened<Beam>();
        thkdPlate1.StartPoint = p1;
        thkdPlate1.EndPoint = p2;
        thkdPlate1.Position.Rotation = Position.RotationEnum.FRONT;
        thkdPlate1.Position.Plane = Position.PlaneEnum.MIDDLE;
        thkdPlate1.Position.Depth = Position.DepthEnum.BEHIND;
        thkdPlate1.Insert();

        p1.Z *= -1;
        p2.Z *= -1;
        thkdPlate2 = CreatePlateThickened<Beam>();
        thkdPlate2.StartPoint = p1;
        thkdPlate2.EndPoint = p2;
        thkdPlate2.Position.Rotation = Position.RotationEnum.BACK;
        thkdPlate2.Position.Plane = Position.PlaneEnum.MIDDLE;
        thkdPlate2.Position.Depth = Position.DepthEnum.FRONT;
        thkdPlate2.Insert();

        _ = ModelOperation.ApplyBooleanOperation(prim, thkdPlate1);
        _ = ModelOperation.ApplyBooleanOperation(prim, thkdPlate2);

        var weld = CreateWeld4<Weld>();
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

        weld = CreateWeld5<Weld>();
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

        #region 短梁

        p1 = new Point(secOrigin);
        p2 = p1 + secAxisX * _shortBeamLen;

        var shortBeam = CreatePartShortBeam<Beam>();
        shortBeam.StartPoint = p1;
        shortBeam.EndPoint = p2;
        shortBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
        shortBeam.Position.Rotation = Position.RotationEnum.FRONT;
        shortBeam.Position.Depth = Position.DepthEnum.MIDDLE;
        shortBeam.Insert();

        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(secWorkTp);
        shortBeam = (Beam)Model.SelectModelObject(shortBeam.Identifier);
        shortBeam.Position.RotationOffset = 0.0;
        shortBeam.Modify();
        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(primWorkTp);
        shortBeam = (Beam)Model.SelectModelObject(shortBeam.Identifier);

        var fittingPlane = new Plane { Origin = secOrigin, AxisX = axisY, AxisY = axisZ };
        var fitting = new Fitting { Father = shortBeam, Plane = fittingPlane };
        fitting.Insert();

        #region 切角

        // 腹板焊接需要使用
        Point p4, p5;

        if (_chamferBeamType == 0) {
            p4 = secTIxPrim;
            p5 = secBIxPrim;
            goto SkipCut;
        }

        p1 = secBIxPrim;
        p2 = p1 + secAxisX * 1000;
        var p3 = p1 + axisY * 1000;

        var chamfer = CreateChamferBeam();
        var cp1 = new ContourPoint(p1, chamfer);
        var simulated = cp1.GetSimulatedContourPoints(p2, p3);
        p4 = simulated.Last();

        // 假如梁相对于柱歪斜，原地切割会遗留一小部分实体，所以需要往柱方向多切割一些
        var cps = new ArrayList { new ContourPoint(p1 - secAxisX * 100, chamferNone) };
        cps.AddRange(simulated);
        cps.Add(new ContourPoint(p4 - secAxisX * 100, chamferNone));

        var boolean = ModelOperation.CreateBooleanOperationPolygon(cps, prfSec.s + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(shortBeam, boolean);
        boolean.Delete();

        p1 = secTIxPrim;
        p2 = p1 + secAxisX * 1000;
        p3 = p1 - axisY * 1000;

        cp1 = new ContourPoint(p1, chamfer);
        simulated = cp1.GetSimulatedContourPoints(p2, p3);
        p5 = simulated.Last();

        cps = new ArrayList { new ContourPoint(p1 - secAxisX * 100, chamferNone) };
        cps.AddRange(simulated);
        cps.Add(new ContourPoint(p5 - secAxisX * 100, chamferNone));

        boolean = ModelOperation.CreateBooleanOperationPolygon(cps, prfSec.s + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(shortBeam, boolean);
        boolean.Delete();

        p1 = secOrigin + secAxisX * (_shortBeamLen - _gap * 0.5) - secAxisY * (prfSec.h1 * 0.5 - prfSec.t1);
        p2 = p1 - secAxisX * 1000;
        p3 = p1 + secAxisY * 1000;

        cps = new ArrayList { new ContourPoint(p1 + secAxisX * 50, chamferNone) };

        cp1 = new ContourPoint(p1, chamfer);
        simulated = cp1.GetSimulatedContourPoints(p2, p3);
        cps.AddRange(simulated);

        p1 += secAxisY * (prfSec.h1 - prfSec.t1 * 2);
        p2 = p1 - secAxisX * 1000;
        p3 = p1 - secAxisY * 1000;

        cp1 = new ContourPoint(p1, chamfer.GetReversed());
        simulated = cp1.GetSimulatedContourPoints(p3, p2);
        cps.AddRange(simulated);

        cps.Add(new ContourPoint(p1 + secAxisX * 50, chamferNone));

        boolean = ModelOperation.CreateBooleanOperationPolygon(cps, prfSec.s + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(shortBeam, boolean);
        boolean.Delete();

        SkipCut: ;

        #endregion

        var polyWeld = CreateWeld2<PolygonWeld>();
        polyWeld.MainObject = prim;
        polyWeld.SecondaryObject = shortBeam;
        polyWeld.Polygon = new Polygon { Points = new ArrayList { secTFxPrim, secTBxPrim } };
        polyWeld.Insert();

        polyWeld.Polygon = new Polygon { Points = new ArrayList { secBFIxPrim, secBBIxPrim } };
        polyWeld.Insert();

        if (isSkew) {
            var webFrontPlane = new GeometricPlane(secOrigin + secAxisZ * (prfSec.s * 0.5), secAxisZ);

            var p4Plane = new GeometricPlane(p4, secAxisY);
            var p4Line = Intersection.PlaneToPlane(primRightOutlinePlane, p4Plane);
            p1 = Intersection.LineToPlane(p4Line, webFrontPlane);

            var p5Plane = new GeometricPlane(p5, secAxisY);
            var p5Line = Intersection.PlaneToPlane(primRightOutlinePlane, p5Plane);
            p4 = Intersection.LineToPlane(p5Line, webFrontPlane);

            Polygon polygon;
            if (_weld1Around != 0) {
                var webBehindPlane = new GeometricPlane(secOrigin - secAxisZ * (prfSec.s * 0.5), secAxisZ);
                p2 = Intersection.LineToPlane(p4Line, webBehindPlane);
                p3 = Intersection.LineToPlane(p5Line, webBehindPlane);
                polygon = new Polygon { Points = new ArrayList(new[] { p1, p2, p3, p4, p1 }) };
            } else {
                polygon = new Polygon { Points = new ArrayList(new[] { p1, p4 }) };
            }

            polyWeld = CreateWeld1<PolygonWeld>();
            polyWeld.MainObject = prim;
            polyWeld.SecondaryObject = shortBeam;
            polyWeld.Polygon = polygon;
            polyWeld.Insert();
        } else {
            weld = CreateWeld1<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = shortBeam;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Insert();
        }

        #endregion

        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
            new TransformationPlane(shortBeam.EndPoint, secAxisX, secAxisY));

        #region 腹板连接板及栓接

        p1 = new Point(0, _plateWebSpliceHeight * 0.5, 0);
        p2 = new Point(0, _plateWebSpliceHeight * -0.5, 0);

        var webSpliceFront = CreatePlateWebSplice<Beam>();
        webSpliceFront.StartPoint = p1;
        webSpliceFront.EndPoint = p2;
        webSpliceFront.Position.Plane = Position.PlaneEnum.MIDDLE;
        webSpliceFront.Position.Rotation = Position.RotationEnum.FRONT;
        webSpliceFront.Position.Depth = Position.DepthEnum.FRONT;
        webSpliceFront.Position.DepthOffset = prfSec.s * 0.5;
        webSpliceFront.Insert();

        var webSpliceBehind = CreatePlateWebSplice<Beam>();
        webSpliceBehind.StartPoint = p1;
        webSpliceBehind.EndPoint = p2;
        webSpliceBehind.Position.Plane = Position.PlaneEnum.MIDDLE;
        webSpliceBehind.Position.Rotation = Position.RotationEnum.BACK;
        webSpliceBehind.Position.Depth = Position.DepthEnum.BEHIND;
        webSpliceBehind.Position.DepthOffset = prfSec.s * 0.5;
        webSpliceBehind.Insert();

        var bolt = CreateBoltWeb<BoltArray>();
        bolt.PartToBoltTo = shortBeam;
        bolt.PartToBeBolted = sec;
        bolt.AddOtherPartToBolt(webSpliceFront);
        bolt.AddOtherPartToBolt(webSpliceBehind);
        bolt.FirstPosition = p1;
        bolt.SecondPosition = p2;
        bolt.Position.Rotation = Position.RotationEnum.FRONT;
        bolt.StartPointOffset.Dx = (_plateWebSpliceHeight - webBoltTotalDisX) * 0.5;
        bolt.Insert();

        #endregion

        #region 原梁对齐切割

        fittingPlane = new Plane { Origin = origin, AxisX = axisY, AxisY = axisZ };
        fitting = new Fitting { Father = sec, Plane = fittingPlane };
        fitting.Insert();

        if (_gap == 0 && _chamferBeamType == 0) goto SkipCut4;

        p1 = new Point(-50, -prfSec.h1 * 0.5 + prfSec.t1);
        p2 = new Point(_gap * 0.5, p1.Y);
        p3 = p2 + axisX * 1000;
        p5 = new Point(_gap * 0.5, -p1.Y);
        p4 = p5 + axisX * 1000;
        var p6 = new Point(-50, -p1.Y);

        cps = new ArrayList { new ContourPoint(p1, chamferNone) };

        chamfer = CreateChamferBeam();
        var cp2 = new ContourPoint(p2, chamfer);
        cps.AddRange(cp2.GetSimulatedContourPoints(p3, p5));

        var cp5 = new ContourPoint(p5, chamfer.GetReversed());
        cps.AddRange(cp5.GetSimulatedContourPoints(p2, p4));

        cps.Add(new ContourPoint(p6, chamferNone));

        boolean = ModelOperation.CreateBooleanOperationPolygon(cps, prfSec.s + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(sec, boolean);
        boolean.Delete();

        SkipCut4: ;

        #endregion

        #region 焊接

        p1 = new Point(0, prfSec.h1 * 0.5, prfSec.b1 * 0.5);
        p2 = new Point(p1.X, p1.Y, -p1.Z);

        polyWeld = CreateWeld6<PolygonWeld>();
        polyWeld.Polygon.Points = new ArrayList { p1, p2 };
        polyWeld.MainObject = shortBeam;
        polyWeld.SecondaryObject = sec;
        polyWeld.Insert();

        p1.Y = -prfSec.h1 * 0.5 + prfSec.t1;
        p2.Y = p1.Y;

        polyWeld.Polygon.Points = new ArrayList { p1, p2 };
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

        var chamfer = CreateChamferPrimStif();
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
            stif = CreatePlatePrimStif<ContourPlate>();
            stif.Contour.ContourPoints = cps;
            stif.Insert();

            weld = CreateWeld3<Weld>();
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
                    stif = CreatePlatePrimStif<ContourPlate>();
                    stif.Contour.ContourPoints = arr;
                    stif.Insert();

                    weld = CreateWeld3<Weld>();
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
                stif = CreatePlatePrimStif<ContourPlate>();
                stif.Contour.ContourPoints = contours.First(arr => ((Point)arr[0]).X < 0);
                stif.Insert();

                weld = CreateWeld3<Weld>();
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