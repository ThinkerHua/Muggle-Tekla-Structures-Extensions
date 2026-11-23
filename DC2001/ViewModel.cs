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
 *  ViewModel.cs: view model for view of DC2001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.DC2001;

[Part("LeftCorbel", Profile = "HI530-270-30-30*600", Name = "LeftCorbel")]
[Part("RightCorbel", Name = "RightCorbel")]
[Plate("PrimStif", Thickness = 20, Name = "ColumnStiffener")]
[Plate("LCorbelStif", Thickness = 20, Breadth = 185, Name = "LeftCorbelStiffener")]
[Plate("RCorbelStif", Thickness = 20, Breadth = 185, Name = "RightCorbelStiffener")]
[Plate("LeftPad", Thickness = 30, Breadth = 560, Height = 560, Name = "LeftPad")]
[Plate("RightPad", Thickness = 30, Breadth = 560, Height = 560, Name = "RightPad")]
[Chamfer("PrimStif", Type = 1, X = 25, Y = 25)]
[Weld("Corbel", TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[Weld("PrimStif", TypeAbove = 10, TypeBelow = 10, SizeAbove = 6, SizeBelow = 6, Shop = 1)]
[Weld("CorbelStif", TypeAbove = 10, TypeBelow = 10, SizeAbove = 6, SizeBelow = 6, Shop = 1)]
[Weld("Pad", TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[General(typeof(Double), "LeftCorbelLength", 800, "RightCorbelLength", 800,
    "LeftCorbelStifPos", 500, "RightCorbelStifPos", 500)]
public partial class ViewModel : DetailViewModel;