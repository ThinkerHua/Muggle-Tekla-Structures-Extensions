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
 *  ViewModel.cs: view model for view of MG1001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MG1001;

[PlateProperties("top", name: "Top", material: "Q235B")]
[PlateProperties("diagonal", name: "Diagonal", material: "Q235B")]
[PlateProperties("horizontal", name: "Horizontal", material: "Q235B")]
[PlateProperties("thickened", name: "Thickened", material: "Q235B", @class: 4)]
[PlateProperties("eave", name: "Eave", material: "Q235B")]
[PlateProperties("end1", name: "End1", thickness: 28, breadth: 350, height: 1355, material: "Q235B")]
[PlateProperties("end2", name: "End2", thickness: 28, breadth: 350, height: 1255, material: "Q235B")]
[PlateProperties("flangeStif", name: "FlangeStif", thickness: 10, breadth: 150, height: 150, material: "Q235B")]
[PlateProperties("webStif", name: "WebStif", thickness: 10, breadth: 150, height: 150, material: "Q235B")]
[ChamferProperties("diagonal", type: 1, x: 25, y: 25)]
[ChamferProperties("horizontal", type: 1, x: 25, y: 25)]
[ChamferProperties("stifInside", type: 1, x: 25, y: 25)]
[ChamferProperties("stifOutside", type: 1, x: 25, y: 25)]
[BoltProperties(1, size: 20, standard: "HS10.9", distX: "80 6*150 120", distY: "70")]
[WeldProperties(1, shop: 1, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(2, shop: 1, typeAbove: 5, angleAbove: 15, rootFaceAbove: 4,
    rootOpeningAbove: 2, preparation: 1)]
[WeldProperties(3, shop: 1, typeAbove: 5, sizeAbove: 8, angleAbove: 15, rootFaceAbove: 4,
    rootOpeningAbove: 2, preparation: 1)]
[WeldProperties(4, shop: 1, typeAbove: 10, typeBelow: 10, sizeAbove: 8, sizeBelow: 8)]
[WeldProperties(5, shop: 1, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(6, shop: 1, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(7, shop: 1, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(8, shop: 1, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(9, shop: 1, typeAbove: 10, sizeAbove: 8)]
[GeneralProperties(typeof(Integer), "isTopHorizontal", 0, "flangeStifKind", 0, "webStifKind", 0)]
[GeneralProperties(typeof(Double), "diagPosition1", 80, "diagPosition2", 80, "thickenedPosition", 100,
    "endPosition", 160, "difference", 4, "grooveSlope", 2.5, "boltOffset", 70)]
[GeneralProperties(typeof(String), "webStifDistList", "190 4*150")]
public partial class ViewModel : ConnectionViewModel;