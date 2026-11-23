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

[Plate("top", Name = "Top", Material = "Q235B")]
[Plate("diagonal", Name = "Diagonal", Material = "Q235B")]
[Plate("horizontal", Name = "Horizontal", Material = "Q235B")]
[Plate("thickened", Name = "Thickened", Material = "Q235B", Class = 4)]
[Plate("eave", Name = "Eave", Material = "Q235B")]
[Plate("end1", Name = "End1", Thickness = 28, Breadth = 350, Height = 1355, Material = "Q235B")]
[Plate("end2", Name = "End2", Thickness = 28, Breadth = 350, Height = 1255, Material = "Q235B")]
[Plate("flangeStif", Name = "FlangeStif", Thickness = 10, Breadth = 150, Height = 150, Material = "Q235B")]
[Plate("webStif", Name = "WebStif", Thickness = 10, Breadth = 150, Height = 150, Material = "Q235B")]
[Chamfer("diagonal", Type = 1, X = 25, Y = 25)]
[Chamfer("horizontal", Type = 1, X = 25, Y = 25)]
[Chamfer("stifInside", Type = 1, X = 25, Y = 25)]
[Chamfer("stifOutside", Type = 1, X = 25, Y = 25)]
[Bolt(1, Size = 20, Standard = "HS10.9", DistX = "80 6*150 120", DistY = "70")]
[Weld(1, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[Weld(2, Shop = 1, TypeAbove = 5, AngleAbove = 15, RootFaceAbove = 4,
    RootOpeningAbove = 2, Preparation = 1)]
[Weld(3, Shop = 1, TypeAbove = 5, SizeAbove = 8, AngleAbove = 15, RootFaceAbove = 4,
    RootOpeningAbove = 2, Preparation = 1)]
[Weld(4, Shop = 1, TypeAbove = 10, TypeBelow = 10, SizeAbove = 8, SizeBelow = 8)]
[Weld(5, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[Weld(6, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[Weld(7, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[Weld(8, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[Weld(9, Shop = 1, TypeAbove = 10, SizeAbove = 8)]
[General(typeof(Integer), "isTopHorizontal", 0, "flangeStifKind", 0, "webStifKind", 0)]
[General(typeof(Double), "diagPosition1", 80, "diagPosition2", 80, "thickenedPosition", 100,
    "endPosition", 160, "difference", 4, "grooveSlope", 2.5, "boltOffset", 70)]
[General(typeof(DistanceList), "webStifDistList", "190 4*150")]
public partial class ViewModel : ConnectionViewModel;