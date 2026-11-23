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
 *  PluginData.cs: data class for 'MG1002' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.MG1002;

[Plate("End", Name = "EndPlate", Thickness = 30, Breadth = 460, Height = 1090, Material = "Q235B")]
[Plate("FlangeStif", Name = "FlangeStiffener", Thickness = 10, Breadth = 115, Height = 175, Material = "Q235B")]
[Plate("WebStif", Name = "WebStiffener", Thickness = 10, Breadth = 225, Height = 225, Material = "Q235B")]
[Plate("Vertical", Name = "VerticalPlate", Thickness = 16, Material = "Q235B")]
[Plate("Diagonal", Name = "DiagonalPlate", Thickness = 8, Material = "Q235B")]
[Plate("SplicingWeb", Name = "SplicingWeb", Material = "Q235B")]
[Chamfer("Vertical", Type = 0, X = 25, Y = 25)]
[Chamfer("Diagonal", Type = 0, X = 25, Y = 25)]
[Chamfer("StifInside", Type = 0, X = 25, Y = 25)]
[Chamfer("StifOutside", Type = 0, X = 25, Y = 25)]
[Bolt(1, Standard = "HS10.9", Size = 24, DistX = "135 3*220 135", DistY = "200")]
[Weld(1, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 8, TypeBelow = 10, SizeBelow = 8, Shop = 1)]
[Weld(3, TypeAbove = 4, SizeAbove = 10, AngleAbove = 15, RootFaceAbove = 2, RootOpeningAbove = 2, Placement = 1, Preparation = 2, Shop = 1)]
[Weld(4, TypeAbove = 4, SizeAbove = 10, AngleAbove = 15, RootFaceAbove = 2, RootOpeningAbove = 2, Placement = 1, Preparation = 3, Shop = 1)]
[Weld(5, TypeAbove = 5, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 2, RootOpeningAbove = 2, Preparation = 1, Shop = 1)]
[Weld(6, TypeAbove = 10, SizeAbove = 8, TypeBelow = 10, SizeBelow = 8, Shop = 1)]
[Weld(7, TypeAbove = 10, SizeAbove = 8, TypeBelow = 10, SizeBelow = 8, Shop = 1)]
[Weld(8, TypeAbove = 10, SizeAbove = 6, TypeBelow = 10, SizeBelow = 6, Shop = 1)]
[General(typeof(int), "FlangeStifKind", 0, "WebStifKind", 0)]
[General(typeof(double), "DiagonalPos1", 100, "DiagonalPos2", 100, "DistWebFlangeSeam", 200, "BoltStartOffsetX", 70)]
[General(typeof(string), "WebStifDistList", "180 2*250")]
public partial class PluginData;