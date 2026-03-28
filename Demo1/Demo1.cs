using System;
using System.Collections.Generic;
using System.Globalization;
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Distance = Tekla.Structures.Datatype.Distance;

namespace Muggle.TsExtensions.Demo1;

[PlateFields(1, 2)]
[BoltFields(1)]
[WeldFields(1)]
public partial class PluginData {
}

[Plugin("Demo1")]
[PluginUserInterface("Muggle.TsExtensions.Demo1.Views.MainWindow")]
public class Demo1 : PluginBase {
    public Model Model { get; set; }

    public PluginData Data {
        get;
        set {
            field = value;
            GetValuesFromDialog();
        }
    }

    public Demo1(PluginData data) {
        Model = new Model();
        Data = data;
    }

    public override List<InputDefinition> DefineInput() {
        var picker = new Picker();
        var point1 = picker.PickPoint();
        var point2 = picker.PickPoint(point1);
        return [new InputDefinition(point1), new InputDefinition(point2)];
    }

    public override bool Run(List<InputDefinition> input) {
        var point1 = input[0].GetInput() as Point;
        var point2 = input[1].GetInput() as Point;
        var v = new Vector(point2 - point1);

        var plate1 = new Beam(point1, point2) {
            Name = "Plate1",
            Profile = new Profile { ProfileString = $"PL{Data.Plate1Thickness}*{Data.Plate1Breadth}" },
            Material = new Material { MaterialString = Data.Plate1Material },
            AssemblyNumber = new NumberingSeries(Data.Plate1AssemblyPrefix, Data.Plate1AssemblyStartNumber),
            PartNumber = new NumberingSeries(Data.Plate1PartPrefix, Data.Plate1PartStartNumber),
            Class = Data.Plate1Class.ToString(),
            Position = new Position {
                Plane = Position.PlaneEnum.MIDDLE,
                Rotation = Position.RotationEnum.FRONT,
                Depth = Position.DepthEnum.BEHIND
            }
        };

        v.Normalize(Data.Plate2Height);
        var plate2 = new Beam(point1, point1 + v) {
            Name = "Plate2",
            Profile = new Profile { ProfileString = $"PL{Data.Plate2Thickness}*{Data.Plate2Breadth}" },
            Material = new Material { MaterialString = Data.Plate2Material },
            AssemblyNumber = new NumberingSeries(Data.Plate2AssemblyPrefix, Data.Plate2AssemblyStartNumber),
            PartNumber = new NumberingSeries(Data.Plate2PartPrefix, Data.Plate2PartStartNumber),
            Class = Data.Plate2Class.ToString(),
            Position = new Position {
                Plane = Position.PlaneEnum.MIDDLE,
                Rotation = Position.RotationEnum.BACK,
                Depth = Position.DepthEnum.FRONT
            }
        };
        plate1.Insert();
        plate2.Insert();

        var distX = DistanceList.Parse(Data.Bolt1DistXText, CultureInfo.CurrentCulture, Distance.CurrentUnitType);
        var distY = DistanceList.Parse(Data.Bolt1DistYText, CultureInfo.CurrentCulture, Distance.CurrentUnitType);
        var bolt1 = new BoltArray() {
            PartToBoltTo = plate1,
            PartToBeBolted = plate2,
            FirstPosition = point1,
            SecondPosition = point2,
            StartPointOffset = new Offset { Dx = 50 },
            BoltStandard = Data.Bolt1Standard,
            BoltSize = Data.Bolt1Size,
            BoltType = EnumParse<BoltGroup.BoltTypeEnum>(Data.Bolt1Type),
            Tolerance = Data.Bolt1Tolerance,
            CutLength = Data.Bolt1CutLength,
            ExtraLength = Data.Bolt1ExtraLength,
            Bolt = Data.Bolt1IsBolt == 1,
            Hole1 = Data.Bolt1Hole1 == 1,
            Hole2 = Data.Bolt1Hole2 == 1,
            Hole3 = Data.Bolt1Hole3 == 1,
            Hole4 = Data.Bolt1Hole4 == 1,
            Hole5 = Data.Bolt1Hole5 == 1,
            HoleType = EnumParse<BoltGroup.BoltHoleTypeEnum>(Data.Bolt1HoleType),
            PlainHoleType = EnumParse<BoltGroup.BoltPlainHoleTypeEnum>(Data.Bolt1PlainType),
            BlindHoleDepth = Data.Bolt1BlindHoleDepth,
            RotateSlots = EnumParse<BoltGroup.BoltRotateSlotsEnum>(Data.Bolt1RotateSlots),
            SlottedHoleX = Data.Bolt1SlottedHoleX,
            SlottedHoleY = Data.Bolt1SlottedHoleY,
            ThreadInMaterial = EnumParse<BoltGroup.BoltThreadInMaterialEnum>(Data.Bolt1ThreadInMaterial),
            Washer1 = Data.Bolt1UseWasher1 == 1,
            Washer2 = Data.Bolt1UseWasher2 == 1,
            Washer3 = Data.Bolt1UseWasher3 == 1,
            Nut1 = Data.Bolt1UseNut1 == 1,
            Nut2 = Data.Bolt1UseNut2 == 1
        };
        foreach (var distance in distX) {
            bolt1.AddBoltDistX(distance.Value);
        }

        foreach (var distance in distY) {
            bolt1.AddBoltDistY(distance.Value);
        }

        bolt1.Insert();

        var weld1 = new Weld() {
            MainObject = plate1,
            SecondaryObject = plate2,
            AroundWeld = Data.Weld1Around == 1,
            ShopWeld = Data.Weld1Shop == 1,
            Preparation = EnumParse<BaseWeld.WeldPreparationTypeEnum>(Data.Weld1Preparation),
            Placement = EnumParse<BaseWeld.WeldPlacementTypeEnum>(Data.Weld1Placement),
            IntermittentType = EnumParse<BaseWeld.WeldIntermittentTypeEnum>(Data.Weld1Intermittent),
            TypeAbove = EnumParse<BaseWeld.WeldTypeEnum>(Data.Weld1TypeAbove),
            TypeBelow = EnumParse<BaseWeld.WeldTypeEnum>(Data.Weld1TypeBelow),
            SizeAbove = Data.Weld1SizeAbove,
            SizeBelow = Data.Weld1SizeBelow,
            AngleAbove = Data.Weld1AngleAbove,
            AngleBelow = Data.Weld1AngleBelow,
            RootFaceAbove = Data.Weld1RootFaceAbove,
            RootFaceBelow = Data.Weld1RootFaceBelow,
            RootOpeningAbove = Data.Weld1RootOpeningAbove,
            RootOpeningBelow = Data.Weld1RootOpeningBelow,
            ContourBelow = EnumParse<BaseWeld.WeldContourEnum>(Data.Weld1ContourBelow),
            ContourAbove = EnumParse<BaseWeld.WeldContourEnum>(Data.Weld1ContourAbove),
            EffectiveThroatAbove = Data.Weld1EffectiveThroatAbove,
            EffectiveThroatBelow = Data.Weld1EffectiveThroatBelow,
            FinishAbove = EnumParse<BaseWeld.WeldFinishEnum>(Data.Weld1FinishAbove),
            FinishBelow = EnumParse<BaseWeld.WeldFinishEnum>(Data.Weld1FinishBelow),
            IncrementAmountAbove = Data.Weld1IncrementAmountAbove,
            IncrementAmountBelow = Data.Weld1IncrementAmountBelow,
            LengthAbove = Data.Weld1LengthAbove,
            LengthBelow = Data.Weld1LengthBelow,
            PitchAbove = Data.Weld1PitchAbove,
            PitchBelow = Data.Weld1PitchBelow,
            ReferenceText = Data.Weld1ReferenceText,
        };
        weld1.Insert();

        Vector y;
        var z = new Vector(0, 0, 1);
        if (Parallel.VectorToVector(z, v)) {
            y = new Vector(1, 0, 0);
        } else {
            y = z.Cross(v);
        }
        var tp = new TransformationPlane(point1, v, y);
        Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(tp);
        weld1 = Model.SelectModelObject(weld1.Identifier) as Weld;
        weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld1.Modify();

        return true;
    }

