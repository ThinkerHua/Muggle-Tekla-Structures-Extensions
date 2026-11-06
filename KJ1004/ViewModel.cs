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
 *  ViewModel.cs: view model for view of KJ1004
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1004;

[PartProperties("ShortBeam")]
[PlateProperties("PrimStif", thickness: 10, material: "Q235B")]
[PlateProperties("Thickened")]
[PlateProperties("WebSplice", thickness: 16, breadth: 350, height: 300, material: "Q235B")]
[ChamferProperties("PrimStif", type: 1, x: 25, y: 25)]
[ChamferProperties("Beam", type: 3, x: 25, y: 25)]
[BoltProperties("Web", size: 20, standard: "HS10.9", distX: "3*70", distY: "70 100 70")]
[WeldProperties(1, typeAbove: 10, sizeAbove: 8, around: 1)]
[WeldProperties(2, typeAbove: 0, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
[WeldProperties(3, typeAbove: 10, sizeAbove: 8)]
[WeldProperties(4, typeAbove: 6, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
[WeldProperties(5, typeAbove: 6, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2, placement: 1)]
[WeldProperties(6, typeAbove: 5, sizeAbove: 10, angleAbove: 45, rootFaceAbove: 4, rootOpeningAbove: 2)]
[GeneralProperties(typeof(Distance), "ExtraLen", 150, "Gap", 15, "ShortBeamLen", 1200)]
[GeneralProperties(typeof(DistanceList), "MoreStifDisList", "")]
public partial class ViewModel : ConnectionViewModel;