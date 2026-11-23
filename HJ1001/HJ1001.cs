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
 *  HJ1001.cs: round tube butting
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
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

namespace Muggle.TsExtensions.HJ1001;

[Plugin("HJ1001")]
[PluginUserInterface("Muggle.TsExtensions.HJ1001.View")]
[InputObjectType(InputObjectType.INPUTOBJECT_PART)]
[SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
[PositionType(PositionTypeEnum.END_END_PLANE)]
[FieldsFromData]
public partial class HJ1001 : ConnectionBase {

    private Localization Localization { get; }

    private Model Model { get; }

    private PluginData Data { get; }

    public HJ1001(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(HJ1001)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            Data.SetFieldsToDefaultIfUnset();
            GetFieldValuesFrom(Data);

            VerifyProfile(out var prfPrim, out var prfSec);

            Model.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(GetWorkTransformationPlane(out var primIsCurve, out var secIsCurve));

            CreateConnection(prfPrim, prfSec, primIsCurve, secIsCurve);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }

    /// <summary>
    /// 目前仅支持圆钢或圆管的正圆固定截面形式，不支持椭圆形式。
    /// </summary>
    private void VerifyProfile(out ProfileCircular_Perfect prfPrim, out ProfileCircular_Perfect prfSec) {
        var prim = (Part)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        try {
            prfPrim = new ProfileCircular_Perfect(prim.Profile.ProfileString);
            if (Math.Abs(prfPrim.d1 - prfPrim.d2) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_primary_profile_not_supported"));
        }

        try {
            prfSec = new ProfileCircular_Perfect(sec.Profile.ProfileString);
            if (Math.Abs(prfSec.d1 - prfSec.d2) > GeometryConstants.DISTANCE_EPSILON)
                throw new UnAcceptableProfileException();
        } catch (UnAcceptableProfileException) {
            throw new Exception(Localization.GetText("exception_secondary_profile_not_supported"));
        }
    }

    private TransformationPlane GetWorkTransformationPlane(out bool primIsCurve, out bool secIsCurve) {
        var prim = (Part)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        var primCenterline = prim.GetCenterLine(false);
        var primStartPoint = (Point)primCenterline[0];
        var primEndPoint = (Point)primCenterline[primCenterline.Count - 1];

        var secCenterline = sec.GetCenterLine(false);
        var secStartPoint = (Point)secCenterline[0];
        var secEndPoint = (Point)secCenterline[secCenterline.Count - 1];

        var disPsToSs = Distance.PointToPoint(primStartPoint, secStartPoint);
        var disPsToSe = Distance.PointToPoint(primStartPoint, secEndPoint);
        var disPeToSs = Distance.PointToPoint(primEndPoint, secStartPoint);
        var disPeToSe = Distance.PointToPoint(primEndPoint, secEndPoint);

        var primStartCloser = Math.Min(disPsToSs, disPsToSe) < Math.Min(disPeToSs, disPeToSe);
        var secStartCloser = Math.Min(disPsToSs, disPeToSs) < Math.Min(disPsToSe, disPeToSe);

        primIsCurve = false;
        switch (prim) {
        case Beam:
            primIsCurve = primCenterline.Count != 2;
            break;
        case PolyBeam polyBeam:
            var contourPoints = polyBeam.Contour.ContourPoints;
            var chamfer = ((ContourPoint)contourPoints[primStartCloser ? 1 : contourPoints.Count - 2]).Chamfer;
            primIsCurve = chamfer.Type == Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT;
            break;
        }

        secIsCurve = false;
        switch (sec) {
        case Beam:
            secIsCurve = secCenterline.Count != 2;
            break;
        case PolyBeam polyBeam:
            var contourPoints = polyBeam.Contour.ContourPoints;
            var chamfer = ((ContourPoint)contourPoints[secStartCloser ? 1 : contourPoints.Count - 2]).Chamfer;
            secIsCurve = chamfer.Type == Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT;
            break;
        }

        var index = primStartCloser ? 0 : primCenterline.Count - 1;
        var step = primStartCloser ? 1 : -1;
        var p1 = (Point)primCenterline[index];
        var p2 = (Point)primCenterline[index + step];

        Vector axisX, axisY;
        if (primIsCurve) {
            var p3 = (Point)primCenterline[index + step + step];
            var arc = new Arc(p1, p3, p2);
            axisX = arc.StartDirection;
            axisY = arc.StartDirection.Cross(arc.StartTangent);
        } else {
            var axisZ = new Vector(p2 - p1);

            // 有时 PolyBeam.GetPolybeamCoordinateSystems 方法返回的是空集合
            // 所以不对 PolyBeam 进行特殊操作
            var primCs = prim.GetCoordinateSystem();
            axisX = axisZ.Cross(Parallel.VectorToVector(axisZ, primCs.AxisX) ? primCs.AxisY : primCs.AxisX);
            axisY = axisX.Cross(axisZ);
        }

        return new TransformationPlane(p1, axisX, axisY);
    }

    private void CreateConnection(ProfileCircular_Perfect prfPrim, ProfileCircular_Perfect prfSec,
        bool primIsCurve, bool secIsCurve) {

        #region 初始信息

        // var currentTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

        var origin = new Point();
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        var prim = (Part)Model.SelectModelObject(Primary);
        var sec = (Part)Model.SelectModelObject(Secondaries[0]);

        var rotationAngel = _quantity == 0 ? 0 : 2 * Math.PI / _quantity;
        var matrix = MatrixFactory.Rotate(rotationAngel, axisZ);

        #endregion

        #region 对齐零件

        var plane = new Plane { Origin = new Point(0, 0, -_plateEndThickness), AxisX = axisX, AxisY = axisY, };
        var fitting = new Fitting { Father = prim, Plane = plane };
        fitting.Insert();

        fitting.Plane.Origin.Z *= -1;
        fitting.Father = sec;
        fitting.Insert();

        #endregion

        #region 创建端板并焊接

        var width = _plateEndBreadth * 0.5;
        var p1 = new Point(-width, 0, 0);
        var p2 = new Point(0, -width, 0);
        var p3 = new Point(width, 0, 0);
        var p4 = new Point(0, width, 0);

        var chamferNone = new Chamfer();
        var chamferArcPoint = new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT);

        var cp1 = new ContourPoint(p1, chamferNone);
        var cp2 = new ContourPoint(p2, chamferArcPoint);
        var cp3 = new ContourPoint(p3, chamferNone);
        var cp4 = new ContourPoint(p4, chamferArcPoint);

        var endplate1 = Data.CreatePlateEnd<ContourPlate>();
        endplate1.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        endplate1.Position.Depth = Position.DepthEnum.BEHIND;
        endplate1.Insert();

        var endplate2 = Data.CreatePlateEnd<ContourPlate>();
        endplate2.Contour.ContourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
        endplate2.Position.Depth = Position.DepthEnum.FRONT;
        endplate2.Insert();

        var weld = Data.CreateWeld1<Weld>();
        weld.MainObject = prim;
        weld.SecondaryObject = endplate1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
        weld.Insert();

        weld.MainObject = sec;
        weld.SecondaryObject = endplate2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        #endregion

        #region 创建加劲板并焊接

        if (_quantity <= 0 || _plateStiffenerThickness <= 0) goto SkipStif;

        var primStifHeight = (_plateEndBreadth - prfPrim.d1) * 0.5 - _margin;
        var secStifHeight = (_plateEndBreadth - prfSec.d1) * 0.5 - _margin;

        Part primBoolean = null, secBoolean = null;
        if (primIsCurve) {
            primBoolean = ClonInputPartAsBoolean(prfPrim.d1, prim);
            primBoolean.Insert();
        }

        if (secIsCurve) {
            secBoolean = ClonInputPartAsBoolean(prfSec.d1, sec);
            secBoolean.Insert();
        }

        for (int i = 0; i < _quantity; i++) {
            _ = InsertStiffenerAndWeld(primStifHeight, false, prfPrim.d1, primIsCurve, prim, endplate1, primBoolean);
            _ = InsertStiffenerAndWeld(secStifHeight, true, prfSec.d1, secIsCurve, sec, endplate2, secBoolean);

            var cs = new CoordinateSystem();
            cs = matrix.Transform(cs);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(cs));
        }

        if (primIsCurve) primBoolean.Delete();
        if (secIsCurve) secBoolean.Delete();

        // Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTp);

        SkipStif: ;

        #endregion

        #region 创建螺栓

        if (_bolt1Diameter <= 0 || _bolt1NumberOfBolts <= 0) goto SkipBolt;

        matrix = MatrixFactory.Rotate(Math.PI / _bolt1NumberOfBolts, axisZ);
        p1 = matrix.Transform(axisX * 100);

        var bolt = Data.CreateBolt1<BoltCircle>();
        bolt.FirstPosition = origin;
        bolt.SecondPosition = p1;
        bolt.PartToBoltTo = endplate1;
        bolt.PartToBeBolted = endplate2;
        bolt.Insert();

        SkipBolt: ;

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>不能使用 Operation.CopyObject 方法来复制组件输入对象并用于布尔操作，
    /// 否则每次 Modify 组件，都会产生重复 BooleanPart 对象(上次运行产生的对象不消除)</remarks>
    /// <param name="diameter"></param>
    /// <param name="inputPart"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static PolyBeam ClonInputPartAsBoolean(double diameter, Part inputPart) {
        if (inputPart == null) {
            throw new ArgumentNullException(nameof(inputPart));
        }

        var boolean = new PolyBeam {
            Profile = { ProfileString = $"D{diameter}" },
            Material = { MaterialString = "ANTIMATERIAL" },
            DeformingData = inputPart.DeformingData,
            Class = BooleanPart.BooleanOperativeClassName
        };

        var contours = new ArrayList();
        switch (inputPart) {
        case Beam beam:
            var chamferNone = new Chamfer();
            var centerline = beam.GetCenterLine(false);

            contours.Add(new ContourPoint((Point)centerline[0], chamferNone));
            if (centerline.Count > 2) {
                contours.Add(new ContourPoint((Point)centerline[centerline.Count / 2],
                    new Chamfer(0, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT)));
            }

            contours.Add(new ContourPoint((Point)centerline[centerline.Count - 1], chamferNone));
            boolean.Position.Depth = Position.DepthEnum.MIDDLE;
            break;
        case PolyBeam polyBeam:
            contours.AddRange(polyBeam.Contour.ContourPoints);
            boolean.Position = polyBeam.Position;
            break;
        }

        boolean.Contour.ContourPoints = contours;

        return boolean;
    }

    private ContourPlate InsertStiffenerAndWeld(double stifHeight, bool aboveXyPlane,
        double inputPartDiameter, bool inputPartIsCurve, Part inputPart, ContourPlate endplate,
        Part booleanOperativePart) {
        var axisX = new Vector(1, 0, 0);
        var axisZ = new Vector(0, 0, 1);
        var chamferInside = Data.CreateChamferInside();
        var chamferNone = new Chamfer();
        var chamferOutside = Data.CreateChamferOutside();

        var p1 = new Point(-inputPartDiameter * 0.5, 0, aboveXyPlane ? _plateEndThickness : -_plateEndThickness);
        var p2 = p1 - axisX * stifHeight;
        var p4 = p1 + axisZ * (aboveXyPlane ? _plateStiffenerBreadth : -_plateStiffenerBreadth);

        var cp1 = new ContourPoint(p1, chamferInside);
        var cp2 = new ContourPoint(p2, chamferNone);
        var cp4 = new ContourPoint(p4, chamferNone);

        ContourPoint cp5 = null, cp6 = null;
        List<ContourPoint> simulated = null;
        if (inputPartIsCurve) {
            simulated = cp1.GetSimulatedContourPoints(p4, p2);
            var p5 = new Point(0, 0, p4.Z);
            var p6 = new Point(0, 0, simulated.First().Z);
            cp5 = new ContourPoint(p5, chamferNone);
            cp6 = new ContourPoint(p6, chamferNone);
        }

        ContourPoint[] contourPoints;
        if (_stifKind != 0) {
            var p3 = p2 + axisZ * (aboveXyPlane ? _plateStiffenerBreadth : -_plateStiffenerBreadth);
            var cp3 = new ContourPoint(p3, chamferOutside);

            contourPoints = inputPartIsCurve ? [.. simulated, cp2, cp3, cp4, cp5, cp6] : [cp1, cp2, cp3, cp4];
        } else {
            contourPoints = inputPartIsCurve ? [.. simulated, cp2, cp4, cp5, cp6] : [cp1, cp2, cp4];
        }

        var stif = Data.CreatePlateStiffener<ContourPlate>();
        stif.Contour.ContourPoints = new ArrayList(contourPoints);
        stif.Insert();

        if (inputPartIsCurve) {
            _ = ModelOperation.ApplyBooleanOperation(stif, booleanOperativePart);
        }

        var weld = Data.CreateWeld2<Weld>();
        weld.MainObject = inputPart;
        weld.SecondaryObject = stif;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        weld.MainObject = endplate;
        weld.Position = aboveXyPlane
            ? Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z
            : Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
        weld.Insert();
        return stif;
    }
}