using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Plugins;

namespace Muggle.TsExtensions.Demo1;

[PartFields("Primary", "Secondary")]
[PlateFields("Stiffener", "Splice")]
[BoltFields("Connect", "Stud")]
[WeldFields(1, 2)]
[GeneralFields(typeof(int), "CreatStud", "StifChamferType")]
[GeneralFields(typeof(double), "Gap", "BoltOffsetX", "StudOffsetX", "StifChamferX", "StifChamferY", "StifChamferDz1", "StifChamferDz2")]
public partial class PluginData {
    [StructuresField("UselessAttribute")] public int UselessAttribute;
}