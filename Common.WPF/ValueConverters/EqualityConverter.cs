using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    
    /// <summary>
    /// Provide a two-way conversion with equality.
    /// The forward conversion returns whether the source value equals the parameter value.
    /// The backward conversion returns the parameter value if the target value is true; otherwise, it does nothing.
    /// </summary>
    public class EqualityConverter : IValueConverter {
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
