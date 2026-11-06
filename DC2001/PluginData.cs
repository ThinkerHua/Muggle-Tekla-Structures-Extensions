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
 *  PluginData.cs: data class for 'DC2001' detail
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.DC2001;

[PartFields("LeftCorbel", "RightCorbel")]
[PlateFields("PrimStif", "LCorbelStif", "RCorbelStif", "LeftPad", "RightPad")]
[ChamferFields("PrimStif")]
[WeldFields("Corbel", "PrimStif", "CorbelStif", "Pad")]
[GeneralFields(typeof(double), "LeftCorbelLength", "RightCorbelLength", "LeftCorbelStifPos", "RightCorbelStifPos")]
public partial class PluginData;