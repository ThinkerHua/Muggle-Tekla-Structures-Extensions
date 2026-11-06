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
 *  PluginData.cs: data class for 'KJ1004' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.KJ1004;

[PartFields("ShortBeam")]
[PlateFields("PrimStif", "Thickened", "WebSplice")]
[ChamferFields("PrimStif", "Beam")]
[BoltFields("Web")]
[WeldFields(1, 2, 3, 4, 5, 6)]
[GeneralFields(typeof(double), "ExtraLen", "Gap", "ShortBeamLen")]
[GeneralFields(typeof(string), "MoreStifDisList")]
public partial class PluginData;