using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Distance = Tekla.Structures.Datatype.Distance;
using Win = System.Windows;
using static Muggle.TsExtensions.Common.Operation;

namespace Muggle.TsExtensions.Demo1;

[Plugin("Demo1")]
[PluginUserInterface("Muggle.TsExtensions.Demo1.Views.MainWindow")]
[InputObjectDependency(InputObjectDependency.DEPENDENT)]
[PluginCoordinateSystem(CoordinateSystemType.FROM_FIRST_POINT_AND_GLOBAL)]
[FieldsFrom(typeof(PluginData))]
public partial class Demo1 : PluginBase {
    public Model Model { get; set; }

    [PartFieldDefaultValues("Primary", profile: "HW502*470*20*25", name: "COLUMN", @class: 4)]
    [PartFieldDefaultValues("Secondary", profile: "HM244*175*7*11", name: "BEAM", @class: 11)]
    [PlateFieldDefaultValues("Stiffener", thickness: 10)]
    [PlateFieldDefaultValues("Splice", thickness: 10, breadth: 300, height: 200)]
    [BoltFieldDefaultValues("Connect", distXText: "70", distYText: "2*80")]
    [BoltFieldDefaultValues("Stud", 10.0, "STUD", "4*90", "80")]
    [WeldFieldDefaultValues(1, 10, 10, 6.0, 6.0)]
    [WeldFieldDefaultValues(2, 10, sizeAbove: 6.0)]
    [GeneralFieldDefaultValues("CreatStud", 1, "Gap", 15.0, "BoltOffsetX", 50, "StudOffsetX", 50, 
        "StifChamferType", 1, "StifChamferX", 15.0, "StifChamferY", 15.0, "StifChamferDz1", 0.0, "StifChamferDz2", 0.0)]
    public PluginData Data { get; set; }

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
            var primary = new Beam(p1, p2) {
                Name = _partPrimaryName,
                Profile = { ProfileString = _partPrimaryProfile },
                Material = { MaterialString = _partPrimaryMaterial },
                Finish = _partPrimaryFinish,
                Class = _partPrimaryClass.ToString(),
                AssemblyNumber = { Prefix = _partPrimaryAssemblyPrefix, StartNumber = _partPrimaryAssemblyStartNumber },
                PartNumber = { Prefix = _partPrimaryPartPrefix, StartNumber = _partPrimaryPartStartNumber },
                Position = {
                    Depth = Position.DepthEnum.MIDDLE,
                    Plane = Position.PlaneEnum.MIDDLE,
                    Rotation = Position.RotationEnum.TOP
                }
            };
            primary.Insert();

            #endregion

            #region Secondary

            double primHeight = 0.0;
            if (!primary.GetReportProperty("HEIGHT", ref primHeight))
                primary.GetReportProperty("PROFILE.HEIGHT", ref primHeight);

            p1 = basePoint + new Vector(primHeight * 0.5, 0, 0);
            p2 = p1 + new Vector(Secondary1Length - _gap * 0.5, 0, 0);
            var secondary1 = new Beam(p1, p2) {
                Name = _partSecondaryName,
                Profile = { ProfileString = _partSecondaryProfile },
                Material = { MaterialString = _partSecondaryMaterial },
                Finish = _partSecondaryFinish,
                Class = _partPrimaryClass.ToString(),
                AssemblyNumber = {
                    Prefix = _partSecondaryAssemblyPrefix, StartNumber = _partSecondaryAssemblyStartNumber
                },
                PartNumber = { Prefix = _partSecondaryPartPrefix, StartNumber = _partSecondaryPartStartNumber },
                Position = {
                    Depth = Position.DepthEnum.BEHIND,
                    Plane = Position.PlaneEnum.MIDDLE,
                    Rotation = Position.RotationEnum.TOP
                }
            };
            secondary1.Insert();

