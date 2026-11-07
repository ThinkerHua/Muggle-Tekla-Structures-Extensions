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
 *  ViewModel.cs: view model for view of MG1002
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MG1002;

[PlateProperties("End", name: "EndPlate", thickness: 30, breadth: 460, height: 1090, material: "Q235B")]
[PlateProperties("FlangeStif", name: "FlangeStiffener", thickness: 10, breadth: 115, height: 175, material: "Q235B")]
[PlateProperties("WebStif", name: "WebStiffener", thickness: 10, breadth: 225, height: 225, material: "Q235B")]
[PlateProperties("Vertical", name: "VerticalPlate", thickness: 16, material: "Q235B")]
[PlateProperties("Diagonal", name: "DiagonalPlate", thickness: 8, material: "Q235B")]
[PlateProperties("SplicingWeb", name: "SplicingWeb", material: "Q235B")]
[ChamferProperties("Vertical", type: 0, x: 25, y: 25)]
[ChamferProperties("Diagonal", type: 0, x: 25, y: 25)]
[ChamferProperties("StifInside", type: 0, x: 25, y: 25)]
[ChamferProperties("StifOutside", type: 0, x: 25, y: 25)]
[BoltProperties(1, standard: "HS10.9", size: 24, distX: "135 3*220 135", distY: "200")]
[WeldProperties(1, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 8, typeBelow: 10, sizeBelow: 8, shop: 1)]
[WeldProperties(3, typeAbove: 4, sizeAbove: 10, angleAbove: 15, rootFaceAbove: 2, rootOpeningAbove: 2, placement: 1, preparation: 2, shop: 1)]
[WeldProperties(4, typeAbove: 4, sizeAbove: 10, angleAbove: 15, rootFaceAbove: 2, rootOpeningAbove: 2, placement: 1, preparation: 3, shop: 1)]
[WeldProperties(5, typeAbove: 5, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 2, rootOpeningAbove: 2, preparation: 1, shop: 1)]
[WeldProperties(6, typeAbove: 10, sizeAbove: 8, typeBelow: 10, sizeBelow: 8, shop: 1)]
[WeldProperties(7, typeAbove: 10, sizeAbove: 8, typeBelow: 10, sizeBelow: 8, shop: 1)]
[WeldProperties(8, typeAbove: 10, sizeAbove: 6, typeBelow: 10, sizeBelow: 6, shop: 1)]
[GeneralProperties(typeof(Integer), "FlangeStifKind", 0, "WebStifKind", 0)]
[GeneralProperties(typeof(Double), "DiagonalPos1", 100, "DiagonalPos2", 100, "DistWebFlangeSeam", 200, "BoltStartOffsetX", 70)]
[GeneralProperties(typeof(DistanceList), "WebStifDistList", "180 2*250")]
public partial class ViewModel : ConnectionViewModel { }