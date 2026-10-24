using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Muggle.TsExtensions.Common.WPF.Localization;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Provide a one-way conversion from the Bolt.HoleType value to the display name of the Bolt.SlottedHoleX property
    /// </summary>
    public class BoltHoleTypeToSlottedHoleXDisplayNameValueConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int intValue;
            switch (value) {
            case int i:
                intValue = i;
                break;
            case Integer integer:
                intValue = integer.Value;
                break;
            default:
                return DependencyProperty.UnsetValue;
            }

            return intValue switch {
                0 => TranslationService.Instance["albl_Bolt_Header_Oversized"],
                3 => TranslationService.Instance["albl_Bolt_Header_CoreHoleSize"],
                _ => TranslationService.Instance["albl_Bolt_Header_SlottedHoleX"],
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}