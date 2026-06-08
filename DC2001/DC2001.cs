using System;
using System.Linq;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;
using static Muggle.TsExtensions.Common.Operation;
using static TeklaStructuresAttributes.Attributes;
using Win = System.Windows;

namespace Muggle.TsExtensions.DC2001 {
    [PartFields("LeftCorbel", "RightCorbel")]
    [PlateFields("PrimStif", "LCorbelStif", "RCorbelStif", "LeftPad", "RightPad")]
    [WeldFields("Corbel", "PrimStif", "CorbelStif", "Pad")]
    [GeneralFields(typeof(int), "PrimStifChamferType", "RightCorbelCreation")]
    [GeneralFields(typeof(double), "LeftCorbelLength", "RightCorbelLength", "LeftCorbelStifDist", "RightCorbelStifDist",
        "PrimStifChamferX", "PrimStifChamferY", "PrimStifChamferDz1", "PrimStifChamferDz2")]
    public partial class PluginData;

    [Plugin("DC2001")]
    [PluginUserInterface("Muggle.TsExtensions.DC2001.View")]
    [InputObjectType(InputObjectType.INPUTOBJECT_PART)]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ZERO)]
    [DetailType(DetailTypeEnum.INTERMEDIATE)]
    [PositionType(PositionTypeEnum.MIDDLE_PLANE)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_DETAIL)]
    [FieldsFrom(typeof(PluginData))]
    public partial class DC2001 : ConnectionBase {
        private Model Model { get; }

        [PartFieldDefaultValues("LeftCorbel", profile: "HI530-270-30-30*600", name: "LeftCorbel")]
        [PartFieldDefaultValues("RightCorbel", profile: "HI600-300-30-30*600", name: "RightCorbel")]
        [PlateFieldDefaultValues("PrimStif", 20, name: "ColumnStiffener")]
        [PlateFieldDefaultValues("LCorbelStif", 20, 185, name: "LeftCorbelStiffener")]
        [PlateFieldDefaultValues("RCorbelStif", 20, 185, name: "RightCorbelStiffener")]
        [PlateFieldDefaultValues("LeftPad", 30, 560, 560, name: "LeftPad")]
        [PlateFieldDefaultValues("RightPad", 30, 560, 560, name: "RightPad")]
        [WeldFieldDefaultValues("Corbel", typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
        [WeldFieldDefaultValues("PrimStif", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
        [WeldFieldDefaultValues("CorbelStif", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
        [WeldFieldDefaultValues("Pad", typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
        [GeneralFieldDefaultValues("RightCorbelCreation", 0, "LeftCorbelLength", 800, "RightCorbelLength", 800,
            "LeftCorbelStifDist", 500, "RightCorbelStifDist", 500, "PrimStifChamferType", 1,
            "PrimStifChamferX", 25, "PrimStifChamferY", 25,
            "PrimStifChamferDz1", 0, "PrimStifChamferDz2", 0)]
        private PluginData Data { get; }

        public DC2001(PluginData data) {
            Model = new Model();
            Data = data;

            SetDataToDefaultIfUnset();
            GetFieldValuesFrom(Data);
        }

        public override bool Run() {
            try {
                var globalTp = new TransformationPlane();
                var originTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

                var globalZ = new Vector(0, 0, 1000);
                var axisX = new Vector(1000, 0, 0);
                var axisY = new Vector(0, 1000, 0);
                var axisZ = new Vector(0, 0, 1000);

                if (axisX.Dot(globalZ.Transform(globalTp, originTp)) < 0) axisZ *= -1;

                var workTp = new TransformationPlane(new Point(), axisY, axisZ);
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

                var column = (Beam)Model.SelectModelObject(Primary);
                var solid = column.GetSolid(Solid.SolidCreationTypeEnum.RAW);

                var p1 = new Point();
                var p2 = new Point(-1000, 0, 0);
                var intersects = solid.Intersect(p1, p2);
                var intersect = intersects.Cast<Point>().OrderBy(p => p.X).First();
                p1 = intersect;
                p2 = p1 + p2;

                var depthOffset = DepthOffsetOfVariableHeightSection(_partLeftCorbelProfile);
                if (depthOffset == 0.0) (p1, p2) = (p2, p1);

                var leftCorbel = CreatPartLeftCorbel<Beam>();
                leftCorbel.StartPoint = p1;
                leftCorbel.EndPoint = p2;
                leftCorbel.Position = new Position {
                    Rotation = Position.RotationEnum.BELOW,
                    Depth = Position.DepthEnum.BEHIND,
                    DepthOffset = depthOffset,
                };
                leftCorbel.Insert();

                var weld = CreatWeldCorbel<Weld>();
                weld.MainObject = column;
                weld.SecondaryObject = leftCorbel;
                weld.Insert();

                Beam rightCorbel = null;
                Point p3 = null, p4 = null;
                switch (_rightCorbelCreation) {
                case 1:
                    p3 = new Point(p1.X * -1, p1.Y, p1.Z);
                    p4 = new Point(p2.X * -1, p2.Y, p2.Z);
                    rightCorbel = CreatPartLeftCorbel<Beam>();
                    rightCorbel.StartPoint = p3;
                    rightCorbel.EndPoint = p4;
                    break;
                case 2:
                    p3 = new Point(p1.X * -1, p1.Y, p1.Z);
                    p4 = p3 + new Point(_rightCorbelLength, 0, 0);
                    depthOffset = DepthOffsetOfVariableHeightSection(_partRightCorbelProfile);
                    if (depthOffset == 0.0) (p3, p4) = (p4, p3);

                    rightCorbel = CreatPartRightCorbel<Beam>();
                    rightCorbel.StartPoint = p3;
                    rightCorbel.EndPoint = p4;
                    break;
                }

                if (rightCorbel is not null) {
                    rightCorbel.Position = new Position {
                        Rotation = Position.RotationEnum.BELOW,
                        Depth = Position.DepthEnum.BEHIND,
                        DepthOffset = depthOffset,
                    };
                    rightCorbel.Insert();

                    weld = CreatWeldCorbel<Weld>();
                    weld.MainObject = column;
                    weld.SecondaryObject = rightCorbel;
                    weld.Insert();
                }

                double leftCorbelHeight = 0.0,
                    leftCorbelWidth = 0.0,
                    leftCorbelWebThickness = 0.0,
                    rightCorbelHeight = 0.0,
                    rightCorbelWidth = 0.0,
                    rightCorbelWebThickness = 0.0;
                leftCorbel.GetReportProperty(HEIGHT, ref leftCorbelHeight);
                leftCorbel.GetReportProperty(WIDTH, ref leftCorbelWidth);
                leftCorbel.GetReportProperty(WEB_THICKNESS, ref leftCorbelWebThickness);
                if (rightCorbel is not null) {
                    rightCorbel.GetReportProperty(HEIGHT, ref rightCorbelHeight);
                    rightCorbel.GetReportProperty(WIDTH, ref rightCorbelWidth);
                    rightCorbel.GetReportProperty(WEB_THICKNESS, ref rightCorbelWebThickness);
                }

                solid = leftCorbel.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                var p = solid.MaximumPoint + new Point(0, 0, _platePrimStifThickness * -0.5);

                var primStifs = ModelOperation.CreatStiffeners(column, p,
                    _platePrimStifThickness,
                    _platePrimStifMaterial,
                    _platePrimStifClass.ToString(),
                    chamferType: EnumParse<Chamfer.ChamferTypeEnum>(_primStifChamferType),
                    chamferSizeX: _primStifChamferX, chamferSizeY: _primStifChamferY,
                    chamferDz1: _primStifChamferDz1, chamferDz2: _primStifChamferDz2);

                p = solid.MinimumPoint + new Point(0, 0, _platePrimStifThickness * 0.5);
                primStifs = primStifs.Concat(ModelOperation.CreatStiffeners(column, p,
                    _platePrimStifThickness,
                    _platePrimStifMaterial,
                    _platePrimStifClass.ToString(),
                    chamferType: EnumParse<Chamfer.ChamferTypeEnum>(_primStifChamferType),
                    chamferSizeX: _primStifChamferX, chamferSizeY: _primStifChamferY,
                    chamferDz1: _primStifChamferDz1, chamferDz2: _primStifChamferDz2));

                if (rightCorbel is not null &&
                    Math.Abs(rightCorbelHeight - leftCorbelHeight) > GeometryConstants.DISTANCE_EPSILON) {
                    solid = rightCorbel.GetSolid(Solid.SolidCreationTypeEnum.RAW);
                    p = solid.MinimumPoint + new Point(0, 0, _platePrimStifThickness * 0.5);
                    primStifs = primStifs.Concat(ModelOperation.CreatStiffeners(column, p,
                        _platePrimStifThickness,
                        _platePrimStifMaterial,
                        _platePrimStifClass.ToString(),
                        chamferType: EnumParse<Chamfer.ChamferTypeEnum>(_primStifChamferType),
                        chamferSizeX: _primStifChamferX, chamferSizeY: _primStifChamferY,
                        chamferDz1: _primStifChamferDz1, chamferDz2: _primStifChamferDz2));
                }

                var primCs = column.GetCoordinateSystem();
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(primCs));

                var primSectionType = SectionType(column.Profile.ProfileString);

                var cnt = 0;
                foreach (var stif in primStifs) {
                    cnt++;

                    weld = CreatWeldPrimStif<Weld>();
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

                p = intersect + new Vector(-_leftCorbelStifDist, 0, 0);
                var indent = (leftCorbelWidth - leftCorbelWebThickness) * 0.5 - _plateLCorbelStifBreadth;
                var corbelStifs = ModelOperation.CreatStiffeners(leftCorbel, p, _plateLCorbelStifThickness,
                    _plateLCorbelStifMaterial, _plateLCorbelStifClass.ToString(), indent: indent);

                Model.GetWorkPlaneHandler()
                    .SetCurrentTransformationPlane(new TransformationPlane(leftCorbel.GetCoordinateSystem()));

                cnt = 0;
                foreach (var stif in corbelStifs) {
                    cnt++;

                    weld = CreatWeldCorbelStif<Weld>();
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

                if (rightCorbel is not null) {
                    p = intersect.Multiply(-1) + new Vector(_rightCorbelStifDist, 0, 0);
                    indent = (rightCorbelWidth - rightCorbelWebThickness) * 0.5 - _plateRCorbelStifBreadth;
                    corbelStifs = ModelOperation.CreatStiffeners(rightCorbel, p, _plateRCorbelStifThickness,
                        _plateRCorbelStifMaterial, _plateRCorbelStifClass.ToString(), indent: indent);

                    Model.GetWorkPlaneHandler()
                        .SetCurrentTransformationPlane(new TransformationPlane(rightCorbel.GetCoordinateSystem()));

                    cnt = 0;
                    foreach (var stif in corbelStifs) {
                        cnt++;

                        weld = CreatWeldCorbelStif<Weld>();
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
                }

                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

                p1 += new Point(-_leftCorbelStifDist + _plateLeftPadHeight * 0.5, 0, 0);
                p2 = p1 + new Point(-_plateLeftPadHeight, 0, 0);

                var leftPad = CreatPlateLeftPad<Beam>();
                leftPad.StartPoint = p1;
                leftPad.EndPoint = p2;
                leftPad.Position.Rotation = Position.RotationEnum.FRONT;
                leftPad.Position.Depth = Position.DepthEnum.FRONT;
                leftPad.Insert();

                weld = CreatWeldPad<Weld>();
                weld.MainObject = leftCorbel;
                weld.SecondaryObject = leftPad;
                weld.Insert();

                if (rightCorbel is not null) {
                    p3 += new Point(_rightCorbelStifDist - _plateRightPadHeight * 0.5, 0, 0);
                    p4 = p3 + new Point(_plateRightPadHeight, 0, 0);
                    var rightPad = CreatPlateRightPad<Beam>();
                    rightPad.StartPoint = p3;
                    rightPad.EndPoint = p4;
                    rightPad.Position.Rotation = Position.RotationEnum.FRONT;
                    rightPad.Position.Depth = Position.DepthEnum.FRONT;
                    rightPad.Insert();

                    weld = CreatWeldPad<Weld>();
                    weld.MainObject = rightCorbel;
                    weld.SecondaryObject = rightPad;
                    weld.Insert();
                }

                return true;
            } catch (Exception e) {
                Win.MessageBox.Show(e.ToString(), "Error", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
                return false;
            }
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
            var planeOffset = 0.0;
            try {
                var profile = new ProfileH(profileText);
                if (profile.h1 > profile.h2)
                    planeOffset = (profile.h2 - profile.h1) * 0.5;
            } catch {
                // ignored
            }

            return planeOffset;
        }
    }
}