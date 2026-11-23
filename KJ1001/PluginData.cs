/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  PluginData.cs: data class for 'KJ1001' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.KJ1001;

[Plate("PrimStif", Thickness = 10, Material = "Q235B")]
[Plate("Thickened")]
[Plate("TopCover", Thickness = 8, Breadth = 260, Height = 400, Material = "Q235B")]
[Plate("BotCover", Thickness = 8, Breadth = 320, Height = 400, Material = "Q235B")]
[Plate("WebSplice", Thickness = 16, Breadth = 350, Height = 300, Material = "Q235B")]
[Chamfer("PrimStif", Type = 1, X = 25, Y = 25)]
[Chamfer("Beam", Type = 0, X = 25, Y = 25)]
[Chamfer("TopCover", Type = 1, X = 150, Y = 50)]
[Chamfer("BotCover", Type = 1, X = 150, Y = 75)]
[Bolt("Web", Size = 20, Standard = "HS10.9", DistX = "3*70", DistY = "70 100 70")]
[Weld(2, TypeAbove = 0, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2)]
[Weld(3, TypeAbove = 10, SizeAbove = 8)]
[Weld(4, TypeAbove = 6, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2)]
[Weld(5, TypeAbove = 6, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2, Placement = 1)]
[Weld(6, TypeAbove = 10, SizeAbove = 8, Around = 1)]
[Weld(7, TypeAbove = 10, SizeAbove = 8)]
[Weld(8, TypeAbove = 6, SizeAbove = 10, AngleAbove = 45, RootOpeningAbove = 8)]
// 0 - 仅前面
// 1 - 仅后面
// 2 - 前后
[General(typeof(int), "SpliceCreation", 2)]
[General(typeof(double), "ExtraLen", 150, "Gap", 15)]
[General(typeof(string), "MoreStifDisList", "")]
public partial class PluginData;