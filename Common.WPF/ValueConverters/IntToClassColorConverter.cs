using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Converts integer values to corresponding color values for use in data binding scenarios, typically to visually
    /// distinguish classes or categories.
    /// </summary>
    /// <remarks>Implements the IValueConverter interface to enable conversion between integer identifiers and
    /// a predefined set of colors. This converter is commonly used in WPF or XAML-based applications to map class or
    /// category indices to distinct colors in the user interface. The mapping is deterministic and ensures that the
    /// same integer value always results in the same color. The converter supports both integer and string
    /// representations of the value to convert. When converting back, it returns the index of the color in the
    /// predefined set, or Binding.DoNothing if the value is not a recognized color.</remarks>
    public class IntToClassColorConverter : IValueConverter {
        private static readonly Color[] Colors = [
            Color.FromRgb(0x00, 0x00, 0x00),
            Color.FromRgb(0x9d, 0x9d, 0xa9),
            Color.FromRgb(0xe7, 0x53, 0x5c),
            Color.FromRgb(0x5b, 0x97, 0x23),
            Color.FromRgb(0x1e, 0x43, 0x83),
            Color.FromRgb(0x21, 0xca, 0x9c),
            Color.FromRgb(0xff, 0xe5, 0x00),
            Color.FromRgb(0x92, 0x38, 0xb1),
            Color.FromRgb(0xb5, 0x67, 0x33),
            Color.FromRgb(0xc0, 0x29, 0x8d),
            Color.FromRgb(0xae, 0xc7, 0x66),
            Color.FromRgb(0x0c, 0x8a, 0xcf),
            Color.FromRgb(0xaf, 0x81, 0xc0),
            Color.FromRgb(0xf4, 0xb1, 0x00),
            Color.FromRgb(0x3d, 0x00, 0xa3),
        ];

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int intValue ||
                value is string strValue && int.TryParse(strValue, out intValue))
                return Colors[IntToColorIndex(intValue)];

            return DependencyProperty.UnsetValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Color color) return Array.IndexOf(Colors, color);

            return Binding.DoNothing;
        }

        private static int IntToColorIndex(int value) {
            return value == 0 ? 0 : Math.Abs(value - 1) % 14 + 1;
        }
    }
}