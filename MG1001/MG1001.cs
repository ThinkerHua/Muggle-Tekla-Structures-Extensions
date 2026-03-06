/*==============================================================================
 *  Muggle Tekla-Plugins - tools and plugins for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.                 
 *
 *  This library is free software, licensed under the terms of the GNU 
 *  General Public License as published by the Free Software Foundation, 
 *  either version 3 of the License, or (at your option) any later version. 
 *  You should have received a copy of the GNU General Public License 
 *  along with this program. If not, see <http://www.gnu.org/licenses/>. 
 *==============================================================================
 *  MG1001.cs: "MG1001" connection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using TSDatatype = Tekla.Structures.Datatype;
using TSG3d = Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.MG1001 {
    /// <summary>
    /// <para>prfStr_TOP:柱顶板规格，[PLt[*b]]，缺省的参数表示与梁翼缘相同</para>
    /// <para>bol_TOPHOR:柱顶板水平布置或顺坡</para>
    /// <para>prfStr_DIAG:对角板规格，PLt[*b]，缺省的参数表示与梁翼缘相同</para>
    /// <para>pos_DIAG1:对角板定位，左上距柱翼缘内表面距离</para>
    /// <para>pos_DIAG2:对角板定位，右下距端板1外表面距离</para>
    /// <para>chamfer_DIAG:对角板倒角</para>
    /// <para>prfStr_HOR:平板规格，[PLt[*b]]，缺省的参数表示与梁翼缘相同</para>
    /// <para>chamfer_HOR:平板倒角</para>
    /// <para>thk_THKED:腹板加厚区厚度</para>
    /// <para>pos_THKED:腹板加厚区定位，距平板下表面距离</para>
    /// <para>len_Eave:檐口挑出长度</para>
    /// <para>hgt_Eave:檐口高度</para>
    /// <para>thk_Eave:檐口竖板厚度，缺省表示与柱腹板同厚（如果存在加厚区的话，则与加厚区同厚）</para>
    /// <para>diff_THK:需开坡口的板厚差</para>
    /// <para>slope_THK:坡口坡度</para>
    /// <para>prfStr_EndPlate1:端板1规格，PLt*b*l，不可缺省</para>
    /// <para>prfStr_EndPlate2:端板2规格，PLt*b*l，不可缺省</para>
    /// <para>pos_EndPlate:端板定位</para>
    /// <para>prfStr_STIF_FLNG:翼缘加劲板规格，三角形板，PLt*b*l，不可缺省</para>
    /// <para>prfStr_STIF_Web:腹板加劲板规格，可选三角形板或矩形板，PLt*b*l，不可缺省</para>
    /// <para>chamfer_STIF_in:翼缘和腹板加劲板倒角内倒角</para>
    /// <para>type_STIF_Web:腹板加劲板形式</para>
    /// <para>chamfer_STIF_out:腹板加劲板外倒角</para>
    /// <para>disLstStr_STIF_Web:腹板加劲板间距</para>
    /// <para>disLstStr_Bolt_X:螺栓间距X</para>
    /// <para>disLstStr_Bolt_Y:螺栓间距Y</para>
    /// <para>bolt_Standard:螺栓标准</para>
    /// <para>bolt_Size:螺栓尺寸</para>
    /// <para>materialStr:节点零件材质</para>
    /// </summary>
    public class MG1001Data {
        [StructuresField("prfStr_TOP")]
        public string prfStr_TOP;
        [StructuresField("bol_TOPHOR")]
        public int bol_TOPHOR;
        [StructuresField("prfStr_DIAG")]
        public string prfStr_DIAG;
        [StructuresField("pos_DIAG1")]
        public double pos_DIAG1;
        [StructuresField("pos_DIAG2")]
        public double pos_DIAG2;
        [StructuresField("chamfer_DIAG")]
        public double chamfer_DIAG;
        [StructuresField("prfStr_HOR")]
        public string prfStr_HOR;
        [StructuresField("chamfer_HOR")]
        public double chamfer_HOR;
        [StructuresField("thk_THKED")]
        public double thk_THKED;
        [StructuresField("pos_THKED")]
        public double pos_THKED;
        [StructuresField("len_Eave")]
        public double len_Eave;
        [StructuresField("hgt_Eave")]
        public double hgt_Eave;
        [StructuresField("thk_Eave")]
        public double thk_Eave;
        [StructuresField("diff_THK")]
        public double diff_THK;
        [StructuresField("slope_THK")]
        public double slope_THK;
        [StructuresField("prfStr_EndPlate1")]
        public string prfStr_EndPlate1;
        [StructuresField("prfStr_EndPlate2")]
        public string prfStr_EndPlate2;
        [StructuresField("pos_EndPlate")]
        public double pos_EndPlate;
        [StructuresField("prfStr_STIF_FLNG")]
        public string prfStr_STIF_FLNG;
        [StructuresField("prfStr_STIF_Web")]
        public string prfStr_STIF_Web;
        [StructuresField("chamfer_STIF_in")]
        public double chamfer_STIF_in;
        [StructuresField("type_STIF_Web")]
        public int type_STIF_Web;
        [StructuresField("chamfer_STIF_out")]
        public double chamfer_STIF_out;
        [StructuresField("disLstStr_STIF_Web")]
        public string disLstStr_STIF_Web;
        [StructuresField("disLstStr_Bolt_X")]
        public string disLstStr_Bolt_X;
        [StructuresField("disLstStr_Bolt_Y")]
        public string disLstStr_Bolt_Y;
        [StructuresField("bolt_Standard")]
        public string bolt_Standard;
        [StructuresField("bolt_Size")]
        public double bolt_Size;
        [StructuresField("materialStr")]
        public string materialStr;
    }

    [Plugin("MG1001")]
    [PluginUserInterface("Muggle.TsExtensions.MG1001.Views.MainWindow")]
    //[InputObjectType(InputObjectType.INPUTOBJECT_PART)]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ONE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_BASIC)]
    //[PositionType(PositionTypeEnum.GUSSET_PLANE)]

    public class MG1001 : ConnectionBase {
        #region Fields
        private readonly Model _model;
        private readonly MG1001Data _data;

        private string prfStr_TOP;
        private int bol_TOPHOR;
        private string prfStr_DIAG;
        private double pos_DIAG1;
        private double pos_DIAG2;
        private double chamfer_DIAG;
        private string prfStr_HOR;
        private double chamfer_HOR;
        private double thk_THKED;
        private double pos_THKED;
        private double len_Eave;
        private double hgt_Eave;
        private double thk_Eave;
        private double diff_THK;
        private double slope_THK;
        private string prfStr_EndPlate1;
        private string prfStr_EndPlate2;
        private double pos_EndPlate;
        private string prfStr_STIF_FLNG;
        private string prfStr_STIF_Web;
        private double chamfer_STIF_in;
        private int type_STIF_Web;
        private double chamfer_STIF_out;
        private string disLstStr_Bolt_X;
        private string disLstStr_Bolt_Y;
        private string disLstStr_STIF_Web;
        private DistanceList disLst_STIF_Web;
        private DistanceList disLst_Bolt_X;
        private DistanceList disLst_Bolt_Y;
        private Offset bolt_startOffset;
        private string bolt_Standard;
        private double bolt_Size;
        private string materialStr;

        private TransformationPlane originTP, workTP;
        private double slope;
        #endregion

        #region Constructor
        public MG1001(MG1001Data data) {
            _model = new Model();
            _data = data;
            GetValuesFromDialog();
        }
        #endregion

        #region Overrides
        public override bool Run() {
            bool result = false;
            Beam PRIMPart, SECPart;

            try {
                PRIMPart = _model.SelectModelObject(Primary) as Beam;
                SECPart = _model.SelectModelObject(Secondaries[0]) as Beam;

                CheckIfAcceptableProfile(PRIMPart, SECPart, out ProfileH prf_PRIM, out ProfileH prf_SEC);

                if (originTP == null) {
                    originTP = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                }
                if (workTP == null) {
                    workTP = SetWorkTransformationPlane(prf_PRIM, prf_SEC);//And get slope at the same time;
                }

                result = CreatConnection(PRIMPart, SECPart);
            } catch (Exception Exc) {
                MessageBox.Show(Exc.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }
        #endregion

        #region Private methods
        private void GetValuesFromDialog() {
            prfStr_TOP = _data.prfStr_TOP;
            bol_TOPHOR = _data.bol_TOPHOR;
            prfStr_DIAG = _data.prfStr_DIAG;
            pos_DIAG1 = _data.pos_DIAG1;
            pos_DIAG2 = _data.pos_DIAG2;
            chamfer_DIAG = _data.chamfer_DIAG;
            prfStr_HOR = _data.prfStr_HOR;
            chamfer_HOR = _data.chamfer_HOR;
            thk_THKED = _data.thk_THKED;
            pos_THKED = _data.pos_THKED;
            len_Eave = _data.len_Eave;
            hgt_Eave = _data.hgt_Eave;
            thk_Eave = _data.thk_Eave;
            diff_THK = _data.diff_THK;
            slope_THK = _data.slope_THK;
            prfStr_EndPlate1 = _data.prfStr_EndPlate1;
            prfStr_EndPlate2 = _data.prfStr_EndPlate2;
            pos_EndPlate = _data.pos_EndPlate;
            prfStr_STIF_FLNG = _data.prfStr_STIF_FLNG;
            prfStr_STIF_Web = _data.prfStr_STIF_Web;
            chamfer_STIF_in = _data.chamfer_STIF_in;
            type_STIF_Web = _data.type_STIF_Web;
            chamfer_STIF_out = _data.chamfer_STIF_out;
            disLstStr_STIF_Web = _data.disLstStr_STIF_Web;
            disLstStr_Bolt_X = _data.disLstStr_Bolt_X;
            disLstStr_Bolt_Y = _data.disLstStr_Bolt_Y;
            bolt_Standard = _data.bolt_Standard;
            bolt_Size = _data.bolt_Size;
            materialStr = _data.materialStr;

            if (IsDefaultValue(prfStr_TOP))
                prfStr_TOP = string.Empty;
            if (IsDefaultValue(bol_TOPHOR))
                bol_TOPHOR = 1;
            if (IsDefaultValue(prfStr_DIAG))
                prfStr_DIAG = string.Empty;
            if (IsDefaultValue(pos_DIAG1) || pos_DIAG1 < 0)
                pos_DIAG1 = 80;
            if (IsDefaultValue(pos_DIAG2) || pos_DIAG2 < 0)
                pos_DIAG2 = 80;
            if (IsDefaultValue(chamfer_DIAG) || chamfer_DIAG < 0)
                chamfer_DIAG = 0;
            if (IsDefaultValue(prfStr_HOR))
                prfStr_HOR = string.Empty;
            if (IsDefaultValue(chamfer_HOR) || chamfer_HOR < 0)
                chamfer_HOR = 0;
            if (IsDefaultValue(thk_THKED) || thk_THKED < 0)
                thk_THKED = 0;
            if (IsDefaultValue(pos_THKED))
                pos_THKED = 0;
            if (IsDefaultValue(len_Eave) || len_Eave < 0)
                len_Eave = 0;
            if (IsDefaultValue(hgt_Eave) || hgt_Eave < 0)
                hgt_Eave = 100;
            if (IsDefaultValue(thk_Eave) || thk_Eave < 0)
                thk_Eave = 0;
            if (IsDefaultValue(diff_THK) || diff_THK < 0)
                diff_THK = 4;
            if (IsDefaultValue(slope_THK) || slope_THK == 0)
                slope_THK = 2.5;
            if (IsDefaultValue(prfStr_EndPlate1) || prfStr_EndPlate1 == string.Empty)
                prfStr_EndPlate1 = "PL28*350*1355";
            if (IsDefaultValue(prfStr_EndPlate2) || prfStr_EndPlate2 == string.Empty)
                prfStr_EndPlate2 = "PL28*350*1255";
            if (IsDefaultValue(pos_EndPlate) || pos_EndPlate == 0)
                pos_EndPlate = 136;
            if (IsDefaultValue(prfStr_STIF_FLNG) || prfStr_STIF_FLNG == string.Empty)
                prfStr_STIF_FLNG = "PL10*205*135";
            if (IsDefaultValue(prfStr_STIF_Web) || prfStr_STIF_Web == string.Empty)
                prfStr_STIF_Web = "PL10*175*170";
            if (IsDefaultValue(chamfer_STIF_in) || chamfer_STIF_in < 0)
                chamfer_STIF_in = 0;
            if (IsDefaultValue(type_STIF_Web))
                type_STIF_Web = 0;
            if (IsDefaultValue(chamfer_STIF_out) || chamfer_STIF_out < 0)
                chamfer_STIF_out = 25;
            if (IsDefaultValue(disLstStr_STIF_Web) || disLstStr_STIF_Web == string.Empty)
                disLstStr_STIF_Web = "190 4*150";
            if (IsDefaultValue(disLstStr_Bolt_X) || disLstStr_Bolt_X == string.Empty)
                disLstStr_Bolt_X = "80 6*150 120";
            if (IsDefaultValue(disLstStr_Bolt_Y) || disLstStr_Bolt_Y == string.Empty)
                disLstStr_Bolt_Y = "70";
            if (IsDefaultValue(bolt_Standard) || bolt_Standard == string.Empty)
                bolt_Standard = "HS10.9";
            if (IsDefaultValue(bolt_Size) || bolt_Size == 0)
                bolt_Size = 20;
            if (IsDefaultValue(materialStr) || materialStr == string.Empty)
                materialStr = "Q345B";

            disLst_STIF_Web = DistanceList.Parse(disLstStr_STIF_Web, System.Globalization.CultureInfo.InvariantCulture, TSDatatype.Distance.CurrentUnitType);
            disLst_Bolt_X = DistanceList.Parse(disLstStr_Bolt_X, System.Globalization.CultureInfo.InvariantCulture, TSDatatype.Distance.CurrentUnitType);
            bolt_startOffset = new Offset { Dx = disLst_Bolt_X.First().Value, Dy = 0.0, Dz = 0.0 };
            disLst_Bolt_X.RemoveAt(0);
            disLst_Bolt_Y = DistanceList.Parse(disLstStr_Bolt_Y, System.Globalization.CultureInfo.InvariantCulture, TSDatatype.Distance.CurrentUnitType);
        }
        /// <summary>
        /// 仅支持等截面和楔形变截面，不支持对称变截面
        /// </summary>
        /// <param name="primPart"></param>
        /// <param name="secPart"></param>
        private void CheckIfAcceptableProfile(Beam primPart, Beam secPart, out ProfileH prf_PRIM, out ProfileH prf_SEC) {
            try {
                prf_PRIM = new ProfileH(primPart.Profile.ProfileString);
                prf_SEC = new ProfileH(secPart.Profile.ProfileString);

                if (prf_PRIM.ProfileText.Contains("I_VAR_A") && prf_PRIM.h2 != prf_PRIM.h1 || prf_PRIM.b2 != prf_PRIM.b1)
                    throw new UnAcceptableProfileException(prf_PRIM.ProfileText);
                if (prf_SEC.ProfileText.Contains("I_VAR_A") && prf_SEC.h2 != prf_SEC.h1 || prf_SEC.b2 != prf_SEC.b1)
                    throw new UnAcceptableProfileException(prf_SEC.ProfileText);
            } catch (UnAcceptableProfileException) {
                throw;
            }
        }
        private TransformationPlane SetWorkTransformationPlane(ProfileH prf_PRIM, ProfileH prf_SEC) {
            var PRIMPart = _model.SelectModelObject(Primary) as Beam;
            var SECPart = _model.SelectModelObject(Secondaries[0]) as Beam;

            var primCS = PRIMPart.GetCoordinateSystem();
            var secCS = SECPart.GetCoordinateSystem();

            var primCenterLine = PRIMPart.GetCenterLine(false);
            var secCenterLine = SECPart.GetCenterLine(false);
            var p1 = primCenterLine[0] as Point;
            var p2 = primCenterLine[primCenterLine.Count - 1] as Point;
            var p3 = secCenterLine[0] as Point;
            var p4 = secCenterLine[secCenterLine.Count - 1] as Point;

            var primApproximateDirection = new Vector(p2).GetLength() > new Vector(p1).GetLength()
                ? new Vector(p2 - p1) : new Vector(p1 - p2);
            var secApproximateDirection = new Vector(p4).GetLength() > new Vector(p3).GetLength()
                ? new Vector(p4 - p3) : new Vector(p3 - p4);

            var primTranslateVector = new Vector(0, prf_PRIM.h2 * 0.5, 0)
                * (primCS.AxisY.Dot(secApproximateDirection) > 0 ? -1 : 1);
            var secTranslateVector = new Vector(0, prf_SEC.h2 * 0.5, 0)
                * (secCS.AxisY.Dot(primApproximateDirection) > 0 ? -1 : 1);

            var primLeftLine = new Line(
                (p1.TransformTo(primCS) + primTranslateVector).TransformFrom(primCS),
                (p2.TransformTo(primCS) + primTranslateVector).TransformFrom(primCS));
            var secTopLine = new Line(
                (p3.TransformTo(secCS) + secTranslateVector).TransformFrom(secCS),
                (p4.TransformTo(secCS) + secTranslateVector).TransformFrom(secCS));

            var origin = IntersectionExtension.LineToLine(primLeftLine, secTopLine)?.StartPoint;
            if (TSG3d.Distance.PointToPoint(origin, p1)
                > TSG3d.Distance.PointToPoint(origin, p2)) {
                primLeftLine.Direction *= -1;
            }//  统一为从节点近端指向远端
            if (TSG3d.Distance.PointToPoint(origin, p3)
                > TSG3d.Distance.PointToPoint(origin, p4)) {
                secTopLine.Direction *= -1;
            }//  统一为从节点近端指向远端

            var axisZ = primLeftLine.Direction.Cross(secTopLine.Direction);
            var axisX = axisZ.Cross(primLeftLine.Direction);
            var axisY = axisZ.Cross(axisX);

            workTP = new TransformationPlane(origin, axisX, axisY);

            slope = Math.Abs(Math.Tan(axisX.GetAngleBetween(secTopLine.Direction)));

            return workTP;
        }
        private void FixPrim(ProfileH prf_PRIM, double endPlate_EndPoint_Y) {
            if (prf_PRIM.h1 == prf_PRIM.h2) {
                return;
            }

            Beam PRIMPart = (Beam)_model.SelectModelObject(Primary);
            Point point1, point2, point3, point4, point5;
            Point origin;
            Line line;
            Vector direction;
            Vector axisX = new Vector(1, 0, 0), axisY = new Vector(0, 1, 0);
            double angle, distance, d;

            origin = new Point();
            point5 = new Point(Math.Max(prf_PRIM.h1, prf_PRIM.h2) - prf_PRIM.t1, endPlate_EndPoint_Y, 0);
            point1 = new Point(PRIMPart.StartPoint);
            point2 = new Point(PRIMPart.EndPoint);
            //point1.Transform(originTP, workTP);
            //point2.Transform(originTP, workTP);

            if (TSG3d.Distance.PointToPoint(origin, point1) > TSG3d.Distance.PointToPoint(origin, point2)) {
                point1.Y += PRIMPart.StartPointOffset.Dx;
                point3 = new Point(point1);
                point3.X += prf_PRIM.h1;

                direction = new Vector(point5 - point3);
                angle = direction.GetAngleBetween(axisX);
                distance = TSG3d.Distance.PointToPoint(point3, point5);
                angle -= Math.Asin(prf_PRIM.t1 / distance);
                line = new Line(point3, Geometry3dOperation.GetDirectionByAngle(angle));

                point4 = TSG3d.Intersection.LineToLine(line,
                    new Line(new Point(prf_PRIM.h2, 0, 0), axisY)).StartPoint;

                d = point4.Y - point2.Y;
                if (PRIMPart.EndPointOffset.Dx == d) {

                } else {
                    PRIMPart.EndPointOffset.Dx = d;
                    PRIMPart.Modify();
                }
            } else {
                point2.Y -= PRIMPart.EndPointOffset.Dx;
                point4 = new Point(point2);
                point4.X += prf_PRIM.h2;

                direction = new Vector(point5 - point4);
                angle = direction.GetAngleBetween(axisX);
                distance = TSG3d.Distance.PointToPoint(point4, point5);
                angle -= Math.Asin(prf_PRIM.t1 / distance);
                line = new Line(point4, Geometry3dOperation.GetDirectionByAngle(angle));

                point3 = TSG3d.Intersection.LineToLine(line,
                    new Line(new Point(prf_PRIM.h1, 0, 0), axisY)).StartPoint;

                d = point1.Y - point3.Y;
                if (PRIMPart.StartPointOffset.Dx == d) {

                } else {
                    PRIMPart.StartPointOffset.Dx = d;
                    PRIMPart.Modify();
                }
            }
        }
        private bool CreatConnection(Beam PRIMPart, Beam SECPart) {

            #region 声明

            Point origin;                                                       //工作平面
            Vector axisX, axisY, axisZ;

            ProfileH prf_PRIM, prf_SEC;                                         //主次零件规格
            NumberingSeries PRIM_ASMNUM, SEC_ASMNUM;                            //主次零件构件编号

            ProfilePlate prf_End1, prf_End2, prf_TOP, prf_HOR, prf_DIAG;        //节点零件规格
            ProfilePlate prf_STIF_FLNG, prf_STIF_Web;

            Beam endPlate1, endPlate2, topPlate;                                //节点零件
            ContourPlate eavePlate, horPlate_Front, horPlate_Behind,
                thkedPlate, diagPlate_Front, diagPlate_Behind,
                stifFLNG_PRIMTOP, stifFLNG_SECTOP, stifFLNG_SECBTM;
            List<ContourPlate> stifWeb_PRIM, stifWeb_SEC;

            Line PRIM_LEFT_Line, PRIM_RIGHT_Line, SEC_TOP_Line, SEC_BTM_Line;   //主次零件边线
            Line ENDPlate_Line, TOPPlate_Line, HORPlate_Line, DIAGPlate_Line;   //端板边线，顶板边线，水平板边线，对角板中心线

            Point point1, point2, point3, point4, point5;                       //节点零件角点
            Chamfer chamfer_None = new Chamfer();
            Chamfer chamfer_Line, chamfer_Line2;
            ContourPoint cp1, cp2, cp3, cp4, cp5;

            ContourPlate cutplate;                                              //多边形切割
            ArrayList contourPoints;
            #endregion

            #region 初始信息、工作平面
            prf_PRIM = new ProfileH(PRIMPart.Profile.ProfileString);
            prf_SEC = new ProfileH(SECPart.Profile.ProfileString);

            PRIM_ASMNUM = PRIMPart.AssemblyNumber;
            SEC_ASMNUM = SECPart.AssemblyNumber;

            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTP);
            origin = new Point();
            axisX = new Vector(1, 0, 0);
            axisY = new Vector(0, 1, 0);
            axisZ = new Vector(0, 0, 1);
            #endregion

            #region 求主零件边线
            point1 = new Point(PRIMPart.StartPoint).Transform(originTP, workTP);
            point2 = new Point(PRIMPart.EndPoint).Transform(originTP, workTP);

            if (TSG3d.Distance.PointToPoint(origin, point1) > TSG3d.Distance.PointToPoint(origin, point2)) {
                point1.Y += PRIMPart.StartPointOffset.Dx;
                point2.Y += PRIMPart.EndPointOffset.Dx;
                PRIM_LEFT_Line = new Line(point1, point2);
                point1.X += Math.Min(prf_PRIM.h1, prf_PRIM.h2);
                point2.X += Math.Max(prf_PRIM.h1, prf_PRIM.h2);
                PRIM_RIGHT_Line = new Line(point1, point2);
            } else {
                point1.Y -= PRIMPart.StartPointOffset.Dx;
                point2.Y -= PRIMPart.EndPointOffset.Dx;
                PRIM_LEFT_Line = new Line(point2, point1);
                point1.X += Math.Max(prf_PRIM.h1, prf_PRIM.h2);
                point2.X += Math.Min(prf_PRIM.h1, prf_PRIM.h2);
                PRIM_RIGHT_Line = new Line(point2, point1);
            }//统一为从远端指向近端
            #endregion

            #region 求次零件边线
            TransformationPlane SECPart_TP = new TransformationPlane(SECPart.GetCoordinateSystem());
            point1 = new Point(SECPart.StartPoint).Transform(originTP, SECPart_TP);
            point2 = new Point(SECPart.EndPoint).Transform(originTP, SECPart_TP);
            point1.X += SECPart.StartPointOffset.Dx;
            point2.X += SECPart.EndPointOffset.Dx;
            point3 = new Point(point1);
            point4 = new Point(point2);

            point1 = point1.Transform(SECPart_TP, workTP);
            point2 = point2.Transform(SECPart_TP, workTP);

            if (TSG3d.Distance.PointToPoint(origin, point1) > TSG3d.Distance.PointToPoint(origin, point2)) {
                SEC_TOP_Line = new Line(point1, point2);

                point3.Y += Math.Min(prf_SEC.h1, prf_SEC.h2);
                point4.Y += Math.Max(prf_SEC.h1, prf_SEC.h2);
                point3 = point3.Transform(SECPart_TP, workTP);
                point4 = point4.Transform(SECPart_TP, workTP);
                SEC_BTM_Line = new Line(point3, point4);

            } else {
                SEC_TOP_Line = new Line(point2, point1);

                point3.Y += Math.Max(prf_SEC.h1, prf_SEC.h2);
                point4.Y += Math.Min(prf_SEC.h1, prf_SEC.h2);
                point3 = point3.Transform(SECPart_TP, workTP);
                point4 = point4.Transform(SECPart_TP, workTP);
                SEC_BTM_Line = new Line(point4, point3);
            }//统一为从远端指向近端
            #endregion

            #region 创建端板
            try {
                prf_End1 = new ProfilePlate(prfStr_EndPlate1);
                prf_End2 = new ProfilePlate(prfStr_EndPlate2);
            } catch {
                throw;
            }

            point1 = new Point {
                X = Math.Max(prf_PRIM.h1, prf_PRIM.h2) - prf_PRIM.t1 + prf_End1.t
            };
            point1.Y = (point1.X + prf_End2.t) * slope + pos_EndPlate;
            point2 = new Point(point1);
            point2.Y -= prf_End1.l;
            endPlate1 = ModelOperation.CreatBeam(point1, point2, "MG1001", "PL" + prf_End1.t + "*" + prf_End1.b,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99",
                planeEnum: Position.PlaneEnum.RIGHT, depthEnum: Position.DepthEnum.MIDDLE, rotationEnum: Position.RotationEnum.TOP);
            point2 = new Point(point2);
            point2.Y += prf_End1.l - prf_End2.l;
            endPlate2 = ModelOperation.CreatBeam(point1, point2, "MG1001", "PL" + prf_End2.t + "*" + prf_End2.b,
                materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99",
                planeEnum: Position.PlaneEnum.LEFT, depthEnum: Position.DepthEnum.MIDDLE, rotationEnum: Position.RotationEnum.TOP);

            ENDPlate_Line = new Line(point1, point2);
            #endregion

            #region 创建柱顶板
            try {
                prf_TOP = new ProfilePlate(prfStr_TOP);
            } catch {
                prf_TOP = new ProfilePlate();
            }
            if (prf_TOP.t == 0) {
                prf_TOP.t = prf_SEC.t1;
            }
            if (prf_TOP.b == 0) {
                prf_TOP.b = prf_SEC.b1;
            }

            point1 = new Point();
            point1.X -= len_Eave;
            point2 = new Point {
                X = Math.Max(prf_PRIM.h1, prf_PRIM.h2) - prf_PRIM.t1
            };

            if (bol_TOPHOR > 0) {
                point2.Y = endPlate1.StartPoint.Y - pos_EndPlate;
                point1.Y = point2.Y;
            } else {
                point2.Y = point2.X * slope;
                point1.Y = point1.X * slope;
            }

            topPlate = ModelOperation.CreatBeam(point1, point2, "MG1001", "PL" + prf_TOP.t + "*" + prf_TOP.b,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99",
                planeEnum: Position.PlaneEnum.RIGHT, depthEnum: Position.DepthEnum.MIDDLE, rotationEnum: Position.RotationEnum.TOP);

            Fitting fit_TopPlate_Start = new Fitting {
                Father = topPlate,
                Plane = new Plane {
                    Origin = point1,
                    AxisX = -1 * axisY,
                    AxisY = axisZ
                }
            };
            fit_TopPlate_Start.Insert();
            Fitting fit_TopPlate_End = new Fitting {
                Father = topPlate,
                Plane = new Plane {
                    Origin = point2,
                    AxisX = -1 * axisY,
                    AxisY = axisZ
                }
            };
            fit_TopPlate_End.Insert();

            TOPPlate_Line = new Line(point1, point2);

            #endregion

            #region 创建檐口竖板
            if (len_Eave == 0) {
                eavePlate = null;
                goto skip_Eave;
            }
            if (thk_Eave == 0) {
                thk_Eave = Math.Max(prf_PRIM.s, thk_THKED);
            }

            point1 = TSG3d.Intersection.LineToLine(PRIM_LEFT_Line,
                TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = TSG3d.Intersection.LineToLine(PRIM_LEFT_Line.Offset(len_Eave, LineExtension.OffsetDirectionEnum.LEFT),
                TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point3 = new Point(topPlate.StartPoint);
            point3.Y -= hgt_Eave;
            point4 = new Point(point3);
            point4.X += len_Eave;

            cp1 = new ContourPoint(point1, chamfer_None);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);

            eavePlate = ModelOperation.CreatContourPlate(new ArrayList { cp1, cp2, cp3, cp4 }, "MG1001", "PL" + thk_Eave,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

        skip_Eave:
            #endregion

            #region 创建水平板
            try {
                prf_HOR = new ProfilePlate(prfStr_HOR);
            } catch {
                prf_HOR = new ProfilePlate {
                    t = prf_SEC.t1
                };
            }
            if (prf_HOR.b == 0) {
                prf_HOR.b = (prf_SEC.b1 - Math.Max(prf_PRIM.s, thk_THKED)) * 0.5;
            }

            point2 = TSG3d.Intersection.LineToLine(ENDPlate_Line.Offset(prf_End2.t, LineExtension.OffsetDirectionEnum.LEFT),
                SEC_BTM_Line).StartPoint;
            point2.X -= prf_End1.t + prf_End2.t;
            point2.Z += Math.Max(prf_PRIM.s, thk_THKED) * 0.5;
            point1 = new Point(point2);
            point1.X -= Math.Max(prf_PRIM.h1, prf_PRIM.h2) - prf_PRIM.t1 * 2;
            point3 = new Point(point2);
            point3.Z += prf_HOR.b;
            point4 = new Point(point1);
            point4.Z += prf_HOR.b;

            chamfer_Line = new Chamfer(chamfer_HOR, chamfer_HOR, Chamfer.ChamferTypeEnum.CHAMFER_LINE);
            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_Line);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);

            horPlate_Front = ModelOperation.CreatContourPlate(new ArrayList { cp1, cp2, cp3, cp4 }, "MG1001", "PL" + prf_HOR.t,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.BEHIND);

            point1 = new Point(point1);
            point2 = new Point(point2);
            point3 = new Point(point3);
            point4 = new Point(point4);
            point1.Z *= -1;
            point2.Z *= -1;
            point3.Z *= -1;
            point4.Z *= -1;
            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_Line);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);

            horPlate_Behind = ModelOperation.CreatContourPlate(new ArrayList { cp1, cp2, cp3, cp4 }, "MG1001", "PL" + prf_HOR.t,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.FRONT);

            point1.Z = 0;
            point2.Z = 0;
            HORPlate_Line = new Line(point1, point2);

            #endregion

            #region 创建柱加厚板
            if (thk_THKED <= prf_PRIM.s) {
                thkedPlate = null;
                goto skip_THKED;
            }

            point1 = TSG3d.Intersection.LineToLine(PRIM_LEFT_Line.Offset(prf_PRIM.t1, LineExtension.OffsetDirectionEnum.RIGHT),
                TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = TSG3d.Intersection.LineToLine(PRIM_LEFT_Line.Offset(prf_PRIM.t1, LineExtension.OffsetDirectionEnum.RIGHT),
                HORPlate_Line).StartPoint;
            point2.Y -= pos_THKED;
            point5 = TSG3d.Intersection.LineToLine(ENDPlate_Line.Offset(prf_End1.t, LineExtension.OffsetDirectionEnum.RIGHT),
                TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            cp1 = new ContourPoint(point1, chamfer_None);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp5 = new ContourPoint(point5, chamfer_None);
            if (point2.Y < endPlate1.EndPoint.Y) {
                point3 = TSG3d.Intersection.LineToLine(PRIM_RIGHT_Line.Offset(prf_PRIM.t1, LineExtension.OffsetDirectionEnum.LEFT),
                    HORPlate_Line.Offset(pos_THKED, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
                point4 = new Point(endPlate1.EndPoint);
                point4.X -= prf_End1.t;
                cp3 = new ContourPoint(point3, chamfer_None);
                cp4 = new ContourPoint(point4, chamfer_None);

                contourPoints = new ArrayList { cp1, cp2, cp3, cp4, cp5 };
            } else {
                point4 = TSG3d.Intersection.LineToLine(ENDPlate_Line.Offset(prf_End1.t, LineExtension.OffsetDirectionEnum.RIGHT),
                    HORPlate_Line.Offset(pos_THKED, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
                cp4 = new ContourPoint(point4, chamfer_None);

                contourPoints = new ArrayList { cp1, cp2, cp4, cp5 };
            }
            thkedPlate = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + thk_THKED,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

        skip_THKED:
            #endregion

            #region 创建对角板
            if (prfStr_DIAG == null || prfStr_DIAG == string.Empty) {
                diagPlate_Front = diagPlate_Behind = null;
                goto skip_DIAG;
            }
            try {
                prf_DIAG = new ProfilePlate(prfStr_DIAG);
            } catch {
                throw;
            }
            if (prf_DIAG.b == 0) {
                prf_DIAG.b = (prf_SEC.b1 - Math.Max(prf_PRIM.s, thk_THKED)) * 0.5;
            }

            point1 = TSG3d.Intersection.LineToLine(TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT),
                PRIM_LEFT_Line.Offset(prf_PRIM.t1 + pos_DIAG1, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = TSG3d.Intersection.LineToLine(HORPlate_Line.Offset(prf_HOR.t, LineExtension.OffsetDirectionEnum.LEFT),
                ENDPlate_Line.Offset(prf_End1.t + pos_DIAG2, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;

            DIAGPlate_Line = new Line(point1, point2);
            point1 = TSG3d.Intersection.LineToLine(TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT),
                DIAGPlate_Line.Offset(prf_DIAG.t * 0.5, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;
            point1 = TSG3d.Projection.PointToLine(point1, DIAGPlate_Line);
            point2 = TSG3d.Intersection.LineToLine(HORPlate_Line.Offset(prf_HOR.t, LineExtension.OffsetDirectionEnum.LEFT),
                DIAGPlate_Line.Offset(prf_DIAG.t * 0.5, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = TSG3d.Projection.PointToLine(point2, DIAGPlate_Line);
            point1.Z = Math.Max(prf_PRIM.s, thk_THKED) * 0.5;
            point2.Z = point1.Z;

            point3 = new Point(point2);
            point3.Z += prf_DIAG.b;
            point4 = new Point(point1) {
                Z = point3.Z
            };

            chamfer_Line = new Chamfer(chamfer_DIAG, chamfer_DIAG, Chamfer.ChamferTypeEnum.CHAMFER_LINE);
            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_Line);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);
            contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
            diagPlate_Front = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_DIAG.t,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

            point1 = new Point(point1);
            point2 = new Point(point2);
            point3 = new Point(point3);
            point4 = new Point(point4);
            point1.Z *= -1;
            point2.Z *= -1;
            point3.Z *= -1;
            point4.Z *= -1;
            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_Line);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);
            contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };
            diagPlate_Behind = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_DIAG.t,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);


        skip_DIAG:
            #endregion

            #region 创建翼缘加劲板
            try {
                prf_STIF_FLNG = new ProfilePlate(prfStr_STIF_FLNG);
            } catch {
                throw;
            }
            if (prf_STIF_FLNG.b == 0 || prf_STIF_FLNG.l == 0) {
                throw new UnAcceptableProfileException();
            }
            point1 = TSG3d.Intersection.LineToLine(TOPPlate_Line,
                ENDPlate_Line.Offset(prf_End1.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = new Point(point1);
            point2.Y += prf_STIF_FLNG.l;
            point3 = TSG3d.Intersection.LineToLine(TOPPlate_Line,
                ENDPlate_Line.Offset(prf_End1.t + prf_STIF_FLNG.b, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;

            chamfer_Line = new Chamfer(chamfer_STIF_in, chamfer_STIF_in, Chamfer.ChamferTypeEnum.CHAMFER_LINE);
            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            contourPoints = new ArrayList { cp1, cp2, cp3 };

            stifFLNG_PRIMTOP = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_FLNG.t,
                materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

            point1 = TSG3d.Intersection.LineToLine(SEC_TOP_Line,
                ENDPlate_Line.Offset(prf_End2.t, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;
            point2 = new Point(point1);
            point2.Y += prf_STIF_FLNG.l;
            point3 = TSG3d.Intersection.LineToLine(SEC_TOP_Line,
                ENDPlate_Line.Offset(prf_End2.t + prf_STIF_FLNG.b, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;

            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            contourPoints = new ArrayList { cp1, cp2, cp3 };

            stifFLNG_SECTOP = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_FLNG.t,
                materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

            point1 = TSG3d.Intersection.LineToLine(SEC_BTM_Line,
                ENDPlate_Line.Offset(prf_End2.t, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;
            point2 = new Point(point1);
            point2.Y -= prf_STIF_FLNG.l;
            point3 = TSG3d.Intersection.LineToLine(SEC_BTM_Line,
                ENDPlate_Line.Offset(prf_End2.t + prf_STIF_FLNG.b, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;

            cp1 = new ContourPoint(point1, chamfer_Line);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            contourPoints = new ArrayList { cp1, cp2, cp3 };

            stifFLNG_SECBTM = ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_FLNG.t,
                materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE);

            #endregion

            #region 创建腹板加劲板
            try {
                prf_STIF_Web = new ProfilePlate(prfStr_STIF_Web);
            } catch {
                throw;
            }
            if (prf_STIF_Web.l == 0 || prf_STIF_Web.l == 0) {
                throw new UnAcceptableProfileException();
            }

            stifWeb_PRIM = new List<ContourPlate>();
            stifWeb_SEC = new List<ContourPlate>();

            point1 = Intersection.LineToLine(SEC_TOP_Line,
                ENDPlate_Line.Offset(prf_End2.t, LineExtension.OffsetDirectionEnum.LEFT)).StartPoint;
            point1.Z = prf_SEC.s * 0.5;
            point2 = new Point(point1);
            point2.X += prf_STIF_Web.b;
            point3 = new Point(point2);
            point3.Z += prf_STIF_Web.l;
            point4 = new Point(point1);
            point4.Z += prf_STIF_Web.l;

            point5 = new Point {
                Y = point1.Y//临时存储初始状态
            };

            chamfer_Line2 = new Chamfer(chamfer_STIF_out, chamfer_STIF_out, Chamfer.ChamferTypeEnum.CHAMFER_LINE);

            if (type_STIF_Web == 0) {
                foreach (var distance in disLst_STIF_Web) {
                    point1.Y -= distance.Value;
                    point2.Y -= distance.Value;
                    point4.Y -= distance.Value;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp4 };

                    stifWeb_SEC.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                    point1 = new Point(point1);
                    point2 = new Point(point2);
                    point4 = new Point(point4);
                    point1.Z *= -1;
                    point2.Z *= -1;
                    point4.Z *= -1;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp4 };

                    stifWeb_SEC.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));
                }

                point1 = new Point(point1);
                point2 = new Point(point2);
                point4 = new Point(point4);
                point1.X -= prf_End1.t + prf_End2.t;
                point2.X -= prf_End1.t + prf_End2.t + prf_STIF_Web.b * 2;
                point4.X = point1.X;
                point1.Y = point2.Y = point4.Y = point5.Y;
                point1.Z = Math.Abs(point1.Z);
                point1.Z += (Math.Max(prf_PRIM.s, thk_THKED) - prf_SEC.s) * 0.5;
                point2.Z = point1.Z;
                point4.Z = point1.Z + prf_STIF_Web.l;

                foreach (var distance in disLst_STIF_Web) {
                    point1.Y -= distance.Value;
                    point2.Y -= distance.Value;
                    point4.Y -= distance.Value;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp4 };

                    stifWeb_PRIM.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                    point1 = new Point(point1);
                    point2 = new Point(point2);
                    point4 = new Point(point4);
                    point1.Z *= -1;
                    point2.Z *= -1;
                    point4.Z *= -1;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp4 };

                    stifWeb_PRIM.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                }
            } else if (type_STIF_Web == 1) {
                foreach (var distance in disLst_STIF_Web) {
                    point1.Y -= distance.Value;
                    point2.Y -= distance.Value;
                    point3.Y -= distance.Value;
                    point4.Y -= distance.Value;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp3 = new ContourPoint(point3, chamfer_Line2);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };

                    stifWeb_SEC.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                    point1 = new Point(point1);
                    point2 = new Point(point2);
                    point3 = new Point(point3);
                    point4 = new Point(point4);
                    point1.Z *= -1;
                    point2.Z *= -1;
                    point3.Z *= -1;
                    point4.Z *= -1;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp3 = new ContourPoint(point3, chamfer_Line2);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };

                    stifWeb_SEC.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, SEC_ASMNUM.Prefix, SEC_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));
                }

                point1 = new Point(point1);
                point2 = new Point(point2);
                point3 = new Point(point3);
                point4 = new Point(point4);
                point1.X -= prf_End1.t + prf_End2.t;
                point2.X -= prf_End1.t + prf_End2.t + prf_STIF_Web.b * 2;
                point3.X = point2.X;
                point4.X = point1.X;
                point1.Y = point2.Y = point3.Y = point4.Y = point5.Y;
                point1.Z = Math.Abs(point1.Z);
                point1.Z += (Math.Max(prf_PRIM.s, thk_THKED) - prf_SEC.s) * 0.5;
                point2.Z = point1.Z;
                point3.Z = point2.Z + prf_STIF_Web.l;
                point4.Z = point3.Z;

                foreach (var distance in disLst_STIF_Web) {
                    point1.Y -= distance.Value;
                    point2.Y -= distance.Value;
                    point3.Y -= distance.Value;
                    point4.Y -= distance.Value;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp3 = new ContourPoint(point3, chamfer_Line2);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };

                    stifWeb_PRIM.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                    point1 = new Point(point1);
                    point2 = new Point(point2);
                    point3 = new Point(point3);
                    point4 = new Point(point4);
                    point1.Z *= -1;
                    point2.Z *= -1;
                    point3.Z *= -1;
                    point4.Z *= -1;

                    cp1 = new ContourPoint(point1, chamfer_Line);
                    cp2 = new ContourPoint(point2, chamfer_None);
                    cp3 = new ContourPoint(point3, chamfer_Line2);
                    cp4 = new ContourPoint(point4, chamfer_None);
                    contourPoints = new ArrayList { cp1, cp2, cp3, cp4 };

                    stifWeb_PRIM.Add(ModelOperation.CreatContourPlate(contourPoints, "MG1001", "PL" + prf_STIF_Web.t,
                        materialStr, PRIM_ASMNUM.Prefix, PRIM_ASMNUM.StartNumber, "P", 1, "99", Position.DepthEnum.MIDDLE));

                }
            }

            #endregion

            #region 端板开坡口
            point1 = new Point(endPlate1.EndPoint);
            point1.X -= prf_End1.t - 4;
            point2 = TSG3d.Intersection.LineToLine(new Line(point1, Geometry3dOperation.GetDirectionByAngle(-25.0 / 180.0 * Math.PI)),
                PRIM_RIGHT_Line).StartPoint;
            point3 = TSG3d.Intersection.LineToLine(PRIM_RIGHT_Line,
                new Line(point1, axisX)).StartPoint;

            cp1 = new ContourPoint(point1, chamfer_None);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            cutplate = ModelOperation.CreatBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3 }, prf_PRIM.b1);

            ModelOperation.ApplyBooleanOperation(PRIMPart, cutplate, BooleanPart.BooleanTypeEnum.BOOLEAN_WELDPREP);
            cutplate.Delete();

            point2 = new Point(endPlate1.EndPoint);
            if (diff_THK > prf_End1.t - prf_PRIM.t1) {
                point3 = TSG3d.Intersection.LineToLine(ENDPlate_Line,
                    new Line(point1, Geometry3dOperation.GetDirectionByAngle(25.0 / 180.0 * Math.PI))).StartPoint;

                cp2 = new ContourPoint(point2, chamfer_None);
                cp3 = new ContourPoint(point3, chamfer_None);

                cutplate = ModelOperation.CreatBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3 }, prf_End1.b);
                ModelOperation.ApplyBooleanOperation(endPlate1, cutplate, BooleanPart.BooleanTypeEnum.BOOLEAN_WELDPREP);
                cutplate.Delete();
            } else {
                point4 = TSG3d.Intersection.LineToLine(new Line(point3, axisY),
                    new Line(point1, Geometry3dOperation.GetDirectionByAngle(25.0 / 180.0 * Math.PI))).StartPoint;
                point3 = TSG3d.Intersection.LineToLine(ENDPlate_Line,
                    new Line(point4, Geometry3dOperation.GetDirectionByAngle(Math.Atan(slope_THK)))).StartPoint;

                cp2 = new ContourPoint(point2, chamfer_None);
                cp3 = new ContourPoint(point3, chamfer_None);
                cp4 = new ContourPoint(point4, chamfer_None);

                cutplate = ModelOperation.CreatBooleanOperationPolygon(new ArrayList { cp1, cp2, cp3, cp4 }, prf_End1.b);
                ModelOperation.ApplyBooleanOperation(endPlate1, cutplate, BooleanPart.BooleanTypeEnum.BOOLEAN_WELDPREP);
                cutplate.Delete();
            }

            #endregion

            #region 主零件对齐修剪
            Line line = TOPPlate_Line.Offset(prf_TOP.t, LineExtension.OffsetDirectionEnum.RIGHT);
            line.Direction.Normalize(1);
            Fitting fit_PRIM = new Fitting {
                Father = PRIMPart,
                Plane = new Plane {
                    Origin = line.Origin,
                    AxisX = line.Direction,
                    AxisY = axisZ
                }
            };

            fit_PRIM.Insert();

            point1 = TSG3d.Intersection.LineToLine(line,
                ENDPlate_Line.Offset(prf_End1.t, LineExtension.OffsetDirectionEnum.RIGHT)).StartPoint;
            point2 = new Point(endPlate1.EndPoint);
            point2.X -= prf_End1.t;
            point3 = TSG3d.Intersection.LineToLine(new Line(point2, axisX),
                PRIM_RIGHT_Line).StartPoint;
            point4 = TSG3d.Intersection.LineToLine(line,
                PRIM_RIGHT_Line).StartPoint;

            cp1 = new ContourPoint(point1, chamfer_None);
            cp2 = new ContourPoint(point2, chamfer_None);
            cp3 = new ContourPoint(point3, chamfer_None);
            cp4 = new ContourPoint(point4, chamfer_None);

            contourPoints = new ArrayList();

            if (thkedPlate != null) {
                for (int i = 0; i < thkedPlate.Contour.ContourPoints.Count - 1; i++) {
                    contourPoints.Add(thkedPlate.Contour.ContourPoints[i]);
                }

                if (!Point.AreEqual(cp2,
                    (ContourPoint)thkedPlate.Contour.ContourPoints[thkedPlate.Contour.ContourPoints.Count - 2])) {
                    contourPoints.Add(cp2);
                }
                contourPoints.Add(cp3);
                contourPoints.Add(cp4);
            } else {
                contourPoints.Add(cp1);
                contourPoints.Add(cp2);
                contourPoints.Add(cp3);
                contourPoints.Add(cp4);
            }

            cutplate = ModelOperation.CreatBooleanOperationPolygon(contourPoints, prf_PRIM.b1);

            ModelOperation.ApplyBooleanOperation(PRIMPart, cutplate);
            cutplate.Delete();

            #endregion

            #region 次零件对齐
            Fitting fit_SEC = new Fitting {
                Father = SECPart,
                Plane = new Plane {
                    Origin = new Point(endPlate1.StartPoint),
                    AxisX = -1 * axisY,
                    AxisY = axisZ
                }
            };
            fit_SEC.Plane.Origin.X += prf_End2.t;

            fit_SEC.Insert();
            #endregion

            #region 焊接
            //端板
            ModelOperation.CreatWeld(PRIMPart, endPlate1);
            ModelOperation.CreatWeld(SECPart, endPlate2);
            //柱顶板
            ModelOperation.CreatWeld(PRIMPart, topPlate);
            ModelOperation.CreatWeld(endPlate1, topPlate);
            //檐口竖板
            if (eavePlate != null) {
                ModelOperation.CreatWeld(PRIMPart, eavePlate);
                ModelOperation.CreatWeld(topPlate, eavePlate);
            }
            //水平板
            ModelOperation.CreatWeld(PRIMPart, horPlate_Front);
            ModelOperation.CreatWeld(PRIMPart, horPlate_Behind);
            ModelOperation.CreatWeld(endPlate1, horPlate_Front);
            ModelOperation.CreatWeld(endPlate1, horPlate_Behind);
            //柱加厚板
            if (thk_THKED > prf_PRIM.s) {
                //CreatWeld(PRIMPart, thkedPlate);//有时会不起作用，改用PolygonWeld
                Polygon polygon = new Polygon();
                point1 = new Point((Point)thkedPlate.Contour.ContourPoints[0]);
                point2 = new Point((Point)thkedPlate.Contour.ContourPoints[1]);
                point3 = new Point(point2);
                point4 = new Point((Point)thkedPlate.Contour.ContourPoints[2]);
                point1.Z = thk_THKED * 0.5;
                point2.Z = point1.Z;
                point3.Z = prf_PRIM.s * 0.5;
                point4.Z = point3.Z;

                polygon.Points.Add(point1);
                polygon.Points.Add(point2);
                polygon.Points.Add(point3);
                polygon.Points.Add(point4);
                if (thkedPlate.Contour.ContourPoints.Count == 5) {
                    polygon.Points.Add(new Point(point4.X, point4.Y, thk_THKED * 0.5));
                    point5 = new Point((Point)thkedPlate.Contour.ContourPoints[3]);
                    point5.Z = point1.Z;
                    polygon.Points.Add(point5);
                }
                _ = ModelOperation.CreatPolygonWeld(PRIMPart, thkedPlate, polygon);

                polygon = polygon.Clone();
                foreach (Point point in polygon.Points) {
                    point.Z *= -1;
                }
                _ = ModelOperation.CreatPolygonWeld(PRIMPart, thkedPlate, polygon);


                ModelOperation.CreatWeld(thkedPlate, endPlate1);
                ModelOperation.CreatWeld(thkedPlate, topPlate);
                if (pos_THKED >= 0) {
                    ModelOperation.CreatWeld(thkedPlate, horPlate_Front);
                    ModelOperation.CreatWeld(thkedPlate, horPlate_Behind);
                }

            }
            //斜板
            if (diagPlate_Front != null) {
                if (thkedPlate != null) {
                    ModelOperation.CreatWeld(thkedPlate, diagPlate_Front);
                    ModelOperation.CreatWeld(thkedPlate, diagPlate_Behind);
                } else {
                    ModelOperation.CreatWeld(PRIMPart, diagPlate_Front);
                    ModelOperation.CreatWeld(PRIMPart, diagPlate_Behind);
                }
                ModelOperation.CreatWeld(topPlate, diagPlate_Front);
                ModelOperation.CreatWeld(topPlate, diagPlate_Behind);
                ModelOperation.CreatWeld(horPlate_Front, diagPlate_Front);
                ModelOperation.CreatWeld(horPlate_Behind, diagPlate_Behind);
            }
            //翼缘加劲板
            ModelOperation.CreatWeld(endPlate1, stifFLNG_PRIMTOP);
            ModelOperation.CreatWeld(topPlate, stifFLNG_PRIMTOP);
            ModelOperation.CreatWeld(SECPart, stifFLNG_SECTOP);
            ModelOperation.CreatWeld(endPlate2, stifFLNG_SECTOP);
            ModelOperation.CreatWeld(SECPart, stifFLNG_SECBTM);
            ModelOperation.CreatWeld(endPlate2, stifFLNG_SECBTM);
            //腹板加劲板
            foreach (var cp in stifWeb_PRIM) {
                if (thk_THKED > prf_PRIM.s) {
                    ModelOperation.CreatWeld(thkedPlate, cp);
                } else {
                    ModelOperation.CreatWeld(PRIMPart, cp);
                }
                ModelOperation.CreatWeld(endPlate1, cp);
            }
            foreach (var cp in stifWeb_SEC) {
                ModelOperation.CreatWeld(SECPart, cp);
                ModelOperation.CreatWeld(endPlate2, cp);
            }
            #endregion

            #region 创建螺栓
            ModelOperation.CreatBoltArray(endPlate1, endPlate2, null,
                endPlate1.StartPoint, endPlate1.EndPoint,
                disLst_Bolt_X, disLst_Bolt_Y,
                null, bolt_startOffset, null,
                bolt_Standard, bolt_Size);
            #endregion

            #region FixPrim
            FixPrim(prf_PRIM, endPlate1.EndPoint.Y);
            #endregion

            return true;
        }
        #endregion
    }
}

