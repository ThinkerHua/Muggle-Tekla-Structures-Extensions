using System.ComponentModel;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Muggle.TsExtensions.Common.WPF.ViewModels;

public class NotificationObject : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
