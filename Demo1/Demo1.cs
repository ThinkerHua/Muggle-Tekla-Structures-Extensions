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
using Win = System.Windows;

namespace Muggle.TsExtensions.Demo1 {

    [PartFields(1)]
    [PlateFields([1, 2])]
    [BoltFields(1)]
    [BoltCircleFields(1)]
    [WeldFields(1)]
    public partial class PluginData {

        [StructuresField("UselessAttribute")]
        public int UselessAttribute;

    }

    [Plugin("Demo1")]
    [PluginUserInterface("Muggle.TsExtensions.Demo1.Views.MainWindow")]
    [InputObjectDependency(InputObjectDependency.DEPENDENT)]
    [PluginCoordinateSystem(CoordinateSystemType.FROM_FIRST_POINT_AND_GLOBAL)]
    [FieldsFrom(typeof(PluginData))]
    public partial class Demo1 : PluginBase {

        public Model Model { get; set; }

        [PartFieldDefaultValues(1, name: "Part1", profile: "HM244*175*7*11", material: "Q235", finish: "", @class: 1,
            partPrefix: "P", partStartNumber: 1, assemblyPrefix: "A", assemblyStartNumber: 1)]
        [PlateFieldDefaultValues(1, name: "EndPlate1", breadth: 300, thickness: 14, material: "Q235",
            finish: "", @class: 1, partPrefix: "P", partStartNumber: 1, assemblyPrefix: "A", assemblyStartNumber: 1)]
        [PlateFieldDefaultValues(2, name: "EndPlate2", breadth: 200, thickness: 10, height: 200, material: "Q235",
            finish: "", @class: 1, partPrefix: "P", partStartNumber: 1, assemblyPrefix: "A", assemblyStartNumber: 1)]
        [WeldFieldDefaultValues(1, typeAbove: 6, sizeAbove: 8, angleAbove: 15, rootFaceAbove: 2, around: 0,
            preparation: 3)]
        [BoltFieldDefaultValues(1, size: 20, standard: "HS10.9", distXText: "3*50", distYText: "2*70")]
        [BoltCircleFieldDefaultValues(1, size: 20, standard: "HS10.9", numberOfBolts: 6, diameter: 120)]
        public PluginData Data { get; set; }

        public Demo1(PluginData data) {
            Model = new Model();
            Data = data;
            SetDataToDefaultIfUnset();
            GetFieldValuesFrom(data);
        }

