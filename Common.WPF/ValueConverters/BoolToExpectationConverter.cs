using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Provides a one-way conversion from bool to expectation value.
    /// If source value is true, the converter returns the parameter value. 
    /// If source value is false, the converter returns the default value of the parameter type.
    /// </summary>
    public class BoolToExpectationConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is true)
                return parameter;

            switch (parameter) {
            case string str when !string.IsNullOrEmpty(str):
                if (int.TryParse(str, out _)) return 0;
                if (double.TryParse(str, out _)) return 0.0;
                break;
            case ValueType:
                return Activator.CreateInstance(parameter.GetType());
            }

            return DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}