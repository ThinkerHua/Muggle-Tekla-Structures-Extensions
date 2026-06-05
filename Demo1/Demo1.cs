using System;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Win = System.Windows;
using static Muggle.TsExtensions.Common.Operation;

namespace Muggle.TsExtensions.Demo1;

[Plugin("Demo1")]
[PluginUserInterface("Muggle.TsExtensions.Demo1.Views.MainWindow")]
[InputObjectDependency(InputObjectDependency.DEPENDENT)]
[PluginCoordinateSystem(CoordinateSystemType.FROM_FIRST_POINT_AND_GLOBAL)]
[FieldsFrom(typeof(PluginData))]
public partial class Demo1 : PluginBase {
    private Model Model { get; }

    [PartFieldDefaultValues("Primary", Profile = "HW502*470*20*25", Name = "COLUMN", Class = 4)]
    [PartFieldDefaultValues("Secondary", profile: "HM244*175*7*11", name: "BEAM", @class: 11)]
    [PlateFieldDefaultValues("Stiffener", Thickness = 10)]
    [PlateFieldDefaultValues("Splice", thickness: 10, breadth: 200, height: 300)]
    [BoltFieldDefaultValues("Connect", distXText: "70", distYText: "2*80")]
    [BoltFieldDefaultValues("Stud", 10.0, "STUD", "4*90", "80")]
    [WeldFieldDefaultValues(1, 10, 10, 6.0, 6.0)]
    [WeldFieldDefaultValues(2, 10, sizeAbove: 6.0)]
    [GeneralFieldDefaultValues("CreatStud", 1, "gap", 15.0, "BoltOffsetX", 50, "StudOffsetX", 50,
        "StifChamferType", 1, "StifChamferX", 15.0, "StifChamferY", 15.0, "StifChamferDz1", 0.0, "StifChamferDz2", 0.0)]
    private PluginData Data { get; }

    public Demo1(PluginData data) {
        Model = new Model();
        Data = data;

        SetDataToDefaultIfUnset();
        GetFieldValuesFrom(Data);
    }

