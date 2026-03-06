using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.MainWindow.Converters {
    public class MultiplesValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((double)parameter == 0.0) return DependencyProperty.UnsetValue;
            return (int)((double)value / (double)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((double)parameter == 0.0) return Binding.DoNothing;
            return (double)value / (double)parameter;
        }
    }
}
