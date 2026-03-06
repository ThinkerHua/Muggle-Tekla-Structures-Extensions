using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    /// <remarks>This value converter is typically used in data binding scenarios where the inverse of a
    /// boolean value  is required, such as toggling visibility or enabling/disabling UI elements.</remarks>
    public class BoolInverterValueConverter : IValueConverter {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b) return !b;
            return DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b) return !b;
            return DependencyProperty.UnsetValue;
        }
    }
}
