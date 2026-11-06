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
 *  PluginData.cs: data class for 'KJ2001' detail
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Muggle.TsExtensions.CodingHelper.Generators;

namespace Muggle.TsExtensions.KJ2001;

[PartFields("AnchorRod")]
[PlateFields("Base", "Washer", "Stif")]
[ChamferFields("Stif")]
[BoltFields("AnchorRod", "StudX", "StudY")]
[WeldFields(1, 2)]
[GeneralFields(typeof(double), "AnchorLen1", "AnchorLen2", "AnchorLen3", "AnchorLen4", "AnchorLen5",
    "StudXOffset", "StudYOffset", "StifDistToBase", "PouringHoleDiameter", "ExhaustHoleDiameter", 
    "ExhaustHolePosX", "ExhaustHolePosY")]
public partial class PluginData { }