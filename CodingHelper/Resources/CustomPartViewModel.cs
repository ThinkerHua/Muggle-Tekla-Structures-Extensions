using System;

namespace Muggle.TsExtensions.CodingHelper.Generators {

    /// <summary>
    /// A view model base suitable for custom part plugin of Tekla Structures,
    /// include several commonly used properties.
    /// Inherits from <see cref="NotificationObject"/>, 
    /// so it can send notifications when its properties changed.
    /// </summary>
    public abstract class CustomPartViewModel : NotificationObject {

        private global::Tekla.Structures.Datatype.Integer _onPlane = 0;
        private global::Tekla.Structures.Datatype.Integer _rotation = 1;
        private global::Tekla.Structures.Datatype.Integer _atDepth = 0;
        private global::Tekla.Structures.Datatype.Integer _thirdHandle = 0;

        private global::Tekla.Structures.Datatype.Double _onPlaneValue = 0.0;
        private global::Tekla.Structures.Datatype.Double _rotationValue = 0.0;
        private global::Tekla.Structures.Datatype.Double _atDepthValue = 0.0;
        
        [global::Tekla.Structures.Dialog.StructuresDialog("OnPlane", typeof(global::Tekla.Structures.Datatype.Integer))]
        public global::Tekla.Structures.Datatype.Integer OnPlane {
            get {
                return _onPlane;
            }
            set {
                _onPlane = value < 0 || value > 2 ? 0 : value;
                OnPropertyChanged();
            }
        }
        
        [global::Tekla.Structures.Dialog.StructuresDialog("Rotation", typeof(global::Tekla.Structures.Datatype.Integer))]
        public global::Tekla.Structures.Datatype.Integer Rotation {
            get {
                return _rotation;
            }
            set {
                _rotation = value < 0 || value > 3 ? 1 : value;
                OnPropertyChanged();
            }
        }
        
        [global::Tekla.Structures.Dialog.StructuresDialog("AtDepth", typeof(global::Tekla.Structures.Datatype.Integer))]
        public global::Tekla.Structures.Datatype.Integer AtDepth {
            get {
                return _atDepth;
            }
            set {
                _atDepth = value < 0 || value > 2 ? 0 : value;
                OnPropertyChanged();
            }
        }
        
        [global::Tekla.Structures.Dialog.StructuresDialog("ThirdHandle", typeof(global::Tekla.Structures.Datatype.Integer))]
        public global::Tekla.Structures.Datatype.Integer ThirdHandle {
            get {
                return _thirdHandle;
            }
            set {
                _thirdHandle = value < 0 || value > 2 ? 0 : value;
                OnPropertyChanged();
            }
        }

        [global::Tekla.Structures.Dialog.StructuresDialog("OnPlaneValue", typeof(global::Tekla.Structures.Datatype.Double))]
        public global::Tekla.Structures.Datatype.Double OnPlaneValue {
            get {
                return _onPlaneValue;
            }
            set {
                _onPlaneValue = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }

        [global::Tekla.Structures.Dialog.StructuresDialog("RotationValue", typeof(global::Tekla.Structures.Datatype.Double))]
        public global::Tekla.Structures.Datatype.Double RotationValue {
            get {
                return _rotationValue;
            }
            set {
                _rotationValue = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }

        [global::Tekla.Structures.Dialog.StructuresDialog("AtDepthValue", typeof(global::Tekla.Structures.Datatype.Double))]
        public global::Tekla.Structures.Datatype.Double AtDepthValue {
            get {
                return _atDepthValue;
            }
            set {
                _atDepthValue = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged();
            }
        }
    }

}