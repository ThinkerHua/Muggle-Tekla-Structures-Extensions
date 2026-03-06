/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2025 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  MainWindowViewModel.cs: view model for main window of KJ2001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System.Collections.Generic;
using Muggle.TsExtensions.Common.WPF.ViewModels;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ2001.ViewModels {
    /// <summary>
    /// Data logic for MainWindow
    /// </summary>
    public class MainWindowViewModel : DetailViewModel {

        private string basePlate_material = "Q355";
        [StructuresDialog("base_MATL", typeof(TD.String))]
        public string BasePlateMaterial {
            get { return basePlate_material; }
            set {
                basePlate_material = string.IsNullOrEmpty(value) ? "Q355" : value;
                OnPropertyChanged("BasePlateMaterial");
            }
        }

        private double basePlate_thickness = 22.0;
        [StructuresDialog("base_THK", typeof(TD.Double))]
        public double BasePlateThickness {
            get { return basePlate_thickness; }
            set {
                basePlate_thickness = value == int.MinValue ? 22.0 : value;
                OnPropertyChanged("BasePlateThickness");
            }
        }

        private double basePlate_width = 840.0;
        [StructuresDialog("base_WID", typeof(TD.Double))]
        public double BasePlateWidth {
            get { return basePlate_width; }
            set {
                basePlate_width = value == int.MinValue ? 840.0 : value;
                OnPropertyChanged("BasePlateWidth");
            }
        }

        private double basePlate_length = 840.0;
        [StructuresDialog("base_LEN", typeof(TD.Double))]
        public double BasePlateLength {
            get { return basePlate_length; }
            set {
                basePlate_length = value == int.MinValue ? 840.0 : value;
                OnPropertyChanged("BasePlateLength");
            }
        }

        private string anchorRod_material = "Q355";
        [StructuresDialog("anchor_MATL", typeof(TD.String))]
        public string AnchorRodMaterial {
            get { return anchorRod_material; }
            set {
                anchorRod_material = string.IsNullOrEmpty(value) ? "Q355" : value;
                OnPropertyChanged("AnchorRodMaterial");
            }
        }

        private double anchorRod_size = 24.0;
        [StructuresDialog("anchor_size", typeof(TD.Double))]
        public double AnchorRodSize {
            get { return anchorRod_size; }
            set {
                anchorRod_size = value == int.MinValue ? 24.0 : value;
                OnPropertyChanged("AnchorRodSize");
            }
        }

        private double anchorRod_tolerance = 2.0;
        [StructuresDialog("anchor_TOL", typeof(TD.Double))]
        public double AnchorRodTolerance {
            get { return anchorRod_tolerance; }
            set {
                anchorRod_tolerance = value == int.MinValue ? 2.0 : value;
                OnPropertyChanged("AnchorRodTolerance");
            }
        }

        private double anchorRod_length1 = 120.0;
        [StructuresDialog("anchor_LEN1", typeof(TD.Double))]
        public double AnchorRodLength1 {
            get { return anchorRod_length1; }
            set {
                anchorRod_length1 = value == int.MinValue ? 120.0 : value;
                OnPropertyChanged("AnchorRodLength1");
            }
        }

        private double anchorRod_length2 = 50.0;
        [StructuresDialog("anchor_LEN2", typeof(TD.Double))]
        public double AnchorRodLength2 {
            get { return anchorRod_length2; }
            set {
                anchorRod_length2 = value == int.MinValue ? 50.0 : value;
                OnPropertyChanged("AnchorRodLength2");
            }
        }

        private double anchorRod_length3 = 510.0;
        [StructuresDialog("anchor_LEN3", typeof(TD.Double))]
        public double AnchorRodLength3 {
            get { return anchorRod_length3; }
            set {
                anchorRod_length3 = value == int.MinValue ? 510.0 : value;
                OnPropertyChanged("AnchorRodLength3");
            }
        }

        private double anchorRod_length4 = 330.0;
        [StructuresDialog("anchor_LEN4", typeof(TD.Double))]
        public double AnchorRodLength4 {
            get { return anchorRod_length4; }
            set {
                anchorRod_length4 = value == int.MinValue ? 330.0 : value;
                OnPropertyChanged("AnchorRodLength4");
            }
        }

        private double anchorRod_length5 = 100.0;
        [StructuresDialog("anchor_LEN5", typeof(TD.Double))]
        public double AnchorRodLength5 {
            get { return anchorRod_length5; }
            set {
                anchorRod_length5 = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("AnchorRodLength5");
            }
        }

        private double anchorRod_distance_to_edge = 50.0;
        [StructuresDialog("anchor_D2E", typeof(TD.Double))]
        public double AnchorRodDistanceToEdge {
            get { return anchorRod_distance_to_edge; }
            set {
                anchorRod_distance_to_edge = value == int.MinValue ? 50.0 : value;
                OnPropertyChanged("AnchorRodDistanceToEdge");
            }
        }

        private string washerPlate_material = "Q355";
        [StructuresDialog("WSHRPLT_MATL", typeof(TD.String))]
        public string WasherPlateMaterial {
            get { return washerPlate_material; }
            set {
                washerPlate_material = string.IsNullOrEmpty(value) ? "Q355" : value;
                OnPropertyChanged("WasherPlateMaterial");
            }
        }

        private double washerPlate_thickness = 22.0;
        [StructuresDialog("WSHRPLT_THK", typeof(TD.Double))]
        public double WasherPlateThickness {
            get { return washerPlate_thickness; }
            set {
                washerPlate_thickness = value == int.MinValue ? 22.0 : value;
                OnPropertyChanged("WasherPlateThickness");
            }
        }

        private double washerPlate_width = 70.0;
        [StructuresDialog("WSHRPLT_WID", typeof(TD.Double))]
        public double WasherPlateWidth {
            get { return washerPlate_width; }
            set {
                washerPlate_width = value == int.MinValue ? 70.0 : value;
                OnPropertyChanged("WasherPlateWidth");
            }
        }

        private double washerPlate_holeDiameter = 26.0;
        [StructuresDialog("WSHRPLT_Hole_DIA", typeof(TD.Double))]
        public double WasherPlateHoleDiameter {
            get { return washerPlate_holeDiameter; }
            set {
                washerPlate_holeDiameter = value == int.MinValue ? 26.0 : value;
                OnPropertyChanged("WasherPlateHoleDiameter");
            }
        }

        private string stud_standard = "STUD";
        [StructuresDialog("stud_STD", typeof(TD.String))]
        public string StudStandard {
            get { return stud_standard; }
            set {
                stud_standard = string.IsNullOrEmpty(value) ? "STUD" : value;
                OnPropertyChanged("StudStandard");
                OnPropertyChanged("StudLengths");
            }
        }

        private TD.Distance stud_size = new TD.Distance(16.0);
        [StructuresDialog("stud_size", typeof(TD.Distance))]
        public TD.Distance StudSize {
            get { return stud_size; }
            set {
                stud_size = value;
                OnPropertyChanged("StudSize");
                OnPropertyChanged("StudLengths");
            }
        }

        public List<double> StudLengths {
            get {
                var catalogHandler = new CatalogHandler();
                var boltItemEnumerator = catalogHandler.GetBoltItems();
                BoltItem boltItem;
                while (boltItemEnumerator.MoveNext()) {
                    boltItem = boltItemEnumerator.Current;
                    if (boltItem.Standard == StudStandard && boltItem.Size == StudSize.Value) {
                        return boltItem.Lengths;
                    }
                }

                return new List<double>();
            }
        }

        private double stud_length = 80.0;
        [StructuresDialog("stud_LEN", typeof(TD.Double))]
        public double StudLength {
            get { return stud_length; }
            set {
                stud_length = value == int.MinValue ? 80.0 : value;
                OnPropertyChanged("StudLength");
            }
        }

        private string stud_distanceList_X = "200";
        [StructuresDialog("stud_DISLST_X", typeof(TD.String))]
        public string StudDistanceListX {
            get { return stud_distanceList_X; }
            set {
                stud_distanceList_X = string.IsNullOrEmpty(value) ? "200" : value;
                OnPropertyChanged("StudDistanceListX");
            }
        }

        private string stud_distanceList_Y = "200";
        [StructuresDialog("stud_DISLST_Y", typeof(TD.String))]
        public string StudDistanceListY {
            get { return stud_distanceList_Y; }
            set {
                stud_distanceList_Y = string.IsNullOrEmpty(value) ? "200" : value;
                OnPropertyChanged("StudDistanceListY");
            }
        }

        private double stud_offset_XZ = 100.0;
        [StructuresDialog("stud_offset_XZ", typeof(TD.Double))]
        public double StudOffsetXZ {
            get { return stud_offset_XZ; }
            set {
                stud_offset_XZ = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("StudOffsetXZ");
            }
        }

        private double stud_offset_YZ = 100.0;
        [StructuresDialog("stud_offset_YZ", typeof(TD.Double))]
        public double StudOffsetYZ {
            get { return stud_offset_YZ; }
            set {
                stud_offset_YZ = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("StudOffsetYZ");
            }
        }

        private string stud_distanceList_XZ = "8*200";
        [StructuresDialog("stud_DISLST_XZ", typeof(TD.String))]
        public string StudDistanceListXZ {
            get { return stud_distanceList_XZ; }
            set {
                stud_distanceList_XZ = string.IsNullOrEmpty(value) ? "8*200" : value;
                OnPropertyChanged("StudDistanceListXZ");
            }
        }

        private string stud_distanceList_YZ = "8*200";
        [StructuresDialog("stud_DISLST_YZ", typeof(TD.String))]
        public string StudDistanceListYZ {
            get { return stud_distanceList_YZ; }
            set {
                stud_distanceList_YZ = string.IsNullOrEmpty(value) ? "8*200" : value;
                OnPropertyChanged("StudDistanceListYZ");
            }
        }

        private string innerStiffener_material = "Q355";
        [StructuresDialog("stif_MATL", typeof(TD.String))]
        public string InnerStiffenerMaterial {
            get { return innerStiffener_material; }
            set {
                innerStiffener_material = string.IsNullOrEmpty(value) ? "Q355" : value;
                OnPropertyChanged("InnerStiffenerMaterial");
            }
        }

        private double innerStiffener_thickness = 20.0;
        [StructuresDialog("stif_THK", typeof(TD.Double))]
        public double InnerStiffenerThickness {
            get { return innerStiffener_thickness; }
            set {
                innerStiffener_thickness = value == int.MinValue ? 20.0 : value;
                OnPropertyChanged("InnerStiffenerThickness");
            }
        }

        private double innerStiffener_distance_to_base = 1800.0;
        [StructuresDialog("stif_D2B", typeof(TD.Double))]
        public double InnerStiffenerDistanceToBase {
            get { return innerStiffener_distance_to_base; }
            set {
                innerStiffener_distance_to_base = value == int.MinValue ? 1800.0 : value;
                OnPropertyChanged("InnerStiffenerDistanceToBase");
            }
        }

        private double pouringtHole_diameter = 200.0;
        [StructuresDialog("pouringHole_DIA", typeof(TD.Double))]
        public double PouringHoleDiameter {
            get { return pouringtHole_diameter; }
            set {
                pouringtHole_diameter = value == int.MinValue ? 200.0 : value;
                OnPropertyChanged("PouringHoleDiameter");
            }
        }

        private double exhaustHole_diameter = 25.0;
        [StructuresDialog("exhaustHole_DIA", typeof(TD.Double))]
        public double ExhaustHoleDiameter {
            get { return exhaustHole_diameter; }
            set {
                exhaustHole_diameter = value == int.MinValue ? 25.0 : value;
                OnPropertyChanged("ExhaustHoleDiameter");
            }
        }

        private double exhaustHole_position_X = 100.0;
        [StructuresDialog("exhaustHole_POS_X", typeof(TD.Double))]
        public double ExhaustHolePositionX {
            get { return exhaustHole_position_X; }
            set {
                exhaustHole_position_X = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("ExhaustHolePositionX");
            }
        }

        private double exhaustHole_position_Y = 100.0;
        [StructuresDialog("exhaustHole_POS_Y", typeof(TD.Double))]
        public double ExhaustHolePositionY {
            get { return exhaustHole_position_Y; }
            set {
                exhaustHole_position_Y = value == int.MinValue ? 100.0 : value;
                OnPropertyChanged("ExhaustHolePositionY");
            }
        }

    }
}
