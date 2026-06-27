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
 *  PluginData.cs: data class for 'DC4001' custom part
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.DC4001;

[PartFields("main")]
[PlateFields(4, 5, 10, 11, 12, 13)]
[ChamferFields(5, 10, 13)]
[WeldFields("4_1", "4_2", "4_3", "5", "10_1", "10_2", "11", "12", "13")]
[GeneralFields(typeof(int), "kind")]
[GeneralFields(typeof(double), "h", "b1", "b2", "tw", "tf", "gap",
    "stretch4", "shrink5", "distance5", "position10", "position11")]
public partial class PluginData;