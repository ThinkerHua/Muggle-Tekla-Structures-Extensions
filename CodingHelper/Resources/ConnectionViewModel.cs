namespace Muggle.TsExtensions.CodingHelper.Generators {
    
    /// <summary>
    /// A view model base suitable for connection plugin of Tekla Structures,
    /// include several commonly used properties.
    /// Inherits from <see cref="NotificationObject"/>, 
    /// so it can send notifications when its properties changed.
    /// </summary>
    public abstract class ConnectionViewModel : NotificationObject {
        
        private int upDirection = 7;
        [global::Tekla.Structures.Dialog.StructuresDialog("zsuunta", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int UpDirection {
            get {
                return upDirection;
            }
            set {
                upDirection = value <= 0 || value > 7 ? 7 : value;
                OnPropertyChanged();
            }
        }
        
        private double rotationAngleY = 0.0;
        [global::Tekla.Structures.Dialog.StructuresDialog("zang1", typeof(global::Tekla.Structures.Datatype.Double))]
        public double RotationAngleY {
            get {
                return rotationAngleY;
            }
            set {
                rotationAngleY = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }
        
        private double rotationAngleX = 0.0;
        [global::Tekla.Structures.Dialog.StructuresDialog("zang2", typeof(global::Tekla.Structures.Datatype.Double))]
        public double RotationAngleX {
            get {
                return rotationAngleX;
            }
            set {
                rotationAngleX = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }
        
        private int locked = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("OBJECT_LOCKED", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int Locked {
            get {
                return locked;
            }
            set {
                locked = value == 1 ? 1 : 0;
                OnPropertyChanged();
            }
        }
        
        private int @class = -1;
        [global::Tekla.Structures.Dialog.StructuresDialog("group_no", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int Class {
            get {
                return @class;
            }
            set {
                @class = value == int.MinValue ? -1 : value;
                OnPropertyChanged();
            }
        }
        
        private string connectionCode = string.Empty;
        [global::Tekla.Structures.Dialog.StructuresDialog("joint_code", typeof(global::Tekla.Structures.Datatype.String))]
        public string ConnectionCode {
            get {
                return connectionCode;
            }
            set {
                connectionCode = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
        
        private string autoDefaults = string.Empty;
        [global::Tekla.Structures.Dialog.StructuresDialog("ad_root", typeof(global::Tekla.Structures.Datatype.String))]
        public string AutoDefaults {
            get {
                return autoDefaults;
            }
            set {
                autoDefaults = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
        
        private string autoConnection = string.Empty;
        [global::Tekla.Structures.Dialog.StructuresDialog("ac_root", typeof(global::Tekla.Structures.Datatype.String))]
        public string AutoConnection {
            get {
                return autoConnection;
            }
            set {
                autoConnection = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }
    
}