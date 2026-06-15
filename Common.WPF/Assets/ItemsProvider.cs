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
 *  ItemsProvider.cs: provider that can provide localized symbol array
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Muggle.TsExtensions.Common.WPF.DataTemplates;
using Muggle.TsExtensions.Common.WPF.Localization;

namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class ItemsProvider {
    public static Symbol[] SymbolArray(string[] names, DrawingGroup[] drawings) {
        if (names is null) {
            throw new ArgumentNullException(nameof(names));
        }

        if (drawings is null) {
            throw new ArgumentNullException(nameof(drawings));
        }

        if (names.Length != drawings.Length) {
            throw new ArgumentException($"Length of {nameof(names)} and {nameof(drawings)} doesn't match.");
        }

        var symbols = new Symbol[names.Length];
        for (int i = 0; i < names.Length; i++) {
            var name = names[i];
            var drawing = drawings[i];

            symbols[i] = new Symbol(
                IsInDesignMode() ? name : TranslationService.Instance[name],
                drawing);
        }

        return symbols;
    }

    public static Symbol[] SymbolArray((string name, DrawingGroup drawing)[] items) {
        if (items is null) {
            throw new ArgumentNullException(nameof(items));
        }

        var symbols = new Symbol[items.Length];
        for (int i = 0; i < items.Length; i++) {
            var name = items[i].name;
            var drawing = items[i].drawing;

            symbols[i] = new Symbol(
                IsInDesignMode() ? name : TranslationService.Instance[name],
                drawing);
        }

        return symbols;
    }

    public static string[] TranslationArray(string[] keys) {
        if (keys is null) {
            throw new ArgumentNullException(nameof(keys));
        }

        if (IsInDesignMode()) return keys;

        var translationArray = new string[keys.Length];
        for (int i = 0; i < keys.Length; i++) {
            var key = keys[i];
            translationArray[i] = TranslationService.Instance[key];
        }

        return translationArray;
    }

    private static bool IsInDesignMode() {
        return DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }
}