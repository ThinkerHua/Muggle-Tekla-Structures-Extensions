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
 *  MainWindowViewModel.cs: view model for main window of KJ1001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ1001.ViewModels {
    public class MainWindowViewModel : ConnectionViewModel {
        private int type = 0;
        [StructuresDialog("type", typeof(TD.Integer))]
        public int Type {
            get { return type; }
            set {
                type = value < 0 || value > 2 ? 0 : value;
                OnPropertyChanged("Type");
            }
        }

        private double innerStiffener_thickness = 0;
        [StructuresDialog("innerSTF_THK", typeof(TD.Double))]
        public double InnerStiffenerThickness {
            get { return innerStiffener_thickness; }
            set {
                innerStiffener_thickness = value == int.MinValue ? -1 : value;
                OnPropertyChanged("InnerStiffenerThickness");
            }
        }

        private double innerStiffener_chamferSize = 25.0;
        [StructuresDialog("innerSTF_chamfer", typeof(TD.Double))]
        public double InnerStiffenerChamferSize {
            get { return innerStiffener_chamferSize; }
            set {
                innerStiffener_chamferSize = value == int.MinValue ? 25.0 : value;
                OnPropertyChanged("InnerStiffenerChamferSize");
            }
        }

        private string moreInnerStiffeners_distanceList = string.Empty;
        [StructuresDialog("moreSTF_DISLST", typeof(TD.String))]
        public string MoreInnerStiffenersDistanceList {
            get { return moreInnerStiffeners_distanceList; }
            set {
                moreInnerStiffeners_distanceList = value ?? string.Empty;
                OnPropertyChanged("MoreInnerStiffenersDistanceList");
            }
        }

        private double thickenedStiffener_thickness = 0.0;
        [StructuresDialog("THKDSTF_THK", typeof(TD.Double))]
        public double ThickenedStiffenerThickness {
            get { return thickenedStiffener_thickness; }
            set {
                thickenedStiffener_thickness = value == int.MinValue ? 0.0 : value;
                OnPropertyChanged("ThickenedStiffenerThickness");
            }
        }

        private double thickenedStiffener_extensionLength = 150.0;
        [StructuresDialog("THKDSTF_EXTLEN", typeof(TD.Double))]
        public double ThickenedStiffenerExtensionLength {
            get { return thickenedStiffener_extensionLength; }
            set {
                thickenedStiffener_extensionLength = value == int.MinValue ? 150.0 : value;
                OnPropertyChanged("ThickenedStiffenerExtensionLength");
            }
        }

        private string stiffener_material = string.Empty;
        [StructuresDialog("STF_MATL", typeof(TD.String))]
        public string StiffenerMaterial {
            get { return stiffener_material; }
            set {
                stiffener_material = value ?? string.Empty;
                OnPropertyChanged("StiffenerMaterial");
            }
        }

        private double ratHole_radius = 35.0;
        [StructuresDialog("ratHole_radius", typeof(TD.Double))]
        public double RatHoleRadius {
            get { return ratHole_radius; }
            set {
                ratHole_radius = value == int.MinValue ? 35.0 : value;
                OnPropertyChanged("RatHoleRadius");
            }
        }

        private double gap = 0.0;
        [StructuresDialog("gap", typeof(TD.Double))]
        public double Gap {
            get { return gap; }
            set {
                gap = value == int.MinValue
                    ? type switch {
                        0 => 0.0,
                        1 => 15.0,
                        2 => 10.0,
                        _ => 0.0
                    }
                    : value;
                OnPropertyChanged("Gap");
            }
        }

        private double weld_angle = 30.0;
        [StructuresDialog("weld_angle", typeof(TD.Double))]
        public double WeldAngle {
            get { return weld_angle; }
            set {
                weld_angle = value == int.MinValue ? 30.0 : value;
                OnPropertyChanged("WeldAngle");
            }
        }

        private double weld_root_face = 2.0;
        [StructuresDialog("root_face", typeof(TD.Double))]
        public double WeldRootFace {
            get { return weld_root_face; }
            set {
                weld_root_face = value == int.MinValue ? 2.0 : value;
                OnPropertyChanged("WeldRootFace");
            }
        }

        private double weld_root_opening = 6.0;
        [StructuresDialog("root_opening", typeof(TD.Double))]
        public double WeldRootOpening {
            get { return weld_root_opening; }
            set {
                weld_root_opening = value == int.MinValue ? 6.0 : value;
                OnPropertyChanged("WeldRootOpening");
            }
        }

        private double cover_thickness = 6.0;
        [StructuresDialog("cover_THK", typeof(TD.Double))]
        public double CoverThickness {
            get { return cover_thickness; }
            set {
                cover_thickness = value == int.MinValue ? 6.0 : value;
                OnPropertyChanged("CoverThickness");
            }
        }

        private double cover_length1 = 50.0;
        [StructuresDialog("cover_LEN1", typeof(TD.Double))]
        public double CoverLength1 {
            get { return cover_length1; }
            set {
                cover_length1 = value == int.MinValue ? 50.0 : value;
                OnPropertyChanged("CoverLength1");
            }
        }

        private double cover_length2 = 350.0;
        [StructuresDialog("cover_LEN2", typeof(TD.Double))]
        public double CoverLength2 {
            get { return cover_length2; }
            set {
                cover_length2 = value == int.MinValue ? 35.0 : value;
                OnPropertyChanged("CoverLength2");
            }
        }

        private double topCover_width1 = 52.0;
        [StructuresDialog("topCover_WD1", typeof(TD.Double))]
        public double TopCoverWidth1 {
            get { return topCover_width1; }
            set {
                topCover_width1 = value == int.MinValue ? 52.0 : value;
                OnPropertyChanged("TopCoverWidth1");
            }
        }

        private double topCover_width2 = 160.0;
        [StructuresDialog("topCover_WD2", typeof(TD.Double))]
        public double TopCoverWidth2 {
            get { return topCover_width2; }
            set {
                topCover_width2 = value == int.MinValue ? 160.0 : value;
                OnPropertyChanged("TopCoverWidth2");
            }
        }

        private double bottomCover_width1 = 88.0;
        [StructuresDialog("BTMCover_WD1", typeof(TD.Double))]
        public double BottomCoverWidth1 {
            get { return bottomCover_width1; }
            set {
                bottomCover_width1 = value == int.MinValue ? 88.0 : value;
                OnPropertyChanged("BottomCoverWidth1");
            }
        }

        private double bottomCover_width2 = 160.0;
        [StructuresDialog("BTMCover_WD2", typeof(TD.Double))]
        public double BottomCoverWidth2 {
            get { return bottomCover_width2; }
            set {
                bottomCover_width2 = value == int.MinValue ? 160.0 : value;
                OnPropertyChanged("BottomCoverWidth2");
            }
        }

        private string cover_material = string.Empty;
        [StructuresDialog("cover_MATL", typeof(TD.String))]
        public string CoverMaterial {
            get { return cover_material; }
            set {
                cover_material = value ?? string.Empty;
                OnPropertyChanged("CoverMaterial");
            }
        }

        private int webConnectionPlate_creationEnum = 0;
        [StructuresDialog("webCNXPL_enum", typeof(TD.Integer))]
        public int WebConnectionPlateCreationEnum {
            get { return webConnectionPlate_creationEnum; }
            set {
                webConnectionPlate_creationEnum = value < 0 || value > 2 ? 0 : value;
                OnPropertyChanged("WebConnectionPlateCreationEnum");
            }
        }

        private double webConnectionPlate_thickness = 16.0;
        [StructuresDialog("webCNXPL_THK", typeof(TD.Double))]
        public double WebConnectionPlateThickness {
            get { return webConnectionPlate_thickness; }
            set {
                webConnectionPlate_thickness = value == int.MinValue ? 16.0 : value;
                OnPropertyChanged("WebConnectionPlateThickness");
            }
        }

        private string connectionPlate_material = string.Empty;
        [StructuresDialog("CNXPL_MATL", typeof(TD.String))]
        public string ConnectionPlateMaterial {
            get { return connectionPlate_material; }
            set {
                connectionPlate_material = value ?? string.Empty;
                OnPropertyChanged("ConnectionPlateMaterial");
            }
        }

        private string webBolt_standard = "HS10.9";
        [StructuresDialog("webBolt_STD", typeof(TD.String))]
        public string WebBoltStandard {
            get { return webBolt_standard; }
            set {
                webBolt_standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                OnPropertyChanged("WebBoltStandard");
            }
        }

        private TD.Distance webBolt_size = new TD.Distance(20.0);
        [StructuresDialog("webBolt_size", typeof(TD.Distance))]
        public TD.Distance WebBoltSize {
            get { return webBolt_size; }
            set {
                webBolt_size = value;
                OnPropertyChanged("WebBoltSize");
            }
        }

        private string webPosition_distanceList_X = "40 70 40";
        [StructuresDialog("webPOS_X", typeof(TD.String))]
        public string WebPositionDistanceListX {
            get { return webPosition_distanceList_X; }
            set {
                webPosition_distanceList_X = value ??
                type switch {
                    0 => webPosition_distanceList_X = "40 70 40",
                    1 => webPosition_distanceList_X = "50 3*70 110 3*70 50",
                    2 => webPosition_distanceList_X = "40 70 90 70 40",
                    _ => webPosition_distanceList_X = "40 70 40"
                };
                OnPropertyChanged("WebPositionDistanceListX");
            }
        }

        private string webPosition_distanceList_Y = "61 59 8*70 59";
        [StructuresDialog("webPOS_Y", typeof(TD.String))]
        public string WebPositionDistanceListY {
            get { return webPosition_distanceList_Y; }
            set {
                webPosition_distanceList_Y = value ??
                type switch {
                    0 => "61 59 8*70 59",
                    1 => "77 84 11*80 84",
                    2 => "55 50 7*70 50",
                    _ => "61 59 8*70 59"
                };
                OnPropertyChanged("WebPositionDistanceListY");
            }
        }

        private double shortBeam_length = 1200.0;
        [StructuresDialog("shortBeam_LEN", typeof(TD.Double))]
        public double ShortBeamLength {
            get { return shortBeam_length; }
            set {
                shortBeam_length = value == int.MinValue ? 1200.0 : value;
                OnPropertyChanged("ShortBeamLength");
            }
        }

        private string shortBeam_profile = string.Empty;
        [StructuresDialog("shortBeam_PRF", typeof(TD.String))]
        public string ShortBeamProfile {
            get { return shortBeam_profile; }
            set {
                shortBeam_profile = value ?? string.Empty;
                OnPropertyChanged("ShortBeamProfile");
            }
        }

        private string shortBeam_material = string.Empty;
        [StructuresDialog("shortBeam_MATL", typeof(TD.String))]
        public string ShortBeamMaterial {
            get { return shortBeam_material; }
            set {
                shortBeam_material = value ?? string.Empty;
                OnPropertyChanged("ShortBeamMaterial");
            }
        }

        private double outterFlangeConnectionPlate_thickness = 25.0;
        [StructuresDialog("outFLNGCNXPL_THK", typeof(TD.Double))]
        public double OutterFlangeConnectionPlateThickness {
            get { return outterFlangeConnectionPlate_thickness; }
            set {
                outterFlangeConnectionPlate_thickness = value == int.MinValue ? 25.0 : value;
                OnPropertyChanged("OutterFlangeConnectionPlateThickness");
            }
        }

        private double innerFlangeConnectionPlate_thickness = 30.0;
        [StructuresDialog("innerFLNGCNXPL_THK", typeof(TD.Double))]
        public double InnerFlangeConnectionPlateThickness {
            get { return innerFlangeConnectionPlate_thickness; }
            set {
                innerFlangeConnectionPlate_thickness = value == int.MinValue ? 30.0 : value;
                OnPropertyChanged("InnerFlangeConnectionPlateThickness");
            }
        }

        private string flangeBolt_standard = "HS10.9";
        [StructuresDialog("FLNGBolt_STD", typeof(TD.String))]
        public string FlangeBoltStandard {
            get { return flangeBolt_standard; }
            set {
                flangeBolt_standard = string.IsNullOrEmpty(value) ? "HS10.9" : value;
                OnPropertyChanged("FlangeBoltStandard");
            }
        }

        private TD.Distance flangeBolt_size = new TD.Distance(20.0);
        [StructuresDialog("FLNGBolt_size", typeof(TD.Distance))]
        public TD.Distance FlangeBoltSize {
            get { return flangeBolt_size; }
            set {
                flangeBolt_size = value;
                OnPropertyChanged("FlangeBoltSize");
            }
        }

        private string flangePosition_distanceList_X = "50 6*70 110 6*70 50";
        [StructuresDialog("FLNGPOS_X", typeof(TD.String))]
        public string FlangePositionDistanceListX {
            get { return flangePosition_distanceList_X; }
            set {
                flangePosition_distanceList_X = string.IsNullOrEmpty(value) ? "50 6*70 110 6*70 50" : value;
                OnPropertyChanged("FlangePositionDistanceListX");
            }
        }

        private string flangePosition_distanceList_Y = "55 70";
        [StructuresDialog("FLNGPOS_Y", typeof(TD.String))]
        public string FlangePositionDistanceListY {
            get { return flangePosition_distanceList_Y; }
            set {
                flangePosition_distanceList_Y = string.IsNullOrEmpty(value) ? "55 70" : value;
                OnPropertyChanged("FlangePositionDistanceListY");
            }
        }

    }
}