            var weld = new Weld {
                MainObject = primary,
                SecondaryObject = secondary1,
                TypeAbove = EnumParse<BaseWeld.WeldTypeEnum>(_weld2TypeAbove),
                TypeBelow = EnumParse<BaseWeld.WeldTypeEnum>(_weld2TypeBelow),
                SizeAbove = _weld2SizeAbove,
                SizeBelow = _weld2SizeBelow,
                AngleAbove = _weld2AngleAbove,
                AngleBelow = _weld2AngleBelow,
                ContourAbove = EnumParse<BaseWeld.WeldContourEnum>(_weld2ContourAbove),
                ContourBelow = EnumParse<BaseWeld.WeldContourEnum>(_weld2ContourBelow),
                FinishAbove = EnumParse<BaseWeld.WeldFinishEnum>(_weld2FinishAbove),
                FinishBelow = EnumParse<BaseWeld.WeldFinishEnum>(_weld2FinishBelow),
                RootFaceAbove = _weld2RootFaceAbove,
                RootFaceBelow = _weld2RootFaceBelow,
                EffectiveThroatAbove = _weld2EffectiveThroatAbove,
                EffectiveThroatBelow = _weld2EffectiveThroatBelow,
                RootOpeningAbove = _weld2RootOpeningAbove,
                RootOpeningBelow = _weld2RootOpeningBelow,
                IncrementAmountAbove = _weld2IncrementAmountAbove,
                IncrementAmountBelow = _weld2IncrementAmountBelow,
                LengthAbove = _weld2LengthAbove,
                LengthBelow = _weld2LengthBelow,
                PitchAbove = _weld2PitchAbove,
                PitchBelow = _weld2PitchBelow,
                //AroundWeld = _weld2Around != 0,
                AroundWeld = true,
                ShopWeld = _weld2Shop != 0,
                Placement = EnumParse<BaseWeld.WeldPlacementTypeEnum>(_weld2Placement),
                IntermittentType = EnumParse<BaseWeld.WeldIntermittentTypeEnum>(_weld2Intermittent),
                ReferenceText = _weld2ReferenceText,
            };

            weld.Insert();

            p1 = p2 + new Vector(_gap, 0, 0);
            p2 = basePoint + new Vector(SecondaryTotalLength, 0, 0);
            var secondary2 = new Beam(p1, p2) {
                Name = _partSecondaryName,
                Profile = { ProfileString = _partSecondaryProfile },
                Material = { MaterialString = _partSecondaryMaterial },
                Finish = _partSecondaryFinish,
                Class = _partSecondaryClass.ToString(),
                AssemblyNumber = {
                    Prefix = _partSecondaryAssemblyPrefix, StartNumber = _partSecondaryAssemblyStartNumber
                },
                PartNumber = { Prefix = _partSecondaryPartPrefix, StartNumber = _partSecondaryPartStartNumber },
                Position = {
                    Depth = Position.DepthEnum.BEHIND,
                    Plane = Position.PlaneEnum.MIDDLE,
                    Rotation = Position.RotationEnum.TOP
                }
            };
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
                weld = new Weld {
                    MainObject = primary,
                    SecondaryObject = stiffener,
                    TypeAbove = EnumParse<BaseWeld.WeldTypeEnum>(_weld1TypeAbove),
                    TypeBelow = EnumParse<BaseWeld.WeldTypeEnum>(_weld1TypeBelow),
                    SizeAbove = _weld1SizeAbove,
                    SizeBelow = _weld1SizeBelow,
                    AngleAbove = _weld1AngleAbove,
                    AngleBelow = _weld1AngleBelow,
                    ContourAbove = EnumParse<BaseWeld.WeldContourEnum>(_weld1ContourAbove),
                    ContourBelow = EnumParse<BaseWeld.WeldContourEnum>(_weld1ContourBelow),
                    FinishAbove = EnumParse<BaseWeld.WeldFinishEnum>(_weld1FinishAbove),
                    FinishBelow = EnumParse<BaseWeld.WeldFinishEnum>(_weld1FinishBelow),
                    RootFaceAbove = _weld1RootFaceAbove,
                    RootFaceBelow = _weld1RootFaceBelow,
                    EffectiveThroatAbove = _weld1EffectiveThroatAbove,
                    EffectiveThroatBelow = _weld1EffectiveThroatBelow,
                    RootOpeningAbove = _weld1RootOpeningAbove,
                    RootOpeningBelow = _weld1RootOpeningBelow,
                    IncrementAmountAbove = _weld1IncrementAmountAbove,
                    IncrementAmountBelow = _weld1IncrementAmountBelow,
                    LengthAbove = _weld1LengthAbove,
                    LengthBelow = _weld1LengthBelow,
                    PitchAbove = _weld1PitchAbove,
                    PitchBelow = _weld1PitchBelow,
                    AroundWeld = _weld1Around != 0,
                    ShopWeld = _weld1Shop != 0,
                    Placement = EnumParse<BaseWeld.WeldPlacementTypeEnum>(_weld1Placement),
                    IntermittentType = EnumParse<BaseWeld.WeldIntermittentTypeEnum>(_weld1Intermittent),
                    ReferenceText = _weld1ReferenceText,
                    Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X
                };
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

