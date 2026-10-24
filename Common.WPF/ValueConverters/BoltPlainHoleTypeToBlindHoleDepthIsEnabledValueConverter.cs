using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Provide a one-way conversion from the Bolt.PlainHoleType value to the enable state of the Bolt.BlindHoleDepth property
    /// </summary>
    public class BoltPlainHoleTypeToBlindHoleDepthIsEnabledValueConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int intValue;
            switch (value) {
            case int i:
                intValue = i;
                break;
            case Integer integer:
                intValue = integer.Value;
                break;
            default:
                return DependencyProperty.UnsetValue;
            }

            return intValue == 1;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}