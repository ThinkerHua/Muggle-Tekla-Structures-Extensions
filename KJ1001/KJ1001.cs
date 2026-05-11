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
 *  KJ1001.cs: connection between box column and H-beam
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using Point = Tekla.Structures.Geometry3d.Point;
using TSD = Tekla.Structures.Datatype;
using TSG = Tekla.Structures.Geometry3d;
using Vector = Tekla.Structures.Geometry3d.Vector;

namespace Muggle.TsExtensions.KJ1001 {
    public class PluginData {
        /// <summary>
        /// <list type="bullet">
        ///     <item>0 - 翼缘焊接，腹板栓接</item>
        ///     <item>1 - 焊短梁，栓接</item>
        ///     <item>2 - 焊短梁，翼缘焊接，腹板栓接</item>
        /// </list>
        /// </summary>
        [StructuresField("type")]
        public int type;
        [StructuresField("innerSTF_THK")]
        public double innerStiffener_thickness;
        [StructuresField("innerSTF_chamfer")]
        public double innerStiffener_chamferSize;
        [StructuresField("moreSTF_DISLST")]
        public string moreInnerStiffeners_distanceListStr;
        [StructuresField("THKDSTF_THK")]
        public double thickenedStiffener_thickness;
        [StructuresField("THKDSTF_EXTLEN")]
        public double thickenedStiffener_extensionLength;
        [StructuresField("STF_MATL")]
        public string stiffener_materialStr;
        [StructuresField("ratHole_radius")]
        public double ratHoel_radius;
        [StructuresField("gap")]
        public double gap;
        [StructuresField("weld_angle")]
        public double weld_angle;
        [StructuresField("root_face")]
        public double weld_root_face;
        [StructuresField("root_opening")]
        public double weld_root_opening;

        [StructuresField("cover_THK")]
        public double cover_thickness;
        [StructuresField("cover_LEN1")]
        public double cover_length1;
        [StructuresField("cover_LEN2")]
        public double cover_length2;
        [StructuresField("topCover_WD1")]
        public double topCover_width1;
        [StructuresField("topCover_WD2")]
        public double topCover_width2;
        [StructuresField("BTMCover_WD1")]
        public double bottomCover_width1;
        [StructuresField("BTMCover_WD2")]
        public double bottomCover_width2;
        [StructuresField("cover_MATL")]
        public string cover_materialStr;
        /// <summary>
        /// 腹板连接板创建类型枚举
        /// <list type="bullet">
        ///     <item>0 - 仅前面</item>
        ///     <item>1 - 仅后面</item>
        ///     <item>2 - 前后</item>
        /// </list>
        /// </summary>
        [StructuresField("webCNXPL_enum")]
        public int webConnectionPlate_creationEnum;
        [StructuresField("webCNXPL_THK")]
        public double webConnectionPlate_thickness;
        [StructuresField("CNXPL_MATL")]
        public string connectionPlate_materialStr;
        [StructuresField("webBolt_STD")]
        public string webBolt_standard;
        [StructuresField("webBolt_size")]
        public double webBolt_size;
        [StructuresField("webPOS_X")]
        public string webPosition_distanceListStr_X;
        [StructuresField("webPOS_Y")]
        public string webPosition_distanceListStr_Y;

        [StructuresField("shortBeam_LEN")]
        public double shortBeamLength;
        [StructuresField("shortBeam_PRF")]
        public string shortBeam_prfStr;
        [StructuresField("shortBeam_MATL")]
        public string shortBeam_materialStr;
        [StructuresField("outFLNGCNXPL_THK")]
        public double outterFlangeConnectionPlate_thickness;
        [StructuresField("innerFLNGCNXPL_THK")]
        public double innerFlangeConnectionPlate_thickness;
        [StructuresField("FLNGBolt_STD")]
        public string flangeBolt_standard;
        [StructuresField("FLNGBolt_size")]
        public double flangeBolt_size;
        [StructuresField("FLNGPOS_X")]
        public string flangePosition_distanceListStr_X;
        [StructuresField("FLNGPOS_Y")]
        public string flangePosition_distanceListStr_Y;
    }

    [Plugin("KJ1001")]
    [PluginUserInterface("Muggle.TsExtensions.KJ1001.Views.MainWindow")]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
    [PositionType(PositionTypeEnum.MIDDLE_PLANE)]
    public class KJ1001 : ConnectionBase {
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private int type;
        private double innerStiffener_thickness;
        private double innerStiffener_chamferSize;
        private string moreInnerStiffeners_distanceListStr;
        private double thickenedStiffener_thickness;
        private double thickenedStiffener_extensionLength;
        private string stiffener_materialStr;
        private double ratHoel_radius;
        private double gap;
        private double weld_angle;
        private double weld_root_face;
        private double weld_root_opening;

        private double cover_thickness;
        private double cover_length1;
        private double cover_length2;
        private double topCover_width1;
        private double topCover_width2;
        private double bottomCover_width1;
        private double bottomCover_width2;
        private string cover_materialStr;
        private int webConnectionPlate_creationEnum;
        private double webConnectionPlate_thickness;
        private string connectionPlate_materialStr;
        private string webBolt_standard;
        private double webBolt_size;
        private string webPosition_distanceListStr_X;
        private string webPosition_distanceListStr_Y;

        private double shortBeamLength;
        private string shortBeam_prfStr;
        private string shortBeam_materialStr;
        private double outterFlangeConnectionPlate_thickness;
        private double innerFlangeConnectionPlate_thickness;
        private string flangeBolt_standard;
        private double flangeBolt_size;
        private string flangePosition_distanceListStr_X;
        private string flangePosition_distanceListStr_Y;

        private DistanceList moreInnerStiffeners_distanceList;
        private DistanceList webPosition_distanceList_X;
        private DistanceList webPosition_distanceList_Y;
        private DistanceList flangePosition_distanceList_X;
        private DistanceList flangePosition_distanceList_Y;

        private ProfileRect_Invariant prfPrim;
        private ProfileH_Symmetrical prfSec;
        private ProfileH_Symmetrical prfShort;

