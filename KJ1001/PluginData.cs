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
 *  PluginData.cs: data class for 'KJ1001' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
 
using Muggle.TsExtensions.CodingHelper.Generators;
 
namespace Muggle.TsExtensions.KJ1001;

[PlateFields("PrimStif", "Thickened", "TopCover", "BotCover", "WebSplice")]
[ChamferFields("PrimStif", "Beam", "TopCover", "BotCover")]
[BoltFields("Web")]
[WeldFields(2, 3, 4, 5, 6, 7, 8)]
// 仅适用于 Kind == 0 的情形
// 0 - 仅前面
// 1 - 仅后面
// 2 - 前后
[GeneralFields(typeof(int), "SpliceCreation")]
[GeneralFields(typeof(double), "ExtraLen", "Gap")]
[GeneralFields(typeof(string), "MoreStifDisList")]
public partial class PluginData;