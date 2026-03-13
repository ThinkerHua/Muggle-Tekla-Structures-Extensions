using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    /// <summary>
    /// Provide mutual conversion between the up direction enumeration value in the general component tab 
    /// and the selected index of the ComboBox.<br/>
    /// 提供组件通用选项卡中的向上方向枚举值与ComboBox的选中索引之间的相互转换。
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
