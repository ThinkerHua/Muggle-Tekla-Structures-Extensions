using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Demo1.ViewModels;

[PlateProperties(1, 2)]
[BoltProperties(1)]
[WeldProperties(1)]
public partial class MainWindowViewModel : NotificationObject {
    public MainWindowViewModel() {
        Plate1Thickness = 14;
        Plate1Breadth = 300.0;
        Plate1Height = 500;
        Plate1Name = "Plate1";
        Plate1Material = "Q235B";
        Plate1Finish = "";
        Plate1Class = 99;
        Plate1PartPrefix = "P";
        Plate1PartStartNumber = 1;
        Plate1AssemblyPrefix = "A-";
        Plate1AssemblyStartNumber = 1;

        Plate2Thickness = 10;
        Plate2Breadth = 300.0;
        Plate2Height = 500;
        Plate2Name = "Plate2";
        Plate2Material = "Q345";
        Plate2Finish = "";
        Plate2Class = 99;
        Plate2PartPrefix = "P";
        Plate2PartStartNumber = 1;
        Plate2AssemblyPrefix = "A-";
        Plate2AssemblyStartNumber = 1;

        Bolt1Size = new TD.Distance(8.0);
        Bolt1Standard = "A";
        Bolt1DistX = new TD.DistanceList(Enumerable.Repeat(new TD.Distance(50.0), 3));
        Bolt1DistY = new TD.DistanceList(Enumerable.Repeat(new TD.Distance(70.0), 2));
        Bolt1Type = 0;
        Bolt1ThreadInMaterial = 1;
        Bolt1CutLength = 100.0;
        Bolt1ExtraLength = 0.0;
        Bolt1Tolerance = 2.0;
        Bolt1PlainType = 0;
        Bolt1BlindHoleDepth = 0.0;
        Bolt1Hole1 = 1;
        Bolt1Hole2 = 1;
        Bolt1Hole3 = 0;
        Bolt1Hole4 = 0;
        Bolt1Hole5 = 0;
        Bolt1HoleType = 0;
        Bolt1SlottedHoleX = 0.0;
        Bolt1SlottedHoleY = 8.0;
        Bolt1RotateSlots = 2;
        Bolt1IsBolt = 1;
        Bolt1UseNut1 = 1;
        Bolt1UseNut2 = 0;
        Bolt1UseWasher1 = 0;
        Bolt1UseWasher2 = 0;
        Bolt1UseWasher3 = 1;

        Weld1SizeAbove = 6.0;
        Weld1SizeBelow = 0.0;
        Weld1TypeAbove = 10;
        Weld1TypeBelow = 0;
        Weld1AngleAbove = 15.0;
        Weld1AngleBelow = 0.0;
        Weld1ContourAbove = 0;
        Weld1ContourBelow = 0;
        Weld1FinishAbove = 0;
        Weld1FinishBelow = 0;
        Weld1RootFaceAbove = 2;
        Weld1RootFaceBelow = 0;
        Weld1EffectiveThroatAbove = 0;
        Weld1EffectiveThroatBelow = 0;
        Weld1RootOpeningAbove = 0;
        Weld1RootOpeningBelow = 0;
        Weld1IncrementAmountAbove = 0;
        Weld1IncrementAmountBelow = 0;
        Weld1LengthAbove = 0;
        Weld1LengthBelow = 0;
        Weld1PitchAbove = 0;
        Weld1PitchBelow = 0;
        Weld1Around = 0;
        Weld1Shop = 0;
        Weld1Placement = 0;
        Weld1Preparation = 1;
        Weld1Intermittent = 0;
        Weld1ReferenceText = string.Empty;
    }
}