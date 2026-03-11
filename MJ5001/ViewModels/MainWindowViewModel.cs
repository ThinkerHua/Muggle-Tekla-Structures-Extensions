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
 *  MainWindowViewModel.cs: view model for main window of MJ5001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.MJ5001.ViewModels {
    /// <summary>
    /// Data logic for MainWindow
    /// </summary>
    public class MainWindowViewModel : NotificationObject {
        private double gap = 15.0;
        [StructuresDialog("gap", typeof(TD.Double))]
        public double Gap {
            get => gap;
            set {
                gap = value == int.MinValue ? 15.0 : value;
                OnPropertyChanged("Gap");
            }
        }

        private double ratHole_radius = 30.0;
        [StructuresDialog("ratHole_radius", typeof(TD.Double))]
        public double RatHoleRadius {
            get => ratHole_radius;
            set {
                ratHole_radius = value == int.MinValue ? 30.0 : value;
                OnPropertyChanged("RatHoleRadius");
            }
        }

        private double embedment_thickness = 20.0;
        [StructuresDialog("embedment_THK", typeof(TD.Double))]
        public double EmbedmentThickness {
            get => embedment_thickness;
            set {
                embedment_thickness = value == int.MinValue ? 20.0 : value;
                OnPropertyChanged("EmbedmentThickness");
            }
        }

        private double embedment_width = 300.0;
        [StructuresDialog("embedment_WDTH", typeof(TD.Double))]
        public double EmbedmentWidth {
            get => embedment_width;
            set {
                embedment_width = value == int.MinValue ? 300.0 : value;
                OnPropertyChanged("EmbedmentWidth");
            }
        }

        private double embedment_exten = 90.0;
        [StructuresDialog("embedment_EXTN", typeof(TD.Double))]
        public double EmbedmentExten {
            get => embedment_exten;
            set {
                embedment_exten = value == int.MinValue ? 90.0 : value;
                OnPropertyChanged("EmbedmentExten");
            }
        }

        private string embedment_material = "Q345B";
        [StructuresDialog("embedment_MATL", typeof(TD.String))]
        public string EmbedmentMaterial {
            get => embedment_material;
            set {
                embedment_material = string.IsNullOrEmpty(value) ? "Q345B" : value;
                OnPropertyChanged("EmbedmentMaterial");
            }
        }

        private double anchorRod_length = 150.0;
        [StructuresDialog("anchorRod_LEN", typeof(TD.Double))]
        public double AnchorRodLength {
            get => anchorRod_length;
            set {
                anchorRod_length = value == int.MinValue ? 150.0 : value;
                OnPropertyChanged("AnchorRodLength");
            }
        }

        private double anchorRod_size = 20.0;
        [StructuresDialog("anchorRod_size", typeof(TD.Double))]
        public double AnchorRodSize {
            get => anchorRod_size;
            set {
                anchorRod_size = value == int.MinValue ? 20.0 : value;
                OnPropertyChanged("AnchorRodSize");
            }
        }

        private string anchorRod_material = "Q345B";
        [StructuresDialog("anchorRod_MATL", typeof(TD.String))]
        public string AnchorRodMaterial {
            get => anchorRod_material;
            set {
                anchorRod_material = string.IsNullOrEmpty(value) ? "Q345B" : value;
                OnPropertyChanged("AnchorRodMaterial");
            }
        }

        private string anchorRod_disListStr_X = "250 4*125";
        [StructuresDialog("anchorRod_disList_X", typeof(TD.String))]
        public string AnchorRodDistanceListX {
            get => anchorRod_disListStr_X;
            set {
                anchorRod_disListStr_X = string.IsNullOrEmpty(value) ? "250 4*125" : value;
                OnPropertyChanged("AnchorRodDistanceListX");
            }
        }

        private string anchorRod_disListStr_Y = "160";
        [StructuresDialog("anchorRod_disList_Y", typeof(TD.String))]
        public string AnchorRodDistanceListY {
            get => anchorRod_disListStr_Y;
            set {
                anchorRod_disListStr_Y = string.IsNullOrEmpty(value) ? "160" : value;
                OnPropertyChanged("AnchorRodDistanceListY");
            }
        }

        private double cleat_thickness = 12.0;
        [StructuresDialog("cleat_THK", typeof(TD.Double))]
        public double CleatThickness {
            get => cleat_thickness;
            set {
                cleat_thickness = value == int.MinValue ? 12.0 : value;
                OnPropertyChanged("CleatThickness");
            }
        }

        private double cleat_width = 95.0;
        [StructuresDialog("cleat_WDTH", typeof(TD.Double))]
        public double CleatWidth {
            get => cleat_width;
            set {
                cleat_width = value == int.MinValue ? 95.0 : value;
                OnPropertyChanged("CleatWidth");
            }
        }

        private double cleat_dis_with_innerEdge = 40.0;
        [StructuresDialog("cleat_dis_innerEdge", typeof(TD.Double))]
        public double CleatDistanceWithInnerEdge {
            get => cleat_dis_with_innerEdge;
            set {
                cleat_dis_with_innerEdge = value == int.MinValue ? 40.0 : value;
                OnPropertyChanged("CleatDistanceWithInnerEdge");
            }
        }

        private string cleat_material = "Q345B";
        [StructuresDialog("cleat_MATL", typeof(TD.String))]
        public string CleatMaterial {
            get => cleat_material;
            set {
                cleat_material = string.IsNullOrEmpty(value) ? "Q345B" : value;
                OnPropertyChanged("CleatMaterial");
            }
        }

        private string bolt_standard = "HS10.9";
        [StructuresDialog("bolt_STD", typeof(TD.String))]
        public string BoltStandard {
            get => bolt_standard;
            set {
                bolt_standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                OnPropertyChanged("BoltStandard");
            }
        }

        private TD.Distance bolt_size = new TD.Distance(20.0);
        [StructuresDialog("bolt_size", typeof(TD.Distance))]
        public TD.Distance BoltSize {
            get => bolt_size;
            set {
                bolt_size = value;
                OnPropertyChanged("BoltSize");
            }
        }

        private string bolt_disListStr_X = "40 7*70";
        [StructuresDialog("bolt_disList_X", typeof(TD.String))]
        public string BoltDistanceListX {
            get => bolt_disListStr_X;
            set {
                bolt_disListStr_X = string.IsNullOrEmpty(value) ? "40 7*70" : value;
                OnPropertyChanged("BoltDistanceListX");
            }
        }

        private string bolt_disListStr_Y = "0";
        [StructuresDialog("bolt_disList_Y", typeof(TD.String))]
        public string BoltDistanceListY {
            get => bolt_disListStr_Y;
            set {
                bolt_disListStr_Y = string.IsNullOrEmpty(value) ? "0" : value;
                OnPropertyChanged("BoltDistanceListY");
            }
        }

        private int group_no = -1;
        [StructuresDialog("group_no", typeof(TD.Integer))]
        public int GroupNo {
            get => group_no;
            set {
                group_no = value == int.MinValue ? 0 : value;
                OnPropertyChanged("GroupNo");
            }
        }

    }
}
