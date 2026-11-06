/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025-2026 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  ViewModel.cs: view model for view of KJ2001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ2001;


[PartProperties("AnchorRod", material: "Q355", @class: 0)]
[PlateProperties("Base", thickness: 22, material: "Q355")]
[PlateProperties("Washer", thickness: 22, breadth: 70, height: 70, material: "Q355")]
[PlateProperties("Stif", material: "Q355")]
[ChamferProperties("Stif", 1, 25, 25)]
[BoltProperties("AnchorRod", size: 24)]
[BoltProperties("StudX", standard: "STUD", size: 16, length: 80, distX: "8*200", distY: "200", type: 1)]
[BoltProperties("StudY", standard: "STUD", size: 16, length: 80, distX: "8*200", distY: "200", type: 1)]
[WeldProperties(1, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[GeneralProperties(typeof(Double), "AnchorLen1", 120, "AnchorLen2", 50, "AnchorLen3", 510, "AnchorLen4", 330,
    "AnchorLen5", 100, "AnchorDistToEdge", 50, "StudXOffset", 100, "StudYOffset", 100,
    "StifDistToBase", 1800, "PouringHoleDiameter", 200, "ExhaustHoleDiameter", 25,
    "ExhaustHolePosX", 100, "ExhaustHolePosY", 100)]
public partial class ViewModel : DetailViewModel { }