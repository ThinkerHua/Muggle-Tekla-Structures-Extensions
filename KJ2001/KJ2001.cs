/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2025-2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  KJ2001.cs: box column outer column base
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Muggle.TsExtensions.CodingHelper.Generators;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.Model;
using Muggle.TsExtensions.Common.Profile;
using Muggle.TsExtensions.Common.Resources;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Plugins;
using TsData = Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.KJ2001 {

    [Plugin("KJ2001")]
    [PluginUserInterface("Muggle.TsExtensions.KJ2001.View")]
    [SecondaryType(SecondaryType.SECONDARYTYPE_ZERO)]
    [AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_DETAIL)]
    [DetailType(DetailTypeEnum.END)]
    [FieldsFromData]
    public partial class KJ2001 : ConnectionBase {

        private Model Model { get; }

        private PluginData Data { get; }

        private Localization Localization { get; }

        private ProfileRect_Invariant _primProfile;

        public KJ2001(PluginData data) {
            Model = new Model();
            Data = data;

            try {
                var language = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
                language = LanguageExtension.GetTsLocalizationLanguage(language);

                var xsDataDir = string.Empty;
                TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

                var ailPath = Path.Combine(xsDataDir,
                    $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(KJ2001)}.ail");

                Localization = new Localization(ailPath, language);
            } catch {
                Localization = new Localization();
            }
        }

        public override bool Run() {
            try {
                var prim = (Part)Model.SelectModelObject(Primary);
                try {
                    _primProfile = new ProfileRect_Invariant(prim.Profile.ProfileString);
                } catch (UnAcceptableProfileException) {
                    throw new Exception(Localization.GetText("exception_column_profile_not_supported"));
                }

                Data.SetFieldsToDefaultIfUnset();
                if (Data.PlateBaseBreadth <= 0.0) Data.PlateBaseBreadth = _primProfile.b1 + 200;
                if (Data.PlateBaseHeight <= 0.0) Data.PlateBaseHeight = _primProfile.h1 + 200;
                if (IsDefaultValue(Data.BoltAnchorRodDistX) ||
                    Regex.IsMatch(Data.BoltAnchorRodDistX, "0(\\.0+)?"))
                    Data.BoltAnchorRodDistX = $"50 2*{_primProfile.h1 * 0.5 + 100} {_primProfile.h1 + 150}";
                if (IsDefaultValue(Data.BoltAnchorRodDistY) ||
                    Regex.IsMatch(Data.BoltAnchorRodDistY, "0(\\.0+)?"))
                    Data.BoltAnchorRodDistY = $"0 {_primProfile.b1 * -0.5 - 50} {_primProfile.b1 * 0.5 + 50} 0";
                Data.PartAnchorRodProfile = $"D{Data.BoltAnchorRodSize}";
                Data.PlateStifBreadth = _primProfile.b1 - _primProfile.s * 2;
                GetFieldValuesFrom(Data);

                var workTp = GetWorkTransformationPlane();
                Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

                CreateDetail();

                return true;
            } catch (Exception e) {
                Operation.DisplayPrompt(e.Message);
                return false;
            }
        }

        private TransformationPlane GetWorkTransformationPlane() {
            var prim = (Part)Model.SelectModelObject(Primary);

            var primCs = prim.GetCoordinateSystem();
            var solid = prim.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var centerPoint = (solid.MinimumPoint + solid.MaximumPoint).Multiply(0.5);

            var axisX = primCs.AxisY;
            var axisZ = centerPoint.IsZero(GeometryConstants.DISTANCE_EPSILON)
                ? new Vector(1000.0, 0.0, 0.0)
                : new Vector(centerPoint);
            var axisY = axisZ.Cross(axisX);

            return new TransformationPlane(new Point(), axisX, axisY);
        }

        private void CreateDetail() {
            var prim = (Part)Model.SelectModelObject(Primary);
            var origin = new Point(0.0, 0.0, 0.0);
            var axisX = new Vector(1.0, 0.0, 0.0);
            var axisY = new Vector(0.0, 1.0, 0.0);
            var axisZ = new Vector(0.0, 0.0, 1.0);

            #region 末端对齐

            var plane = new Plane { AxisX = axisX * 10, AxisY = axisY * 10, Origin = origin };
            var fitting = new Fitting { Father = prim, Plane = plane };
            fitting.Insert();

            #endregion

            #region 创建底板、开孔

            var p1 = new Point(_plateBaseHeight * -0.5, 0.0, 0.0);
            var p2 = new Point(_plateBaseHeight * 0.5, 0.0, 0.0);

            var basePlate = Data.CreatePlateBase<Beam>();
            basePlate.StartPoint = p1;
            basePlate.EndPoint = p2;
            basePlate.Position.Rotation = Position.RotationEnum.FRONT;
            basePlate.Position.Plane = Position.PlaneEnum.MIDDLE;
            basePlate.Position.Depth = Position.DepthEnum.BEHIND;
            basePlate.Insert();

            var weld = Data.CreateWeld1<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = basePlate;

            if (_weld1Around != 0) {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();
            } else {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                weld.Insert();
            }

            var anchorRodDistListX = TsData.DistanceList.Parse(_boltAnchorRodDistX, CultureInfo.CurrentCulture,
                TsData.Distance.CurrentUnitType);
            var anchorRodDistListY = TsData.DistanceList.Parse(_boltAnchorRodDistY, CultureInfo.CurrentCulture,
                TsData.Distance.CurrentUnitType);

            var bolt = Data.CreateBoltAnchorRod<BoltXYList>();
            bolt.PartToBoltTo = basePlate;
            bolt.PartToBeBolted = basePlate;
            bolt.FirstPosition = p1;
            bolt.SecondPosition = p2;
            bolt.Bolt = false;
            bolt.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            bolt.Position.Rotation = Position.RotationEnum.FRONT;
            bolt.Insert();

            #endregion

            #region 创建锚栓

            var positions = anchorRodDistListX
                .Select(dis => _plateBaseHeight * -0.5 + dis.Value)
                .Zip(anchorRodDistListY.Select(dis => dis.Value), (x, y) => new Point(x, y, -_plateBaseThickness))
                .ToList();
            var hookDirections = positions
                .Select(p => Math.Abs(p.X) >= Math.Abs(p.Y)
                    ? new Vector(p.X, 0, 0)
                    : new Vector(0, p.Y, 0))
                .ToList();

            foreach (var (p, v) in positions.Zip(hookDirections, (p, v) => (p, v))) {
                var anchorRodParts = ModelOperation.InsertAnchorRod(
                    p, p - axisZ * 100, _anchorLen1, _anchorLen2, _anchorLen3, _anchorLen4, _anchorLen5,
                    v, 3.5, _partAnchorRodMaterial, _boltAnchorRodSize, _boltAnchorRodTolerance,
                    _partAnchorRodClass.ToString(), _plateWasherThickness > 0, _plateWasherThickness,
                    _plateWasherBreadth, _plateBaseThickness, _boltAnchorRodSize + _boltAnchorRodTolerance * 2,
                    _boltAnchorRodUseWasher1 != 0, _boltAnchorRodUseWasher2 != 0, _boltAnchorRodUseWasher3 != 0,
                    _boltAnchorRodUseNut1 != 0, _boltAnchorRodUseNut2 != 0);

                var anchorRod = anchorRodParts.FirstOrDefault(part => part.Name == "ANCHORROD");
                if (anchorRod is not null) {
                    Data.ModifyToPartAnchorRod(ref anchorRod);
                    anchorRod.Modify();
                }

                if (anchorRodParts.FirstOrDefault(part => part.Name == "WASHERPLATE") is Beam washerPlate) {
                    Data.ModifyToPlateWasher(ref washerPlate);
                    washerPlate.Modify();
                }
            }

            #endregion

            #region 创建栓钉

            p1 = new Point(0, _primProfile.b1 * -0.5);
            p2 = p1 + axisZ * _stifDistToBase;

            var stud = Data.CreateBoltStudX<BoltArray>();
            stud.PartToBoltTo = prim;
            stud.PartToBeBolted = prim;
            stud.FirstPosition = p1;
            stud.SecondPosition = p2;
            stud.Position.Rotation = Position.RotationEnum.TOP;
            stud.StartPointOffset.Dx = _studXOffset;
            stud.Insert();

            p1.Y *= -1;
            p2.Y *= -1;

            stud.FirstPosition = p1;
            stud.SecondPosition = p2;
            stud.Position.Rotation = Position.RotationEnum.BELOW;
            stud.Insert();

            p1 = new Point(_primProfile.h1 * -0.5, 0);
            p2 = p1 + axisZ * _stifDistToBase;

            stud = Data.CreateBoltStudY<BoltArray>();
            stud.PartToBoltTo = prim;
            stud.PartToBeBolted = prim;
            stud.FirstPosition = p1;
            stud.SecondPosition = p2;
            stud.Position.Rotation = Position.RotationEnum.FRONT;
            stud.StartPointOffset.Dx = _studYOffset;
            stud.Insert();

            p1.X *= -1;
            p2.X *= -1;

            stud.FirstPosition = p1;
            stud.SecondPosition = p2;
            stud.Position.Rotation = Position.RotationEnum.BACK;
            stud.Insert();

            #endregion

            #region 创建内部加劲板

            if (_plateStifThickness <= 0) goto SkipStiffener;

            p1 = new Point(_primProfile.h1 * 0.5 - _primProfile.t, _primProfile.b1 * -0.5 + _primProfile.s,
                _stifDistToBase);
            p2 = new Point(_primProfile.h1 * -0.5 + _primProfile.t, _primProfile.b1 * -0.5 + _primProfile.s,
                _stifDistToBase);
            var p3 = new Point(p2.X, -p2.Y, _stifDistToBase);
            var p4 = new Point(p1.X, -p1.Y, _stifDistToBase);

            var chamfer = Data.CreateChamferStif();
            var chamferReversed = chamfer.GetReversed();
            var contours = new ArrayList {
                new ContourPoint(p1, chamfer),
                new ContourPoint(p2, chamferReversed),
                new ContourPoint(p3, chamfer),
                new ContourPoint(p4, chamferReversed)
            };

            var stif = Data.CreatePlateStif<ContourPlate>();
            stif.Contour.ContourPoints = contours;
            stif.Position.Depth = Position.DepthEnum.BEHIND;
            stif.Insert();

            weld = Data.CreateWeld2<Weld>();
            weld.MainObject = prim;
            weld.SecondaryObject = stif;
            if (_weld1Around != 0) {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Insert();
            } else {
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
                weld.Insert();

                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
                weld.Insert();
            }

            #endregion

            #region 开浇筑孔、排气孔

            if (_pouringHoleDiameter <= 0) goto SkipPouringHole;

            p1 = new Point(0.0, 0.0, _stifDistToBase);
            p2 = new Point(0.0, 0.0, _stifDistToBase - _plateStifThickness);

            var pouringHole = new Beam {
                StartPoint = p1,
                EndPoint = p2,
                Profile = { ProfileString = $"D{_pouringHoleDiameter}" },
                Position = { Plane = Position.PlaneEnum.MIDDLE, Depth = Position.DepthEnum.MIDDLE }
            };
            pouringHole.Insert();
            _ = ModelOperation.ApplyBooleanOperation(stif, pouringHole);
            pouringHole.Delete();

            SkipPouringHole: ;

            if (_exhaustHoleDiameter <= 0) goto SkipExhaustHole;

            p1 = new Point(
                _primProfile.h1 * -0.5 + _primProfile.t + _exhaustHolePosX,
                _primProfile.b1 * -0.5 + _primProfile.s + _exhaustHolePosY,
                _stifDistToBase);
            p2 = new Point(p1.X, p1.Y, p1.Z - _plateStifThickness);

            var exhaustHole = new Beam {
                StartPoint = p1,
                EndPoint = p2,
                Profile = { ProfileString = $"D{_exhaustHoleDiameter}" },
                Position = { Plane = Position.PlaneEnum.MIDDLE, Depth = Position.DepthEnum.MIDDLE }
            };
            exhaustHole.Insert();
            _ = ModelOperation.ApplyBooleanOperation(stif, exhaustHole);

            p1.X *= -1;
            p2.X *= -1;
            exhaustHole.StartPoint = p1;
            exhaustHole.EndPoint = p2;
            exhaustHole.Modify();
            _ = ModelOperation.ApplyBooleanOperation(stif, exhaustHole);

            p1.Y *= -1;
            p2.Y *= -1;
            exhaustHole.StartPoint = p1;
            exhaustHole.EndPoint = p2;
            exhaustHole.Modify();
            _ = ModelOperation.ApplyBooleanOperation(stif, exhaustHole);

            p1.X *= -1;
            p2.X *= -1;
            exhaustHole.StartPoint = p1;
            exhaustHole.EndPoint = p2;
            exhaustHole.Modify();
            _ = ModelOperation.ApplyBooleanOperation(stif, exhaustHole);

            exhaustHole.Delete();

            SkipExhaustHole: ;

            SkipStiffener: ;

            #endregion

        }
    }
}