        public override List<InputDefinition> DefineInput() {
            try {
                var picker = new Picker();
                var point1 = picker.PickPoint();
                var point2 = picker.PickPoint(point1);
                return [new InputDefinition(point1), new InputDefinition(point2)];
            } catch (Exception e) when (e.Message != "User interrupt") {
                Win.MessageBox.Show(e.Message, "Error", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
                return [];
            }
        }

        public override bool Run(List<InputDefinition> input) {
            try {

                var point1 = input[0].GetInput() as Point;
                var point2 = input[1].GetInput() as Point;

                var v = new Vector(point2 - point1);

                var plate1 = new Beam(point1, point2) {
                    Name = "Plate1",
                    Profile = new Profile { ProfileString = $"PL{_plate1Thickness}*{_plate1Breadth}" },
                    Material = new Material { MaterialString = _plate1Material },
                    AssemblyNumber = new NumberingSeries(_plate1AssemblyPrefix, _plate1AssemblyStartNumber),
                    PartNumber = new NumberingSeries(_plate1PartPrefix, _plate1PartStartNumber),
                    Class = _plate1Class.ToString(),
                    Position = new Position {
                        Plane = Position.PlaneEnum.MIDDLE,
                        Rotation = Position.RotationEnum.FRONT,
                        Depth = Position.DepthEnum.BEHIND
                    }
                };

                v.Normalize(_plate2Height);
                var plate2 = new Beam(point1, point1 + v) {
                    Name = "Plate2",
                    Profile = new Profile { ProfileString = $"PL{_plate2Thickness}*{_plate2Breadth}" },
                    Material = new Material { MaterialString = _plate2Material },
                    AssemblyNumber = new NumberingSeries(_plate2AssemblyPrefix, _plate2AssemblyStartNumber),
                    PartNumber = new NumberingSeries(_plate2PartPrefix, _plate2PartStartNumber),
                    Class = _plate2Class.ToString(),
                    Position = new Position {
                        Plane = Position.PlaneEnum.MIDDLE,
                        Rotation = Position.RotationEnum.BACK,
                        Depth = Position.DepthEnum.FRONT
                    }
                };
                plate1.Insert();
                plate2.Insert();

                var distX = DistanceList.Parse(_bolt1DistXText, CultureInfo.CurrentCulture, Distance.CurrentUnitType);
                var distY = DistanceList.Parse(_bolt1DistYText, CultureInfo.CurrentCulture, Distance.CurrentUnitType);
                var bolt1 = new BoltArray() {
                    PartToBoltTo = plate1,
                    PartToBeBolted = plate2,
                    FirstPosition = point1,
                    SecondPosition = point2,
                    StartPointOffset = new Offset { Dx = 50 },
                    BoltStandard = _bolt1Standard,
                    BoltSize = _bolt1Size,
                    BoltType = EnumParse<BoltGroup.BoltTypeEnum>(_bolt1Type),
                    Tolerance = _bolt1Tolerance,
                    CutLength = _bolt1CutLength,
                    ExtraLength = _bolt1ExtraLength,
                    Bolt = _bolt1IsBolt == 1,
                    Hole1 = _bolt1Hole1 == 1,
                    Hole2 = _bolt1Hole2 == 1,
                    Hole3 = _bolt1Hole3 == 1,
                    Hole4 = _bolt1Hole4 == 1,
                    Hole5 = _bolt1Hole5 == 1,
                    HoleType = EnumParse<BoltGroup.BoltHoleTypeEnum>(_bolt1HoleType),
                    PlainHoleType = EnumParse<BoltGroup.BoltPlainHoleTypeEnum>(_bolt1PlainType),
                    BlindHoleDepth = _bolt1BlindHoleDepth,
                    RotateSlots = EnumParse<BoltGroup.BoltRotateSlotsEnum>(_bolt1RotateSlots),
                    SlottedHoleX = _bolt1SlottedHoleX,
                    SlottedHoleY = _bolt1SlottedHoleY,
                    ThreadInMaterial = EnumParse<BoltGroup.BoltThreadInMaterialEnum>(_bolt1ThreadInMaterial),
                    Washer1 = _bolt1UseWasher1 == 1,
                    Washer2 = _bolt1UseWasher2 == 1,
                    Washer3 = _bolt1UseWasher3 == 1,
                    Nut1 = _bolt1UseNut1 == 1,
                    Nut2 = _bolt1UseNut2 == 1
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
                    AroundWeld = _weld1Around == 1,
                    ShopWeld = _weld1Shop == 1,
                    Preparation = EnumParse<BaseWeld.WeldPreparationTypeEnum>(_weld1Preparation),
                    Placement = EnumParse<BaseWeld.WeldPlacementTypeEnum>(_weld1Placement),
                    IntermittentType = EnumParse<BaseWeld.WeldIntermittentTypeEnum>(_weld1Intermittent),
                    TypeAbove = EnumParse<BaseWeld.WeldTypeEnum>(_weld1TypeAbove),
                    TypeBelow = EnumParse<BaseWeld.WeldTypeEnum>(_weld1TypeBelow),
                    SizeAbove = _weld1SizeAbove,
                    SizeBelow = _weld1SizeBelow,
                    AngleAbove = _weld1AngleAbove,
                    AngleBelow = _weld1AngleBelow,
                    RootFaceAbove = _weld1RootFaceAbove,
                    RootFaceBelow = _weld1RootFaceBelow,
                    RootOpeningAbove = _weld1RootOpeningAbove,
                    RootOpeningBelow = _weld1RootOpeningBelow,
                    ContourBelow = EnumParse<BaseWeld.WeldContourEnum>(_weld1ContourBelow),
                    ContourAbove = EnumParse<BaseWeld.WeldContourEnum>(_weld1ContourAbove),
                    EffectiveThroatAbove = _weld1EffectiveThroatAbove,
                    EffectiveThroatBelow = _weld1EffectiveThroatBelow,
                    FinishAbove = EnumParse<BaseWeld.WeldFinishEnum>(_weld1FinishAbove),
                    FinishBelow = EnumParse<BaseWeld.WeldFinishEnum>(_weld1FinishBelow),
                    IncrementAmountAbove = _weld1IncrementAmountAbove,
                    IncrementAmountBelow = _weld1IncrementAmountBelow,
                    LengthAbove = _weld1LengthAbove,
                    LengthBelow = _weld1LengthBelow,
                    PitchAbove = _weld1PitchAbove,
                    PitchBelow = _weld1PitchBelow,
                    ReferenceText = _weld1ReferenceText,
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
            } catch (Exception e) {
                Win.MessageBox.Show(e.Message, "Error", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
                return false;
            }
        }

        private T EnumParse<T>(string value) where T : Enum {
            T result;
            try {
                result = (T)Enum.Parse(typeof(T), value);
            } catch (Exception e) when (e is ArgumentException || e is OverflowException) {
                result = default;
            }

            return result;
        }

        private T EnumParse<T>(int value) where T : Enum {
            return EnumParse<T>(value.ToString());
        }
    }
}
