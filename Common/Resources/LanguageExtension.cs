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
 *  LanguageExtension.cs: extension of Muggle.TsExtensions.Common.Resources.Language
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;

namespace Muggle.TsExtensions.Common.Resources;

/// <summary>
/// <see cref="Language"/> 的扩展。
/// </summary>
public static class LanguageExtension {
    private record struct LanguageName(
        string TsAdvancedOptionLanguage,
        string TsLocalizationLanguage,
        string IsoLanguage);

    private static LanguageName[] LanguageNames => [
        new("ENGLISH", "enu", "en-US"),
        new("DUTCH", "nld", "nl-NL"),
        new("FRENCH", "fra", "fr-FR"),
        new("GERMAN", "deu", "de-DE"),
        new("ITALIAN", "ita", "it-IT"),
        new("SPANISH", "esp", "es-ES"),
        new("JAPANESE", "jpn", "ja-JP"),
        new("CHINESE SIMPLIFIED", "chs", "zh-CN"),
        new("CHINESE TRADITIONAL", "cht", "zh-TW"),
        new("CZECH", "csy", "cs-CZ"),
        new("PORTUGUESE BRAZILIAN", "ptb", "pt-PT"),
        new("HUNGARIAN", "hun", "hu-HU"),
        new("POLISH", "plk", "pl-PL"),
        new("RUSSIAN", "rus", "ru-RU"),
    ];

    private static int Match(string language) {
        if (language == null) return -1;

        return Array.FindIndex(LanguageNames, l =>
            string.Equals(l.TsAdvancedOptionLanguage, language, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(l.TsLocalizationLanguage, language, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(l.IsoLanguage, language, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Get the language name used in Tekla Structures advanced option.
    /// </summary>
    /// <param name="language">Language name, such as "ENGLISH", "enu", "en-US".</param>
    /// <returns>Returns the matched language name used in Tekla Structures advanced options,
    /// or the default value "ENGLISH" if no match is found.</returns>
    public static string GetTsAdvancedOptionLanguage(string language) {
        var matchIndex = Match(language);
        if (matchIndex < 0) matchIndex = 0;

        return LanguageNames[matchIndex].TsAdvancedOptionLanguage;
    }

    /// <summary>
    /// Get the language name used in Tekla.Structures.Dialog.Localization class.
    /// </summary>
    /// <param name="language">Language name, such as "ENGLISH", "enu", "en-US".</param>
    /// <returns>Returns the matched language name used in Tekla.Structures.Dialog.Localization class,
    /// or the default value "enu" if no match is found.</returns>
    public static string GetTsLocalizationLanguage(string language) {
        var matchIndex = Match(language);
        if (matchIndex < 0) matchIndex = 0;

        return LanguageNames[matchIndex].TsLocalizationLanguage;
    }

    /// <summary>
    /// Get the language name used in ISO language code.
    /// </summary>
    /// <param name="language">Language name, such as "ENGLISH", "enu", "en-US".</param>
    /// <returns>Returns the matched language name used in ISO language code,
    /// or the default value "en-US" if no match is found.</returns>
    public static string GetIsoLanguage(string language) {
        var matchIndex = Match(language);
        if (matchIndex < 0) matchIndex = 0;

        return LanguageNames[matchIndex].IsoLanguage;
    }
}