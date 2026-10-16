using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// 提供枚举值和整型值之间的相互转换。
    /// </summary>
    public class EnumToIntValueConverter : IValueConverter {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Enum enumValue && parameter is Type { IsEnum: true } type) {
                return (int)Enum.Parse(type, enumValue.ToString());
            }

            return DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int intValue && parameter is Type { IsEnum: true } type) {
                return Enum.ToObject(type, intValue);
            }

            return Binding.DoNothing;
        }
    }
}