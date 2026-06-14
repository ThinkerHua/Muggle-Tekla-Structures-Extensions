using System;
using System.Windows.Markup;

namespace Muggle.TsExtensions.Common.WPF.Localization;

[MarkupExtensionReturnType(typeof(string))]
public class TranslateExtension : MarkupExtension {
    private string Key { get; set; }

    public TranslateExtension(string key) {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        return TranslationService.Instance[Key];
    }
}