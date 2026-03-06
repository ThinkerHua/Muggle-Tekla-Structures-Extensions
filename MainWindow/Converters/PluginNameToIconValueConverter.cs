using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Muggle.TsExtensions.MainWindow.Converters {
    public class PluginNameToIconValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //  用于Tekla Structures目录
            var uriStr = new DirectoryInfo(
                string.Format(
                    @"{0}..\..\..\..\Bitmaps\et_element_{1}.bmp",
                    currentDirectory,
                    value as string))
                .FullName;

            //  用于VS工程目录
            if (!File.Exists(uriStr)) {
                uriStr = new DirectoryInfo(
                    string.Format(
                        @"{0}..\bitmaps\et_element_{1}.bmp",
                        currentDirectory,
                        value as string))
                    .FullName;
            }

            return new BitmapImage(new Uri(uriStr, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
