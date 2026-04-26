using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// A base class that can send notifications when its properties change.
    /// </summary>
    public abstract class NotificationObject : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}