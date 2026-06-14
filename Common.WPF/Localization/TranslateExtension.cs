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