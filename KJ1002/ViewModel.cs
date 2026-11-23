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

[Part("Brace", Profile = "L70*5.0", Material = "Q235B")]
[Plate("Stiffener", Thickness = 8, Material = "Q235B")]
[Plate("Gusset", Thickness = 8, Material = "Q235B")]
[Bolt(1, Standard = "HS10.9", Size = 16, DistX = "70")]
[Weld(1, TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[General(typeof(Integer), "CreateUpperSplices", 0)]
[General(typeof(Double), "SectionLength", 1000, "Clearance", 50, "ExtendedDistance", 30, "BoltOffsetX", 50)]
public partial class ViewModel : ConnectionViewModel { }