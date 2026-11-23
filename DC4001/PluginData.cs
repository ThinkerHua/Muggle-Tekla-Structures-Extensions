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
 *  PluginData.cs: data class for 'DC4001' custom part
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.DC4001;

[Part("main", Name = "Main", Material = "Q345B")]
[Plate(4, Thickness = 16, Breadth = 250, Name = "4")]
[Plate(5, Thickness = 8, Breadth = 90, Name = "5")]
[Plate(10, Thickness = 10, Breadth = 120, Name = "10")]
[Plate(11, Thickness = 10, Breadth = 120, Name = "11")]
[Plate(12, Thickness = 20, Breadth = 90, Height = 340, Name = "12")]
[Plate(13, Thickness = 10, Breadth = 120, Name = "13")]
[Weld("4_1", TypeAbove = 10, TypeBelow = 10, SizeAbove = 8, SizeBelow = 8, Shop = 1)]
[Weld("4_2", TypeAbove = 10, TypeBelow = 10, SizeAbove = 10, SizeBelow = 10, Shop = 1)]
[Weld("4_3", TypeAbove = 10, TypeBelow = 10, SizeAbove = 10, SizeBelow = 10, Shop = 1)]
[Weld("5", TypeAbove = 10, TypeBelow = 10, SizeAbove = 6, SizeBelow = 6, Shop = 1)]
[Weld("10_1", TypeAbove = 4, SizeAbove = 2, AngleAbove = 45, RootOpeningAbove = 2, Shop = 1, Preparation = 1)]
[Weld("10_2", TypeAbove = 10, TypeBelow = 10, SizeAbove = 8, SizeBelow = 8, Shop = 1)]
[Weld("11", TypeAbove = 10, TypeBelow = 10, SizeAbove = 6, SizeBelow = 6, Shop = 1)]
[Weld("12", TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld("13", TypeAbove = 10, TypeBelow = 10, SizeAbove = 8, SizeBelow = 8, Shop = 1)]
[Chamfer(5, Type = 1, X = 30, Y = 45)]
[Chamfer(10, Type = 1, X = 20, Y = 20)]
[Chamfer(13, Type = 1, X = 20, Y = 20)]
[General(typeof(int), "kind", 0)]
[General(typeof(double), "h", 900, "b1", 550, "b2", 300, "tw", 12, "tf", 18,
    "gap", 5, "stretch4", 20, "shrink5", 54, "distance5", 1500, "position10", 600, "position11", 163)]
public partial class PluginData;