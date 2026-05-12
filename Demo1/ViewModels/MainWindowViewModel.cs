using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Demo1.ViewModels {
    [PartPropertiesWithDefaultValues("Primary", profile: "HW502*470*20*25", @class: 4)]
    [PartPropertiesWithDefaultValues("Secondary", profile: "HM244*175*7*11", @class: 11)]
    [PlatePropertiesWithDefaultValues("Stiffener", thickness: 10)]
    [PlatePropertiesWithDefaultValues("Splice", thickness: 10, breadth: 300, height: 200)]
    [BoltPropertiesWithDefaultValues("Connect", distX: "70", distY: "2*80")]
    [BoltPropertiesWithDefaultValues("Stud", 10.0, "STUD", "4*90", "80")]
    [WeldPropertiesWithDefaultValues(1, 10, 10, 6.0, 6.0)]
    [WeldPropertiesWithDefaultValues(2, 10, sizeAbove: 6.0)]
    [GeneralPropertiesWithDefaultValues(typeof(Integer), "CreatStud", 1, "StifChamferType", 1)]
    [GeneralPropertiesWithDefaultValues(typeof(Double), "StifChamferX", 25.0, "StifChamferY", 25.0, "StifChamferDz1", 0.0, "StifChamferDz2", 0.0)]
    [GeneralPropertiesWithDefaultValues(typeof(Distance), "Gap", 15.0, "BoltOffsetX", 50, "StudOffsetX", 50)]
    public partial class MainWindowViewModel : NotificationObject {

    }
}