using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.Demo1.ViewModels {
    [PartProperties(1)]
    [PartPropertiesWithDefaultValues(1, profile: "HM244*175*7*11")]
    [PlatePropertiesWithDefaultValues(1, 14, breadth: 300.0, name: "Plate1", material: "Q235B")]
    [PlatePropertiesWithDefaultValues(2, 10, 200.0, 200, "Q345", "Plate2")]
    [PlatePropertiesWithDefaultValues("Stiffener", 10, 200.0, 200)]
    [BoltPropertiesWithDefaultValues(1, distX: "3*50", distY: "2*70", standard: "B")]
    [BoltCirclePropertiesWithDefaultValues(1)]
    [WeldPropertiesWithDefaultValues(1, 4, sizeAbove: 8.0, angleAbove: 15, rootFaceAbove: 2, preparation: 3)]
    public partial class MainWindowViewModel : NotificationObject {
        
    }
}