        private TransformationPlane originTP;
        private TransformationPlane workTP_prim;
        private TransformationPlane workTP_sec;
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
        public KJ1001(PluginData data) {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override bool Run() {
            try {
                var prim = Model.SelectModelObject(Primary) as Part;
                var sec = Model.SelectModelObject(Secondaries[0]) as Part;
                prfPrim = new ProfileRect_Invariant(prim.Profile.ProfileString);
                prfSec = new ProfileH_Symmetrical(sec.Profile.ProfileString);
                if (prfSec.h1 != prfSec.h2) {
                    throw new UnAcceptableProfileException(prfSec.ProfileText);
                }

                GetValuesFromDialog();
                prfShort = new ProfileH_Symmetrical(shortBeam_prfStr);
                if (prfShort.h1 != prfShort.h2 || prfShort.h1 != prfSec.h1) {
                    throw new UnAcceptableProfileException(prfShort.ProfileText);
                }

                originTP ??= Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

                if (workTP_prim == null || workTP_sec == null) {
                    workTP_prim = GetWorkTransformationPlane(out workTP_sec);
                }
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP_prim);

#if DEBUG
                //Internal.ShowTransformationPlane(originTP, 1);
                //Internal.ShowTransformationPlane(workTP_prim, 4);
                //Internal.ShowTransformationPlane(workTP_sec, 2);
#endif

                CreatConnection();

                return true;
            } catch (Exception e) {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region Private methods
        private void GetValuesFromDialog() {
            var prim = Model.SelectModelObject(Primary) as Part;
            var sec = Model.SelectModelObject(Secondaries[0]) as Part;

            type = _Data.type;
            innerStiffener_thickness = _Data.innerStiffener_thickness;
            innerStiffener_chamferSize = _Data.innerStiffener_chamferSize;
            moreInnerStiffeners_distanceListStr = _Data.moreInnerStiffeners_distanceListStr;
            thickenedStiffener_thickness = _Data.thickenedStiffener_thickness;
            thickenedStiffener_extensionLength = _Data.thickenedStiffener_extensionLength;
            stiffener_materialStr = _Data.stiffener_materialStr;
            ratHoel_radius = _Data.ratHoel_radius;
            gap = _Data.gap;
            weld_angle = _Data.weld_angle;
            weld_root_face = _Data.weld_root_face;
            weld_root_opening = _Data.weld_root_opening;
            cover_thickness = _Data.cover_thickness;
            cover_length1 = _Data.cover_length1;
            cover_length2 = _Data.cover_length2;
            topCover_width1 = _Data.topCover_width1;
            topCover_width2 = _Data.topCover_width2;
            bottomCover_width1 = _Data.bottomCover_width1;
            bottomCover_width2 = _Data.bottomCover_width2;
            cover_materialStr = _Data.cover_materialStr;
            webConnectionPlate_creationEnum = _Data.webConnectionPlate_creationEnum;
            webConnectionPlate_thickness = _Data.webConnectionPlate_thickness;
            connectionPlate_materialStr = _Data.connectionPlate_materialStr;
            webBolt_standard = _Data.webBolt_standard;
            webBolt_size = _Data.webBolt_size;
            webPosition_distanceListStr_X = _Data.webPosition_distanceListStr_X;
            webPosition_distanceListStr_Y = _Data.webPosition_distanceListStr_Y;
            shortBeamLength = _Data.shortBeamLength;
            shortBeam_prfStr = _Data.shortBeam_prfStr;
            shortBeam_materialStr = _Data.shortBeam_materialStr;
            outterFlangeConnectionPlate_thickness = _Data.outterFlangeConnectionPlate_thickness;
            innerFlangeConnectionPlate_thickness = _Data.innerFlangeConnectionPlate_thickness;
            flangeBolt_standard = _Data.flangeBolt_standard;
            flangeBolt_size = _Data.flangeBolt_size;
            flangePosition_distanceListStr_X = _Data.flangePosition_distanceListStr_X;
            flangePosition_distanceListStr_Y = _Data.flangePosition_distanceListStr_Y;

            if (IsDefaultValue(type)) {
                type = 0;
            }
            if (type != 0 && type != 1 && type != 2) {
                throw new ArgumentException($"Unknow connection type: {type}");
            }
            if (innerStiffener_thickness < 0.0) {
                innerStiffener_thickness = prfSec.t1;
            }
            if (IsDefaultValue(innerStiffener_chamferSize)) {
                innerStiffener_chamferSize = 25.0;
            }
            if (IsDefaultValue(thickenedStiffener_thickness)) {
                thickenedStiffener_thickness = 0.0;
            }
            if (IsDefaultValue(thickenedStiffener_extensionLength)) {
                thickenedStiffener_extensionLength = 150.0;
            }
            if (IsDefaultValue(stiffener_materialStr)) {
                stiffener_materialStr = prim.Material.MaterialString;
            }
            if (IsDefaultValue(ratHoel_radius)) {
                ratHoel_radius = 35.0;
            }
            if (IsDefaultValue(gap)) {
                gap = type == 0 ? 15.0 : 10.0;
            }
            if (IsDefaultValue(weld_angle)) {
                weld_angle = 30.0;
            }
            if (IsDefaultValue(weld_root_face)) {
                weld_root_face = 2.0;
            }
            if (IsDefaultValue(weld_root_opening)) {
                weld_root_opening = 6.0;
            }
            if (IsDefaultValue(cover_thickness)) {
                cover_thickness = 6.0;
            }
            if (IsDefaultValue(cover_length1)) {
                cover_length1 = 50.0;
            }
            if (IsDefaultValue(cover_length2)) {
                cover_length2 = 350.0;
            }
            if (IsDefaultValue(topCover_width1)) {
                topCover_width1 = 52.0;
            }
            if (IsDefaultValue(topCover_width2)) {
                topCover_width2 = 160.0;
            }
            if (IsDefaultValue(bottomCover_width1)) {
                bottomCover_width1 = 88.0;
            }
            if (IsDefaultValue(bottomCover_width2)) {
                bottomCover_width2 = 160.0;
            }
            if (IsDefaultValue(cover_materialStr)) {
                cover_materialStr = prim.Material.MaterialString;
            }
            if (IsDefaultValue(webConnectionPlate_creationEnum)) {
                webConnectionPlate_creationEnum = 0;
            }
            if (IsDefaultValue(webConnectionPlate_thickness)) {
                webConnectionPlate_thickness = 16.0;
            }
            if (IsDefaultValue(connectionPlate_materialStr)) {
                connectionPlate_materialStr = prim.Material.MaterialString;
            }
            if (IsDefaultValue(webBolt_standard)) {
                webBolt_standard = "HS10.9";
            }
            if (IsDefaultValue(webBolt_size)) {
                webBolt_size = 20.0;
            }
            if (IsDefaultValue(webPosition_distanceListStr_X)) {
                switch (type) {
                case 0:
                    webPosition_distanceListStr_X = "40 70 40";
                    break;
                case 1:
                    webPosition_distanceListStr_X = "50 3*70 110 3*70 50";
                    break;
                case 2:
                    webPosition_distanceListStr_X = "40 70 90 70 40";
                    break;
                default:
                    break;
                }
            }
            if (IsDefaultValue(webPosition_distanceListStr_Y)) {
                switch (type) {
                case 0:
                    webPosition_distanceListStr_Y = "61 59 8*70 59";
                    break;
                case 1:
                    webPosition_distanceListStr_Y = "77 84 11*80 84";
                    break;
                case 2:
                    webPosition_distanceListStr_Y = "55 50 7*70 50";
                    break;
                default:
                    break;
                }
            }
            if (IsDefaultValue(shortBeamLength)) {
                shortBeamLength = 1200.0;
            }
            if (IsDefaultValue(shortBeam_prfStr)) {
                shortBeam_prfStr = sec.Profile.ProfileString;
            }
            if (IsDefaultValue(shortBeam_materialStr)) {
                shortBeam_materialStr = sec.Material.MaterialString;
            }
            if (IsDefaultValue(outterFlangeConnectionPlate_thickness)) {
                outterFlangeConnectionPlate_thickness = 25.0;
            }
            if (IsDefaultValue(innerFlangeConnectionPlate_thickness)) {
                innerFlangeConnectionPlate_thickness = 30.0;
            }
            if (IsDefaultValue(flangeBolt_standard)) {
                flangeBolt_standard = "HS10.9";
            }
            if (IsDefaultValue(flangeBolt_size)) {
                flangeBolt_size = 20.0;
            }
            if (IsDefaultValue(flangePosition_distanceListStr_X)) {
                flangePosition_distanceListStr_X = "50 6*70 110 6*70 50";
            }
            if (IsDefaultValue(flangePosition_distanceListStr_Y)) {
                flangePosition_distanceListStr_Y = "55 70";
            }

            moreInnerStiffeners_distanceList = DistanceList.Parse(
                moreInnerStiffeners_distanceListStr, System.Globalization.CultureInfo.InvariantCulture, TSD.Distance.CurrentUnitType);
            webPosition_distanceList_X = DistanceList.Parse(
                webPosition_distanceListStr_X, System.Globalization.CultureInfo.InvariantCulture, TSD.Distance.CurrentUnitType);
            webPosition_distanceList_Y = DistanceList.Parse(
                webPosition_distanceListStr_Y, System.Globalization.CultureInfo.InvariantCulture, TSD.Distance.CurrentUnitType);
            flangePosition_distanceList_X = DistanceList.Parse(
                flangePosition_distanceListStr_X, System.Globalization.CultureInfo.InvariantCulture, TSD.Distance.CurrentUnitType);
            flangePosition_distanceList_Y = DistanceList.Parse(
                flangePosition_distanceListStr_Y, System.Globalization.CultureInfo.InvariantCulture, TSD.Distance.CurrentUnitType);
        }

        private TransformationPlane GetWorkTransformationPlane(out TransformationPlane secWorkTP) {
            var prim = Model.SelectModelObject(Primary) as Part;
            var sec = Model.SelectModelObject(Secondaries[0]) as Part;
            var globalAxisZ = new Vector(0, 0, 1).TransformFrom(new TransformationPlane());
            var halfPI = Math.PI * 0.5;

            var origin = new Point(0, 0, 0);
            var secCenterLine = sec.GetCenterLine(false);
            var sec_firstPoint = secCenterLine[0] as Point;
            var sec_lastPoint = secCenterLine[secCenterLine.Count - 1] as Point;
            var reverse = TSG.Distance.PointToPoint(sec_firstPoint, origin) > TSG.Distance.PointToPoint(sec_lastPoint, origin);
            var index = reverse ? secCenterLine.Count - 1 : 0;
            var step = reverse ? -1 : 1;
            var secLine = new Line(secCenterLine[index] as Point, secCenterLine[index + step] as Point);
            var sec_direction = secLine.Direction.GetNormal();

            var primCS = prim.GetCoordinateSystem();
            var primCS_axisX = primCS.AxisX.GetNormal();
            var primCS_axisY = primCS.AxisY.GetNormal();
            var primCS_axisZ = primCS_axisX.Cross(primCS_axisY).GetNormal();

            var primCS_YZPlane = new GeometricPlane(primCS.Origin, primCS_axisY, primCS_axisZ);
            var secDirection_projection = ProjectionExtension.VectorToPlane(sec_direction, primCS_YZPlane).GetNormal();

            var angle_Y = Math.Acos(primCS_axisY.Dot(secDirection_projection));
            var angle_Z = Math.Acos(primCS_axisZ.Dot(secDirection_projection));
            if (angle_Y > halfPI) angle_Y = Math.PI - angle_Y;
            if (angle_Z > halfPI) angle_Z = Math.PI - angle_Z;

            var primAxisX = angle_Y < angle_Z ? primCS_axisY : primCS_axisZ;
            primAxisX *= primAxisX.Dot(sec_direction) < 0 ? -1 : 1;
            var primAxisY = primCS_axisX;
            primAxisY *= primAxisY.Dot(globalAxisZ) < 0 ? -1 : 1;
            var centerPlane_XY = new GeometricPlane(primCS.Origin, primAxisX.Cross(primAxisY));
            var secLine_projection = Projection.LineToPlane(secLine, centerPlane_XY);
            var primOrigin = IntersectionExtension.LineToLine(new Line(primCS.Origin, primAxisY), secLine_projection).StartPoint;
            var primWorkTP = new TransformationPlane(primOrigin, primAxisX, primAxisY);

            ArrayList polyBeamCSs;
            CoordinateSystem secCS;
            if (sec is PolyBeam polyBeam) {
                polyBeamCSs = polyBeam.GetPolybeamCoordinateSystems();
                secCS = polyBeamCSs[reverse ? polyBeamCSs.Count - 1 : 0] as CoordinateSystem;
            } else {
                secCS = sec.GetCoordinateSystem();
            }
            var boundaryPlane_YZ = new GeometricPlane(
                primOrigin + primAxisX * (angle_Y < angle_Z ? prfPrim.h1 * 0.5 : prfPrim.b1 * 0.5),
                primAxisX);
            var secOrigin = Intersection.LineToPlane(secLine, boundaryPlane_YZ);
            var secAxisX = secCS.AxisX.GetNormal();
            secAxisX *= reverse ? -1 : 1;
            var secAxisY = secCS.AxisY.GetNormal();
            secAxisY *= secAxisY.Dot(globalAxisZ) < 0 ? -1 : 1;
            secWorkTP = new TransformationPlane(secOrigin, secAxisX, secAxisY);

            return primWorkTP;
        }

        private void CreatConnection() {

            #region 共用变量、方法
            var prim = Model.SelectModelObject(Primary) as Part;
            var sec = Model.SelectModelObject(Secondaries[0]) as Part;

            var origin = new Point(0, 0, 0);
            var axisX = new Vector(1, 0, 0);
            var axisY = new Vector(0, 1, 0);
            var axisZ = new Vector(0, 0, 1);

            var secOrigin = origin.Transform(workTP_sec, workTP_prim);
            var secAxisX = axisX.Transform(workTP_sec, workTP_prim);
            var secAxisY = axisY.Transform(workTP_sec, workTP_prim);
            var secAxisZ = axisZ.Transform(workTP_sec, workTP_prim);

            var primCS = prim.GetCoordinateSystem();
            var strong = primCS.AxisY.GetNormal().Dot(axisX) != 0.0;
            var primCenterLine = new Line(origin, axisY);
            var primRightLine = new Line(origin + axisX * (strong ? prfPrim.h1 * 0.5 : prfPrim.b1 * 0.5), axisY);
            var primRightBoundaryPlane = new GeometricPlane(primRightLine.Origin, axisX);

            var secCenterLine = new Line(origin.Transform(workTP_sec, workTP_prim), axisX.Transform(workTP_sec, workTP_prim));
            var secTopLine = secCenterLine.Offset(secAxisY * (prfSec.h1 * 0.5));
            var secTopFrontLine = secTopLine.Offset(secAxisZ * (prfSec.b1 * 0.5));
            var secTopBehindLine = secTopLine.Offset(secAxisZ * (prfSec.b1 * -0.5));
            var secBottomLine = secCenterLine.Offset(secAxisY * (prfSec.h1 * -0.5));
            var secBottomFrontLine = secBottomLine.Offset(secAxisZ * (prfSec.b1 * 0.5));
            var secBottomBehindLine = secBottomLine.Offset(secAxisZ * (prfSec.b1 * -0.5));
            var secTFxPrimR = Intersection.LineToPlane(secTopFrontLine, primRightBoundaryPlane);
            var secTBxPrimR = Intersection.LineToPlane(secTopBehindLine, primRightBoundaryPlane);
            var secBFxPrimR = Intersection.LineToPlane(secBottomFrontLine, primRightBoundaryPlane);
            var secBBxPrimR = Intersection.LineToPlane(secBottomBehindLine, primRightBoundaryPlane);
            var secTxPrimR = (secTFxPrimR + secTBxPrimR).Multiply(0.5);
            var secBOTxPrimR = (secBFxPrimR + secBBxPrimR).Multiply(0.5);
            var secFxPrimR = (secTFxPrimR + secBFxPrimR).Multiply(0.5);
            var secBEHxPrimR = (secTBxPrimR + secBBxPrimR).Multiply(0.5);

            var transferMatrix = MatrixFactory.FromCoordinateSystem(new CoordinateSystem(secOrigin, secAxisX, secAxisY));
            var shearMatrix_AxisY = MatrixFactoryExtension.Shear(Math.Tan(secAxisY.GetAngleBetween_Precisely(axisY)));
            var shearMatrix_AxisZ = MatrixFactoryExtension.Shear(0, Math.Tan(secAxisZ.GetAngleBetween_Precisely(axisZ)));
            var shearMatrix = shearMatrix_AxisY * shearMatrix_AxisZ;
            var secAngle_XY = secCenterLine.Direction.GetAngleBetween_Precisely(
                ProjectionExtension.VectorToPlane(secCenterLine.Direction, new GeometricPlane(origin, axisZ)));
            var orthogonal = secAngle_XY == 0.0;

            //  正常做法是矩阵连乘进行组合
            //  由于Tekla内部实现的问题，切变矩阵不能与其他矩阵组合，否则变换后的点会有偏差
            static void TransformPoint(IEnumerable<Matrix> matrices, IEnumerable<Point> points) {
                foreach (var m in matrices) {
                    foreach (var p in points) {
                        var newPoint = m.Transform(p);
                        p.X = newPoint.X;
                        p.Y = newPoint.Y;
                        p.Z = newPoint.Z;
                    }
                }
            }

            var ratHoleCornerOffset = ratHoel_radius / Math.Pow(2, 0.5);
            #endregion

            #region 柱加厚补强板
            Beam thickenedStiffener1 = null, thickenedStiffener2 = null;
            if (thickenedStiffener_thickness <= (strong ? prfPrim.s : prfPrim.t))
                goto Skip_THKDSTF;

            var point1 = new Point(secTxPrimR);
            var point2 = new Point(secBOTxPrimR);
            point1.X -= strong ? prfPrim.h1 * 0.5 : prfPrim.b1 * 0.5;
            point2.X = point1.X;
            point1.Y += thickenedStiffener_extensionLength;
            point2.Y -= thickenedStiffener_extensionLength;
            point1.Z = strong ? prfPrim.b1 * 0.5 : prfPrim.h1 * 0.5;
            point2.Z = point1.Z;
            thickenedStiffener1 = ModelOperation.CreatBeam(
                point1, point2, "THICKENED_STIFFENER",
                $"PL{thickenedStiffener_thickness}*{(strong ? prfPrim.h1 : prfPrim.b1)}",
                stiffener_materialStr, depthEnum: Position.DepthEnum.BEHIND);

            point1 = new Point(point1);
            point2 = new Point(point2);
            point1.Z *= -1;
            point2.Z *= -1;
            thickenedStiffener2 = ModelOperation.CreatBeam(
                point1, point2, "THICKENED_STIFFENER",
                $"PL{thickenedStiffener_thickness}*{(strong ? prfPrim.h1 : prfPrim.b1)}",
                stiffener_materialStr, depthEnum: Position.DepthEnum.FRONT);

            ModelOperation.ApplyBooleanOperation(prim, thickenedStiffener1);
            ModelOperation.ApplyBooleanOperation(prim, thickenedStiffener2);

            ModelOperation.CreatWeld(prim, thickenedStiffener1,
                preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: thickenedStiffener_thickness - 2, angleAbove: 30);
            ModelOperation.CreatWeld(prim, thickenedStiffener2,
                preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: thickenedStiffener_thickness - 2, angleAbove: 30);

        Skip_THKDSTF:;
            #endregion

            #region 柱内部加强板
            var chamferNone = new Chamfer();
            var chamferLine = new Chamfer(innerStiffener_chamferSize, innerStiffener_chamferSize, Chamfer.ChamferTypeEnum.CHAMFER_LINE);
            var chamferArcPoint = new Chamfer { Type = Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT };

            if (innerStiffener_thickness == 0.0) goto Skip_InnerSfiffener;

            static ContourPlate CreatAndWeldInnerStiffener(
                Part prim, Part thickenedStiffener1, Part thickenedStiffener2,
                Point point1, Point point2, Point point3, Point point4,
                string name, double thickness, string materialStr, Chamfer chamfer, Position.DepthEnum depthEnum) {

                var plate = ModelOperation.CreatContourPlate(new ArrayList{
                    new ContourPoint(point1, chamfer),
                    new ContourPoint(point2, chamfer),
                    new ContourPoint(point3, chamfer),
                    new ContourPoint(point4, chamfer)
                }, name, $"PL{thickness}", materialStr, depthEnum: depthEnum);

                var p5 = new Point(point1.X + chamfer.X, point1.Y, point1.Z);
                var p6 = new Point(point2.X - chamfer.X, point2.Y, point2.Z);
                ModelOperation.CreatPolygonWeld(thickenedStiffener1 ?? prim, plate, new Polygon { Points = new ArrayList { p5, p6 } },
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);

                p5 = new Point(point2.X, point2.Y, point2.Z - chamfer.X);
                p6 = new Point(point3.X, point3.Y, point3.Z + chamfer.X);
                ModelOperation.CreatPolygonWeld(prim, plate, new Polygon { Points = new ArrayList { p5, p6 } },
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);

                p5 = new Point(point3.X - chamfer.X, point3.Y, point3.Z);
                p6 = new Point(point4.X + chamfer.X, point4.Y, point4.Z);
                ModelOperation.CreatPolygonWeld(thickenedStiffener2 ?? prim, plate, new Polygon { Points = new ArrayList { p5, p6 } },
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);

                p5 = new Point(point4.X, point4.Y, point4.Z + chamfer.X);
                p6 = new Point(point1.X, point1.Y, point1.Z - chamfer.X);
                ModelOperation.CreatPolygonWeld(prim, plate, new Polygon { Points = new ArrayList { p5, p6 } },
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);

                return plate;
            }

            point1 = new Point(secTxPrimR) {
                X = strong ? prfPrim.h1 * -0.5 + prfPrim.t : prfPrim.b1 * -0.5 + prfPrim.s,
                Z = thickenedStiffener1 is null ?
                    (strong ? prfPrim.b1 * 0.5 - prfPrim.s : prfPrim.h1 * 0.5 - prfPrim.t) :
                    (strong ? prfPrim.b1 * 0.5 - thickenedStiffener_thickness : prfPrim.h1 * 0.5 - thickenedStiffener_thickness)
            };
            point2 = new Point(point1) { X = -point1.X };
            var point3 = new Point(point2) { Z = -point2.Z };
            var point4 = new Point(point3) { X = -point3.X };
            CreatAndWeldInnerStiffener(prim, thickenedStiffener1, thickenedStiffener2,
                point1, point2, point3, point4,
                "INNER_STIFFENER", innerStiffener_thickness, stiffener_materialStr, chamferLine, Position.DepthEnum.BEHIND);

            if (moreInnerStiffeners_distanceList.Count == 0)
                goto Skip_MoreInnerStiffeners;

            foreach (var dis in moreInnerStiffeners_distanceList) {
                point1.Y -= dis.Value;
                point2.Y -= dis.Value;
                point3.Y -= dis.Value;
                point4.Y -= dis.Value;
                CreatAndWeldInnerStiffener(prim, thickenedStiffener1, thickenedStiffener2,
                    point1, point2, point3, point4,
                    "INNER_STIFFENER", innerStiffener_thickness, stiffener_materialStr, chamferLine, Position.DepthEnum.BEHIND);
            }

        Skip_MoreInnerStiffeners:;

            point1.Y = secBOTxPrimR.Y;
            point2.Y = secBOTxPrimR.Y;
            point3.Y = secBOTxPrimR.Y;
            point4.Y = secBOTxPrimR.Y;
            CreatAndWeldInnerStiffener(prim, thickenedStiffener1, thickenedStiffener2,
                point1, point2, point3, point4,
                "INNER_STIFFENER", innerStiffener_thickness, stiffener_materialStr, chamferLine, Position.DepthEnum.FRONT);

        Skip_InnerSfiffener:;
            #endregion

            if (type != 0) goto ShortBeam;

            #region Type0
            #region 梁对齐切割
            var fittingPlane = new Plane { Origin = primRightBoundaryPlane.Origin, AxisX = axisY, AxisY = axisZ };
            var fitting = new Fitting { Father = sec, Plane = fittingPlane };
            fitting.Insert();

            point1 = new Point(-100, prfSec.h1 * 0.5 - prfSec.t1, 0);
            point2 = new Point(gap + ratHoel_radius, point1.Y, 0);
            point4 = new Point(gap, point2.Y - ratHoel_radius, 0);
            point3 = new Point(point4.X + ratHoleCornerOffset, point2.Y - ratHoleCornerOffset, 0);
            var point5 = new Point(point4.X, -point4.Y, 0);
            var point6 = new Point(point3.X, -point3.Y, 0);
            var point7 = new Point(point2.X, -point2.Y, 0);
            var point8 = new Point(point1.X, -point1.Y, 0);
            TransformPoint(new[] { shearMatrix_AxisY, transferMatrix },
                new[] { point1, point2, point3, point4, point5, point6, point7, point8 });

            var cp1 = new ContourPoint(point1, chamferNone);
            var cp2 = new ContourPoint(point2, chamferNone);
            var cp3 = new ContourPoint(point3, chamferArcPoint);
            var cp4 = new ContourPoint(point4, chamferNone);
            var cp5 = new ContourPoint(point5, chamferNone);
            var cp6 = new ContourPoint(point6, chamferArcPoint);
            var cp7 = new ContourPoint(point7, chamferNone);
            var cp8 = new ContourPoint(point8, chamferNone);

            var cutting = ModelOperation.CreatBooleanOperationPolygon(
                new ArrayList { cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8 }, prfSec.b1 + 50);

            ModelOperation.ApplyBooleanOperation(sec, cutting);
            cutting.Delete();

            var polygon = new Polygon { Points = new ArrayList { secTFxPrimR, secTBxPrimR } };
            //  此处焊接准备，正交梁（可上下倾斜）正常，但歪梁（左右倾斜）会导致梁模型消失。
            //  使用板拼梁或可解决此问题
            var weld = ModelOperation.CreatPolygonWeld(prim, sec, polygon, false, false,
                orthogonal ? BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY : BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE,
                BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                prfSec.t1 - weld_root_face, weld_angle);
            weld.RootFaceAbove = weld_root_face;
            weld.RootOpeningAbove = weld_root_opening;
            weld.Modify();

            polygon = new Polygon { Points = new ArrayList { secBFxPrimR + secAxisY * prfSec.t2, secBBxPrimR + secAxisY * prfSec.t2 } };
            //  此处焊接准备，
            //  正交梁（可上下倾斜）只在腹板处正常，其余地方只有root opening起作用，root face不起作用。
            //  歪梁（左右倾斜）会导致梁模型消失。
            //  使用板拼梁或可解决此问题
            weld = ModelOperation.CreatPolygonWeld(prim, sec, polygon, false, false,
                orthogonal ? BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY : BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE,
                BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                prfSec.t1, weld_angle);
            weld.RootOpeningAbove = weld_root_opening + (cover_thickness - weld_root_face) * Math.Tan(weld_angle * Math.PI / 180);
            weld.Modify();

            #endregion

            #region 腹板连接板
            Part webConnectionPlate_F = null, webConnectionPlate_B = null;
            point1 = new Point(0, prfSec.h1 * 0.5 - webPosition_distanceList_Y[0].Value, 0);
            point2 = new Point(gap + webPosition_distanceList_X.Sum(dis => dis.Value), point1.Y, 0);
            point3 = new Point(point2.X, -point2.Y, 0);
            point4 = new Point(0, point3.Y, 0);
            TransformPoint(new[] { shearMatrix_AxisY, transferMatrix }, new[] { point1, point2, point3, point4 });

            if (webConnectionPlate_creationEnum == 0 || webConnectionPlate_creationEnum == 2) {
                webConnectionPlate_F = ModelOperation.CreatContourPlate(new ArrayList {
                        new ContourPoint(point1, chamferNone),
                        new ContourPoint(point2, chamferNone),
                        new ContourPoint(point3, chamferNone),
                        new ContourPoint(point4, chamferNone)
                    }, "CONNECTION_PLATE", $"PL{webConnectionPlate_thickness}", connectionPlate_materialStr,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: prfSec.s * 0.5);

                ModelOperation.CreatWeld(prim, webConnectionPlate_F, false,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
            }

            if (webConnectionPlate_creationEnum == 1 || webConnectionPlate_creationEnum == 2) {
                webConnectionPlate_B = ModelOperation.CreatContourPlate(new ArrayList {
                        new ContourPoint(point1, chamferNone),
                        new ContourPoint(point2, chamferNone),
                        new ContourPoint(point3, chamferNone),
                        new ContourPoint(point4, chamferNone)
                    }, "CONNECTION_PLATE", $"PL{webConnectionPlate_thickness}", connectionPlate_materialStr,
                depthEnum: Position.DepthEnum.FRONT, depthOffset: prfSec.s * 0.5);

                ModelOperation.CreatWeld(prim, webConnectionPlate_B, false,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET, sizeBelow: 6);
            }

            #endregion

            #region 腹板栓接
            var cnt = webPosition_distanceList_Y.Count;
            point1 = new Point(gap, prfSec.h1 * 0.5 - webPosition_distanceList_Y[0].Value, 0);
            point2 = new Point(point1.X, point1.Y - 100, point1.Z);
            for (int i = 0; i < webPosition_distanceList_X.Count - 1; i++) {
                point1.X += webPosition_distanceList_X[i].Value;
                point2.X += webPosition_distanceList_X[i].Value;
                point3 = new Point(point1);
                point4 = new Point(point2);
                TransformPoint(new[] { shearMatrix_AxisY, transferMatrix }, new[] { point3, point4 });

                ModelOperation.CreatBoltArray(sec, webConnectionPlate_F ?? webConnectionPlate_B,
                    webConnectionPlate_B == null ? null : new Part[] { webConnectionPlate_B },
                    point3, point4, webPosition_distanceList_Y.Skip(2).Take(cnt - 3), new TSD.Distance[] { new TSD.Distance(0) },
                    new Position { Rotation = Position.RotationEnum.FRONT },
                    startOffset: new Offset { Dx = webPosition_distanceList_Y[1].Value },
                    boltStandard: webBolt_standard, boltSize: webBolt_size);
            }
            #endregion

            #region 盖板
            static ContourPlate CreatCoverPlate(
                Part prim, Point p1, Point p2, Point p3, Point p4, Point p5, Point p6,
                Chamfer chamfer, Matrix shearMatrix, Matrix transferMatrix,
                string name, double thickness, string materialStr, Position.DepthEnum depthEnum,
                double weldSize, double weldAngle, double rootFace, double rootOpening) {

                TransformPoint(new[] { shearMatrix, transferMatrix }, new[] { p1, p2, p3, p4, p5, p6 });

                var contourPoints = new ArrayList {
                        new ContourPoint(p1, chamfer),
                        new ContourPoint(p2, chamfer),
                        new ContourPoint(p3, chamfer),
                        new ContourPoint(p4, chamfer),
                        new ContourPoint(p5, chamfer),
                        new ContourPoint(p6, chamfer)
                    };
                var plate = ModelOperation.CreatContourPlate(
                    contourPoints, name, $"PL{thickness}", materialStr, depthEnum: depthEnum);
                plate.Contour.ContourPoints = contourPoints;//  轮廓点顺序有时会错乱，此处作用为修正顺序
                plate.Modify();

                var weld = ModelOperation.CreatWeld(prim, plate, false, false,
                    preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY,
                    typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_NONE,
                    sizeAbove: 0, angleAbove: 0,
                    typeBelow: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                    sizeBelow: weldSize, angleBelow: weldAngle);
                weld.RootFaceBelow = rootFace;
                weld.RootOpeningBelow = rootOpening;
                weld?.Modify();

                return plate;
            }

            point1 = new Point(0, prfSec.h1 * 0.5, topCover_width1 + topCover_width2 * 0.5);
            point2 = new Point(point1.X + cover_length1, point1.Y, point1.Z);
            point3 = new Point(point2.X + cover_length2, point2.Y, topCover_width2 * 0.5);
            point4 = new Point(point3.X, point3.Y, -point3.Z);
            point5 = new Point(point2.X, point2.Y, -point2.Z);
            point6 = new Point(point1.X, point1.Y, -point1.Z);

            var topCover = CreatCoverPlate(prim, point1, point2, point3, point4, point5, point6, chamferNone,
                shearMatrix, transferMatrix, "TOP_COVER", cover_thickness, cover_materialStr, Position.DepthEnum.FRONT,
                cover_thickness, weld_angle, 0, weld_root_opening + (prfSec.t1 - weld_root_face) * Math.Tan(weld_angle * Math.PI / 180));
            if (topCover_width1 * 2 + topCover_width2 > prfSec.b1) {
                point1 = new Point(0, prfSec.h1 * 0.5, prfSec.b1 * 0.5);
                point6 = new Point(point1.X, point1.Y, -point1.Z);
                TransformPoint(new[] { shearMatrix, transferMatrix }, new[] { point1, point6 });
                point2 = Intersection.LineToLine(new Line(point1, secAxisX), new Line(point2, point3)).StartPoint;
                point5 = Intersection.LineToLine(new Line(point6, secAxisX), new Line(point4, point5)).StartPoint;
            }
            ModelOperation.CreatPolygonWeld(sec, topCover,
                new Polygon { Points = new ArrayList { point1, point2, point3, point4, point5, point6 } });

            point1 = new Point(0, prfSec.h1 * -0.5, bottomCover_width1 + bottomCover_width2 * 0.5);
            point2 = new Point(point1.X + cover_length1, point1.Y, point1.Z);
            point3 = new Point(point2.X + cover_length2, point2.Y, bottomCover_width2 * 0.5);
            point4 = new Point(point3.X, point3.Y, -point3.Z);
            point5 = new Point(point2.X, point2.Y, -point2.Z);
            point6 = new Point(point1.X, point1.Y, -point1.Z);

            var bottomCover = CreatCoverPlate(prim, point1, point2, point3, point4, point5, point6, chamferNone,
                shearMatrix, transferMatrix, "BOTTOM_COVER", cover_thickness, cover_materialStr, Position.DepthEnum.BEHIND,
                cover_thickness - weld_root_face, weld_angle, 2, weld_root_opening);
            if (bottomCover_width1 * 2 + bottomCover_width2 > prfSec.b1) {
                point1 = new Point(0, prfSec.h1 * -0.5, prfSec.b1 * 0.5);
                point6 = new Point(point1.X, point1.Y, -point1.Z);
                TransformPoint(new[] { shearMatrix, transferMatrix }, new[] { point1, point6 });
                point2 = Intersection.LineToLine(new Line(point1, secAxisX), new Line(point2, point3)).StartPoint;
                point5 = Intersection.LineToLine(new Line(point6, secAxisX), new Line(point4, point5)).StartPoint;
            }
            ModelOperation.CreatPolygonWeld(sec, bottomCover,
                new Polygon { Points = new ArrayList { point1, point2, point3, point4, point5, point6 } });
            #endregion

            return;
            #endregion

        ShortBeam:;
            #region 短梁
            var shortBeam = ModelOperation.CreatBeam(secOrigin, secOrigin + secAxisX * shortBeamLength,
                "SHORT_BEAM", shortBeam_prfStr, shortBeam_materialStr,
                sec.AssemblyNumber.Prefix, partPrefix: sec.PartNumber.Prefix,
                rotationOffset: sec.Position.RotationOffset);

            fittingPlane = new Plane { Origin = secOrigin, AxisX = axisY, AxisY = axisZ };
            fitting = new Fitting { Father = shortBeam, Plane = fittingPlane };
            fitting.Insert();

            point1 = new Point(-50, prfShort.h1 * 0.5 - prfShort.t1, 0);
            point2 = new Point(ratHoel_radius, point1.Y, point1.Z);
            point3 = new Point(ratHoleCornerOffset, point1.Y - ratHoleCornerOffset, point1.Z);
            point4 = new Point(0, point1.Y - ratHoel_radius, point1.Z);
            point5 = new Point(-50, point4.Y, point4.Z);
            cp1 = new ContourPoint(point1, chamferNone);
            cp2 = new ContourPoint(point2, chamferNone);
            cp3 = new ContourPoint(point3, chamferArcPoint);
            cp4 = new ContourPoint(point4, chamferNone);
            cp5 = new ContourPoint(point5, chamferNone);
            TransformPoint(new[] { shearMatrix, transferMatrix },
                new[] { cp1, cp2, cp3, cp4, cp5 });
            cutting = ModelOperation.CreatBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3, cp4, cp5 }, prfShort.b1);
            ModelOperation.ApplyBooleanOperation(shortBeam, cutting);
            cutting.Delete();

            point1.Y *= -1; point2.Y *= -1; point3.Y *= -1; point4.Y *= -1; point5.Y *= -1;
            cp1 = new ContourPoint(point1, chamferNone);
            cp2 = new ContourPoint(point2, chamferNone);
            cp3 = new ContourPoint(point3, chamferArcPoint);
            cp4 = new ContourPoint(point4, chamferNone);
            cp5 = new ContourPoint(point5, chamferNone);
            TransformPoint(new[] { shearMatrix, transferMatrix },
                new[] { cp1, cp2, cp3, cp4, cp5 });
            cutting = ModelOperation.CreatBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3, cp4, cp5 }, prfShort.b1);
            ModelOperation.ApplyBooleanOperation(shortBeam, cutting);
            cutting.Delete();

