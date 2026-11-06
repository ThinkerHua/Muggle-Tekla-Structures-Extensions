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
 *  ViewModel.cs: view model for view of HJ1001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.HJ1001;

[PlateProperties("End", thickness: 20, breadth: 550, material: "Q235B")]
[PlateProperties("Stiffener", thickness: 10, breadth: 120, material: "Q235B")]
[ChamferProperties("Inside", type: 1, x: 25, y: 25)]
[ChamferProperties("Outside", type: 1, x: 40, y: 40)]
[BoltCircleProperties(1, size: 20, standard: "HS10.9", diameter: 450, numberOfBolts: 16)]
[WeldProperties(1, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 8, typeBelow: 10, sizeBelow: 8, shop: 1)]
[GeneralProperties(typeof(Integer), "StifKind", 0, "Quantity", 8)]
[GeneralProperties(typeof(Double), "Margin", 10)]
public partial class ViewModel : ConnectionViewModel;