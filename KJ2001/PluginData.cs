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
 *  PluginData.cs: data class for 'KJ2001' detail
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.KJ2001;

[Part("AnchorRod", Material = "Q355", Class = 0)]
[Plate("Base", Thickness = 22, Material = "Q355")]
[Plate("Washer", Thickness = 22, Breadth = 70, Height = 70, Material = "Q355")]
[Plate("Stif", Material = "Q355")]
[Chamfer("Stif", Type = 1, X = 25, Y = 25)]
[Bolt("AnchorRod", Size = 24)]
[Bolt("StudX", Standard = "STUD", Size = 16, Length = 80, DistX = "8*200", DistY = "200", Type = 1)]
[Bolt("StudY", Standard = "STUD", Size = 16, Length = 80, DistX = "8*200", DistY = "200", Type = 1)]
[Weld(1, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[General(typeof(double), "AnchorLen1", 120, "AnchorLen2", 50, "AnchorLen3", 510, "AnchorLen4", 330,
    "AnchorLen5", 100, "AnchorDistToEdge", 50, "StudXOffset", 100, "StudYOffset", 100,
    "StifDistToBase", 1800, "PouringHoleDiameter", 200, "ExhaustHoleDiameter", 25,
    "ExhaustHolePosX", 100, "ExhaustHolePosY", 100)]
public partial class PluginData {
}