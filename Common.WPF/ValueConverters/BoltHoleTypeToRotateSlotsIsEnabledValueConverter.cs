using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {

    /// <summary>
    /// Provide a one-way conversion from the Bolt.HoleType value to the enable state of the Bolt.RotateSlots property
    /// </summary>
    public class BoltHoleTypeToRotateSlotsIsEnabledValueConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value switch {
                int intValue => intValue == 1,
                Integer integerValue => integerValue.Value == 1,
                _ => DependencyProperty.UnsetValue
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}