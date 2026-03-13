/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  MainWindowViewModel.cs: view model for main window of MG1002
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MG1002.ViewModels {
    public class MainWindowViewModel : ConnectionViewModel {

        private string endPlate_profile = "PL30*460*1093";
        [StructuresDialog("prfStr_EndPlate", typeof(TD.String))]
        public string EndPlateProfile {
            get { return endPlate_profile; }
            set {
                endPlate_profile = string.IsNullOrEmpty(value) ? "PL30*460*1093" : value;
                OnPropertyChanged("EndPlateProfile");
            }
        }

        private string flangeStiffener_profile = "PL10*115*175";
        [StructuresDialog("prfStr_STIF_FLNG", typeof(TD.String))]
        public string FlangeStiffenerProfile {
            get { return flangeStiffener_profile; }
            set {
                flangeStiffener_profile = string.IsNullOrEmpty(value) ? "PL10*115*175" : value;
                OnPropertyChanged("FlangeStiffenerProfile");
            }
        }

        private int webStiffener_type = 0;
        [StructuresDialog("type_STIF_WEB", typeof(TD.Integer))]
        public int WebStiffenerType {
            get { return webStiffener_type; }
            set {
                webStiffener_type = value == 1 ? 1 : 0;
                OnPropertyChanged("WebStiffenerType");
            }
        }

        private string webStiffener_profile = "PL10*225*225";
        [StructuresDialog("prfStr_STIF_WEB", typeof(TD.String))]
        public string WebStiffenerProfile {
            get { return webStiffener_profile; }
            set {
                webStiffener_profile = string.IsNullOrEmpty(value) ? "PL10*225*225" : value;
                OnPropertyChanged("WebStiffenerProfile");
            }
        }

        private double chamfer_inside = 20.0;
        [StructuresDialog("cmf_Inside", typeof(TD.Double))]
        public double ChamferSizeInside {
            get { return chamfer_inside; }
            set {
                chamfer_inside = value == int.MinValue ? 20.0 : value;
                OnPropertyChanged("ChamferSizeInside");
            }
        }

        private double chamfer_outside = 25.0;
        [StructuresDialog("cmf_Outside", typeof(TD.Double))]
        public double ChamferSizeOutside {
            get { return chamfer_outside; }
            set {
                chamfer_outside = value == int.MinValue ? 25.0 : value;
                OnPropertyChanged("ChamferSizeOutside");
            }
        }

        private string webStiffeners_distance_list = "233 387";
        [StructuresDialog("disListStr_STIF_WEB", typeof(TD.String))]
        public string WebStiffenersDistanceList {
            get { return webStiffeners_distance_list; }
            set {
                webStiffeners_distance_list = string.IsNullOrEmpty(value) ? "233 387" : value;
                OnPropertyChanged("WebStiffenersDistanceList");
            }
        }

        private string verticalPlate_profile = "PL18";
        [StructuresDialog("prfStr_VERT", typeof(TD.String))]
        public string VerticalPlateProfile {
            get { return verticalPlate_profile; }
            set {
                verticalPlate_profile = string.IsNullOrEmpty(value) ? "PL18" : value;
                OnPropertyChanged("VerticalPlateProfile");
            }
        }

        private string diagonalPlate_profile = string.Empty;
        [StructuresDialog("prfStr_DIAG", typeof(TD.String))]
        public string DiagonalPlateProfile {
            get { return diagonalPlate_profile; }
            set {
                diagonalPlate_profile = value ?? string.Empty;
                OnPropertyChanged("DiagonalPlateProfile");
            }
        }

        private double diagonalPlate_position_1 = 100.0;
        [StructuresDialog("pos_DIAG1", typeof(TD.Double))]
        public double DiagonalPlatePosition1 {
            get { return diagonalPlate_position_1; }
            set {
                diagonalPlate_position_1 = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("DiagonalPlatePosition1");
            }
        }

        private double diagonalPlate_position_2 = 100.0;
        [StructuresDialog("pos_DIAG2", typeof(TD.Double))]
        public double DiagonalPlatePosition2 {
            get { return diagonalPlate_position_2; }
            set {
                diagonalPlate_position_2 = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("DiagonalPlatePosition2");
            }
        }

        private double webSplicingPlate_thickness = 0.0;
        [StructuresDialog("THK_SPLC_WEB", typeof(TD.Double))]
        public double WebSplicingPlateThickness {
            get { return webSplicingPlate_thickness; }
            set {
                webSplicingPlate_thickness = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged("WebSplicingPlateThickness");
            }
        }

        private double distance_webSeam_flangeSeam = 200.0;
        [StructuresDialog("DIS_WSeam_FSeam", typeof(TD.Double))]
        public double DistanceBetweenWebSeamAndFlangeSeam {
            get { return distance_webSeam_flangeSeam; }
            set {
                distance_webSeam_flangeSeam = value == int.MinValue ? 200.0 : value;
                OnPropertyChanged("DistanceBetweenWebSeamAndFlangeSeam");
            }
        }

        private string bolt_standard = "HS10.9";
        [StructuresDialog("bolt_Standard", typeof(TD.String))]
        public string BoltStandard {
            get { return bolt_standard; }
            set {
                bolt_standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                OnPropertyChanged("BoltStandard");
            }
        }

        private TD.Distance bolt_size = new TD.Distance(27.0);
        [StructuresDialog("bolt_Size", typeof(TD.Distance))]
        public TD.Distance BoltSize {
            get { return bolt_size; }
            set {
                bolt_size = value;
                OnPropertyChanged("BoltSize");
            }
        }

        private string bolt_distance_list_x = "60 138 90 130 257 130 90 138";
        [StructuresDialog("disListStr_bolt_X", typeof(TD.String))]
        public string BoltDistanceListX {
            get { return bolt_distance_list_x; }
            set {
                bolt_distance_list_x = string.IsNullOrEmpty(value) ? "60 138 90 130 257 130 90 138" : value;
                OnPropertyChanged("BoltDistanceListX");
            }
        }

        private string bolt_distance_list_y = "220";
        [StructuresDialog("disListStr_bolt_Y", typeof(TD.String))]
        public string BoltDistanceListY {
            get { return bolt_distance_list_y; }
            set {
                bolt_distance_list_y = string.IsNullOrEmpty(value) ? "220" : value;
                OnPropertyChanged("BoltDistanceListY");
            }
        }

        private string material = "Q345B";
        [StructuresDialog("materialStr", typeof(TD.String))]
        public string Material {
            get { return material; }
            set {
                material = string.IsNullOrEmpty(value) ? "Q345B" : value;
                OnPropertyChanged("Material");
            }
        }
    }
}
