using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.Common.WPF.ValueConverters {
    
    /// <summary>
    /// Provide a one-way conversion from the Bolt.Shape value to the visibility state of the Bolt.NumberOfBolts and Bolt.Diameter properties
    /// </summary>
    public class BoltShapeToNumDiamVisibilityConverter : IValueConverter {

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value switch {
                int intValue => intValue == 1 ? Visibility.Visible : Visibility.Hidden,
                Integer integerValue => integerValue.Value == 1 ? Visibility.Visible : Visibility.Hidden,
                _ => DependencyProperty.UnsetValue
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}