            bool secIsHProfileType = true;
            try {
                _ = new ProfileH(_partSecondaryProfile);
            } catch (UnAcceptableProfileException) {
                secIsHProfileType = false;
            }

            p1 = new Point(-_plateSpliceBreadth * 0.5, 0, 0);
            p2 = p1 + new Vector(_plateSpliceBreadth, 0, 0);

            var splice1 = new Beam(p1, p2) {
                Name = _plateSpliceName,
                Profile = { ProfileString = $"PL{_plateSpliceThickness}*{_plateSpliceHeight}" },
                Material = { MaterialString = _plateSpliceMaterial },
                Finish = _plateSpliceFinish,
                Class = _plateSpliceClass.ToString(),
                AssemblyNumber = { Prefix = _plateSpliceAssemblyPrefix, StartNumber = _plateSpliceAssemblyStartNumber },
                PartNumber = { Prefix = _plateSplicePartPrefix, StartNumber = _plateSplicePartStartNumber },
                Position = {
                    Rotation = Position.RotationEnum.TOP,
                    Depth = Position.DepthEnum.MIDDLE,
                    Plane = Position.PlaneEnum.RIGHT,
                    PlaneOffset = secIsHProfileType ? secWebThickness * 0.5 : secWidth * 0.5
                }
            };
            splice1.Insert();

            var splice2 = new Beam(p1, p2) {
                Name = _plateSpliceName,
                Profile = { ProfileString = $"PL{_plateSpliceThickness}*{_plateSpliceHeight}" },
                Material = { MaterialString = _plateSpliceMaterial },
                Finish = _plateSpliceFinish,
                Class = _plateSpliceClass.ToString(),
                AssemblyNumber = { Prefix = _plateSpliceAssemblyPrefix, StartNumber = _plateSpliceAssemblyStartNumber },
                PartNumber = { Prefix = _plateSplicePartPrefix, StartNumber = _plateSplicePartStartNumber },
                Position = {
                    Rotation = Position.RotationEnum.TOP,
                    Depth = Position.DepthEnum.MIDDLE,
                    Plane = Position.PlaneEnum.LEFT,
                    PlaneOffset = secIsHProfileType ? secWebThickness * 0.5 : secWidth * 0.5
                }
            };
            splice2.Insert();

            #endregion

            #region Bolt

            var boltDistListX = DistanceList.Parse(_boltConnectDistXText, CultureInfo.CurrentCulture,
                Distance.CurrentUnitType);
            var boltDistListY = DistanceList.Parse(_boltConnectDistYText, CultureInfo.CurrentCulture,
                Distance.CurrentUnitType);

