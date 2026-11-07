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
 *  PluginData.cs: data class for 'MG1002' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.MG1002;

[PlateFields("End", "FlangeStif", "WebStif", "Vertical", "Diagonal", "SplicingWeb")]
[ChamferFields("Vertical", "Diagonal", "StifInside", "StifOutside")]
[BoltFields(1)]
[WeldFields(1, 2, 3, 4, 5, 6, 7, 8)]
[GeneralFields(typeof(int), "FlangeStifKind", "WebStifKind")]
[GeneralFields(typeof(double), "DiagonalPos1", "DiagonalPos2", "DistWebFlangeSeam", "BoltStartOffsetX")]
[GeneralFields(typeof(string), "WebStifDistList")]
public partial class PluginData;