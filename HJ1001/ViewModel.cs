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

[Plate("End", Thickness = 20, Breadth = 550, Material = "Q235B")]
[Plate("Stiffener", Thickness = 10, Breadth = 120, Material = "Q235B")]
[Chamfer("Inside", Type = 1, X = 25, Y = 25)]
[Chamfer("Outside", Type = 1, X = 40, Y = 40)]
[Bolt(1, Shape = 1, Size = 20, Standard = "HS10.9", Diameter = 450, NumberOfBolts = 16)]
[Weld(1, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 8, TypeBelow = 10, SizeBelow = 8, Shop = 1)]
[General(typeof(Integer), "StifKind", 0, "Quantity", 8)]
[General(typeof(Double), "Margin", 10)]
public partial class ViewModel : ConnectionViewModel;