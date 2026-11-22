using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Plugins;

namespace Muggle.TsExtensions.Demo1;

[Part("Primary", Profile = "HW502*470*20*25", Class = 4)]
[Part("Secondary", Profile = "HM244*175*7*11", Class = 11)]
[Plate("Stiffener", Thickness = 10)]
[Plate("Splice", Thickness = 10, Breadth = 200, Height = 300)]
[Bolt(1, DistX = "70", DistY = "2*80")]
[Stud(1, DistX = "4*90", DistY = "80")]
[Weld(1, TypeAbove = 10, SizeAbove = 6.0)]
[Weld(2, TypeAbove = 10, TypeBelow = 10, SizeAbove = 6.0, SizeBelow = 6.0)]
[Chamfer("Stif", Type = 1, X = 15.0, Y = 15.0)]
[General(typeof(int), "CreateStud", 1)]
[General(typeof(double), "Gap", 15.0, "BoltOffsetX", 50, "StudOffsetX", 50)]
public partial class PluginData {
    [StructuresField("UselessAttribute")] public int UselessAttribute;
    [StructuresField("UselessAttribute2")] public int UselessAttribute2;
    [StructuresField("UselessAttribute3")] public double UselessAttribute3;
}