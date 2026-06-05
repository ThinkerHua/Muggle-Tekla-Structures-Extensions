using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Demo1.ViewModels {
    [PartProperties("Primary", profile: "HW502*470*20*25", Class = 4)]
    [PartProperties("Secondary", profile: "HM244*175*7*11", Class = 11)]
    [PlateProperties("Stiffener", thickness: 10)]
    [PlateProperties("Splice", thickness: 10, Breadth = 200, Height = 300)]
    [BoltProperties("Connect", distX: "70", distY: "2*80")]
    [BoltProperties("Stud", 10.0, "STUD", "4*90", "80")]
    [WeldProperties(1, 10, 10, 6.0, 6.0)]
    [WeldProperties(2, 10, sizeAbove: 6.0)]
    [GeneralProperties(typeof(Integer), "CreatStud", 1, "StifChamferType", 1)]
    [GeneralProperties(typeof(Double), "StifChamferX", 25.0, "StifChamferY", 25.0,
        "StifChamferDz1", 0.0, "StifChamferDz2", 0.0)]
    [GeneralProperties(typeof(Distance), "Gap", 15.0, "BoltOffsetX", 50, "StudOffsetX", 50)]
    public partial class MainWindowViewModel : NotificationObject {

    }
}