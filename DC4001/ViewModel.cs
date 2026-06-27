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
 *  ViewModel.cs: view model for main window of D42001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.DC4001;

[PartProperties("main", name: "Main")]
[PlateProperties(4, thickness: 16, breadth: 250, name: "4")]
[PlateProperties(5, thickness: 8, breadth: 90, name: "5")]
[PlateProperties(10, thickness: 10, breadth: 120, name: "10")]
[PlateProperties(11, thickness: 10, breadth: 120, name: "11")]
[PlateProperties(12, thickness: 20, breadth: 90, height: 340, name: "12")]
[PlateProperties(13, thickness: 10, breadth: 120, name: "13")]
[WeldProperties("4_1", typeAbove: 10, typeBelow: 10, sizeAbove: 8, sizeBelow: 8, shop: 1)]
[WeldProperties("4_2", typeAbove: 10, typeBelow: 10, sizeAbove: 10, sizeBelow: 10, shop: 1)]
[WeldProperties("4_3", typeAbove: 10, typeBelow: 10, sizeAbove: 10, sizeBelow: 10, shop: 1)]
[WeldProperties("5", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
[WeldProperties("10_1", typeAbove: 4, sizeAbove: 2, angleAbove: 45, rootOpeningAbove: 2, shop: 1, preparation: 1)]
[WeldProperties("10_2", typeAbove: 10, typeBelow: 10, sizeAbove: 8, sizeBelow: 8, shop: 1)]
[WeldProperties("11", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
[WeldProperties("12", typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties("13", typeAbove: 10, typeBelow: 10, sizeAbove: 8, sizeBelow: 8, shop: 1)]
[ChamferProperties(5, type: 1, x: 30, y: 45)]
[ChamferProperties(10, type: 1, x: 20, y: 20)]
[ChamferProperties(13, type: 1, x: 20, y: 20)]
[GeneralProperties(typeof(Integer), "kind", 0)]
[GeneralProperties(typeof(Double), "h", 900, "b1", 550, "b2", 300, "tw", 12, "tf", 18,
    "gap", 5, "stretch4", 20, "shrink5", 54, "distance5", 1500, "position10", 600, "position11", 163)]
public partial class ViewModel : CustomPartViewModel;