using System.Windows;

namespace Muggle.TsExtensions.MainWindow.Services {
    public class MessageBoxService : IMessageBoxService {
        public void ShowError(string message) {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
