using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// A view model base suitable for connection plugin of Tekla Structures,
    /// include several commonly used properties.
    /// Inherits from <see cref="NotificationObject"/>, 
    /// so it can send notifications when its properties changed.
    /// </summary>
    public abstract class ConnectionViewModel : NotificationObject {
        
        private int upDirection = 7;
        [StructuresDialog("zsuunta", typeof(TD.Integer))]
        public int UpDirection {
            get { return upDirection; }
            set {
                upDirection = value <= 0 || value > 7 ? 7 : value;
                OnPropertyChanged();
            }
        }
        
        private double rotationAngleY = 0.0;
        [StructuresDialog("zang1", typeof(TD.Double))]
        public double RotationAngleY {
            get { return rotationAngleY; }
            set {
                rotationAngleY = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }
        
        private double rotationAngleX = 0.0;
        [StructuresDialog("zang2", typeof(TD.Double))]
        public double RotationAngleX {
            get { return rotationAngleX; }
            set {
                rotationAngleX = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }
        
        private int locked = 0;
        [StructuresDialog("OBJECT_LOCKED", typeof(TD.Integer))]
        public int Locked {
            get { return locked; }
            set {
                locked = value == 1 ? 1 : 0;
                OnPropertyChanged();
            }
        }
        
        private int @class = -1;
        [StructuresDialog("group_no", typeof(TD.Integer))]
        public int Class {
            get { return @class; }
            set {
                @class = value == int.MinValue ? 0 : value;
                OnPropertyChanged();
            }
        }
        
        private string connectionCode = string.Empty;
        [StructuresDialog("joint_code", typeof(TD.String))]
        public string ConnectionCode {
            get { return connectionCode; }
            set {
                connectionCode = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
        
        private string autoDefaults = string.Empty;
        [StructuresDialog("ad_root", typeof(TD.String))]
        public string AutoDefaults {
            get { return autoDefaults; }
            set {
                autoDefaults = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
        
        private string autoConnection = string.Empty;
        [StructuresDialog("ac_root", typeof(TD.String))]
        public string AutoConnection {
            get { return autoConnection; }
            set {
                autoConnection = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }
    
}