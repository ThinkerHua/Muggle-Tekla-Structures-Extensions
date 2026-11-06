using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Provide a one-way conversion from the Weld.Type(Above/Below) value to the enable state of the Weld.AdditionalSize(Above/Below) property
    /// </summary>
    public class WeldTypeToAdditionalSizeIsEnabledConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value switch {
                int intValue => intValue is 18 or 19,
                Integer integerValue => integerValue.Value is 18 or 19,
                _ => DependencyProperty.UnsetValue
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}