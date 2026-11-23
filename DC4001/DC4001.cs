/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  DC4001.cs: 20G520 series crane girder
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Plugins;
using Point = Tekla.Structures.Geometry3d.Point;
using Vector = Tekla.Structures.Geometry3d.Vector;

namespace Muggle.TsExtensions.DC4001;

[Plugin("DC4001")]
[PluginUserInterface("Muggle.TsExtensions.DC4001.View")]
[CustomPartInputType(CustomPartInputType.INPUT_2_POINTS)]
[CustomPartPositioningType(CustomPartPositioningType.POSITIONING_BY_INPUTPOINTS)]
[FieldsFromData]
public partial class DC4001 : CustomPartBase {

    private Model Model { get; }

    private PluginData Data { get; }

    public DC4001(PluginData data) {
        Model = new Model();
        Data = data;

        Data.SetFieldsToDefaultIfUnset();

        if (IsDefaultValue(Data.Plate4Material)) Data.Plate4Material = Data.PartMainMaterial;
        if (IsDefaultValue(Data.Plate5Material)) Data.Plate5Material = Data.PartMainMaterial;
        if (IsDefaultValue(Data.Plate10Material)) Data.Plate10Material = Data.PartMainMaterial;
        if (IsDefaultValue(Data.Plate11Material)) Data.Plate11Material = Data.PartMainMaterial;
        if (IsDefaultValue(Data.Plate12Material)) Data.Plate12Material = Data.PartMainMaterial;
        if (IsDefaultValue(Data.Plate13Material)) Data.Plate13Material = Data.PartMainMaterial;

        GetFieldValuesFrom(Data);
    }

