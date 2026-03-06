using System;
using System.Globalization;
using System.Windows.Data;

namespace Muggle.TsExtensions.MainWindow.Converters {
    internal class RoundDownValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (double)parameter * ((int)(double)value / (double)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
