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
 *  PluginData.cs: data class for 'WK1001' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.WK1001;

[Part("Pipe", Material = "Q345B")]
[Plate("TopEnd", Thickness = 16, Material = "Q345B")]
[Plate("BotEnd", Thickness = 16, Material = "Q345B")]
[Plate("Stif", Thickness = 25, Material = "Q345B")]
[Weld(1, TypeAbove = 10, SizeAbove = 8, Around = 1)]
[Weld(2, TypeAbove = 10, SizeAbove = 8, Around = 1, Shop = 1)]
[Weld(3, TypeAbove = 6, SizeAbove = 14, AngleAbove = 45, RootFaceAbove = 4, Placement = 2, Preparation = 3, Around = 1,
    Shop = 1)]
[Weld(4, TypeAbove = 10, SizeAbove = 6, Around = 1, Shop = 1)]
[General(typeof(double), "MinDis", 100, "ExtendLenTop", 20, "ExtendLenBot", 20)]
public partial class PluginData;