            weld = ModelOperation.CreatPolygonWeld(prim, shortBeam, new Polygon { Points = new ArrayList { secTFxPrimR, secTBxPrimR } },
                preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: prfShort.t1 - weld_root_face, angleAbove: weld_angle);
            weld.RootFaceAbove = weld_root_face;
            weld.RootOpeningAbove = weld_root_opening;
            weld.Modify();

            point1 = Intersection.LineToPlane(new Line(secBFxPrimR + secAxisY * prfShort.t2, secAxisX), primRightBoundaryPlane);
            point2 = Intersection.LineToPlane(new Line(secBBxPrimR + secAxisY * prfShort.t2, secAxisX), primRightBoundaryPlane);
            // 斜交梁此处焊接准备会导致梁模型消失
            weld = ModelOperation.CreatPolygonWeld(prim, shortBeam,
                new Polygon { Points = new ArrayList { point1, point2 } },
                preparation: orthogonal ? BaseWeld.WeldPreparationTypeEnum.PREPARATION_SECONDARY : BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: prfShort.t2 - weld_root_face, angleAbove: weld_angle);
            weld.RootFaceAbove = weld_root_face;
            weld.RootOpeningAbove = weld_root_opening;
            weld.Modify();

            point1 = new Point(0, prfShort.h1 * 0.5 - prfShort.t1 - ratHoel_radius, prfShort.s * 0.5);
            point2 = new Point(0, prfShort.h1 * -0.5 + prfShort.t2 + ratHoel_radius, prfShort.s * 0.5);
            point3 = new Point(point1) { Z = -point1.Z };
            point4 = new Point(point2) { Z = -point2.Z };
            TransformPoint(new[] { shearMatrix, transferMatrix }, new[] { point1, point2, point3, point4 });
            ModelOperation.CreatPolygonWeld(prim, shortBeam,
                new Polygon { Points = new ArrayList { point1, point2 } });
            ModelOperation.CreatPolygonWeld(prim, shortBeam,
                new Polygon { Points = new ArrayList { point3, point4 } });
            #endregion

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
                new TransformationPlane(shortBeam.EndPoint, secAxisX, secAxisY));

