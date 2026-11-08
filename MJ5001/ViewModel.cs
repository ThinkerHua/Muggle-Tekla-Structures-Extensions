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
 *  ViewModel.cs: view model for view of MJ5001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MJ5001;

[PartProperties("AnchorRod", material: "Q345B", @class: 0)]
[PlateProperties("Embedment", thickness: 20, breadth: 300, height: 800, material: "Q345B")]
[PlateProperties("Cleat", thickness: 12, breadth: 300, height: 300, material: "Q345B")]
[BoltProperties("AnchorRod", size: 20, length: 150, distX: "4*125", distY: "160")]
[BoltProperties("1", standard: "HS10.9", size: 20, distX: "3*70", distY: "2*70")]
[WeldProperties(1, typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
[WeldProperties(2, typeAbove: 10, sizeAbove: 8, around: 1, shop: 1)]
[WeldProperties(3, sizeAbove: 10, angleAbove: 45, rootOpeningAbove: 2, rootFaceAbove: 2)]
[ChamferProperties(1, type: 3, x: 30)]
[GeneralProperties(typeof(Double), "Gap", 15)]
public partial class ViewModel : NotificationObject;