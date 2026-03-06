using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// Converts an enumeration value to a boolean and vice versa for use in data binding scenarios.
    /// </summary>
    /// <remarks>This value converter is typically used to bind an enumeration value to a boolean property in
    /// the UI.  The <see cref="Convert"/> method checks if the provided value matches the parameter and returns a
    /// boolean result.  The <see cref="ConvertBack"/> method converts a boolean value back to the corresponding
    /// enumeration value.</remarks>
    public class EnumToBoolValueConverter : IValueConverter {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value?.Equals(parameter) ?? DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
