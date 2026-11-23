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

[Part("ShortBeam")]
[Plate("PrimStif", Thickness = 10, Material = "Q235B")]
[Plate("Thickened")]
[Plate("WebSplice", Thickness = 16, Breadth = 350, Height = 300, Material = "Q235B")]
[Chamfer("PrimStif", Type = 1, X = 25, Y = 25)]
[Chamfer("Beam", Type = 3, X = 25, Y = 25)]
[Bolt("Web", Size = 20, Standard = "HS10.9", DistX = "3*70", DistY = "70 100 70")]
[Weld(1, TypeAbove = 10, SizeAbove = 8, Around = 1)]
[Weld(2, TypeAbove = 0, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2)]
[Weld(3, TypeAbove = 10, SizeAbove = 8)]
[Weld(4, TypeAbove = 6, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2)]
[Weld(5, TypeAbove = 6, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2, Placement = 1)]
[Weld(6, TypeAbove = 5, SizeAbove = 10, AngleAbove = 45, RootFaceAbove = 4, RootOpeningAbove = 2)]
[General(typeof(Distance), "ExtraLen", 150, "Gap", 15, "ShortBeamLen", 1200)]
[General(typeof(DistanceList), "MoreStifDisList", "")]
public partial class ViewModel : ConnectionViewModel;