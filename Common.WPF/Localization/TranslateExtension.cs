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
 *  TranslateExtension.cs: provide localization feature, used by share library in XAML,
 *      assembly witch loaded by system directly can use Tekla.Structures.Dialog.LocExtension
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Muggle.TsExtensions.Common.WPF.Localization;

[MarkupExtensionReturnType(typeof(string))]
public class TranslateExtension : MarkupExtension {
    private string Key { get; set; }

    public TranslateExtension(string key) {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        if (IsInDesignMode()) return Key;

        return TranslationService.Instance[Key];
    }

    private static bool IsInDesignMode() {
        return DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }
}