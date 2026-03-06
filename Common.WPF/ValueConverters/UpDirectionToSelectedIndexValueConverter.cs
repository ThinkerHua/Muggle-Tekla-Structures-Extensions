using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// 用于将组件通用选项卡中的向上方向枚举值转换为ComboBox的选中索引。
    /// </summary>
    public class UpDirectionToSelectedIndexValueConverter : IValueConverter {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not int index) return DependencyProperty.UnsetValue;

            return 7 - (index < 0 ? 0 : index);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not int upDirectionEnum) return DependencyProperty.UnsetValue;

            return 7 - upDirectionEnum;
        }
    }
}