    public override bool Run() {
        try {

            var origin = new Point();
            var axisX = new Vector(1, 0, 0);
            var axisY = new Vector(0, 1, 0);
            var axisZ = new Vector(0, 0, 1);

            var originTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            var workTp = new TransformationPlane(origin, axisX, -1 * axisZ);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            for (int i = 0; i < Positions.Count; i++) {
                Positions[i] = Positions[i].Transform(originTp, workTp);
            }

            var span = Distance.PointToPoint(Positions[0], Positions[1]);

            // 为调整 AtDepth 为 Front 或 Behind 时，自定义零件能恰好与当前平面平齐，
            // 添加 axisZ * (Math.Max(_stretch4, _plate12Thickness) * 0.5) 修正系数
            var ps = Positions[0] + axisZ * (Math.Max(_stretch4, _plate12Thickness) * 0.5);
            var pe = Positions[1] + axisZ * (Math.Max(_stretch4, _plate12Thickness) * 0.5);

            #region MainPart

            var mainProfile = $"WI{_h}-{_tw}-{_tf}*{_b1}-{_tf}*{_b2}";
            var p1 = ps + axisX * (_gap + _plate4Thickness);
            var p2 = pe - axisX * (_gap + (_kind == 0 ? _plate4Thickness : 0));

            var main = Data.CreatePartMain<Beam>();
            main.Profile.ProfileString = mainProfile;
            main.StartPoint = p1;
            main.EndPoint = p2;
            main.Position.Rotation = Position.RotationEnum.TOP;
            main.Position.Depth = Position.DepthEnum.MIDDLE;
            main.Insert();

            #endregion

            #region Plate4

            var p3 = p1 + axisZ * (_h * 0.5 - _tf * 0.5);
            var p4 = p1 - axisZ * (_h * 0.5 + _stretch4);
            var plate4S = Data.CreatePlate4<Beam>();
            plate4S.StartPoint = p3;
            plate4S.EndPoint = p4;
            plate4S.Position.Rotation = Position.RotationEnum.FRONT;
            plate4S.Position.Depth = Position.DepthEnum.BEHIND;
            plate4S.Insert();

            var weld = Data.CreateWeld4_1<Weld>();
            weld.MainObject = main;
            weld.SecondaryObject = plate4S;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();

            weld = Data.CreateWeld4_2<Weld>();
            weld.MainObject = main;
            weld.SecondaryObject = plate4S;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            weld = Data.CreateWeld4_3<Weld>();
            weld.MainObject = main;
            weld.SecondaryObject = plate4S;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();

            if (_kind == 0) {
                p3 = p2 + axisZ * (_h * 0.5 - _tf * 0.5);
                p4 = p2 - axisZ * (_h * 0.5 + _stretch4);
                var plate4E = Data.CreatePlate4<Beam>();
                plate4E.StartPoint = p3;
                plate4E.EndPoint = p4;
                plate4E.Position.Rotation = Position.RotationEnum.BACK;
                plate4E.Position.Depth = Position.DepthEnum.FRONT;
                plate4E.Insert();

                weld = Data.CreateWeld4_1<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate4E;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld = Data.CreateWeld4_2<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate4E;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                weld.Insert();

                weld = Data.CreateWeld4_3<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate4E;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
            }

            #endregion

            #region Plate5

            var mod = Math.Abs(span % _distance5);
            var cnt = (int)(span / _distance5);

            if (mod <= GeometryConstants.DISTANCE_EPSILON) --cnt;
            else ++cnt;

            if (_kind is 1 or 2) {
                switch (mod) {
                case <= GeometryConstants.DISTANCE_EPSILON when _distance5 <= _position10:
                    cnt -= (int)(_position10 / _distance5);
                    break;
                case > GeometryConstants.DISTANCE_EPSILON when mod * 0.5 <= _position10:
                    cnt -= (int)((_position10 - mod * 0.5) / _distance5) + 1;
                    break;
                }
            }

            p3 = ps + axisX * (mod <= GeometryConstants.DISTANCE_EPSILON ? _distance5 : mod * 0.5)
                    + axisZ * (_h * 0.5 - _tf);
            p4 = p3 - axisZ * (_h - _tf * 2 - _shrink5);

            for (int i = 0; i < cnt; i++, p3 += axisX * _distance5, p4 += axisX * _distance5) {

                var cps = new ArrayList {
                    new ContourPoint(p3 - axisY * (_tw * 0.5), Data.CreateChamfer5()),
                    new ContourPoint(p3 - axisY * (_tw * 0.5 + _plate5Breadth), new Chamfer()),
                    new ContourPoint(p4 - axisY * (_tw * 0.5 + _plate5Breadth), new Chamfer()),
                    new ContourPoint(p4 - axisY * (_tw * 0.5), new Chamfer()),
                };

                var plate5 = Data.CreatePlate5<ContourPlate>();
                plate5.Contour.ContourPoints = cps;
                plate5.Position.Depth = Position.DepthEnum.MIDDLE;
                plate5.Insert();

                weld = Data.CreateWeld5<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate5;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                foreach (ContourPoint cp in cps) {
                    cp.Y *= -1;
                }

                plate5.Contour.ContourPoints = cps;
                plate5.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();
            }

            #endregion

            #region Plate10,Plate13

            if (_kind != 1 && _kind != 2) goto SkipPlate10Plate13;

            p3 = pe - axisX * _position10 + axisZ * (_h * 0.5 - _tf);
            p4 = p3 - axisZ * (_h - _tf * 2);
            var contour = new ArrayList {
                new ContourPoint(p3 - axisY * (_tw * 0.5),
                    _kind == 1 ? Data.CreateChamfer10() : Data.CreateChamfer13()),
                new ContourPoint(p3 - axisY * (_tw * 0.5 + (_kind == 1 ? _plate10Breadth : _plate13Breadth)),
                    new Chamfer()),
                new ContourPoint(p4 - axisY * (_tw * 0.5 + (_kind == 1 ? _plate10Breadth : _plate13Breadth)),
                    new Chamfer()),
                new ContourPoint(p4 - axisY * (_tw * 0.5), new Chamfer()),
            };

            var plate = _kind == 1 ? Data.CreatePlate10<ContourPlate>() : Data.CreatePlate13<ContourPlate>();
            plate.Contour.ContourPoints = contour;
            plate.Position.Depth = Position.DepthEnum.MIDDLE;
            plate.Insert();

            switch (_kind) {
            case 1:
                weld = Data.CreateWeld10_1<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld = Data.CreateWeld10_2<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
                break;
            case 2:
                weld = Data.CreateWeld13<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
                break;
            }

            foreach (ContourPoint cp in contour) {
                cp.Y *= -1;
            }

            plate.Contour.ContourPoints = contour;
            plate.Insert();

            switch (_kind) {
            case 1:
                weld = Data.CreateWeld10_1<Weld>();
                weld.SwapAboveBelow();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld = Data.CreateWeld10_2<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
                break;
            case 2:
                weld = Data.CreateWeld13<Weld>();
                weld.MainObject = main;
                weld.SecondaryObject = plate;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                weld.Insert();
                break;
            }

            SkipPlate10Plate13: ;

            #endregion

            #region Plate11,Plate5

            if (_kind != 1 && _kind != 2) goto SkipPlate11Plate5;

            p3 = pe - axisX * (_gap + _position11) + axisZ * (_h * 0.5 - _tf);
            p4 = p3 - axisZ * (_h - _tf * 2 - _shrink5);
            contour = new ArrayList {
                new ContourPoint(p3 - axisY * (_tw * 0.5), Data.CreateChamfer5()),
                new ContourPoint(p3 - axisY * (_tw * 0.5 + _plate11Breadth), new Chamfer()),
                new ContourPoint(p4 - axisY * (_tw * 0.5 + _plate11Breadth), new Chamfer()),
                new ContourPoint(p4 - axisY * (_tw * 0.5), new Chamfer()),
            };

            plate = _kind == 1 ? Data.CreatePlate11<ContourPlate>() : Data.CreatePlate5<ContourPlate>();
            plate.Contour.ContourPoints = contour;
            plate.Position.Depth = Position.DepthEnum.MIDDLE;
            plate.Insert();

            weld = _kind == 1 ? Data.CreateWeld11<Weld>() : Data.CreateWeld5<Weld>();
            weld.MainObject = main;
            weld.SecondaryObject = plate;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            foreach (ContourPoint cp in contour) {
                cp.Y *= -1;
            }

            plate.Contour.ContourPoints = contour;
            plate.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();

            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Insert();

            SkipPlate11Plate5: ;

            #endregion

            #region Plate12

            if (_kind != 1 && _kind != 2) goto SkipPlate12;

            p3 = pe - axisX * _position10 - axisY * (_plate12Height * 0.5) - axisZ * (_h * 0.5);
            p4 = p3 + axisY * _plate12Height;

            var plate12 = Data.CreatePlate12<Beam>();
            plate12.StartPoint = p3;
            plate12.EndPoint = p4;
            plate12.Position.Rotation = Position.RotationEnum.BACK;
            // plate12.Position.Plane = Position.PlaneEnum.MIDDLE;
            plate12.Position.Depth = Position.DepthEnum.BEHIND;
            plate12.Insert();

            weld = Data.CreateWeld12<Weld>();
            weld.MainObject = main;
            weld.SecondaryObject = plate12;
            weld.Insert();

            SkipPlate12: ;

            #endregion

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(originTp);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }
}