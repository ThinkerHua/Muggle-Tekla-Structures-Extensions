using System;
using System.Windows.Controls;

namespace Muggle.TsExtensions.MainWindow.Services {
    public class NavigationService : INavigationService {
        private Frame frame;

        public void SetFrame(Frame frame) {
            this.frame = frame ?? throw new ArgumentNullException(nameof(frame), "Frame cannot be null.");
        }

        public void Navigate(Page page) {
            if (frame == null) throw new InvalidOperationException("Frame is not set. Call SetFrame() first.");
            if (page == null) throw new ArgumentNullException(nameof(page), "Page cannot be null.");
            frame.Navigate(page);
        }
    }
}