            #region 腹板连接板栓接
            point1 = new Point(gap * 0.5, prfShort.h1 * 0.5 - webPosition_distanceList_Y[0].Value, 0);
            point2 = new Point(point1) { Y = prfShort.h1 * 0.5 - webPosition_distanceList_Y.Sum(dis => dis.Value) };
            webConnectionPlate_F = ModelOperation.CreatBeam(point1, point2,
                "WEB_CONNECTION_PLATE",
                $"PL{webConnectionPlate_thickness}*{webPosition_distanceList_X.Sum(dis => dis.Value)}",
                connectionPlate_materialStr,
                depthEnum: Position.DepthEnum.FRONT, depthOffset: prfSec.s * 0.5,
                rotationEnum: Position.RotationEnum.FRONT);
            webConnectionPlate_B = ModelOperation.CreatBeam(point1, point2,
                "WEB_CONNECTION_PLATE",
                $"PL{webConnectionPlate_thickness}*{webPosition_distanceList_X.Sum(dis => dis.Value)}",
                connectionPlate_materialStr,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: prfSec.s * 0.5,
                rotationEnum: Position.RotationEnum.FRONT);

            var cntX = webPosition_distanceList_X.Count;
            var cntY = webPosition_distanceList_Y.Count;
            ModelOperation.CreatBoltArray(sec, shortBeam, new[] { webConnectionPlate_F, webConnectionPlate_B },
                point1, point2,
                webPosition_distanceList_Y.Skip(2).Take(cntY - 3),
                webPosition_distanceList_X.Skip(1).Take(cntX - 2),
                position: new Position { Rotation = Position.RotationEnum.FRONT },
                startOffset: new Offset { Dx = webPosition_distanceList_Y[1].Value },
                boltStandard: webBolt_standard, boltSize: webBolt_size);
            #endregion

