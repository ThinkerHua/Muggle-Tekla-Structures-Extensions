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
 *  MJ5001.cs: H-beam connect to concrete
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
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Localization = Tekla.Structures.Dialog.Localization;
using TsData = Tekla.Structures.Datatype;
using TsG3d = Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.MJ5001;

[Plugin("MJ5001")]
[PluginUserInterface("Muggle.TsExtensions.MJ5001.View")]
[InputObjectDependency(InputObjectDependency.DEPENDENT)]
[FieldsFrom(typeof(PluginData))]
public partial class MJ5001 : PluginBase {

    private readonly record struct Profile(char Type, double H, double B, double S, double T) {
        public readonly char Type = Type;
        public readonly double H = H;
        public readonly double B = B;
        public readonly double S = S;
        public readonly double T = T;
    }

    private Model Model { get; }

    [PartFieldDefaultValues("AnchorRod", material: "Q345B", @class: 0)]
    [PlateFieldDefaultValues("Embedment", thickness: 20, breadth: 300, height: 800, material: "Q345B")]
    [PlateFieldDefaultValues("Cleat", thickness: 12, breadth: 300, height: 300, material: "Q345B")]
    [BoltFieldDefaultValues("AnchorRod", size: 20, length: 150, distXText: "4*125", distYText: "160")]
    [BoltFieldDefaultValues("1", standard: "HS10.9", size: 20, distXText: "3*70", distYText: "2*70")]
    [WeldFieldDefaultValues(1, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
    [WeldFieldDefaultValues(2, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
    [WeldFieldDefaultValues(3, sizeAbove: 10, angleAbove: 45, rootOpeningAbove: 2, rootFaceAbove: 2)]
    [ChamferFieldDefaultValues(1, type: 3, x: 30)]
    [GeneralFieldDefaultValues("Gap", 15)]
    private PluginData Data { get; }

    private Localization Localization { get; }

    public MJ5001(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(MJ5001)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    /// <summary>
    /// 定义输入。
    /// </summary>
    /// <returns>集合第一个元素为梁ID，第二个元素为安放预埋件的混凝土构件ID，
    /// 第三个元素是面（点集合，ArrayList类型）。</returns>
    public override List<InputDefinition> DefineInput() {
        var picker = new Picker();
        var beam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);
        var face = picker.PickFace();

        var beamInput = new InputDefinition(beam.Identifier);
        InputDefinition concreteInput = null, faceInput = null;

        var enumerator = face.GetEnumerator();
        while (enumerator.MoveNext()) {
            var item = (InputItem)enumerator.Current;
            if (item!.GetInputType() == InputItem.InputTypeEnum.INPUT_POLYGON) {
                var points = (ArrayList)item.GetData();
                faceInput = new InputDefinition(points);
            } else if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_1_OBJECT) {
                var obj = (ModelObject)item.GetData();
                concreteInput = new InputDefinition(obj.Identifier);
            }
        }

        (enumerator as IDisposable)?.Dispose();

        return [beamInput, concreteInput, faceInput];
    }

    public override bool Run(List<InputDefinition> input) {
        try {
            var beamId = (Identifier)input[0].GetInput();
            Verify(beamId, out var beamProfile);

            var concreteId = (Identifier)input[1].GetInput();
            var points = ((ArrayList)input[2].GetInput()).Cast<Point>().ToArray();

            SetDataToDefaultIfUnset();
            Data.PartAnchorRodProfile = $"D{Data.BoltAnchorRodSize}";
            GetFieldValuesFrom(Data);

            var workTp = GetWorkPlane(beamId, points, out var embedmentTp);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            CreateConnection(beamId, beamProfile, concreteId, embedmentTp);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }

    private void Verify(Identifier beamId, out Profile beamProfile) {
        var beam = (Part)Model.SelectModelObject(beamId);
        if (beam is not Beam && beam is not PolyBeam) {
            throw new NotSupportedException(Localization.GetText("exception_not_supported_part_type"));
        }

        beamProfile = new Profile();

        try {
            var prfH = new ProfileH_Symmetrical(beam.Profile.ProfileString);
            if (Math.Abs(prfH.h2 - prfH.h1) > GeometryConstants.DISTANCE_EPSILON ||
                Math.Abs(prfH.b2 - prfH.b1) > GeometryConstants.DISTANCE_EPSILON) {
                throw new NotSupportedException(
                    Localization.GetText("exception_not_supported_section_profile"));
            }

            beamProfile = new Profile('H', prfH.h1, prfH.b1, prfH.s, prfH.t1);
        } catch (UnAcceptableProfileException) {
            // ignore
        }

        try {
            var prfR = new ProfileRect_Invariant(beam.Profile.ProfileString);

            beamProfile = new Profile('R', prfR.h1, prfR.b1, prfR.s, prfR.t);
        } catch {
            // ignore
        }

        if (beamProfile.Type is '\0') {
            throw new NotSupportedException(Localization.GetText("exception_not_supported_section_profile"));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>（梁）工作平面 Origin 是梁中心线与混凝土表面的交点，
    /// AxisX 是梁中心线的方向且指向远处，
    /// AxisY 是梁零件坐标系的 Y 轴且大致指向全局向上方向；
    /// 预埋件工作平面 Origin 与 梁工作平面相同，
    /// AxisX 是梁零件坐标系 XY 平面与埋件表面的交线的方向表大致指向全局向上方向，
    /// AxisZ 是埋件表面法向，
    /// AxisY 是 AxisZ.Cross(AxisX)。 
    /// </remarks>
    /// <param name="beamId"></param>
    /// <param name="points"></param>
    /// <param name="embedmentTp"></param>
    /// <returns></returns>
    private TransformationPlane GetWorkPlane(Identifier beamId, Point[] points,
        out TransformationPlane embedmentTp) {
        var part = (Part)Model.SelectModelObject(beamId);
        var plane = GeometricPlaneFactory.ByPoints(points[0], points[1], points[2]);

        var centerLine = part.GetCenterLine(false).Cast<Point>().ToArray();
        var firstPoint = centerLine.First();
        var lastPoint = centerLine.Last();

        var disFirst = TsG3d.Distance.PointToPlane(firstPoint, plane);
        var disLast = TsG3d.Distance.PointToPlane(lastPoint, plane);
        var index = disFirst <= disLast ? 0 : centerLine.Length - 1;
        var step = disFirst <= disLast ? 1 : -1;

        var origin = Intersection.LineToPlane(new Line(centerLine[index], centerLine[index + step]), plane);

        CoordinateSystem cs = null;
        switch (part) {
        case Beam beam:
            cs = beam.GetCoordinateSystem();
            break;
        case PolyBeam polyBeam:
            var css = polyBeam.GetPolybeamCoordinateSystemsEx();
            cs = disFirst <= disLast ? css.First() : css.Last();
            break;
        // no need default, already checked in Run method
        }

        var axisX = cs!.AxisX;
        if (disFirst > disLast) axisX *= -1;
        var axisY = cs.AxisY;

        var globalZ = new Vector(0, 0, 100).TransformFrom(new TransformationPlane());
        if (globalZ.Dot(axisY) < 0) axisY *= -1;

        var partXyPlane = new GeometricPlane(origin, axisX.Cross(axisY));
        var crossLine = Intersection.PlaneToPlane(partXyPlane, plane);

        var embedmentAxisX = crossLine.Direction;
        if (embedmentAxisX.Dot(axisY) < 0) embedmentAxisX *= -1;

        var embedmentAxisZ = plane.Normal;
        if (embedmentAxisZ.Dot(axisX) < 0) embedmentAxisZ *= -1;
        var embedmentAxisY = embedmentAxisZ.Cross(embedmentAxisX);

        embedmentTp = new TransformationPlane(origin, embedmentAxisX, embedmentAxisY);

        return new TransformationPlane(origin, axisX, axisY);
    }

    private void CreateConnection(Identifier beamId, Profile beamProfile, Identifier concreteId,
        TransformationPlane embedmentTp) {
        var beam = (Part)Model.SelectModelObject(beamId);
        var concrete = (Part)Model.SelectModelObject(concreteId);

        var origin = new Point();
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        var beamTopLine = new Line(origin + axisY * (beamProfile.H * 0.5), axisX);
        var beamTopInsideLine = beamTopLine.Offset(beamProfile.T, LineExtension.OffsetDirection.Right);
        var beamBottomLine = new Line(origin - axisY * (beamProfile.H * 0.5), axisX);
        var beamBottomInsideLine = beamBottomLine.Offset(beamProfile.T, LineExtension.OffsetDirection.Left);

        var beamTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

        var embedmentAxisX = axisX.Transform(embedmentTp, beamTp);
        var embedmentAxisY = axisY.Transform(embedmentTp, beamTp);
        var embedmentAxisZ = axisZ.Transform(embedmentTp, beamTp);

        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(embedmentTp);

        #region 创建预埋板

        var p1 = new Point(_plateEmbedmentHeight * -0.5, 0);
        var p2 = new Point(_plateEmbedmentHeight * 0.5, 0);

        var embedment = CreatePlateEmbedment<Beam>();
        embedment.StartPoint = p1;
        embedment.EndPoint = p2;
        embedment.Position.Rotation = Position.RotationEnum.FRONT;
        embedment.Position.Plane = Position.PlaneEnum.MIDDLE;
        embedment.Position.Depth = Position.DepthEnum.BEHIND;
        embedment.Insert();

        _ = ModelOperation.ApplyBooleanOperation(concrete, embedment);

        #endregion

        #region 布置锚杆

        var anchorRodDisListX = DistanceList.Parse(_boltAnchorRodDistXText, CultureInfo.CurrentCulture,
            TsData.Distance.CurrentUnitType);
        var anchorRodDisListY = DistanceList.Parse(_boltAnchorRodDistYText, CultureInfo.CurrentCulture,
            TsData.Distance.CurrentUnitType);

        var anchorRodPosX = new double[anchorRodDisListX.Count + 1];
        anchorRodPosX[0] = 0.0;
        var aggregate = 0.0;
        for (int i = 0; i < anchorRodDisListX.Count; i++) {
            aggregate += anchorRodDisListX[i].Value;
            anchorRodPosX[i + 1] = aggregate;
        }

        var anchorRodPosY = new double[anchorRodDisListY.Count + 1];
        anchorRodPosY[0] = 0.0;
        aggregate = 0.0;
        for (int i = 0; i < anchorRodDisListY.Count; i++) {
            aggregate += anchorRodDisListY[i].Value;
            anchorRodPosY[i + 1] = aggregate;
        }

        foreach (var x in anchorRodPosX) {
            foreach (var y in anchorRodPosY) {
                var pos = new Point(anchorRodPosX.Last() * -0.5 + x, anchorRodPosY.Last() * -0.5 + y);
                p1 = pos - axisZ * _plateEmbedmentThickness;
                p2 = p1 - axisZ * _boltAnchorRodLength;
                CreateAnchorRodAndWeld(p1, p2);
            }
        }

        void CreateAnchorRodAndWeld(Point startPoint, Point endPoint) {
            var anchorRod = CreatePartAnchorRod<Beam>();
            anchorRod.Position.Plane = Position.PlaneEnum.MIDDLE;
            anchorRod.Position.Depth = Position.DepthEnum.MIDDLE;
            anchorRod.StartPoint = startPoint;
            anchorRod.EndPoint = endPoint;
            anchorRod.Insert();

            var weld = CreateWeld1<Weld>();
            weld.MainObject = embedment;
            weld.SecondaryObject = anchorRod;
            weld.Insert();
        }

        #endregion

        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(beamTp);

        #region 梁末端对齐

        var plane = new Plane {
            Origin = origin + embedmentAxisZ * (_weld3TypeAbove == 0 ? _gap : 0),
            AxisX = embedmentAxisX,
            AxisY = embedmentAxisY
        };
        var fitting = new Fitting { Father = beam, Plane = plane };
        fitting.Insert();

        #endregion

        #region 开焊孔

        var chamferNone = new Chamfer();
        
        if (_chamfer1Type == 0) goto SkipChamfer;

        var fittingPlane = GeometricPlaneFactory.ByPlane(plane);
        if (_weld3TypeAbove != 0) fittingPlane.Origin += embedmentAxisZ * _gap;
        
        p1 = Intersection.LineToPlane(beamBottomInsideLine, fittingPlane);
        p2 = p1 + axisX * 1000;
        var p3 = Intersection.LineToPlane(beamTopInsideLine, fittingPlane);
        var p4 = p3 + axisX * 1000;

        var chamfer = CreateChamfer1();
        var cp1 = new ContourPoint(p1, chamfer);
        var simulated = cp1.GetSimulatedContourPoints(p2, p3);

        var cps = new ArrayList();
        cps.Add(new ContourPoint(p1 - axisX * 50, chamferNone));
        cps.AddRange(simulated);

        var cp3 = new ContourPoint(p3, chamfer.GetReversed());
        simulated = cp3.GetSimulatedContourPoints(p1, p4);

        cps.AddRange(simulated);
        cps.Add(new ContourPoint(p3 - axisX * 50, chamferNone));
        
        var boolean = ModelOperation.CreateBooleanOperationPolygon(cps, beamProfile.B + 10);
        boolean.Insert();
        _ = ModelOperation.ApplyBooleanOperation(beam, boolean);
        boolean.Delete();

        SkipChamfer: ;

        #endregion

        #region 焊梁

        var embedmentXyPlane = new GeometricPlane(origin, embedmentAxisX, embedmentAxisY);

        if (_weld3TypeAbove == 0) goto SkipWeldBeam;

        p1 = Intersection.LineToPlane(beamTopLine.Offset(axisZ.GetNormal(beamProfile.B * 0.5)), embedmentXyPlane);
        p2 = Intersection.LineToPlane(beamTopLine.Offset(axisZ.GetNormal(beamProfile.B * -0.5)), embedmentXyPlane);

        var polyWeld = CreateWeld3<PolygonWeld>();
        polyWeld.Polygon = new Polygon { Points = new ArrayList { p1, p2 } };
        polyWeld.MainObject = embedment;
        polyWeld.SecondaryObject = beam;
        polyWeld.Insert();

        p1 = Intersection.LineToPlane(beamBottomInsideLine.Offset(axisZ.GetNormal(beamProfile.B * 0.5)),
            embedmentXyPlane);
        p2 = Intersection.LineToPlane(beamBottomInsideLine.Offset(axisZ.GetNormal(beamProfile.B * -0.5)),
            embedmentXyPlane);

        polyWeld = CreateWeld3<PolygonWeld>();
        polyWeld.Polygon = new Polygon { Points = new ArrayList { p1, p2 } };
        polyWeld.MainObject = embedment;
        polyWeld.SecondaryObject = beam;
        polyWeld.Insert();

        SkipWeldBeam: ;

        #endregion

        #region 创建夹板

        var embedmentXzPlane = new GeometricPlane(origin, embedmentAxisX, embedmentAxisZ);
        var axisXProjOnEmbedXz = ProjectionExtension.VectorToPlane(axisX, embedmentXzPlane);
        var isSloping = axisXProjOnEmbedXz.GetAngleBetweenPrecisely(embedmentAxisZ) >
                        GeometryConstants.ANGULAR_EPSILON;

        var embedmentYzPlane = new GeometricPlane(origin, embedmentAxisY, embedmentAxisZ);
        var axisXProjOnEmbedYz = ProjectionExtension.VectorToPlane(axisX, embedmentYzPlane);
        var isSkew = axisXProjOnEmbedYz.GetAngleBetweenPrecisely(embedmentAxisZ) >
                     GeometryConstants.ANGULAR_EPSILON;

        var cleatAxisX = axisX.GetNormal(1 / axisXProjOnEmbedYz.GetLength());

        p1 = origin + embedmentAxisX * (_plateCleatHeight * 0.5);
        p2 = p1 + cleatAxisX * _plateCleatBreadth;
        p4 = origin - embedmentAxisX * (_plateCleatHeight * 0.5);
        p3 = p4 + cleatAxisX * _plateCleatBreadth;

        if (isSkew) {
            p1 -= axisX * 100;
            p4 -= axisX * 100;
        }

        var contours = new ArrayList {
            new ContourPoint(p1, chamferNone),
            new ContourPoint(p2, chamferNone),
            new ContourPoint(p3, chamferNone),
            new ContourPoint(p4, chamferNone)
        };

        var depthOffset = beamProfile.Type is 'H'
            ? beamProfile.S * 0.5
            : beamProfile.B * 0.5 - beamProfile.S - _plateCleatThickness;

        var cleatFront = CreatePlateCleat<ContourPlate>();
        cleatFront.Contour.ContourPoints = contours;
        cleatFront.Position.Depth = Position.DepthEnum.FRONT;
        cleatFront.Position.DepthOffset = depthOffset;
        cleatFront.Insert();

        var cleatBehind = CreatePlateCleat<ContourPlate>();
        cleatBehind.Contour.ContourPoints = contours;
        cleatBehind.Position.Depth = Position.DepthEnum.BEHIND;
        cleatBehind.Position.DepthOffset = depthOffset;
        cleatBehind.Insert();

        if (isSkew) {
            p1 = Intersection.LineToPlane(beamTopLine, embedmentXyPlane) + embedmentAxisX * 100;
            p2 = Intersection.LineToPlane(beamBottomLine, embedmentXyPlane) - embedmentAxisX * 100;
            boolean = ModelOperation.CreateBooleanOperationPolygon(
                [
                    p1 + embedmentAxisY * (beamProfile.B * 0.5 + 10) - embedmentAxisZ * 75,
                    p1 - embedmentAxisY * (beamProfile.B * 0.5 + 10) - embedmentAxisZ * 75,
                    p2 - embedmentAxisY * (beamProfile.B * 0.5 + 10) - embedmentAxisZ * 75,
                    p2 + embedmentAxisY * (beamProfile.B * 0.5 + 10) - embedmentAxisZ * 75,
                ],
                150);
            boolean.Insert();
            _ = ModelOperation.ApplyBooleanOperation(cleatFront, boolean);
            _ = ModelOperation.ApplyBooleanOperation(cleatBehind, boolean);
            boolean.Delete();
        }

        var weld = CreateWeld2<Weld>();
        weld.MainObject = embedment;
        weld.SecondaryObject = cleatFront;
        weld.Insert();

        weld.SecondaryObject = cleatBehind;
        weld.Insert();

        #endregion

        #region 布置螺栓

        var boltDisListX =
            DistanceList.Parse(_bolt1DistXText, CultureInfo.CurrentCulture, TsData.Distance.CurrentUnitType);
        var boltDisListY =
            DistanceList.Parse(_bolt1DistYText, CultureInfo.CurrentCulture, TsData.Distance.CurrentUnitType);
        var boltTotalDisX = boltDisListX.Sum(dis => dis.Value);
        var boltTotalDisY = boltDisListY.Sum(dis => dis.Value);

        var bolt = CreateBolt1<BoltArray>();
        bolt.PartToBoltTo = beam;
        bolt.PartToBeBolted = cleatFront;
        bolt.AddOtherPartToBolt(cleatBehind);
        bolt.Position.Rotation = Position.RotationEnum.FRONT;

        if (isSloping && boltDisListY.Count > 1) {
            for (int i = boltDisListY.Count - 1; i >= 0; i--) {
                bolt.RemoveBoltDistY(i);
            }

            bolt.AddBoltDistY(0.0);

            p1 = origin + embedmentAxisX * (boltTotalDisX * 0.5) +
                 cleatAxisX * (_gap + (_plateCleatBreadth - boltTotalDisY) * 0.5);
            p2 = p1 - embedmentAxisX * boltTotalDisX;

            bolt.FirstPosition = p1;
            bolt.SecondPosition = p2;
            bolt.Insert();

            foreach (var y in boltDisListY) {
                p1 += cleatAxisX * y.Value;
                p2 += cleatAxisX * y.Value;

                bolt.FirstPosition = p1;
                bolt.SecondPosition = p2;
                bolt.Insert();
            }
        } else {
            p1 = origin + embedmentAxisX * (boltTotalDisX * 0.5) +
                 cleatAxisX * (_gap + (_plateCleatBreadth - _gap) * 0.5);
            p2 = p1 - embedmentAxisX * boltTotalDisX;

            bolt.FirstPosition = p1;
            bolt.SecondPosition = p2;
            bolt.Insert();
        }

        #endregion

    }
}