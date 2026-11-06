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
 *  ViewModel.cs: view model for view of KJ1002
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1002;

[PartProperties("Brace", profile: "L70*5.0", material: "Q235B")]
[PlateProperties("Stiffener", thickness: 8, material: "Q235B")]
[PlateProperties("Gusset", thickness: 8, material: "Q235B")]
[BoltProperties(1, standard: "HS10.9", size: 16, distX: "70")]
[WeldProperties(1, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
[GeneralProperties(typeof(Integer), "CreateUpperSplices", 0)]
[GeneralProperties(typeof(Double), "SectionLength", 1000, "Clearance", 50, "ExtendedDistance", 30, "BoltOffsetX", 50)]
public partial class ViewModel : ConnectionViewModel { }