    public override List<InputDefinition> DefineInput() {
        try {
            var picker = new Picker();
            var p = picker.PickPoint();
            return [new InputDefinition(p)];
        } catch (Exception e) when (e.Message != "User interrupt") {
            Win.MessageBox.Show(e.Message, "Error", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
            return [];
        }
    }

    public override bool Run(List<InputDefinition> input) {
        var basePoint = input[0].GetInput() as Point;

        const double Secondary1Length = 450.0;
        const double SecondaryTotalLength = 1200;

        try {

            #region Primary

            var p1 = basePoint + new Vector(0, 0, 20);
            var p2 = basePoint - new Vector(0, 0, 1000);
            var primary = CreatPartPrimary<Beam>();
            primary.StartPoint = p1;
            primary.EndPoint = p2;
            primary.Position.Depth = Position.DepthEnum.MIDDLE;
            primary.Position.Plane = Position.PlaneEnum.MIDDLE;
            primary.Position.Rotation = Position.RotationEnum.TOP;
            primary.Insert();

            #endregion

            #region Secondary

            var primHeight = 0.0;
            if (!primary.GetReportProperty("HEIGHT", ref primHeight))
                primary.GetReportProperty("PROFILE.HEIGHT", ref primHeight);

            p1 = basePoint + new Vector(primHeight * 0.5, 0, 0);
            p2 = p1 + new Vector(Secondary1Length - _gap * 0.5, 0, 0);
            var secondary1 = CreatPartSecondary<Beam>();
            secondary1.StartPoint = p1;
            secondary1.EndPoint = p2;
            secondary1.Position.Depth = Position.DepthEnum.BEHIND;
            secondary1.Position.Plane = Position.PlaneEnum.MIDDLE;
            secondary1.Position.Rotation = Position.RotationEnum.TOP;
            secondary1.Insert();

            var weld = CreatWeld1<Weld>();
            weld.MainObject = primary;
            weld.SecondaryObject = secondary1;
            weld.AroundWeld = true;
            weld.Insert();

            p1 = p2 + new Vector(_gap, 0, 0);
            p2 = basePoint + new Vector(SecondaryTotalLength, 0, 0);
            var secondary2 = CreatPartSecondary<Beam>();
            secondary2.StartPoint = p1;
            secondary2.EndPoint = p2;
            secondary2.Position.Depth = Position.DepthEnum.BEHIND;
            secondary2.Position.Plane = Position.PlaneEnum.MIDDLE;
            secondary2.Position.Rotation = Position.RotationEnum.TOP;
            secondary2.Insert();

            #endregion

            #region Stiffener

            var secHeight = 0.0;
            if (!secondary2.GetReportProperty("HEIGHT", ref secHeight))
                secondary2.GetReportProperty("PROFILE.HEIGHT", ref secHeight);
            var secFlangeThickness = 0.0;
            if (!secondary2.GetReportProperty("FLANGE_THICKNESS", ref secFlangeThickness))
                secondary2.GetReportProperty("PROFILE.FLANGE_THICKNESS", ref secFlangeThickness);

            if (_plateStiffenerThickness <= 0.0) _plateStiffenerThickness = secFlangeThickness;

            var stiffeners = ModelOperation.CreatStiffeners(primary,
                new Point(0, 0, -_plateStiffenerThickness * 0.5),
                _plateStiffenerThickness, _plateStiffenerMaterial, _plateStiffenerClass.ToString(),
                chamferType: EnumParse<Chamfer.ChamferTypeEnum>(_stifChamferType),
                chamferSizeX: _stifChamferX, chamferSizeY: _stifChamferY,
                chamferDz1: _stifChamferDz1, chamferDz2: _stifChamferDz2);
            stiffeners = stiffeners.Concat(ModelOperation.CreatStiffeners(primary,
                new Point(0, 0, -secHeight + _plateStiffenerThickness * 0.5),
                _plateStiffenerThickness, _plateStiffenerMaterial, _plateStiffenerClass.ToString(),
                chamferType: EnumParse<Chamfer.ChamferTypeEnum>(_stifChamferType),
                chamferSizeX: _stifChamferX, chamferSizeY: _stifChamferY,
                chamferDz1: _stifChamferDz1, chamferDz2: _stifChamferDz2));

            var primProfileType = string.Empty;
            try {
                _ = new ProfileH(_partPrimaryProfile);
                primProfileType = "H";
            } catch (UnAcceptableProfileException) {
            }

            try {
                _ = new ProfileRect(_partPrimaryProfile);
                primProfileType = "R";
            } catch (UnAcceptableProfileException) {
            }

            foreach (var stiffener in stiffeners) {
                weld = CreatWeld1<Weld>();
                weld.MainObject = primary;
                weld.SecondaryObject = stiffener;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                weld.Insert();

                switch (primProfileType) {
                case "H":
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                    weld.Insert();
                    break;
                case "R":
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                    weld.Insert();
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                    weld.Insert();
                    break;
                }
            }

            #endregion

            var originTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            var workTp = new TransformationPlane(
                new Point(primHeight * 0.5 + Secondary1Length, 0, -secHeight * 0.5),
                new Vector(100, 0, 0), new Vector(0, 100, 0));
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            #region Splice

            var secWebThickness = 0.0;
            if (!secondary1.GetReportProperty("WEB_THICKNESS", ref secWebThickness))
                secondary1.GetReportProperty("PROFILE.WEB_THICKNESS", ref secWebThickness);

            var secWidth = 0.0;
            if (!secondary2.GetReportProperty("WIDTH", ref secWidth))
                secondary2.GetReportProperty("PROFILE.WIDTH", ref secWidth);

            var secIsHProfileType = true;
            try {
                _ = new ProfileH(_partSecondaryProfile);
            } catch (UnAcceptableProfileException) {
                secIsHProfileType = false;
            }

            p1 = new Point(-_plateSpliceHeight * 0.5, 0, 0);
            p2 = p1 + new Vector(_plateSpliceHeight, 0, 0);

            var splice1 = CreatPlateSplice<Beam>();
            splice1.StartPoint = p1;
            splice1.EndPoint = p2;
            splice1.Position.Rotation = Position.RotationEnum.TOP;
            splice1.Position.Depth = Position.DepthEnum.MIDDLE;
            splice1.Position.Plane = Position.PlaneEnum.RIGHT;
            splice1.Position.PlaneOffset = secIsHProfileType ? secWebThickness * 0.5 : secWidth * 0.5;
            splice1.Insert();

            var splice2 = CreatPlateSplice<Beam>();
            splice2.StartPoint = p1;
            splice2.EndPoint = p2;
            splice2.Position.Rotation = Position.RotationEnum.TOP;
            splice2.Position.Depth = Position.DepthEnum.MIDDLE;
            splice2.Position.Plane = Position.PlaneEnum.LEFT;
            splice2.Position.PlaneOffset = secIsHProfileType ? secWebThickness * 0.5 : secWidth * 0.5;
            splice2.Insert();

            #endregion

            #region Bolt

            var bolt = CreatBoltConnect<BoltArray>();
            bolt.PartToBoltTo = secondary1;
            bolt.PartToBeBolted = splice1;
            bolt.AddOtherPartToBolt(splice2);
            bolt.FirstPosition = new Point();
            bolt.SecondPosition = new Point(-_plateSpliceBreadth * 0.5, 0, 0);
            bolt.Position.Rotation = Position.RotationEnum.BELOW;
            bolt.StartPointOffset.Dx = _boltOffsetX;
            bolt.Insert();

            bolt.PartToBoltTo = secondary2;
            bolt.SecondPosition.X *= -1;
            bolt.Position.Rotation = Position.RotationEnum.TOP;
            bolt.Insert();

            #endregion

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(originTp);

            #region Stud

            if (_creatStud == 0) goto DontCreatStud;

            var stud = CreatBoltStud<BoltArray>();
            stud.PartToBoltTo = secondary1;
            stud.PartToBeBolted = secondary1;
            stud.FirstPosition = secondary1.StartPoint;
            stud.SecondPosition = secondary1.EndPoint;
            stud.Position.Rotation = Position.RotationEnum.FRONT;
            stud.StartPointOffset.Dx = _studOffsetX;
            stud.Insert();

            stud.PartToBoltTo = secondary2;
            stud.PartToBeBolted = secondary2;
            stud.FirstPosition = secondary2.StartPoint;
            stud.SecondPosition = secondary2.EndPoint;
            stud.Insert();

            DontCreatStud: ;

            #endregion
            
            return true;
        } catch (Exception e) {
            Win.MessageBox.Show(e.Message, "Error", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
            return false;
        }
    }
}