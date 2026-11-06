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
 *  PluginData.cs: data class for 'KJ1002' connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.KJ1002;

[PartFields("Brace")]
[PlateFields("Stiffener", "Gusset")]
[BoltFields(1)]
[WeldFields(1, 2)]
[GeneralFields(typeof(int), "CreateUpperSplices")]
[GeneralFields(typeof(double), "SectionLength", "Clearance", "ExtendedDistance", "BoltOffsetX")]
public partial class PluginData { }