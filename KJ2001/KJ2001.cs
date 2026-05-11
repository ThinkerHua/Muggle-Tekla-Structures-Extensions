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
 *  KJ2001.cs: Box column outer column base
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using TSD = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ2001 {
    public class PluginData {
        [StructuresField("base_MATL")]
        public string basePlate_material;
        [StructuresField("base_THK")]
        public double basePlate_thickness;
        [StructuresField("base_WID")]
        public double basePlate_width;
        [StructuresField("base_LEN")]
        public double basePlate_length;
        [StructuresField("anchor_MATL")]
        public string anchorRod_material;
        [StructuresField("anchor_size")]
        public double anchorRod_size;
        [StructuresField("anchor_TOL")]
        public double anchorRod_tolerance;
        [StructuresField("anchor_LEN1")]
        public double anchorRod_length1;
        [StructuresField("anchor_LEN2")]
        public double anchorRod_length2;
        [StructuresField("anchor_LEN3")]
        public double anchorRod_length3;
        [StructuresField("anchor_LEN4")]
        public double anchorRod_length4;
        [StructuresField("anchor_LEN5")]
        public double anchorRod_length5;
        [StructuresField("anchor_D2E")]
        public double anchorRod_distance_to_edge;
        [StructuresField("WSHRPLT_MATL")]
        public string washerPlate_material;
        [StructuresField("WSHRPLT_THK")]
        public double washerPlate_thickness;
        [StructuresField("WSHRPLT_WID")]
        public double washerPlate_width;
        [StructuresField("WSHRPLT_Hole_DIA")]
        public double washerPlate_holeDiameter;
        [StructuresField("stud_STD")]
        public string stud_standard = "STUD";
        [StructuresField("stud_size")]
        public double stud_size;
        [StructuresField("stud_LEN")]
        public double stud_length;
        [StructuresField("stud_DISLST_X")]
        public string stud_distanceList_X;
        [StructuresField("stud_DISLST_Y")]
        public string stud_distanceList_Y;
        [StructuresField("stud_offset_XZ")]
        public double stud_offset_XZ;
        [StructuresField("stud_offset_YZ")]
        public double stud_offset_YZ;
        [StructuresField("stud_DISLST_XZ")]
        public string stud_distanceList_XZ;
        [StructuresField("stud_DISLST_YZ")]
        public string stud_distanceList_YZ;
        [StructuresField("stif_MATL")]
        public string innerStiffener_material;
        [StructuresField("stif_THK")]
        public double innerStiffener_thickness;
        [StructuresField("stif_D2B")]
        public double innerStiffener_distance_to_Base;
        [StructuresField("pouringHole_DIA")]
        public double pouringHole_diameter;
        [StructuresField("exhaustHole_DIA")]
        public double exhaustHole_diameter;
        [StructuresField("exhaustHole_POS_X")]
        public double exhaustHole_position_X;
        [StructuresField("exhaustHole_POS_Y")]
        public double exhaustHole_position_Y;
    }

    [Plugin("KJ2001")]
    [PluginUserInterface("Muggle.TsExtensions.KJ2001.Views.MainWindow")]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ZERO)]
    [AutoDirectionType(Tekla.Structures.AutoDirectionTypeEnum.AUTODIR_DETAIL)]
    [DetailType(Tekla.Structures.DetailTypeEnum.END)]
    public class KJ2001 : ConnectionBase {
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private string basePlate_material;
        private double basePlate_thickness;
        private double basePlate_width;
        private double basePlate_length;
        private string anchorRod_material;
        private double anchorRod_size;
        private double anchorRod_tolerance;
        private double anchorRod_length1;
        private double anchorRod_length2;
        private double anchorRod_length3;
        private double anchorRod_length4;
        private double anchorRod_length5;
        private double anchorRod_distance_to_edge;
        private string washerPlate_material;
        private double washerPlate_thickness;
        private double washerPlate_width;
        private double washerPlate_holeDiameter;
        private string stud_standard;
        private double stud_size;
        private double stud_length;
        private string stud_distanceListStr_X;
        private string stud_distanceListStr_Y;
        private double stud_offset_XZ;
        private double stud_offset_YZ;
        private string stud_distanceListStr_XZ;
        private string stud_distanceListStr_YZ;
        private string innerStiffener_material;
        private double innerStiffener_thickness;
        private double innerStiffener_distance_to_Base;
        private double pouringHole_diameter;
        private double exhaustHole_diameter;
        private double exhaustHole_position_X;
        private double exhaustHole_position_Y;

        private DistanceList stud_distanceList_X;
        private DistanceList stud_distanceList_Y;
        private DistanceList stud_distanceList_XZ;
        private DistanceList stud_distanceList_YZ;
        private ProfileRect_Invariant primaryPartProfile;
        private TransformationPlane originTP, workTP;
        #endregion

        #region Properties
        private Model Model {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public KJ2001(PluginData data) {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override bool Run() {
            try {
                var primPart = Model.SelectModelObject(Primary) as Part;
                primaryPartProfile = new ProfileRect_Invariant(primPart.Profile.ProfileString);

                GetValuesFromDialog();

                if (originTP == null) {
                    originTP = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                }
#if DEBUG
                //Internal.ShowTransformationPlane(originTP);
#endif
                if (workTP == null) {
                    workTP = GetWorkTransformationPlane();
                }
#if DEBUG
                //Internal.ShowTransformationPlane(workTP);
#endif
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);

                CreatDetail();

                return true;
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region Private methods
        private void GetValuesFromDialog() {
            basePlate_material = Data.basePlate_material;
            basePlate_thickness = Data.basePlate_thickness;
            basePlate_width = Data.basePlate_width;
            basePlate_length = Data.basePlate_length;
            anchorRod_material = Data.anchorRod_material;
            anchorRod_size = Data.anchorRod_size;
            anchorRod_tolerance = Data.anchorRod_tolerance;
            anchorRod_length1 = Data.anchorRod_length1;
            anchorRod_length2 = Data.anchorRod_length2;
            anchorRod_length3 = Data.anchorRod_length3;
            anchorRod_length4 = Data.anchorRod_length4;
            anchorRod_length5 = Data.anchorRod_length5;
            anchorRod_distance_to_edge = Data.anchorRod_distance_to_edge;
            washerPlate_material = Data.washerPlate_material;
            washerPlate_thickness = Data.washerPlate_thickness;
            washerPlate_width = Data.washerPlate_width;
            washerPlate_holeDiameter = Data.washerPlate_holeDiameter;
            stud_standard = Data.stud_standard;
            stud_size = Data.stud_size;
            stud_length = Data.stud_length;
            stud_distanceListStr_X = Data.stud_distanceList_X;
            stud_distanceListStr_Y = Data.stud_distanceList_Y;
            stud_offset_XZ = Data.stud_offset_XZ;
            stud_offset_YZ = Data.stud_offset_YZ;
            stud_distanceListStr_XZ = Data.stud_distanceList_XZ;
            stud_distanceListStr_YZ = Data.stud_distanceList_YZ;
            innerStiffener_material = Data.innerStiffener_material;
            innerStiffener_thickness = Data.innerStiffener_thickness;
            innerStiffener_distance_to_Base = Data.innerStiffener_distance_to_Base;
            pouringHole_diameter = Data.pouringHole_diameter;
            exhaustHole_diameter = Data.exhaustHole_diameter;
            exhaustHole_position_X = Data.exhaustHole_position_X;
            exhaustHole_position_Y = Data.exhaustHole_position_Y;

            if (IsDefaultValue(basePlate_material))
                basePlate_material = "Q355";
            if (IsDefaultValue(basePlate_thickness))
                basePlate_thickness = 22.0;
            if (IsDefaultValue(basePlate_width))
                basePlate_width = 840.0;
            if (IsDefaultValue(basePlate_length))
                basePlate_length = 840.0;
            if (IsDefaultValue(anchorRod_material))
                anchorRod_material = "Q355";
            if (IsDefaultValue(anchorRod_size))
                anchorRod_size = 24.0;
            if (IsDefaultValue(anchorRod_tolerance))
                anchorRod_tolerance = 2.0;
            if (IsDefaultValue(anchorRod_length1))
                anchorRod_length1 = 120.0;
            if (IsDefaultValue(anchorRod_length2))
                anchorRod_length2 = 50.0;
            if (IsDefaultValue(anchorRod_length3))
                anchorRod_length3 = 510.0;
            if (IsDefaultValue(anchorRod_length4))
                anchorRod_length4 = 330.0;
            if (IsDefaultValue(anchorRod_length5))
                anchorRod_length5 = 100.0;
            if (IsDefaultValue(anchorRod_distance_to_edge))
                anchorRod_distance_to_edge = 50.0;
            if (IsDefaultValue(washerPlate_material))
                washerPlate_material = "Q355";
            if (IsDefaultValue(washerPlate_thickness))
                washerPlate_thickness = 22.0;
            if (IsDefaultValue(washerPlate_width))
                washerPlate_width = 70.0;
            if (IsDefaultValue(washerPlate_holeDiameter))
                washerPlate_holeDiameter = 26.0;
            if (IsDefaultValue(stud_standard))
                stud_standard = "STUD";
            if (IsDefaultValue(stud_size))
                stud_size = 16.0;
            if (IsDefaultValue(stud_length))
                stud_length = 80.0;
            if (IsDefaultValue(stud_distanceListStr_X))
                stud_distanceListStr_X = "200";
            if (IsDefaultValue(stud_distanceListStr_Y))
                stud_distanceListStr_Y = "200";
            if (IsDefaultValue(stud_offset_XZ))
                stud_offset_XZ = 100.0;
            if (IsDefaultValue(stud_offset_YZ))
                stud_offset_YZ = 100.0;
            if (IsDefaultValue(stud_distanceListStr_XZ))
                stud_distanceListStr_XZ = "8*200";
            if (IsDefaultValue(stud_distanceListStr_YZ))
                stud_distanceListStr_YZ = "8*200";
            if (IsDefaultValue(innerStiffener_material))
                innerStiffener_material = "Q355";
            if (IsDefaultValue(innerStiffener_thickness))
                innerStiffener_thickness = 20.0;
            if (IsDefaultValue(innerStiffener_distance_to_Base))
                innerStiffener_distance_to_Base = 1800.0;
            if (IsDefaultValue(pouringHole_diameter))
                pouringHole_diameter = 200.0;
            if (IsDefaultValue(exhaustHole_diameter))
                exhaustHole_diameter = 25.0;
            if (IsDefaultValue(exhaustHole_position_X))
                exhaustHole_position_X = 100.0;
            if (IsDefaultValue(exhaustHole_position_Y))
                exhaustHole_position_Y = 100.0;

            stud_distanceList_X = DistanceList.Parse(
                stud_distanceListStr_X,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            stud_distanceList_Y = DistanceList.Parse(
                stud_distanceListStr_Y,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            stud_distanceList_XZ = DistanceList.Parse(
                stud_distanceListStr_XZ,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
            stud_distanceList_YZ = DistanceList.Parse(
                stud_distanceListStr_YZ,
                System.Globalization.CultureInfo.InvariantCulture,
                TSD.Distance.CurrentUnitType);
        }

        private TransformationPlane GetWorkTransformationPlane() {
            var primPart = Model.SelectModelObject(Primary) as Part;
            var solid = primPart.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var centerPoint = (solid.MinimumPoint + solid.MaximumPoint).Multiply(0.5);

            var axisX = new Vector(0.0, 1000.0, 0.0);
            var axisZ = centerPoint.IsZero() ? new Vector(1000.0, 0.0, 0.0) : new Vector(centerPoint);
            var axisY = axisZ.Cross(axisX);

            return new TransformationPlane(new Point(), axisX, axisY);
        }

        private void CreatDetail() {
            var primPart = Model.SelectModelObject(Primary) as Part;
            var origin = new Point(0.0, 0.0, 0.0);
            var axisX = new Vector(1000.0, 0.0, 0.0);
            var axisY = new Vector(0.0, 1000.0, 0.0);
            var axisZ = new Vector(0.0, 0.0, 1000.0);

            #region 末端对齐
            var plane = new Plane { AxisX = axisX.GetNormal(10), AxisY = axisY.GetNormal(10), Origin = origin };
            var fitting = new Fitting {
                Father = primPart,
                Plane = plane
            };
            fitting.Insert();
            #endregion

            #region 创建底板、开孔
            var point1 = new Point(basePlate_length * 0.5, 0.0, 0.0);
            var point2 = new Point(-basePlate_length * 0.5, 0.0, 0.0);
            var basePlate = ModelOperation.CreatBeam(
                point1, point2, "BASEPLATE",
                $"PL{basePlate_thickness}*{basePlate_width}", basePlate_material,
                depthEnum: Position.DepthEnum.BEHIND);

            ModelOperation.CreatWeld(primPart, basePlate);

            var anchorRod_distanceList_X = new DistanceList {
                new TSD.Distance(anchorRod_distance_to_edge),
                new TSD.Distance(basePlate_length * 0.5),
                new TSD.Distance(basePlate_length * 0.5),
                new TSD.Distance(basePlate_length - anchorRod_distance_to_edge),
            };
            var anchorRod_distanceList_Y = new DistanceList {
                new TSD.Distance(0.0),
                new TSD.Distance(basePlate_width * 0.5 - anchorRod_distance_to_edge),
                new TSD.Distance(-basePlate_width * 0.5 + anchorRod_distance_to_edge),
                new TSD.Distance(0.0),
            };
            ModelOperation.CreatBoltXYList(
                basePlate, basePlate, null,
                basePlate.StartPoint, basePlate.EndPoint, anchorRod_distanceList_X, anchorRod_distanceList_Y,
                new Position { Rotation = Position.RotationEnum.FRONT },
                boltSize: anchorRod_size, boltType: BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP,
                tolerance: anchorRod_tolerance, bolt: false);
            #endregion

            #region 创建锚栓
            var assembly = primPart.GetAssembly();
            var matrix = MatrixFactoryExtension.Rotate(new Line(origin, axisZ), Math.PI);
            point1 = new Point(basePlate_length * 0.5 - anchorRod_distance_to_edge, 0.0, -basePlate_thickness);
            point2 = point1 - axisZ;
            var anchorRods = ModelOperation.CreatAnchorRod(
                point1, point2,
                anchorRod_length1, anchorRod_length2, anchorRod_length3, anchorRod_length4, anchorRod_length5,
                axisX, material: anchorRod_material, size: anchorRod_size, @class: "0",
                washerPlateThickness: washerPlate_thickness, washerPlateWidth: washerPlate_width,
                washerPlatePosition: basePlate_thickness, washerPlateHoleDiameter: washerPlate_holeDiameter);
            foreach (var obj in anchorRods) {
                assembly.Add(obj);
            }

            anchorRods = ModelOperation.CreatAnchorRod(
                matrix.Transform(point1), matrix.Transform(point2),
                anchorRod_length1, anchorRod_length2, anchorRod_length3, anchorRod_length4, anchorRod_length5,
                MatrixExtension.Transform(matrix, axisX), material: anchorRod_material, size: anchorRod_size,
                @class: "0",
                washerPlateThickness: washerPlate_thickness, washerPlateWidth: washerPlate_width,
                washerPlatePosition: basePlate_thickness, washerPlateHoleDiameter: washerPlate_holeDiameter);
            foreach (var obj in anchorRods) {
                assembly.Add(obj);
            }

            point1 = new Point(0.0, basePlate_width * 0.5 - anchorRod_distance_to_edge, -basePlate_thickness);
            point2 = point1 - axisZ;
            anchorRods = ModelOperation.CreatAnchorRod(
                point1, point2,
                anchorRod_length1, anchorRod_length2, anchorRod_length3, anchorRod_length4, anchorRod_length5,
                axisY, material: anchorRod_material, size: anchorRod_size, @class: "0",
                washerPlateThickness: washerPlate_thickness, washerPlateWidth: washerPlate_width,
                washerPlatePosition: basePlate_thickness, washerPlateHoleDiameter: washerPlate_holeDiameter);
            foreach (var obj in anchorRods) {
                assembly.Add(obj);
            }

            anchorRods = ModelOperation.CreatAnchorRod(
                matrix.Transform(point1), matrix.Transform(point2),
                anchorRod_length1, anchorRod_length2, anchorRod_length3, anchorRod_length4, anchorRod_length5,
                MatrixExtension.Transform(matrix, axisY), material: anchorRod_material, size: anchorRod_size,
                @class: "0",
                washerPlateThickness: washerPlate_thickness, washerPlateWidth: washerPlate_width,
                washerPlatePosition: basePlate_thickness, washerPlateHoleDiameter: washerPlate_holeDiameter);
            foreach (var obj in anchorRods) {
                assembly.Add(obj);
            }

            assembly.Modify();
            #endregion

            #region 创建栓钉
            point1 = new Point(primaryPartProfile.h1 * 0.5, 0.0, 0.0);
            point2 = new Point(primaryPartProfile.h1 * 0.5, 0.0, 1000.0);
            var stud1 = ModelOperation.CreatBoltArray(
                primPart, primPart, null,
                point1, point2,
                stud_distanceList_XZ, stud_distanceList_X,
                position: new Position { Rotation = Position.RotationEnum.BACK },
                startOffset: new Offset { Dx = stud_offset_XZ },
                boltStandard: stud_standard, boltSize: stud_size, boltType: BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP);
            stud1.Length = stud_length;
            stud1.Modify();

            point1 = new Point(-primaryPartProfile.h1 * 0.5, 0.0, 0.0);
            point2 = new Point(-primaryPartProfile.h1 * 0.5, 0.0, 1000.0);
            var stud2 = ModelOperation.CreatBoltArray(
                primPart, primPart, null,
                point1, point2,
                stud_distanceList_XZ, stud_distanceList_X,
                position: new Position { Rotation = Position.RotationEnum.FRONT },
                startOffset: new Offset { Dx = stud_offset_XZ },
                boltStandard: stud_standard, boltSize: stud_size, boltType: BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP);
            stud2.Length = stud_length;
            stud2.Modify();

            point1 = new Point(0.0, primaryPartProfile.b1 * 0.5, 0.0);
            point2 = new Point(0.0, primaryPartProfile.b1 * 0.5, 1000.0);
            var stud3 = ModelOperation.CreatBoltArray(
                primPart, primPart, null,
                point1, point2,
                stud_distanceList_YZ, stud_distanceList_Y,
                position: new Position { Rotation = Position.RotationEnum.BELOW },
                startOffset: new Offset { Dx = stud_offset_YZ },
                boltStandard: stud_standard, boltSize: stud_size, boltType: BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP);
            stud3.Length = stud_length;
            stud3.Modify();

            point1 = new Point(0.0, -primaryPartProfile.b1 * 0.5, 0.0);
            point2 = new Point(0.0, -primaryPartProfile.b1 * 0.5, 1000.0);
            var stud4 = ModelOperation.CreatBoltArray(
                primPart, primPart, null,
                point1, point2,
                stud_distanceList_YZ, stud_distanceList_Y,
                position: new Position { Rotation = Position.RotationEnum.TOP },
                startOffset: new Offset { Dx = stud_offset_YZ },
                boltStandard: stud_standard, boltSize: stud_size, boltType: BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP);
            stud4.Length = stud_length;
            stud4.Modify();
            #endregion

            #region 创建内部加劲板
            point1 = new Point(
                primaryPartProfile.h1 * 0.5 - primaryPartProfile.t,
                primaryPartProfile.b1 * 0.5 - primaryPartProfile.s,
                innerStiffener_distance_to_Base);
            point2 = new Point(point1.X, -point1.Y, point1.Z);
            var liner1 = ModelOperation.CreatBeam(
                point1, point2, "LINER", $"PL10*30", innerStiffener_material,
                planeEnum: Position.PlaneEnum.RIGHT,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: innerStiffener_thickness);
            point1 = new Point(
                -primaryPartProfile.h1 * 0.5 + primaryPartProfile.t,
                primaryPartProfile.b1 * 0.5 - primaryPartProfile.s,
                innerStiffener_distance_to_Base);
            point2 = new Point(point1.X, -point1.Y, point1.Z);
            var liner2 = ModelOperation.CreatBeam(
                point1, point2, "LINER", $"PL10*30", innerStiffener_material,
                planeEnum: Position.PlaneEnum.LEFT,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: innerStiffener_thickness);

            point1 = new Point(
                primaryPartProfile.h1 * 0.5 - primaryPartProfile.t,
                0.0,
                innerStiffener_distance_to_Base);
            point2 = new Point(-point1.X, point1.Y, point1.Z);
            var innerStiffener = ModelOperation.CreatBeam(
                point1, point2, "INNERSTIFFENER",
                $"PL{innerStiffener_thickness}*{primaryPartProfile.b1 - 2 * primaryPartProfile.s}",
                innerStiffener_material, depthEnum: Position.DepthEnum.BEHIND);

            ModelOperation.CreatWeld(primPart, liner1);
            ModelOperation.CreatWeld(primPart, liner2);
            ModelOperation.CreatWeld(primPart, innerStiffener);
            #endregion

            #region 开浇筑孔、排气孔
            point1 = new Point(0.0, 0.0, innerStiffener_distance_to_Base);
            point2 = new Point(0.0, 0.0, innerStiffener_distance_to_Base - innerStiffener_thickness);
            var pourintHole = ModelOperation.CreatBeam(
                point1, point2,
                "POURINGHOLE",
                $"D{pouringHole_diameter}",
                innerStiffener_material);
            ModelOperation.ApplyBooleanOperation(innerStiffener, pourintHole);
            pourintHole.Delete();

            point1 = new Point(
                primaryPartProfile.h1 * 0.5 - primaryPartProfile.t - exhaustHole_position_X,
                primaryPartProfile.b1 * 0.5 - primaryPartProfile.s - exhaustHole_position_Y,
                innerStiffener_distance_to_Base);
            point2 = new Point(point1.X, point1.Y, point1.Z - innerStiffener_thickness);
            var exhaustHole1 = ModelOperation.CreatBeam(
                point1, point2,
                "EXHAUSTHOLE",
                $"D{exhaustHole_diameter}",
                innerStiffener_material);

            point1 = new Point(-point1.X, point1.Y, point1.Z);
            point2 = new Point(point1.X, point1.Y, point1.Z - innerStiffener_thickness);
            var exhaustHole2 = ModelOperation.CreatBeam(
                point1, point2,
                "EXHAUSTHOLE",
                $"D{exhaustHole_diameter}",
                innerStiffener_material);

            point1 = new Point(point1.X, -point1.Y, point1.Z);
            point2 = new Point(point1.X, point1.Y, point1.Z - innerStiffener_thickness);
            var exhaustHole3 = ModelOperation.CreatBeam(
                point1, point2,
                "EXHAUSTHOLE",
                $"D{exhaustHole_diameter}",
                innerStiffener_material);

            point1 = new Point(-point1.X, point1.Y, point1.Z);
            point2 = new Point(point1.X, point1.Y, point1.Z - innerStiffener_thickness);
            var exhaustHole4 = ModelOperation.CreatBeam(
                point1, point2,
                "EXHAUSTHOLE",
                $"D{exhaustHole_diameter}",
                innerStiffener_material);

            ModelOperation.ApplyBooleanOperation(innerStiffener, exhaustHole1);
            ModelOperation.ApplyBooleanOperation(innerStiffener, exhaustHole2);
            ModelOperation.ApplyBooleanOperation(innerStiffener, exhaustHole3);
            ModelOperation.ApplyBooleanOperation(innerStiffener, exhaustHole4);
            exhaustHole1.Delete();
            exhaustHole2.Delete();
            exhaustHole3.Delete();
            exhaustHole4.Delete();
            #endregion
        }
        #endregion
    }
}
