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
 *  BoltSymbols.cs: sets of bolt symbol
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class BoltSymbols {
    public static string[] BoltTypeSet => ItemsProvider.TranslationArray([
        "albl_Bolt_Type_Site",
        "albl_Bolt_Type_Workshop",
    ]);

    public static string[] BoltThreadInMaterialSet => ItemsProvider.TranslationArray([
        "albl_Bolt_ThreadInMaterial_No",
        "albl_Bolt_ThreadInMaterial_Yes",
    ]);

    public static string[] BoltPlainHoleTypeSet => ItemsProvider.TranslationArray([
        "albl_Bolt_PlainHoleType_Through",
        "albl_Bolt_PlainHoleType_Blind",
    ]);

    public static string[] BoltSpecialHoleTypeSet => ItemsProvider.TranslationArray([
        "albl_Bolt_SpecialHoleType_Oversized",
        "albl_Bolt_SpecialHoleType_Slotted",
        "albl_Bolt_SpecialHoleType_NoHole",
#if D2024 || R2024
        "albl_Bolt_SpecialHoleType_Tapped",
#endif
    ]);

    public static string[] BoltRotateSlotsSet => ItemsProvider.TranslationArray([
        "albl_Bolt_RotateSlots_Odd",
        "albl_Bolt_RotateSlots_Even",
        "albl_Bolt_RotateSlots_Parallel",
    ]);
}