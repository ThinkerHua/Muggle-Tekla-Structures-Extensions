/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2024-2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  WK1001.cs: connections of tubes
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static TeklaStructuresAttributes.Attributes;

namespace Muggle.TsExtensions.WK1001;

[Plugin("WK1001")]
[PluginUserInterface("Muggle.TsExtensions.WK1001.View")]
[SecondaryType(SecondaryType.SECONDARYTYPE_MULTIPLE)]
[PositionType(PositionTypeEnum.COLLISION_PLANE)]
[AutoDirectionType(AutoDirectionTypeEnum.AUTODIR_GLOBAL_Z)]
[FieldsFromData]
public partial class WK1001 : ConnectionBase {
    private const double HalfPi = Math.PI * 0.5;

    private readonly record struct Profile(double Height, double Width) {
        public readonly double Height = Height;
        public readonly double Width = Width;
    }

    private Model Model { get; }
    
    private PluginData Data { get; }

    private Localization Localization { get; }

    public WK1001(PluginData data) {
        Model = new Model();
        Data = data;

        try {
            var language = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_LANGUAGE", ref language);
            language = LanguageExtension.GetTsLocalizationLanguage(language);

            var xsDataDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsDataDir);

            var ailPath = Path.Combine(xsDataDir,
                $"environments\\common\\extensions\\messages\\Muggle.TsExtensions\\{nameof(WK1001)}.ail");

            Localization = new Localization(ailPath, language);
        } catch {
            Localization = new Localization();
        }
    }

    public override bool Run() {
        try {
            Data.SetFieldsToDefaultIfUnset();
            Data.Weld1Around = 1;
            Data.Weld2Around = 1;
            Data.Weld3Around = 1;
            if (!IsDefaultValue(Data.PartPipeProfile)) {
                try {
                    _ = new ProfileCircular_Perfect(Data.PartPipeProfile);
                } catch (UnAcceptableProfileException) {
                    throw new Exception(Localization.GetText("exception_no_supported_pipe_profile"));
                }
            }

            GetFieldValuesFrom(Data);

            var rodIds = new List<Identifier> { Primary };
            rodIds.AddRange(Secondaries);

            var rods = rodIds.Select(id => (Part)Model.SelectModelObject(id)).ToList();

            var profiles = rods.Select(p => {
                    var h = 0.0;
                    var b = 0.0;
                    p.GetReportProperty(HEIGHT, ref h);
                    p.GetReportProperty(WIDTH, ref b);
                    return new Profile(h, b);
                })
                .ToList();

            var centerlines = rods.Select(p => {
                    var l = p.GetCenterLine(false);
                    var first = (Point)l[0];
                    var last = (Point)l[l.Count - 1];
                    return new Line(first, last);
                })
                .ToList();
            var basePoint = GetBasePoint(centerlines);
            // 使中心线统一从节点指向远处
            centerlines = centerlines.Select(l => {
                    var p0 = l.Origin;
                    var p1 = p0 + l.Direction;
                    return Distance.PointToPoint(basePoint, p0) < Distance.PointToPoint(basePoint, p1)
                        ? l
                        : new Line(p1, p0);
                })
                .ToList();

            var originTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            var workTp = GetWorkTransformationPlane(rodIds, centerlines);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);

            // 中心线全部转换到工作变换平面
            foreach (var line in centerlines) {
                line.Origin = new Point();
                line.Direction = line.Direction.Transform(originTp, workTp);
            }

            OrderParts(ref rodIds, ref centerlines, ref profiles, out var angles);

            // 零件坐标系 Z 轴朝上则交换宽高值
            for (int i = 0; i < rodIds.Count; i++) {
                _ = GetRodNormal(rodIds[i], out var normalIsPartAxisY);
                if (!normalIsPartAxisY) {
                    profiles[i] = new Profile(profiles[i].Width, profiles[i].Height);
                }
            }

            CreateConnection(rodIds, centerlines, profiles, angles);

            return true;
        } catch (Exception e) {
            Operation.DisplayPrompt(e.Message);
            return false;
        }
    }

    private static Point GetBasePoint(IEnumerable<Line> lines) {
        if (lines == null) {
            throw new ArgumentNullException(nameof(lines));
        }

        if (lines is not IList<Line>) lines = lines.ToList();

        // ReSharper disable PossibleMultipleEnumeration
        var baseDirection = lines.First().Direction;
        var orderedIndexes = lines
            .Select((l, i) => (l.Direction, i))
            .OrderBy(pair => {
                var radian = baseDirection.GetAngleBetween(pair.Direction);
                if (radian > HalfPi) radian = Math.PI - radian;
                return radian;
            })
            .Take(2)
            .Select(pair => pair.i)
            .ToArray();
        var p00 = lines.ElementAt(orderedIndexes[0]).Origin;
        var p01 = p00 + lines.ElementAt(orderedIndexes[0]).Direction;
        var p10 = lines.ElementAt(orderedIndexes[1]).Origin;
        var p11 = p00 + lines.ElementAt(orderedIndexes[1]).Direction;
        // ReSharper restore PossibleMultipleEnumeration

        return Math.Min(Distance.PointToPoint(p00, p10), Distance.PointToPoint(p00, p11)) <
               Math.Min(Distance.PointToPoint(p01, p10), Distance.PointToPoint(p01, p11))
            ? p00
            : p01;
    }

    private Vector GetRodNormal(Identifier id, out bool normalIsPartAxisY) {
        var rod = (Part)Model.SelectModelObject(id);
        var cs = rod.GetCoordinateSystem();
        var globalZ = new Vector(0, 0, 1).TransformFrom(new TransformationPlane());

        var axisY = cs.AxisY;
        var axisZ = cs.AxisX.Cross(cs.AxisY);
        var angleY = globalZ.GetAngleBetweenPrecisely(axisY);
        var angleZ = globalZ.GetAngleBetweenPrecisely(axisZ);

        if (angleY > HalfPi) {
            angleY = Math.PI - angleY;
            axisY *= -1;
        }

        if (angleZ > HalfPi) {
            angleZ = Math.PI - angleZ;
            axisZ *= -1;
        }

        if (angleY <= angleZ) {
            normalIsPartAxisY = true;
            return axisY;
        } else {
            normalIsPartAxisY = false;
            return axisZ;
        }
    }

    private TransformationPlane GetWorkTransformationPlane(List<Identifier> rodIds, List<Line> centerlines) {
        var globalZ = new Vector(0, 0, 1).TransformFrom(new TransformationPlane());

        Point origin;
        Vector axisX, axisY;

        //  只有两根杆件的情形
        if (rodIds.Count == 2) {
            var v1 = GetRodNormal(rodIds[0], out _);
            var v2 = GetRodNormal(rodIds[1], out _);
            var axisZ = new Vector(v1.GetNormal() + v2.GetNormal());
            axisX = centerlines[0].Direction;
            axisY = axisZ.Cross(axisX);
            axisX = axisY.Cross(axisZ);

            origin = centerlines[0].Origin;

            return new TransformationPlane(origin, axisX, axisY);
        }

        //  多根杆件的情形
        //  选取与节点Z轴夹角最小（即法向与节点Z轴夹角最大）的3根杆件
        var orderedIndexes = centerlines
            .Select((l, i) => (l.Direction, i))
            .OrderBy(pair => {
                var radian = globalZ.GetAngleBetween(pair.Direction);
                return radian > HalfPi ? Math.PI - radian : radian;
            })
            .Take(3)
            .Select(pair => pair.i)
            .ToArray();

        var line0 = centerlines[orderedIndexes[0]];
        var line1 = centerlines[orderedIndexes[1]];
        var line2 = centerlines[orderedIndexes[2]];

        var seg = Intersection.LineToLine(line0, line1)
                  ?? Intersection.LineToLine(line0, line2)
                  ?? throw new InvalidOperationException(
                      Localization.GetText("exception_multiple_parallel_rods_unable_to_Create_connection"));
        origin = seg.StartPoint;

        var p0 = line0.Origin + line0.Direction.GetNormal(1000);
        var p1 = line1.Origin + line1.Direction.GetNormal(1000);
        var p2 = line2.Origin + line2.Direction.GetNormal(1000);

        var gPlane = GeometricPlaneFactory.ByPoints(p0, p1, p2);

        var normal = origin.Equals(Projection.PointToPlane(origin, gPlane))
            ? gPlane.GetNormal() //在同一平面上
            : new Vector(origin - Geometry3dOperation.CenterOfSphere(origin, p0, p1, p2)).GetNormal(); //不在同一平面上

        //  使法向基本朝上
        if (normal.Dot(globalZ) < 0) normal *= -1;

        axisY = normal.Cross(line0.Direction);
        axisX = axisY.Cross(normal);

        return new TransformationPlane(origin, axisX, axisY);
    }

    /// <summary>
    /// 按模型中实际的顺序（逆时针方向）调整零件顺序，rodIds, profiles, centerlines 元素顺序均调整。
    /// </summary>
    /// <param name="rodIds"></param>
    /// <param name="centerlines"></param>
    /// <param name="profiles"></param>
    /// <param name="angles">相邻杆件间的角度</param>
    private static void OrderParts(ref List<Identifier> rodIds, ref List<Line> centerlines, ref List<Profile> profiles,
        out List<double> angles) {
        var axisX = new Vector(1, 0, 0);
        var axisZ = new Vector(0, 0, 1);
        var xyPlane = new GeometricPlane(new Point(), axisZ);
        angles = [
            .. centerlines.Select(line =>
                axisX.GetAngleBetweenWithDirection(ProjectionExtension.VectorToPlane(line.Direction, xyPlane), axisZ))
        ];

        var sortedIndex = angles
            .Select((a, i) => (a, i))
            .OrderBy(x => x.a)
            .Select(x => x.i)
            .ToArray();

        var sortedRodIds = new List<Identifier>(sortedIndex.Length);
        var sortedProfiles = new List<Profile>(sortedIndex.Length);
        var sortedCenterlines = new List<Line>(sortedIndex.Length);
        var sortedAngles = new List<double>(sortedIndex.Length);
        foreach (var index in sortedIndex) {
            sortedRodIds.Add(rodIds[index]);
            sortedProfiles.Add(profiles[index]);
            sortedCenterlines.Add(centerlines[index]);
            sortedAngles.Add(angles[index]);
        }

        //调整为相邻杆件间的角度
        angles[0] = Math.PI * 2 + sortedAngles[0] - sortedAngles[sortedAngles.Count - 1];
        for (int i = 1; i < sortedAngles.Count; i++) {
            angles[i] = sortedAngles[i] - sortedAngles[i - 1];
        }

        rodIds = sortedRodIds;
        centerlines = sortedCenterlines;
        profiles = sortedProfiles;
    }

    private void CreateConnection(List<Identifier> rodIds, List<Line> centerlines, List<Profile> profiles,
        List<double> angles) {
        var rods = rodIds.Select(id => (Part)Model.SelectModelObject(id)).ToList();
        var origin = new Point();
        var axisX = new Vector(1, 0, 0);
        var axisY = new Vector(0, 1, 0);
        var axisZ = new Vector(0, 0, 1);

        #region 创建连接筒

        if (!IsDefaultValue(_partPipeProfile))
            goto SkipPipeProfile;

        var maxWidth = profiles.Select(prf => prf.Width).Max();
        var minAngle = angles.Min();

        //  此处求半径做了简化处理，正确值应当用以下公式计算
        //  arcsin(0.5a/r)+arcsin(0.5b/r)+2*arcsin(0.5w/r)=angle
        //  其中a, b分别为相邻杆件的宽度，angle为相邻杆件夹角
        //  w为相邻杆件间最小间距，r为连接筒半径
        var diameter = (maxWidth + _minDis) / minAngle * 2;
        if (diameter < maxWidth) diameter = maxWidth;
        var thickness = Math.Max(_plateTopEndThickness, _plateBotEndThickness);

        var minProfile = ProfileCircular_Perfect.CommonlyUsed
            .FirstOrDefault(prf => prf.d1 >= diameter && prf.t >= thickness);

        diameter = minProfile?.d1 ?? Math.Ceiling(diameter * 0.1) * 10;
        thickness = minProfile?.t ?? Math.Ceiling(thickness * 0.5) * 2;

        Data.PartPipeProfile = $"O{diameter}*{thickness}";
        _partPipeProfile = Data.PartPipeProfile;

        SkipPipeProfile: ;

        var pipPrf = new ProfileCircular_Perfect(_partPipeProfile);
        var innerDiameter = pipPrf.d1 - pipPrf.t * 2;
        if (_plateBotEndBreadth <= pipPrf.d1) {
            Data.PlateBotEndBreadth = innerDiameter;
            _plateBotEndBreadth = Data.PlateBotEndBreadth;
        }

        var isKindA = Math.Abs(_plateBotEndBreadth - innerDiameter) < GeometryConstants.DISTANCE_EPSILON;

        var maxHeight = profiles.Max(prf => prf.Height);
        var p1 = new Point(0, 0, maxHeight * 0.5 + _extendLenTop);
        var p2 = new Point(0, 0, maxHeight * -0.5 - _extendLenBot + (isKindA ? 0 : _plateBotEndThickness));

        var pipe = Data.CreatePartPipe<Beam>();
        pipe.StartPoint = p1;
        pipe.EndPoint = p2;
        pipe.Position.Plane = Position.PlaneEnum.MIDDLE;
        pipe.Position.Depth = Position.DepthEnum.MIDDLE;
        pipe.Insert();

        #endregion

        #region 切割焊接杆件

        var boolean = Data.CreatePartPipe<Beam>();
        boolean.StartPoint = p1 + axisZ * 100;
        boolean.EndPoint = p2 - axisZ * 100;
        boolean.Profile.ProfileString = $"D{pipPrf.d1}";
        boolean.Position.Plane = Position.PlaneEnum.MIDDLE;
        boolean.Position.Depth = Position.DepthEnum.MIDDLE;
        boolean.Insert();

        var workTp = Model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

        var weld = Data.CreateWeld1<Weld>();
        weld.MainObject = pipe;
        for (var i = 0; i < rods.Count; i++) {
            var rod = rods[i];
            _ = ModelOperation.ApplyBooleanOperation(rod, boolean);

            var rodAxisX = centerlines[i].Direction;
            var rodAxisY = rodAxisX.Cross(axisZ * -1);
            var tmpTp = new TransformationPlane(origin, rodAxisX, rodAxisY);
            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(tmpTp);

            weld.SecondaryObject = rod;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();

            Model.GetWorkPlaneHandler().SetCurrentTransformationPlane(workTp);
        }

        boolean.Delete();

        // 需要这行代码，否则后续获取 pipe.StartPoint 或 pipe.EndPoint 会不正确
        pipe = (Beam)Model.SelectModelObject(pipe.Identifier);

        #endregion

        #region 创建端板

        p1 = pipe.StartPoint;
        p2 = p1 - axisZ * _plateTopEndThickness;

        var topEnd = Data.CreatePlateTopEnd<Beam>();
        topEnd.Profile.ProfileString = $"D{innerDiameter}";
        topEnd.StartPoint = p1;
        topEnd.EndPoint = p2;
        topEnd.Position.Plane = Position.PlaneEnum.MIDDLE;
        topEnd.Position.Depth = Position.DepthEnum.MIDDLE;
        topEnd.Insert();

        p2 = pipe.EndPoint - axisZ * (isKindA ? 0 : _plateBotEndThickness);
        p1 = p2 + axisZ * _plateBotEndThickness;

        var botEnd = Data.CreatePlateBotEnd<Beam>();
        botEnd.Profile.ProfileString = $"D{_plateBotEndBreadth}";
        botEnd.StartPoint = p1;
        botEnd.EndPoint = p2;
        botEnd.Position.Plane = Position.PlaneEnum.MIDDLE;
        botEnd.Position.Depth = Position.DepthEnum.MIDDLE;
        botEnd.Insert();

        weld = Data.CreateWeld3<Weld>();
        weld.MainObject = pipe;
        weld.SecondaryObject = topEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        if (isKindA) {
            weld = Data.CreateWeld3<Weld>();
            weld.MainObject = pipe;
            weld.SecondaryObject = botEnd;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
            weld.Insert();
        } else {
            weld = Data.CreateWeld2<Weld>();
            weld.MainObject = pipe;
            weld.SecondaryObject = botEnd;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();
        }

        #endregion

        #region 底端板与杆件间切割焊接

        if (isKindA) goto SkipWeldsBetweenBotEndAndRods;

        var botEndObb = new OBB(
            (p1 + p2).Multiply(0.5),
            axisX, axisY, axisZ,
            _plateBotEndBreadth * 0.5, _plateBotEndBreadth * 0.5, _plateBotEndThickness * 0.5);
        var botEndSolid = botEnd.GetSolid(Solid.SolidCreationTypeEnum.RAW);

        boolean = Data.CreatePlateBotEnd<Beam>();
        boolean.StartPoint = botEnd.StartPoint;
        boolean.EndPoint = botEnd.EndPoint - axisZ * 100;
        boolean.Profile.ProfileString = botEnd.Profile.ProfileString;
        boolean.Material.MaterialString = "ANTIMATERIAL";
        boolean.Class = BooleanPart.BooleanOperativeClassName;
        boolean.Position.Plane = Position.PlaneEnum.MIDDLE;
        boolean.Position.Depth = Position.DepthEnum.MIDDLE;
        boolean.Insert();
        
        for (int i = 0; i < rods.Count; i++) {
            var rodAxisX = centerlines[i].Direction;
            var rodAxisZ = axisZ.Cross(rodAxisX);
            var rodAxisY = rodAxisZ.Cross(rodAxisX);
            var rodObb = new OBB(
                origin + centerlines[i].Direction.GetNormal(pipPrf.d1 * 0.5 + 500),
                rodAxisX, rodAxisY, rodAxisZ,
                600, profiles[i].Height * 0.5, profiles[i].Width * 0.5);
            if (!botEndObb.Intersects(rodObb)) continue;

            var rod = rods[i];
            var rodSolid = rod.GetSolid(Solid.SolidCreationTypeEnum.NORMAL);
            var shells = botEndSolid.GetCutPart(rodSolid);
            if (!shells.MoveNext()) continue;

            _ = ModelOperation.ApplyBooleanOperation(rod, boolean);

            weld = Data.CreateWeld1<Weld>();
            weld.MainObject = botEnd;
            weld.SecondaryObject = rod;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
            weld.Insert();
        }

        boolean.Delete();

        SkipWeldsBetweenBotEndAndRods: ;

        #endregion

        #region 创建加劲肋

        Data.PlateStifBreadth = Distance.PointToPoint(pipe.StartPoint, pipe.EndPoint) - _plateTopEndThickness -
                                (isKindA ? _plateBotEndThickness : 0);
        _plateStifBreadth = Data.PlateStifBreadth;

        p1 = new Point(-innerDiameter * 0.5, 0, pipe.StartPoint.Z - _plateTopEndThickness);
        p2 = new Point(-p1.X, 0, p1.Z);

        var stif1 = Data.CreatePlateStif<Beam>();
        stif1.StartPoint = p1;
        stif1.EndPoint = p2;
        stif1.Position.Rotation = Position.RotationEnum.TOP;
        stif1.Position.Plane = Position.PlaneEnum.MIDDLE;
        stif1.Position.Depth = Position.DepthEnum.BEHIND;
        stif1.Insert();

        p1 = new Point(0, p1.X, p1.Z);
        p2 = new Point(0, _plateStifThickness * -0.5, p1.Z);

        var stif2 = Data.CreatePlateStif<Beam>();
        stif2.StartPoint = p1;
        stif2.EndPoint = p2;
        stif2.Position.Rotation = Position.RotationEnum.TOP;
        stif2.Position.Plane = Position.PlaneEnum.MIDDLE;
        stif2.Position.Depth = Position.DepthEnum.BEHIND;
        stif2.Insert();

        p1.Y *= -1;
        p2.Y *= -1;

        var stif3 = Data.CreatePlateStif<Beam>();
        stif3.StartPoint = p1;
        stif3.EndPoint = p2;
        stif3.Position.Rotation = Position.RotationEnum.TOP;
        stif3.Position.Plane = Position.PlaneEnum.MIDDLE;
        stif3.Position.Depth = Position.DepthEnum.BEHIND;
        stif3.Insert();

        weld = Data.CreateWeld4<Weld>();
        weld.MainObject = pipe;
        weld.SecondaryObject = stif1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
        weld.Insert();
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
        weld.Insert();
        weld.MainObject = topEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
        weld.Insert();
        weld.MainObject = botEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();

        weld.MainObject = pipe;
        weld.SecondaryObject = stif2;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();
        weld.MainObject = topEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
        weld.Insert();
        weld.MainObject = botEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();
        weld.MainObject = stif1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();

        weld.MainObject = pipe;
        weld.SecondaryObject = stif3;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Y;
        weld.Insert();
        weld.MainObject = topEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_Z;
        weld.Insert();
        weld.MainObject = botEnd;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
        weld.Insert();
        weld.MainObject = stif1;
        weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
        weld.Insert();

        #endregion

    }
}