            var bolt = new BoltArray {
                PartToBoltTo = secondary1,
                PartToBeBolted = splice1,
                OtherPartsToBolt = { splice2 },
                BoltSize = _boltConnectSize,
                BoltStandard = _boltConnectStandard,
                BoltType = EnumParse<BoltGroup.BoltTypeEnum>(_boltConnectType),
                ThreadInMaterial = EnumParse<BoltGroup.BoltThreadInMaterialEnum>(_boltConnectThreadInMaterial),
                CutLength = _boltConnectCutLength,
                ExtraLength = _boltConnectExtraLength,
                Tolerance = _boltConnectTolerance,
                PlainHoleType = EnumParse<BoltGroup.BoltPlainHoleTypeEnum>(_boltConnectPlainType),
                BlindHoleDepth = _boltConnectBlindHoleDepth,
                Hole1 = _boltConnectHole1 != 0,
                Hole2 = _boltConnectHole2 != 0,
                Hole3 = _boltConnectHole3 != 0,
                Hole4 = _boltConnectHole4 != 0,
                Hole5 = _boltConnectHole5 != 0,
                HoleType = EnumParse<BoltGroup.BoltHoleTypeEnum>(_boltConnectHoleType),
                SlottedHoleX = _boltConnectSlottedHoleX,
                SlottedHoleY = _boltConnectSlottedHoleY,
                RotateSlots = EnumParse<BoltGroup.BoltRotateSlotsEnum>(_boltConnectRotateSlots),
                Bolt = _boltConnectIsBolt != 0,
                Nut1 = _boltConnectUseNut1 != 0,
                Nut2 = _boltConnectUseNut2 != 0,
                Washer1 = _boltConnectUseWasher1 != 0,
                Washer2 = _boltConnectUseWasher2 != 0,
                Washer3 = _boltConnectUseWasher3 != 0,
                FirstPosition = new Point(),
                SecondPosition = new Point(-_plateSpliceBreadth * 0.5, 0, 0),
                Position = { Rotation = Position.RotationEnum.BELOW, },
                StartPointOffset = new Offset { Dx = _boltOffsetX }
            };
            foreach (var dist in boltDistListX) {
                bolt.AddBoltDistX(dist.Millimeters);
            }

            foreach (var dist in boltDistListY) {
                bolt.AddBoltDistY(dist.Millimeters);
            }

            bolt.Insert();

            bolt.PartToBoltTo = secondary2;
            bolt.SecondPosition.X *= -1;
            bolt.Position.Rotation = Position.RotationEnum.TOP;
            bolt.Insert();

            #endregion

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(originTp);

            #region Stud

            if (_creatStud == 0) goto DontCreatStud;

            var studDistX = DistanceList.Parse(_boltStudDistXText, CultureInfo.CurrentCulture,
                Distance.CurrentUnitType);
            var studDistY = DistanceList.Parse(_boltStudDistYText, CultureInfo.CurrentCulture,
                Distance.CurrentUnitType);

            var stud = new BoltArray {
                PartToBoltTo = secondary1,
                PartToBeBolted = secondary1,
                BoltSize = _boltStudSize,
                BoltStandard = _boltStudStandard,
                BoltType = EnumParse<BoltGroup.BoltTypeEnum>(_boltStudType),
                Length = _boltStudLength,
                CutLength = _boltStudCutLength,
                FirstPosition = secondary1.StartPoint,
                SecondPosition = secondary1.EndPoint,
                Position = { Rotation = Position.RotationEnum.FRONT },
                StartPointOffset = new Offset { Dx = _studOffsetX }
            };
            foreach (var dist in studDistX) {
                stud.AddBoltDistX(dist.Millimeters);
            }

            foreach (var dist in studDistY) {
                stud.AddBoltDistY(dist.Millimeters);
            }

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