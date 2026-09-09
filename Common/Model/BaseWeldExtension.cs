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
 *  BaseWeldExtension.cs: extension of Tekla.Structures.Model.BaseWeld
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {

    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="BaseWeld"/> 的扩展。
    /// </summary>
    public static class BaseWeldExtension {

        /// <summary>
        /// 上下焊缝参数互换。
        /// </summary>
        /// <param name="weld">给定的焊缝。</param>
        public static void SwapAboveBelow(this BaseWeld weld) {
            (weld.PrefixAboveLine, weld.PrefixBelowLine) = (weld.PrefixBelowLine, weld.PrefixAboveLine);
            (weld.TypeAbove, weld.TypeBelow) = (weld.TypeBelow, weld.TypeAbove);
            (weld.SizeAbove, weld.SizeBelow) = (weld.SizeBelow, weld.SizeAbove);
            (weld.AdditionalSizeAbove, weld.AdditionalSizeBelow) = (weld.AdditionalSizeBelow, weld.AdditionalSizeAbove);
            (weld.AngleAbove, weld.AngleBelow) = (weld.AngleBelow, weld.AngleAbove);
            (weld.ContourAbove, weld.ContourBelow) = (weld.ContourBelow, weld.ContourAbove);
            (weld.FinishAbove, weld.FinishBelow) = (weld.FinishBelow, weld.FinishAbove);
            (weld.RootFaceAbove, weld.RootFaceBelow) = (weld.RootFaceBelow, weld.RootFaceAbove);
            (weld.EffectiveThroatAbove, weld.EffectiveThroatBelow) = (weld.EffectiveThroatBelow, weld.EffectiveThroatAbove);
            (weld.RootOpeningAbove, weld.RootOpeningBelow) = (weld.RootOpeningBelow, weld.RootOpeningAbove);
            (weld.IncrementAmountAbove, weld.IncrementAmountBelow) = (weld.IncrementAmountBelow, weld.IncrementAmountAbove);
            (weld.LengthAbove, weld.LengthBelow) = (weld.LengthBelow, weld.LengthAbove);
            (weld.PitchAbove, weld.PitchBelow) = (weld.PitchBelow, weld.PitchAbove);
        }
    }
}