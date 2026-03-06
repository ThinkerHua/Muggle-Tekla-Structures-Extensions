using System.Windows.Controls;

namespace Muggle.TsExtensions.MainWindow.Services {
    public interface INavigationService {
        public void SetFrame(Frame frame);
        public void Navigate(Page page);
    }
}
