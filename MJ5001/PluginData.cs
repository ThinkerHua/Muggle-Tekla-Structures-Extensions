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

[PartFields("AnchorRod")]
[PlateFields("Embedment", "Cleat")]
[BoltFields("AnchorRod", "1")]
[WeldFields(1, 2, 3)]
[ChamferFields(1)]
[GeneralFields(typeof(double), "Gap")]
public partial class PluginData;