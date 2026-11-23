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
 *  PluginData.cs: data class for 'MJ5001' component
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.MJ5001;

[Part("AnchorRod", Material = "Q345B", Class = 0)]
[Plate("Embedment", Thickness = 20, Breadth = 300, Height = 800, Material = "Q345B")]
[Plate("Cleat", Thickness = 12, Breadth = 300, Height = 300, Material = "Q345B")]
[Bolt("AnchorRod", Size = 20, Length = 150, DistX = "4*125", DistY = "160")]
[Bolt("1", Standard = "HS10.9", Size = 20, DistX = "3*70", DistY = "2*70")]
[Weld(1, TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld(3, SizeAbove = 10, AngleAbove = 45, RootOpeningAbove = 2, RootFaceAbove = 2)]
[Chamfer(1, Type = 3, X = 30)]
[General(typeof(double), "Gap", 15)]
public partial class PluginData;