    private void GetValuesFromDialog() {
        if (IsDefaultValue(Data.Plate1Name)) Data.Plate1Name = "EndPlate";
        if (IsDefaultValue(Data.Plate1Thickness)) Data.Plate1Thickness = 10;
        if (IsDefaultValue(Data.Plate1Breadth)) Data.Plate1Breadth = 300;
        if (IsDefaultValue(Data.Plate1Height)) Data.Plate1Height = 500;
        if (IsDefaultValue(Data.Plate1Material)) Data.Plate1Material = "Q235B";
        if (IsDefaultValue(Data.Plate1Finish)) Data.Plate1Finish = string.Empty;
        if (IsDefaultValue(Data.Plate1Class)) Data.Plate1Class = 99;
        if (IsDefaultValue(Data.Plate1AssemblyPrefix)) Data.Plate1AssemblyPrefix = "GL-";
        if (IsDefaultValue(Data.Plate1AssemblyStartNumber)) Data.Plate1AssemblyStartNumber = 1;
        if (IsDefaultValue(Data.Plate1PartPrefix)) Data.Plate1PartPrefix = "P";
        if (IsDefaultValue(Data.Plate1PartStartNumber)) Data.Plate1PartStartNumber = 1;

        if (IsDefaultValue(Data.Plate2Name)) Data.Plate2Name = "EndPlate";
        if (IsDefaultValue(Data.Plate2Thickness)) Data.Plate2Thickness = 10;
        if (IsDefaultValue(Data.Plate2Breadth)) Data.Plate2Breadth = 300;
        if (IsDefaultValue(Data.Plate2Height)) Data.Plate2Height = 500;
        if (IsDefaultValue(Data.Plate2Material)) Data.Plate2Material = "Q235B";
        if (IsDefaultValue(Data.Plate2Finish)) Data.Plate2Finish = string.Empty;
        if (IsDefaultValue(Data.Plate2Class)) Data.Plate2Class = 99;
        if (IsDefaultValue(Data.Plate2AssemblyPrefix)) Data.Plate2AssemblyPrefix = "GL-";
        if (IsDefaultValue(Data.Plate2AssemblyStartNumber)) Data.Plate2AssemblyStartNumber = 1;
        if (IsDefaultValue(Data.Plate2PartPrefix)) Data.Plate2PartPrefix = "P";
        if (IsDefaultValue(Data.Plate2PartStartNumber)) Data.Plate2PartStartNumber = 1;

        if (IsDefaultValue(Data.Bolt1Size)) Data.Bolt1Size = 8.0;
        if (IsDefaultValue(Data.Bolt1Standard)) Data.Bolt1Standard = "A";
        if (IsDefaultValue(Data.Bolt1DistXText)) Data.Bolt1DistXText = "3*50";
        if (IsDefaultValue(Data.Bolt1DistYText)) Data.Bolt1DistYText = "2*70";
        if (IsDefaultValue(Data.Bolt1Type)) Data.Bolt1Type = 0;
        if (IsDefaultValue(Data.Bolt1ThreadInMaterial)) Data.Bolt1ThreadInMaterial = 1;
        if (IsDefaultValue(Data.Bolt1CutLength)) Data.Bolt1CutLength = 100.0;
        if (IsDefaultValue(Data.Bolt1ExtraLength)) Data.Bolt1ExtraLength = 0.0;
        if (IsDefaultValue(Data.Bolt1Tolerance)) Data.Bolt1Tolerance = 2.0;
        if (IsDefaultValue(Data.Bolt1PlainType)) Data.Bolt1PlainType = 0;
        if (IsDefaultValue(Data.Bolt1BlindHoleDepth)) Data.Bolt1BlindHoleDepth = 0.0;
        if (IsDefaultValue(Data.Bolt1Hole1)) Data.Bolt1Hole1 = 1;
        if (IsDefaultValue(Data.Bolt1Hole2)) Data.Bolt1Hole2 = 1;
        if (IsDefaultValue(Data.Bolt1Hole3)) Data.Bolt1Hole3 = 0;
        if (IsDefaultValue(Data.Bolt1Hole4)) Data.Bolt1Hole4 = 0;
        if (IsDefaultValue(Data.Bolt1Hole5)) Data.Bolt1Hole5 = 0;
        if (IsDefaultValue(Data.Bolt1HoleType)) Data.Bolt1HoleType = 0;
        if (IsDefaultValue(Data.Bolt1SlottedHoleX)) Data.Bolt1SlottedHoleX = 0.0;
        if (IsDefaultValue(Data.Bolt1SlottedHoleY)) Data.Bolt1SlottedHoleY = 8.0;
        if (IsDefaultValue(Data.Bolt1RotateSlots)) Data.Bolt1RotateSlots = 2;
        if (IsDefaultValue(Data.Bolt1IsBolt)) Data.Bolt1IsBolt = 1;
        if (IsDefaultValue(Data.Bolt1UseNut1)) Data.Bolt1UseNut1 = 1;
        if (IsDefaultValue(Data.Bolt1UseNut2)) Data.Bolt1UseNut2 = 0;
        if (IsDefaultValue(Data.Bolt1UseWasher1)) Data.Bolt1UseWasher1 = 0;
        if (IsDefaultValue(Data.Bolt1UseWasher2)) Data.Bolt1UseWasher2 = 0;
        if (IsDefaultValue(Data.Bolt1UseWasher3)) Data.Bolt1UseWasher3 = 1;

        if (IsDefaultValue(Data.Weld1SizeAbove)) Data.Weld1SizeAbove = 6.0;
        if (IsDefaultValue(Data.Weld1SizeBelow)) Data.Weld1SizeBelow = 0.0;
        if (IsDefaultValue(Data.Weld1TypeAbove)) Data.Weld1TypeAbove = 10;
        if (IsDefaultValue(Data.Weld1TypeBelow)) Data.Weld1TypeBelow = 0;
        if (IsDefaultValue(Data.Weld1AngleAbove)) Data.Weld1AngleAbove = 15.0;
        if (IsDefaultValue(Data.Weld1AngleBelow)) Data.Weld1AngleBelow = 0.0;
        if (IsDefaultValue(Data.Weld1ContourAbove)) Data.Weld1ContourAbove = 0;
        if (IsDefaultValue(Data.Weld1ContourBelow)) Data.Weld1ContourBelow = 0;
        if (IsDefaultValue(Data.Weld1FinishAbove)) Data.Weld1FinishAbove = 0;
        if (IsDefaultValue(Data.Weld1FinishBelow)) Data.Weld1FinishBelow = 0;
        if (IsDefaultValue(Data.Weld1RootFaceAbove)) Data.Weld1RootFaceAbove = 2;
        if (IsDefaultValue(Data.Weld1RootFaceBelow)) Data.Weld1RootFaceBelow = 0;
        if (IsDefaultValue(Data.Weld1EffectiveThroatAbove)) Data.Weld1EffectiveThroatAbove = 0;
        if (IsDefaultValue(Data.Weld1EffectiveThroatBelow)) Data.Weld1EffectiveThroatBelow = 0;
        if (IsDefaultValue(Data.Weld1RootOpeningAbove)) Data.Weld1RootOpeningAbove = 0;
        if (IsDefaultValue(Data.Weld1RootOpeningBelow)) Data.Weld1RootOpeningBelow = 0;
        if (IsDefaultValue(Data.Weld1IncrementAmountAbove)) Data.Weld1IncrementAmountAbove = 0;
        if (IsDefaultValue(Data.Weld1IncrementAmountBelow)) Data.Weld1IncrementAmountBelow = 0;
        if (IsDefaultValue(Data.Weld1LengthAbove)) Data.Weld1LengthAbove = 15;
        if (IsDefaultValue(Data.Weld1LengthBelow)) Data.Weld1LengthBelow = 0;
        if (IsDefaultValue(Data.Weld1PitchAbove)) Data.Weld1PitchAbove = 0;
        if (IsDefaultValue(Data.Weld1PitchBelow)) Data.Weld1PitchBelow = 0;
        if (IsDefaultValue(Data.Weld1Around)) Data.Weld1Around = 0;
        if (IsDefaultValue(Data.Weld1Shop)) Data.Weld1Shop = 0;
        if (IsDefaultValue(Data.Weld1Placement)) Data.Weld1Placement = 0;
        if (IsDefaultValue(Data.Weld1Preparation)) Data.Weld1Preparation = 1;
        if (IsDefaultValue(Data.Weld1Intermittent)) Data.Weld1Intermittent = 0;
        if (IsDefaultValue(Data.Weld1ReferenceText)) Data.Weld1ReferenceText = string.Empty;
    }

    private T EnumParse<T>(string value) where T : Enum {
        T result;
        try {
            result = (T)Enum.Parse(typeof(T), value);
        } catch (Exception e) when (e is ArgumentException or OverflowException) {
            result = default;
        }

        return result;
    }

    private T EnumParse<T>(int value) where T : Enum {
        return EnumParse<T>(value.ToString());
    }
}