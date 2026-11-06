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
 *  PluginData.cs: data class for 'HJ1001' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.HJ1001;

[PlateFields("End", "Stiffener")]
[ChamferFields("Inside", "Outside")]
[BoltCircleFields(1)]
[WeldFields(1, 2)]
[GeneralFields(typeof(int), "StifKind", "Quantity")]
[GeneralFields(typeof(double), "Margin")]
public partial class PluginData;