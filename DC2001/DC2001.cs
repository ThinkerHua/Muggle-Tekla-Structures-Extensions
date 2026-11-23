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
 *  DC2001.cs: crane girder support corbel
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Plugins;
using static TeklaStructuresAttributes.Attributes;

namespace Muggle.TsExtensions.DC2001 {

    [Plugin("DC2001")]
    [PluginUserInterface("Muggle.TsExtensions.DC2001.View")]
    [InputObjectType(InputObjectType.INPUTOBJECT_PART)]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ZERO)]
    [DetailType(DetailTypeEnum.INTERMEDIATE)]
    [PositionType(PositionTypeEnum.MIDDLE_PLANE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_DETAIL)]
    [FieldsFromData]
    public partial class DC2001 : ConnectionBase {
        private Model Model { get; }


        private PluginData Data { get; }

        public DC2001(PluginData data) {
            Model = new Model();
            Data = data;
        }

        public override bool Run() {
            try {
                Data.SetFieldsToDefaultIfUnset();

                var prim = (Part)Model.SelectModelObject(Primary);
                if (IsDefaultValue(Data.PartLeftCorbelMaterial))
                    Data.PartLeftCorbelMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PartRightCorbelMaterial))
                    Data.PartRightCorbelMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PlatePrimStifMaterial))
                    Data.PlatePrimStifMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PlateLCorbelStifMaterial))
                    Data.PlateLCorbelStifMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PlateRCorbelStifMaterial))
                    Data.PlateRCorbelStifMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PlateLeftPadMaterial))
                    Data.PlateLeftPadMaterial = prim.Material.MaterialString;
                if (IsDefaultValue(Data.PlateRightPadMaterial))
                    Data.PlateRightPadMaterial = prim.Material.MaterialString;

                GetFieldValuesFrom(Data);

                var workTp = GetWorkTransformationPlate();
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

                CreateDetail();

                return true;
            } catch (Exception e) {
                Operation.DisplayPrompt(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取工作变换平面，
        /// 以主零件坐标系的 Y 轴作为工作平面的 X 轴且同向，
        /// 以主零件坐标系的 Z 轴作为工作平面 Y 轴，方向根据组成的坐标系 Z 轴是否与全局 Z 轴同向确定。
        /// </summary>
        /// <returns>工作变换平面。</returns>
        private TransformationPlane GetWorkTransformationPlate() {
            var globalTp = new TransformationPlane();
            var originTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

            var globalZ = new Vector(0, 0, 1000).Transform(globalTp, originTp);
            var axisX = new Vector(1000, 0, 0);
            var axisY = new Vector(0, 1000, 0);
            var axisZ = new Vector(0, 0, 1000);

            if (axisX.Dot(globalZ) < 0) axisZ *= -1;

            return new TransformationPlane(new Point(), axisY, axisZ);
        }

        private static string SectionType(string profileText) {
            try {
                _ = new ProfileH(profileText);
                return "H";
            } catch (UnAcceptableProfileException) {
                // ignored
            }

            try {
                _ = new ProfileRect(profileText);
                return "R";
            } catch (UnAcceptableProfileException) {
                // ignored
            }

            return string.Empty;
        }

        private static double DepthOffsetOfVariableHeightSection(string profileText) {
            var depthOffset = 0.0;
            try {
                var profile = new ProfileH(profileText);
                if (profile.h1 > profile.h2)
                    depthOffset = (profile.h2 - profile.h1) * 0.5;
            } catch {
                // ignored
            }

            return depthOffset;
        }

        private void CreateDetail() {
            var origin = new Point();
            var axisX = new Vector(1, 0, 0);
            var axisZ = new Vector(0, 0, 1);

            var workTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

            var column = (Beam)Model.SelectModelObject(Primary);
            var solid = column.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var intersections = solid.Intersect(origin - axisX * int.MaxValue, origin + axisX * int.MaxValue)
                .Cast<Point>().ToList();
            var intersectionPlusX = intersections.OrderByDescending(p => p.X).First();
            var intersectionMinusX = intersections.OrderBy(p => p.X).First();

            #region LeftCorbel

            var p2 = intersectionMinusX;
            var p1 = p2 - axisX * _leftCorbelLength;

            var depthOffset = DepthOffsetOfVariableHeightSection(_partLeftCorbelProfile);
            if (depthOffset != 0.0) (p1, p2) = (p2, p1);

            var leftCorbel = Data.CreatePartLeftCorbel<Beam>();
            leftCorbel.StartPoint = p1;
            leftCorbel.EndPoint = p2;
            leftCorbel.Position = new Position {
                Rotation = Position.RotationEnum.BELOW, Depth = Position.DepthEnum.BEHIND, DepthOffset = depthOffset,
            };
            leftCorbel.Insert();

            var weld = Data.CreateWeldCorbel<Weld>();
            weld.MainObject = column;
            weld.SecondaryObject = leftCorbel;
            weld.Insert();

            double leftCorbelHeight = 0.0, leftCorbelWidth = 0.0, leftCorbelWebThickness = 0.0;
            leftCorbel.GetReportProperty(HEIGHT, ref leftCorbelHeight);
            leftCorbel.GetReportProperty(WIDTH, ref leftCorbelWidth);
            leftCorbel.GetReportProperty(WEB_THICKNESS, ref leftCorbelWebThickness);

            #endregion

            #region RightCorbel

            Beam rightCorbel = null;
            double rightCorbelHeight = 0.0, rightCorbelWidth = 0.0, rightCorbelWebThickness = 0.0;

            if (string.IsNullOrEmpty(_partRightCorbelProfile)) goto SkipRightCorbel;

            p2 = intersectionPlusX;
            p1 = p2 + axisX * _rightCorbelLength;

            depthOffset = DepthOffsetOfVariableHeightSection(_partRightCorbelProfile);
            if (depthOffset != 0.0) (p1, p2) = (p2, p1);

            rightCorbel = Data.CreatePartRightCorbel<Beam>();
            rightCorbel.StartPoint = p1;
            rightCorbel.EndPoint = p2;
            rightCorbel.Position = new Position {
                Rotation = Position.RotationEnum.BELOW, Depth = Position.DepthEnum.BEHIND, DepthOffset = depthOffset,
            };
            rightCorbel.Insert();

            weld = Data.CreateWeldCorbel<Weld>();
            weld.MainObject = column;
            weld.SecondaryObject = rightCorbel;
            weld.Insert();

            rightCorbel.GetReportProperty(HEIGHT, ref rightCorbelHeight);
            rightCorbel.GetReportProperty(WIDTH, ref rightCorbelWidth);
            rightCorbel.GetReportProperty(WEB_THICKNESS, ref rightCorbelWebThickness);

            SkipRightCorbel: ;

            #endregion

            #region PrimStiffeners

            ContourPlate InsertContourPlate(Func<ContourPlate> CreateContourPlate, ArrayList contourPoints) {
                var contourPlate = CreateContourPlate.Invoke();
                contourPlate.Contour.ContourPoints = contourPoints;
                contourPlate.Position.Depth = Position.DepthEnum.MIDDLE;
                contourPlate.Insert();
                return contourPlate;
            }

            solid = leftCorbel.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var p = solid.MaximumPoint - axisZ * (_platePrimStifThickness * 0.5);

            var chamfer = Data.CreateChamferPrimStif();
            var contours = ModelOperation.CalculateStiffeners(column, p, _platePrimStifThickness, chamfer: chamfer);
            var primStifs = contours.Select(arr => InsertContourPlate(Data.CreatePlatePrimStif<ContourPlate>, arr))
                .ToList();

            p = solid.MinimumPoint + axisZ * (_platePrimStifThickness * 0.5);
            contours = ModelOperation.CalculateStiffeners(column, p, _platePrimStifThickness, chamfer: chamfer);
            primStifs.AddRange(contours.Select(arr => InsertContourPlate(Data.CreatePlatePrimStif<ContourPlate>, arr)));

            if (rightCorbel is not null &&
                Math.Abs(rightCorbelHeight - leftCorbelHeight) > GeometryConstants.DISTANCE_EPSILON) {
                solid = rightCorbel.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                p = solid.MinimumPoint + axisZ * (_platePrimStifThickness * 0.5);
                contours = ModelOperation.CalculateStiffeners(column, p, _platePrimStifThickness, chamfer: chamfer);
                primStifs.AddRange(contours.Select(arr =>
                    InsertContourPlate(Data.CreatePlatePrimStif<ContourPlate>, arr)));
            }

            var primCs = column.GetCoordinateSystem();
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(primCs));

            var primSectionType = SectionType(column.Profile.ProfileString);

            var cnt = 0;
            foreach (var stif in primStifs) {
                cnt++;

                weld = Data.CreateWeldPrimStif<Weld>();
                weld.MainObject = column;
                weld.SecondaryObject = stif;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                weld.Insert();

                switch (primSectionType) {
                case "H":
                    weld.Position = cnt % 2 == 1
                        ? Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z
                        : Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;

                    weld.Insert();

                    break;
                case "R":
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
                    weld.Insert();
                    weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                    weld.Insert();
                    break;
                }
            }

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            #endregion

            #region LeftCorbelStiffeners

            p = intersectionMinusX - axisX * _leftCorbelStifPos;
            var indent = (leftCorbelWidth - leftCorbelWebThickness) * 0.5 - _plateLCorbelStifBreadth;
            contours = ModelOperation.CalculateStiffeners(leftCorbel, p, _plateLCorbelStifThickness, indent: indent);
            var corbelStifs = contours
                .Select(arr => InsertContourPlate(Data.CreatePlateLCorbelStif<ContourPlate>, arr))
                .ToList();

            Model.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(new TransformationPlane(leftCorbel.GetCoordinateSystem()));

            cnt = 0;
            foreach (var stif in corbelStifs) {
                cnt++;

                weld = Data.CreateWeldCorbelStif<Weld>();
                weld.MainObject = leftCorbel;
                weld.SecondaryObject = stif;
                weld.AroundWeld = false;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                weld.Insert();

                weld.Position = cnt % 2 == 1
                    ? Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z
                    : Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;

                weld.Insert();
            }

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            #endregion

            #region RightCorbelStiffeners

            if (rightCorbel is null) goto SkipRightCorbelStiffeners;

            p = intersectionPlusX + axisX * _rightCorbelStifPos;
            indent = (rightCorbelWidth - rightCorbelWebThickness) * 0.5 - _plateRCorbelStifBreadth;
            contours = ModelOperation.CalculateStiffeners(rightCorbel, p, _plateRCorbelStifThickness,
                indent: indent);
            corbelStifs = contours.Select(arr => InsertContourPlate(Data.CreatePlateRCorbelStif<ContourPlate>, arr))
                .ToList();

            Model.GetWorkPlaneHandler()
                .SetCurrentTransformationPlane(new TransformationPlane(rightCorbel.GetCoordinateSystem()));

            cnt = 0;
            foreach (var stif in corbelStifs) {
                cnt++;

                weld = Data.CreateWeldCorbelStif<Weld>();
                weld.MainObject = rightCorbel;
                weld.SecondaryObject = stif;
                weld.AroundWeld = false;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                weld.Insert();

                weld.Position = cnt % 2 == 1
                    ? Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z
                    : Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;

                weld.Insert();
            }

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            SkipRightCorbelStiffeners: ;

            #endregion

            #region LeftPad

            p1 = intersectionMinusX - axisX * (_leftCorbelStifPos + _plateLeftPadHeight * 0.5);
            p2 = p1 + axisX * _plateLeftPadHeight;

            var leftPad = Data.CreatePlateLeftPad<Beam>();
            leftPad.StartPoint = p1;
            leftPad.EndPoint = p2;
            leftPad.Position.Rotation = Position.RotationEnum.FRONT;
            leftPad.Position.Depth = Position.DepthEnum.FRONT;
            leftPad.Insert();

            weld = Data.CreateWeldPad<Weld>();
            weld.MainObject = leftCorbel;
            weld.SecondaryObject = leftPad;
            weld.Insert();

            #endregion

            #region RightPad

            if (rightCorbel is null) goto SkipRightPad;

            p1 = intersectionPlusX + axisX * (_rightCorbelStifPos + _plateRightPadHeight * 0.5);
            p2 = p1 - axisX * _plateRightPadHeight;
            var rightPad = Data.CreatePlateRightPad<Beam>();
            rightPad.StartPoint = p1;
            rightPad.EndPoint = p2;
            rightPad.Position.Rotation = Position.RotationEnum.FRONT;
            rightPad.Position.Depth = Position.DepthEnum.FRONT;
            rightPad.Insert();

            weld = Data.CreateWeldPad<Weld>();
            weld.MainObject = rightCorbel;
            weld.SecondaryObject = rightPad;
            weld.Insert();

            SkipRightPad: ;

            #endregion

        }
    }
}