            if (type == 2) goto Type2;

            #region Type1

            #region 原梁对齐
            fittingPlane = new Plane { Origin = origin + axisX * gap, AxisX = axisY, AxisY = axisZ };
            fitting = new Fitting { Father = sec, Plane = fittingPlane };
            fitting.Insert();
            #endregion

            #region 翼缘连接板栓接
            point1 = new Point(flangePosition_distanceList_X.Sum(dis => dis.Value) * -0.5 + gap * 0.5, prfShort.h1 * 0.5, 0);
            point2 = new Point(point1) { X = -point1.X + gap };
            var flangConnectionPlate_TO = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{outterFlangeConnectionPlate_thickness}*{prfShort.b1}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.LEFT,
                rotationEnum: Position.RotationEnum.TOP);
            var flangeConnectionPlate_TIF = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{innerFlangeConnectionPlate_thickness}*{(prfShort.b1 - prfShort.s) * 0.5}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.RIGHT, planeOffset: prfShort.t1,
                depthEnum: Position.DepthEnum.FRONT, depthOffset: prfShort.s * 0.5,
                rotationEnum: Position.RotationEnum.TOP);
            var flangeConnectionPlate_TIB = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{innerFlangeConnectionPlate_thickness}*{(prfShort.b1 - prfShort.s) * 0.5}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.RIGHT, planeOffset: prfShort.t1,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: prfShort.s * 0.5,
                rotationEnum: Position.RotationEnum.TOP);

            cntX = flangePosition_distanceList_X.Count;
            var disListX = flangePosition_distanceList_X.Skip(1).Take(cntX - 2);
            var disListY = flangePosition_distanceList_Y.Skip(1)
                .Append(new TSD.Distance(prfShort.b1 - flangePosition_distanceList_Y.Sum(dis => dis.Value) * 2))
                .Concat(flangePosition_distanceList_Y.Skip(1));
            var boltArray = ModelOperation.CreatBoltArray(sec, shortBeam,
                new[] { flangConnectionPlate_TO, flangeConnectionPlate_TIF, flangeConnectionPlate_TIB },
                point1, point2, disListX, disListY,
                position: new Position { Rotation = Position.RotationEnum.BELOW },
                startOffset: new Offset { Dx = flangePosition_distanceList_X[0].Value },
                boltStandard: flangeBolt_standard, boltSize: flangeBolt_size);
            boltArray.CutLength = 200;
            boltArray.Modify();

            point1 = new Point(point1) { Y = -point1.Y };
            point2 = new Point(point2) { Y = -point2.Y };
            var flangeConnectionPlate_BO = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{outterFlangeConnectionPlate_thickness}*{prfShort.b1}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.RIGHT,
                rotationEnum: Position.RotationEnum.TOP);
            var flangeConnectionPlate_BIF = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{innerFlangeConnectionPlate_thickness}*{(prfShort.b1 - prfShort.s) * 0.5}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.LEFT, planeOffset: prfShort.t2,
                depthEnum: Position.DepthEnum.FRONT, depthOffset: prfShort.s * 0.5,
                rotationEnum: Position.RotationEnum.TOP);
            var flangeConnectionPlate_BIB = ModelOperation.CreatBeam(point1, point2, "FLANGE_CONNECTION_PLATE",
                $"PL{innerFlangeConnectionPlate_thickness}*{(prfShort.b1 - prfShort.s) * 0.5}",
                connectionPlate_materialStr,
                planeEnum: Position.PlaneEnum.LEFT, planeOffset: prfShort.t2,
                depthEnum: Position.DepthEnum.BEHIND, depthOffset: prfShort.s * 0.5,
                rotationEnum: Position.RotationEnum.TOP);

            boltArray = ModelOperation.CreatBoltArray(sec, shortBeam,
                new[] { flangeConnectionPlate_BO, flangeConnectionPlate_BIF, flangeConnectionPlate_BIB },
                point1, point2, disListX, disListY,
                position: new Position { Rotation = Position.RotationEnum.BELOW },
                startOffset: new Offset { Dx = flangePosition_distanceList_X[0].Value },
                boltStandard: flangeBolt_standard, boltSize: flangeBolt_size);
            boltArray.CutLength = 200;
            boltArray.Modify();
            #endregion

            return;

            #endregion

        Type2:;
            #region Type2

            #region 原梁对齐
            fittingPlane = new Plane { Origin = origin, AxisX = axisY, AxisY = axisZ };
            fitting = new Fitting { Father = sec, Plane = fittingPlane };
            fitting.Insert();
            #endregion

            #region 切割
            point1 = new Point(-ratHoel_radius, prfShort.h1 * 0.5 - prfShort.t1, 0);
            point2 = new Point(-ratHoleCornerOffset, point1.Y - ratHoleCornerOffset, 0);
            point3 = new Point(0, point1.Y - ratHoel_radius, 0);
            point4 = new Point(point3) { Y = -point3.Y };
            point5 = new Point(point2) { Y = -point2.Y };
            point6 = new Point(point1) { Y = -point1.Y };
            point7 = new Point(point6) { X = gap + ratHoel_radius };
            point8 = new Point(gap + ratHoleCornerOffset, point7.Y + ratHoleCornerOffset, 0);
            var point9 = new Point(gap, point7.Y + ratHoel_radius, 0);
            var point10 = new Point(point9) { Y = -point9.Y };
            var point11 = new Point(point8) { Y = -point8.Y };
            var point12 = new Point(point7) { Y = -point7.Y };
            cp1 = new ContourPoint(point1, chamferNone);
            cp2 = new ContourPoint(point2, chamferArcPoint);
            cp3 = new ContourPoint(point3, chamferNone);
            cp4 = new ContourPoint(point4, chamferNone);
            cp5 = new ContourPoint(point5, chamferArcPoint);
            cp6 = new ContourPoint(point6, chamferNone);
            cp7 = new ContourPoint(point7, chamferNone);
            cp8 = new ContourPoint(point8, chamferArcPoint);
            var cp9 = new ContourPoint(point9, chamferNone);
            var cp10 = new ContourPoint(point10, chamferNone);
            var cp11 = new ContourPoint(point11, chamferArcPoint);
            var cp12 = new ContourPoint(point12, chamferNone);
            cutting = ModelOperation.CreatBooleanOperationPolygon(
                new ArrayList { cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9, cp10, cp11, cp12 },
                prfShort.b1);
            ModelOperation.ApplyBooleanOperation(sec, cutting);
            ModelOperation.ApplyBooleanOperation(shortBeam, cutting);
            cutting.Delete();
            #endregion

            #region 焊接
            point1 = new Point(0, prfShort.h1 * 0.5, prfShort.b1 * 0.5);
            point2 = new Point(point1) { Z = -point1.Z };
            weld = ModelOperation.CreatPolygonWeld(sec, shortBeam, new Polygon { Points = new ArrayList { point1, point2 } },
                shopWeld: false, preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_MAIN,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: prfShort.t1 - weld_root_face, angleAbove: weld_angle);
            weld.Placement = BaseWeld.WeldPlacementTypeEnum.PLACEMENT_MAIN;
            weld.RootFaceAbove = weld_root_face;
            weld.RootOpeningAbove = weld_root_opening;
            weld.Modify();

            point1 = new Point(0, prfShort.h1 * -0.5 + prfShort.t2, prfShort.b1 * 0.5);
            point2 = new Point(point1) { Z = -point1.Z };
            weld = ModelOperation.CreatPolygonWeld(sec, shortBeam, new Polygon { Points = new ArrayList { point1, point2 } },
                shopWeld: false, preparation: BaseWeld.WeldPreparationTypeEnum.PREPARATION_MAIN,
                typeAbove: BaseWeld.WeldTypeEnum.WELD_TYPE_SINGLE_BEVEL_BUTT_WITH_BROAD_ROOT_FACE,
                sizeAbove: prfShort.t2 - weld_root_face, angleAbove: weld_angle);
            weld.Placement = BaseWeld.WeldPlacementTypeEnum.PLACEMENT_MAIN;
            weld.RootFaceAbove = weld_root_face;
            weld.RootOpeningAbove = weld_root_opening;
            weld.Modify();
            #endregion

            return;

            #endregion

        }
        #endregion
    }
}
