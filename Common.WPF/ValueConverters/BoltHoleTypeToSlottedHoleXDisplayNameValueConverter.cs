using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Muggle.TsExtensions.Common.WPF.Localization;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    
    /// <summary>
    /// Provide a one-way conversion from the BoltHoleType value to the display name of the SlottedHoleX property
    /// </summary>
    public class BoltHoleTypeToSlottedHoleXDisplayNameValueConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value switch {
                0 => TranslationService.Instance["albl_Bolt_Header_Oversized"],
                int => TranslationService.Instance["albl_Bolt_Header_SlottedHoleX"],
                _ => DependencyProperty.UnsetValue
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}