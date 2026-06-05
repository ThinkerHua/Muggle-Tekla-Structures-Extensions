namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// A view model base suitable for detail plugin of Tekla Structures,
    /// include several commonly used properties.
    /// Inherits from <see cref="NotificationObject"/>, 
    /// so it can send notifications when its properties changed.
    /// </summary>
    public abstract class DetailViewModel : NotificationObject {

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

        private int vertical_position = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("vertical_position", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int VerticalPosition {
            get {
                return vertical_position;
            }
            set {
                vertical_position = value < -1 || value > 2 ? 0 : value;
                OnPropertyChanged();
            }
        }

        private int horizontal_position = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("horizontal_position", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int HorizontalPosition {
            get {
                return horizontal_position;
            }
            set {
                horizontal_position = value < -1 || value > 2 ? 0 : value;
                OnPropertyChanged();
            }
        }

        private double vertical_offset = 0.0;
        [global::Tekla.Structures.Dialog.StructuresDialog("vertical_offset", typeof(global::Tekla.Structures.Datatype.Double))]
        public double VerticalOffset {
            get {
                return vertical_offset;
            }
            set {
                vertical_offset = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }

        private double horizontal_offset = 0.0;
        [global::Tekla.Structures.Dialog.StructuresDialog("horizontal_offset", typeof(global::Tekla.Structures.Datatype.Double))]
        public double HorizontalOffset {
            get {
                return horizontal_offset;
            }
            set {
                horizontal_offset = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }

        private int upMiddleLeft = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("UpMiddleLeft", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int UpMiddleLeft {
            get {
                return upMiddleLeft;
            }
            set {
                upMiddleLeft = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 2; HorizontalPosition = 1;
                }
            }
        }

        private int upMiddleMiddle = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("UpMiddleMiddle", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int UpMiddleMiddle {
            get {
                return upMiddleMiddle;
            }
            set {
                upMiddleMiddle = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 2; HorizontalPosition = 2;
                }
            }
        }

        private int upMiddleRight = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("UpMiddleRight", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int UpMiddleRight {
            get {
                return upMiddleRight;
            }
            set {
                upMiddleRight = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 2; HorizontalPosition = -1;
                }
            }
        }

        private int topLeft = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("TopLeft", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int TopLeft {
            get {
                return topLeft;
            }
            set {
                topLeft = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 1; HorizontalPosition = 1;
                }
            }
        }

        private int topMiddle = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("TopMiddle", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int TopMiddle {
            get {
                return topMiddle;
            }
            set {
                topMiddle = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 1; HorizontalPosition = 0;
                }
            }
        }

        private int topRight = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("TopRight", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int TopRight {
            get {
                return topRight;
            }
            set {
                topRight = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 1; HorizontalPosition = -1;
                }
            }
        }

        private int middleLeft = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("MiddleLeft", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int MiddleLeft {
            get {
                return middleLeft;
            }
            set {
                middleLeft = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 0; HorizontalPosition = 1;
                }
            }
        }

        private int middleMiddle = 1;
        [global::Tekla.Structures.Dialog.StructuresDialog("MiddleMiddle", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int MiddleMiddle {
            get {
                return middleMiddle;
            }
            set {
                middleMiddle = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 0; HorizontalPosition = 0;
                }
            }
        }

        private int middleRight = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("MiddleRight", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int MiddleRight {
            get {
                return middleRight;
            }
            set {
                middleRight = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0;
                    BottomLeft = 0; BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = 0; HorizontalPosition = -1;
                }
            }
        }

        private int bottomLeft = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("BottomLeft", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int BottomLeft {
            get {
                return bottomLeft;
            }
            set {
                bottomLeft = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomMiddle = 0; BottomRight = 0;

                    VerticalPosition = -1; HorizontalPosition = 1;
                }
            }
        }

        private int bottomMiddle = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("BottomMiddle", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int BottomMiddle {
            get {
                return bottomMiddle;
            }
            set {
                bottomMiddle = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomRight = 0;

                    VerticalPosition = -1; HorizontalPosition = 0;
                }
            }
        }

        private int bottomRight = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("BottomRight", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int BottomRight {
            get {
                return bottomRight;
            }
            set {
                bottomRight = value == 1 ? 1 : 0;
                OnPropertyChanged();

                if (value == 1) {
                    UpMiddleLeft = 0; UpMiddleMiddle = 0; UpMiddleRight = 0;
                    TopLeft = 0; TopMiddle = 0; TopRight = 0;
                    MiddleLeft = 0; MiddleMiddle = 0; MiddleRight = 0;
                    BottomLeft = 0; BottomMiddle = 0;

                    VerticalPosition = -1; HorizontalPosition = -1;
                }
            }
        }

        private int detail_type = 0;
        [global::Tekla.Structures.Dialog.StructuresDialog("detail_type", typeof(global::Tekla.Structures.Datatype.Integer))]
        public int DetailType {
            get {
                return detail_type;
            }
            set {
                detail_type = value < 0 || value > 2 ? 0 : value;
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
    }

}