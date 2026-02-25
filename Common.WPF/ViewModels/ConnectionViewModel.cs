using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace Muggle.TeklaPlugins.Common.WPF.ViewModels;

public class ConnectionViewModel : NotificationObject {

    private int upDirection = 7;
    [StructuresDialog("zsuunta", typeof(TD.Integer))]
    public int UpDirection {
        get { return upDirection; }
        set {
            upDirection = value <= 0 || value > 7 ? 7 : value;
            OnPropertyChanged("UpDirection");
        }
    }

    private double rotationAngleY = 0.0;
    [StructuresDialog("zang1", typeof(TD.Double))]
    public double RotationAngleY {
        get { return rotationAngleY; }
        set {
            rotationAngleY = value == int.MinValue ? 0.0 : value;
            OnPropertyChanged("RotationAngleY");
        }
    }

    private double rotationAngleX = 0.0;
    [StructuresDialog("zang2", typeof(TD.Double))]
    public double RotationAngleX {
        get { return rotationAngleX; }
        set {
            rotationAngleX = value == int.MinValue ? 0.0 : value;
            OnPropertyChanged("RotationAngleX");
        }
    }

    private int locked = 0;
    [StructuresDialog("OBJECT_LOCKED", typeof(TD.Integer))]
    public int Locked {
        get { return locked; }
        set {
            locked = value == 1 ? 1 : 0;
            OnPropertyChanged("Locked");
        }
    }

    private int @class = -1;
    [StructuresDialog("group_no", typeof(TD.Integer))]
    public int Class {
        get { return @class; }
        set {
            @class = value == int.MinValue ? 0 : value;
            OnPropertyChanged("Class");
        }
    }

    private string connectionCode = string.Empty;
    [StructuresDialog("joint_code", typeof(TD.String))]
    public string ConnectionCode {
        get { return connectionCode; }
        set {
            connectionCode = value ?? string.Empty;
            OnPropertyChanged("ConnectionCode");
        }
    }

    private string autoDefaults = string.Empty;
    [StructuresDialog("ad_root", typeof(TD.String))]
    public string AutoDefaults {
        get { return autoDefaults; }
        set {
            autoDefaults = value ?? string.Empty;
            OnPropertyChanged("AutoDefaults");
        }
    }

    private string autoConnection = string.Empty;
    [StructuresDialog("ac_root", typeof(TD.String))]
    public string AutoConnection {
        get { return autoConnection; }
        set {
            autoConnection = value ?? string.Empty;
            OnPropertyChanged("AutoConnection");
        }
    }
}
