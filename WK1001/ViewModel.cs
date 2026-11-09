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
 *  ViewModel.cs: view model for view of WK1001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.WK1001;

[PartProperties("Pipe", material: "Q345B")]
[PlateProperties("TopEnd", thickness: 16, material: "Q345B")]
[PlateProperties("BotEnd", thickness: 16, material: "Q345B")]
[PlateProperties("Stif", thickness: 25, material: "Q345B")]
[WeldProperties(1, typeAbove: 10, sizeAbove: 8, around: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties(3, typeAbove: 6, sizeAbove: 14, angleAbove: 45, rootFaceAbove: 4, placement: 2,
    preparation: 3, around: 1, shop: 1)]
[WeldProperties(4, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
[GeneralProperties(typeof(Double), "MinDis", 100, "ExtendLenTop", 20, "ExtendLenBot", 20)]
public partial class ViewModel : ConnectionViewModel { }