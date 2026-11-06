using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Double = Tekla.Structures.Datatype.Double;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// Provides a one-way conversion from inversed Boolean value to opacity.<br/>
    /// 提供反转的布尔值到透明度的单向转换。
    /// </summary>
    /// <remarks>This means that if the input Boolean value is false, the opacity parameter value is output; 
    /// if the input Boolean value is true, the opacity parameter is not output.<br/>
    /// 意味着，如果输入的布尔值为假，则输出透明度参数值；如果输入的布尔值为真，则不输出透明度参数。</remarks>
    public class InversedBoolToOpacityConverter : IValueConverter {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is null || !value.Equals(false) || parameter is null) {
                return DependencyProperty.UnsetValue;
            }

            return parameter switch {
                double d1 => d1,
                Double d2 => d2.Value,
                string str when double.TryParse(str, out var d3) => d3,
                _ => DependencyProperty.UnsetValue
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
