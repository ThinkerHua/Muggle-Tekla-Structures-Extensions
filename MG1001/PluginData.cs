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
 *  PluginData.cs: data class for 'MG1001' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.MG1001;

[PlateFields("top", "diagonal", "horizontal", "thickened", "eave", "end1", "end2", "flangeStif", "webStif")]
[ChamferFields("diagonal", "horizontal", "stifInside", "stifOutside")]
[BoltFields(1)]
[WeldFields(1, 2, 3, 4, 5, 6, 7, 8, 9)]
[GeneralFields(typeof(int), "isTopHorizontal", "flangeStifKind", "webStifKind")]
[GeneralFields(typeof(double), "diagPosition1", "diagPosition2", "thickenedPosition", "endPosition",
    "difference", "grooveSlope", "boltOffset")]
[GeneralFields(typeof(string), "webStifDistList")]
public partial class PluginData;