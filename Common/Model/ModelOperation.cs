/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2024 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  ModelOperation.cs: operations of model object
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using static Muggle.TsExtensions.Common.Profile.ProfileOperation;
using Distance = Tekla.Structures.Datatype.Distance;

namespace Muggle.TsExtensions.Common.Model {
    /// <summary>
    /// 模型操作。
    /// </summary>
    public static class ModelOperation {
        private const string UnknownSectionType = "Unknown section type.";

        /// <summary>
        /// 用给定轮廓点集合创建布尔操作多边形。
        /// </summary>
        /// <param name="contourPoints">轮廓点集合</param>
        /// <param name="thickness">厚度</param>
        /// <returns>布尔操作多边形。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="thickness"/> 不能是 <see cref="double.NaN"/>，
        /// 也不应小于等于 0.0。</exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static ContourPlate CreatBooleanOperationPolygon(ArrayList contourPoints, double thickness) {
            if (contourPoints is null) {
                throw new ArgumentNullException(nameof(contourPoints));
            }

            if (double.IsNaN(thickness) || thickness <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(thickness)}”不能是 double.NaN，也不应小于等于 0.0。");
            }

            ContourPlate contourPlate = new ContourPlate {
                Contour = { ContourPoints = contourPoints },
                Profile = { ProfileString = "PL" + thickness },
                Material = { MaterialString = "ANTIMATERIAL" },
                Class = BooleanPart.BooleanOperativeClassName,
            };
            if (!contourPlate.Insert())
                throw new Exception("Failed to insert BooleanOperationPolygon.");

            return contourPlate;
        }

        /// <summary>
        /// 用给定点集合创建布尔操作多边形。倒角为 <see cref="Chamfer.ChamferTypeEnum.CHAMFER_NONE"/>。
        /// </summary>
        /// <param name="points">点集合</param>
        /// <param name="thickness">厚度</param>
        /// <returns>布尔操作多边形。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="thickness"/> 不能是 <see cref="double.NaN"/>，
        /// 也不应小于等于 0.0。</exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static ContourPlate CreatBooleanOperationPolygon(IEnumerable<Point> points, double thickness) {
            if (points is null) {
                throw new ArgumentNullException(nameof(points));
            }

            if (double.IsNaN(thickness) || thickness <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(thickness)}”不能是 double.NaN，也不应小于等于 0.0。");
            }

            var chamfer = new Chamfer();
            ArrayList contourPoints = [];
            foreach (var point in points) {
                contourPoints.Add(new ContourPoint(point, chamfer));
            }

            var contourPlate = new ContourPlate {
                Contour = { ContourPoints = contourPoints },
                Profile = { ProfileString = "PL" + thickness },
                Material = { MaterialString = "ANTIMATERIAL" },
                Class = BooleanPart.BooleanOperativeClassName,
            };
            if (!contourPlate.Insert())
                throw new Exception("Failed to insert BooleanOperationPolygon.");

            return contourPlate;
        }

        /// <summary>
        /// 用给定多边形板创建布尔操作多边形。
        /// </summary>
        /// <param name="sourceContourPlate">用作布尔操作的多边形板</param>
        /// <returns>布尔操作多边形</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static ContourPlate CreatBooleanOperationPolygon(ContourPlate sourceContourPlate) {
            if (sourceContourPlate is null) {
                throw new ArgumentNullException(nameof(sourceContourPlate));
            }

            var cps = new ArrayList();
            foreach (ContourPoint cp in sourceContourPlate.Contour.ContourPoints) {
                cps.Add(cp.Clone());
            }

            ContourPlate contourPlate = new ContourPlate {
                Contour = { ContourPoints = cps },
                Profile = { ProfileString = sourceContourPlate.Profile.ProfileString },
                Material = { MaterialString = "ANTIMATERIAL" },
                Class = BooleanPart.BooleanOperativeClassName,
            };
            if (!contourPlate.Insert())
                throw new Exception("Failed to insert BooleanOperationPolygon.");

            return contourPlate;
        }

        /// <summary>
        /// 应用布尔操作。
        /// </summary>
        /// <param name="father">被布尔操作的对象</param>
        /// <param name="operativePart">布尔操作对象</param>
        /// <param name="typeEnum">布尔操作类型，默认值 <see cref="BooleanPart.BooleanTypeEnum.BOOLEAN_CUT"/></param>
        /// <returns>操作成功返回 true，失败返回 false。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ApplyBooleanOperation(
            ModelObject father,
            Part operativePart,
            BooleanPart.BooleanTypeEnum typeEnum = BooleanPart.BooleanTypeEnum.BOOLEAN_CUT) {
            if (father is null) {
                throw new ArgumentNullException(nameof(father));
            }

            if (operativePart is null) {
                throw new ArgumentNullException(nameof(operativePart));
            }

            if (operativePart.Class != BooleanPart.BooleanOperativeClassName) {
                //不需要下面这句，会产生重复对象。设置Class属性后，会自动生成BooleanPart对象
                //operativePart = Tekla.Structures.Model.Operations.Operation.CopyObject(operativePart, new Vector()) as Part;
                operativePart.Class = BooleanPart.BooleanOperativeClassName;
            }

            BooleanPart bp = new BooleanPart { Father = father, Type = typeEnum, };

            bp.SetOperativePart(operativePart);

            return bp.Insert();
        }

        /// <summary>
        /// 创建梁。
        /// </summary>
        /// <param name="startPoint">梁起点</param>
        /// <param name="endPoint">梁终点</param>
        /// <param name="name">梁名称，默认值 "BEAM"</param>
        /// <param name="profileStr">梁截面，默认值 "HM244*175*7*11"</param>
        /// <param name="materialStr">梁材质，默认值 "Q345B"</param>
        /// <param name="assemblyPrefix">梁构件编号前缀，默认值 "GL-"</param>
        /// <param name="assemblyStartNumber">梁构件编号起始编号，默认值 1</param>
        /// <param name="partPrefix">梁零件编号前缀，默认值 "P"</param>
        /// <param name="partStartNumber">梁零件编号起始编号，默认值 1</param>
        /// <param name="class">梁的等级，默认值 "99"</param>
        /// <param name="planeEnum">位置属性的平面类型，默认值 <see cref="Position.PlaneEnum.MIDDLE"/></param>
        /// <param name="planeOffset">位置属性的平面偏移，默认值 0.0</param>
        /// <param name="depthEnum">位置属性的深度类型，默认值 <see cref="Position.DepthEnum.MIDDLE"/></param>
        /// <param name="depthOffset">位置属性的深度偏移，默认值 0.0</param>
        /// <param name="rotationEnum">位置属性的旋转类型，默认值 <see cref="Position.RotationEnum.FRONT"/></param>
        /// <param name="rotationOffset">位置属性的旋转偏移，默认值 0.0</param>
        /// <returns>创建的梁</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static Beam CreatBeam(
            Point startPoint, Point endPoint,
            string name = "BEAM", string profileStr = "HM244*175*7*11", string materialStr = "Q345B",
            string assemblyPrefix = "GL-", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1,
            string @class = "99",
            Position.PlaneEnum planeEnum = Position.PlaneEnum.MIDDLE, double planeOffset = 0.0,
            Position.DepthEnum depthEnum = Position.DepthEnum.MIDDLE, double depthOffset = 0.0,
            Position.RotationEnum rotationEnum = Position.RotationEnum.FRONT, double rotationOffset = 0.0) {
            if (startPoint is null) {
                throw new ArgumentNullException(nameof(startPoint));
            }

            if (endPoint is null) {
                throw new ArgumentNullException(nameof(endPoint));
            }

            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(profileStr)) {
                profileStr = "HM244*175*7*11";
            }

            if (materialStr == null) {
                throw new ArgumentNullException(nameof(materialStr));
            }

            assemblyPrefix ??= string.Empty;

            partPrefix ??= string.Empty;

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            Beam beam = new Beam {
                StartPoint = startPoint,
                EndPoint = endPoint,
                Name = name,
                Profile = { ProfileString = profileStr },
                Material = { MaterialString = materialStr },
                AssemblyNumber = { Prefix = assemblyPrefix, StartNumber = assemblyStartNumber },
                PartNumber = { Prefix = partPrefix, StartNumber = partStartNumber },
                Class = @class,
                Position = {
                    Plane = planeEnum,
                    PlaneOffset = planeOffset,
                    Depth = depthEnum,
                    DepthOffset = depthOffset,
                    Rotation = rotationEnum,
                    RotationOffset = rotationOffset,
                }
            };
            if (!beam.Insert())
                throw new Exception("Failed to insert Beam.");

            return beam;
        }

        /// <summary>
        /// 用给定轮廓点集合创建折梁。
        /// </summary>
        /// <param name="contour">折梁的轮廓</param>
        /// <param name="name">折梁名称，默认值 "BEAM"</param>
        /// <param name="profileStr">折梁截面，默认值 "HM244*175*7*11"</param>
        /// <param name="materialStr">折梁材质，默认值 "Q345B"</param>
        /// <param name="assemblyPrefix">折梁构件编号前缀，默认值 "GL-"</param>
        /// <param name="assemblyStartNumber">折梁构件编号起始编号，默认值 1</param>
        /// <param name="partPrefix">折梁零件编号前缀，默认值 "P"</param>
        /// <param name="partStartNumber">折梁零件编号起始编号，默认值 1</param>
        /// <param name="class">折梁的等级，默认值 "99"</param>
        /// <param name="planeEnum">位置属性的平面类型，默认值 <see cref="Position.PlaneEnum.MIDDLE"/></param>
        /// <param name="planeOffset">位置属性的平面偏移，默认值 0.0</param>
        /// <param name="depthEnum">位置属性的深度类型，默认值 <see cref="Position.DepthEnum.MIDDLE"/></param>
        /// <param name="depthOffset">位置属性的深度偏移，默认值 0.0</param>
        /// <param name="rotationEnum">位置属性的旋转类型，默认值 <see cref="Position.RotationEnum.FRONT"/></param>
        /// <param name="rotationOffset">位置属性的旋转偏移，默认值 0.0</param>
        /// <returns>创建的折梁</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static PolyBeam CreatPolyBeam(
            Contour contour,
            string name = "BEAM", string profileStr = "HM244*175*7*11", string materialStr = "Q345B",
            string assemblyPrefix = "GL-", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1,
            string @class = "99",
            Position.PlaneEnum planeEnum = Position.PlaneEnum.MIDDLE, double planeOffset = 0.0,
            Position.DepthEnum depthEnum = Position.DepthEnum.MIDDLE, double depthOffset = 0.0,
            Position.RotationEnum rotationEnum = Position.RotationEnum.FRONT, double rotationOffset = 0.0) {
            if (contour is null) {
                throw new ArgumentNullException(nameof(contour));
            }

            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(profileStr)) {
                profileStr = "HM244*175*7*11";
            }

            if (materialStr == null) {
                throw new ArgumentNullException(nameof(materialStr));
            }

            assemblyPrefix ??= string.Empty;

            partPrefix ??= string.Empty;

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var polyBeam = new PolyBeam {
                Contour = contour,
                Name = name,
                Profile = { ProfileString = profileStr },
                Material = { MaterialString = materialStr },
                AssemblyNumber = { Prefix = assemblyPrefix, StartNumber = assemblyStartNumber },
                PartNumber = { Prefix = partPrefix, StartNumber = partStartNumber },
                Class = @class,
                Position = {
                    Plane = planeEnum,
                    PlaneOffset = planeOffset,
                    Depth = depthEnum,
                    DepthOffset = depthOffset,
                    Rotation = rotationEnum,
                    RotationOffset = rotationOffset,
                }
            };
            if (!polyBeam.Insert())
                throw new Exception("Failed to insert Beam.");

            return polyBeam;
        }

        /// <summary>
        /// 用给定点集合创建折梁，各轮廓点倒角均为 <see cref="Chamfer.ChamferTypeEnum.CHAMFER_NONE"/>。
        /// </summary>
        /// <param name="points">折梁的控制点集合</param>
        /// <param name="name">折梁名称，默认值 "BEAM"</param>
        /// <param name="profileStr">折梁截面，默认值 "HM244*175*7*11"</param>
        /// <param name="materialStr">折梁材质，默认值 "Q345B"</param>
        /// <param name="assemblyPrefix">折梁构件编号前缀，默认值 "GL-"</param>
        /// <param name="assemblyStartNumber">折梁构件编号起始编号，默认值 1</param>
        /// <param name="partPrefix">折梁零件编号前缀，默认值 "P"</param>
        /// <param name="partStartNumber">折梁零件编号起始编号，默认值 1</param>
        /// <param name="class">折梁的等级，默认值 "99"</param>
        /// <param name="planeEnum">位置属性的平面类型，默认值 <see cref="Position.PlaneEnum.MIDDLE"/></param>
        /// <param name="planeOffset">位置属性的平面偏移，默认值 0.0</param>
        /// <param name="depthEnum">位置属性的深度类型，默认值 <see cref="Position.DepthEnum.MIDDLE"/></param>
        /// <param name="depthOffset">位置属性的深度偏移，默认值 0.0</param>
        /// <param name="rotationEnum">位置属性的旋转类型，默认值 <see cref="Position.RotationEnum.FRONT"/></param>
        /// <param name="rotationOffset">位置属性的旋转偏移，默认值 0.0</param>
        /// <returns>创建的折梁</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static PolyBeam CreatPolyBeam(
            IEnumerable<Point> points,
            string name = "BEAM", string profileStr = "HM244*175*7*11", string materialStr = "Q345B",
            string assemblyPrefix = "GL-", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1,
            string @class = "99",
            Position.PlaneEnum planeEnum = Position.PlaneEnum.MIDDLE, double planeOffset = 0.0,
            Position.DepthEnum depthEnum = Position.DepthEnum.MIDDLE, double depthOffset = 0.0,
            Position.RotationEnum rotationEnum = Position.RotationEnum.FRONT, double rotationOffset = 0.0) {
            if (points is null) {
                throw new ArgumentNullException(nameof(points));
            }

            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(profileStr)) {
                profileStr = "HM244*175*7*11";
            }

            if (materialStr == null) {
                throw new ArgumentNullException(nameof(materialStr));
            }

            assemblyPrefix ??= string.Empty;

            partPrefix ??= string.Empty;

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var contour = new Contour();
            foreach (var p in points) {
                contour.ContourPoints.Add(new ContourPoint(p, new Chamfer()));
            }

            var polyBeam = new PolyBeam {
                Contour = contour,
                Name = name,
                Profile = { ProfileString = profileStr },
                Material = { MaterialString = materialStr },
                AssemblyNumber = { Prefix = assemblyPrefix, StartNumber = assemblyStartNumber },
                PartNumber = { Prefix = partPrefix, StartNumber = partStartNumber },
                Class = @class,
                Position = {
                    Plane = planeEnum,
                    PlaneOffset = planeOffset,
                    Depth = depthEnum,
                    DepthOffset = depthOffset,
                    Rotation = rotationEnum,
                    RotationOffset = rotationOffset,
                }
            };
            if (!polyBeam.Insert())
                throw new Exception("Failed to insert Beam.");

            return polyBeam;
        }

        /// <summary>
        /// 用给定轮廓点集合创建多边形板。
        /// </summary>
        /// <param name="contourPoints">板轮廓点</param>
        /// <param name="name">名称，默认值 "PLATE"</param>
        /// <param name="profileStr">截面，默认值 "PL10"</param>
        /// <param name="materialStr">材质，默认值 "Q345B"</param>
        /// <param name="assemblyPrefix">构件编号前缀，默认值 "PLATE"</param>
        /// <param name="assemblyStartNumber">构件编号起始编号，默认值 1</param>
        /// <param name="partPrefix">零件编号前缀，默认值 "P"</param>
        /// <param name="partStartNumber">零件编号起始编号，默认值 1</param>
        /// <param name="class">等级，默认值 "99"</param>
        /// <param name="depthEnum">位置属性的深度类型，默认值 <see cref="Position.DepthEnum.MIDDLE"/></param>
        /// <param name="depthOffset">位置属性的深度偏移，默认值 0.0</param>
        /// <returns>创建的多边形板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static ContourPlate CreatContourPlate(
            ArrayList contourPoints,
            string name = "PLATE", string profileStr = "PL10", string materialStr = "Q345B",
            string assemblyPrefix = "PLATE", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1,
            string @class = "99",
            Position.DepthEnum depthEnum = Position.DepthEnum.MIDDLE, double depthOffset = 0.0) {
            if (contourPoints is null) {
                throw new ArgumentNullException(nameof(contourPoints));
            }

            if (contourPoints.Count == 0) {
                throw new ArgumentException($"“{nameof(contourPoints)}”元素数量不应为 0。");
            }

            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(profileStr)) {
                profileStr = "PL10";
            }

            if (materialStr == null) {
                throw new ArgumentNullException(nameof(materialStr));
            }

            assemblyPrefix ??= string.Empty;

            partPrefix ??= string.Empty;

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            ContourPlate contourPlate = new ContourPlate {
                Contour = { ContourPoints = contourPoints },
                Name = name,
                Profile = { ProfileString = profileStr },
                Material = { MaterialString = materialStr },
                AssemblyNumber = { Prefix = assemblyPrefix, StartNumber = assemblyStartNumber },
                PartNumber = { Prefix = partPrefix, StartNumber = partStartNumber },
                Class = @class,
                Position = { Depth = depthEnum, DepthOffset = depthOffset },
            };

            if (!contourPlate.Insert())
                throw new Exception("Failed to insert ContourPlate.");

            return contourPlate;
        }

        /// <summary>
        /// 用给定点集合创建多边形板。倒角为 <see cref="Chamfer.ChamferTypeEnum.CHAMFER_NONE"/>。
        /// </summary>
        /// <param name="points">点集合</param>
        /// <param name="name">名称，默认值 "PLATE"</param>
        /// <param name="profileStr">截面，默认值 "PL10"</param>
        /// <param name="materialStr">材质，默认值 "Q345B"</param>
        /// <param name="assemblyPrefix">构件编号前缀，默认值 "PLATE"</param>
        /// <param name="assemblyStartNumber">构件编号起始编号，默认值 1</param>
        /// <param name="partPrefix">零件编号前缀，默认值 "P"</param>
        /// <param name="partStartNumber">零件编号起始编号，默认值 1</param>
        /// <param name="class">等级，默认值 "99"</param>
        /// <param name="depthEnum">位置属性的深度类型，默认值 <see cref="Position.DepthEnum.MIDDLE"/></param>
        /// <param name="depthOffset">位置属性的深度偏移，默认值 0.0</param>
        /// <returns>创建的多边形板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="points"/>元素数量为 0 时引发。</exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static ContourPlate CreatContourPlate(
            IEnumerable<Point> points,
            string name = "PLATE", string profileStr = "PL10", string materialStr = "Q345B",
            string assemblyPrefix = "PLATE", int assemblyStartNumber = 1, string partPrefix = "P",
            int partStartNumber = 1,
            string @class = "99",
            Position.DepthEnum depthEnum = Position.DepthEnum.MIDDLE, double depthOffset = 0.0) {
            if (points is null) {
                throw new ArgumentNullException(nameof(points));
            }

            var pointArr = points as IList<Point> ?? [.. points];
            if (!pointArr.Any()) {
                throw new ArgumentException($"“{nameof(points)}”元素数量不应为 0。", nameof(points));
            }

            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(profileStr)) {
                profileStr = "PL10";
            }

            if (materialStr == null) {
                throw new ArgumentNullException(nameof(materialStr));
            }

            assemblyPrefix ??= string.Empty;

            partPrefix ??= string.Empty;

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var contourPoints = new ArrayList();
            var chamfer = new Chamfer();
            foreach (var point in pointArr) {
                contourPoints.Add(new ContourPoint(point, chamfer));
            }

            ContourPlate contourPlate = new ContourPlate {
                Contour = { ContourPoints = contourPoints },
                Name = name,
                Profile = { ProfileString = profileStr },
                Material = { MaterialString = materialStr },
                AssemblyNumber = { Prefix = assemblyPrefix, StartNumber = assemblyStartNumber },
                PartNumber = { Prefix = partPrefix, StartNumber = partStartNumber },
                Class = @class,
                Position = { Depth = depthEnum, DepthOffset = depthOffset },
            };
            if (!contourPlate.Insert())
                throw new Exception("Failed to insert ContourPlate.");

            return contourPlate;
        }

        /// <summary>
        /// 创建焊缝。
        /// </summary>
        /// <param name="mainObject">焊接到对象</param>
        /// <param name="secondaryObject">焊接对象</param>
        /// <param name="aroundWeld">环焊缝(true)或边缘焊缝(false)，默认值 true</param>
        /// <param name="shopWeld">车间焊接(true)或现场焊接(false)，默认值 true</param>
        /// <param name="position">位置，默认值 <see cref="Weld.WeldPositionEnum.WELD_POSITION_PLUS_X"/></param>
        /// <param name="preparation">焊接准备，默认值 <see cref="BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE"/></param>
        /// <param name="typeAbove">上焊缝类型，默认值 <see cref="BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET"/></param>
        /// <param name="sizeAbove">上焊缝尺寸，默认值 6.0</param>
        /// <param name="angleAbove">上焊缝角度，默认值 0.0</param>
        /// <param name="typeBelow">下焊缝类型，默认值 <see cref="BaseWeld.WeldTypeEnum.WELD_TYPE_NONE"/></param>
        /// <param name="sizeBelow">下焊缝尺寸，默认值 0.0</param>
        /// <param name="angleBelow">下焊缝角度，默认值 0.0</param>
        /// <returns>创建的焊缝</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static Weld CreatWeld(
            ModelObject mainObject, ModelObject secondaryObject,
            bool aroundWeld = true, bool shopWeld = true,
            Weld.WeldPositionEnum position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X,
            BaseWeld.WeldPreparationTypeEnum preparation = BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE,
            BaseWeld.WeldTypeEnum typeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET,
            double sizeAbove = 6.0, double angleAbove = 0.0,
            BaseWeld.WeldTypeEnum typeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_NONE,
            double sizeBelow = 0.0, double angleBelow = 0.0) {
            if (mainObject is null) {
                throw new ArgumentNullException(nameof(mainObject));
            }

            if (secondaryObject is null) {
                throw new ArgumentNullException(nameof(secondaryObject));
            }

            Weld weld = new Weld {
                MainObject = mainObject,
                SecondaryObject = secondaryObject,
                AroundWeld = aroundWeld,
                ShopWeld = shopWeld,
                Position = position,
                Preparation = preparation,
                TypeAbove = typeAbove,
                SizeAbove = sizeAbove,
                AngleAbove = angleAbove,
                TypeBelow = typeBelow,
                SizeBelow = sizeBelow,
                AngleBelow = angleBelow,
            };
            if (!weld.Insert())
                throw new Exception("Failed to insert Weld.");

            return weld;
        }

        /// <summary>
        /// 创建多边形焊缝。
        /// </summary>
        /// <param name="mainObject">焊接到对象</param>
        /// <param name="secondaryObject">焊接的对象</param>
        /// <param name="polygon">多边形</param>
        /// <param name="aroundWeld">环焊缝(true)或边缘焊缝(false)，默认值 true</param>
        /// <param name="shopWeld">车间焊接(true)或现场焊接(false)，默认值 true</param>
        /// <param name="preparation">焊接准备，默认值 <see cref="BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE"/></param>
        /// <param name="typeAbove">上焊缝类型，默认值 <see cref="BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET"/></param>
        /// <param name="sizeAbove">上焊缝尺寸，默认值 6.0</param>
        /// <param name="angleAbove">上焊缝角度，默认值 0.0</param>
        /// <param name="typeBelow">下焊缝类型，默认值 <see cref="BaseWeld.WeldTypeEnum.WELD_TYPE_NONE"/></param>
        /// <param name="sizeBelow">下焊缝尺寸，默认值 0.0</param>
        /// <param name="angleBelow">下焊缝角度，默认值 0.0</param>
        /// <returns>创建的多边形焊缝</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static PolygonWeld CreatPolygonWeld(
            ModelObject mainObject, ModelObject secondaryObject, Polygon polygon,
            bool aroundWeld = false, bool shopWeld = true,
            BaseWeld.WeldPreparationTypeEnum preparation = BaseWeld.WeldPreparationTypeEnum.PREPARATION_NONE,
            BaseWeld.WeldTypeEnum typeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET,
            double sizeAbove = 6.0, double angleAbove = 0.0,
            BaseWeld.WeldTypeEnum typeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_NONE,
            double sizeBelow = 0.0, double angleBelow = 0.0) {
            if (mainObject is null) {
                throw new ArgumentNullException(nameof(mainObject));
            }

            if (secondaryObject is null) {
                throw new ArgumentNullException(nameof(secondaryObject));
            }

            if (polygon is null) {
                throw new ArgumentNullException(nameof(polygon));
            }

            PolygonWeld pw = new PolygonWeld {
                MainObject = mainObject,
                SecondaryObject = secondaryObject,
                Polygon = polygon,
                AroundWeld = aroundWeld,
                ShopWeld = shopWeld,
                Preparation = preparation,
                TypeAbove = typeAbove,
                SizeAbove = sizeAbove,
                AngleAbove = angleAbove,
                TypeBelow = typeBelow,
                SizeBelow = sizeBelow,
                AngleBelow = angleBelow,
            };
            if (!pw.Insert())
                throw new Exception("Failed to insert PolygonWeld.");

            return pw;
        }

        /// <summary>
        /// 创建阵列螺栓组。
        /// </summary>
        /// <param name="boltTo">栓接到的零件</param>
        /// <param name="beBolted">要栓接的零件</param>
        /// <param name="otherBeBolted">其他要栓接的零件集合</param>
        /// <param name="firstPosition">第一定位点</param>
        /// <param name="secondPosition">第二定位点</param>
        /// <param name="boltDistX">X方向距离列</param>
        /// <param name="boltDistY">Y方向距离列</param>
        /// <param name="position">螺栓组定位，默认值旋转定位 <see cref="Position.RotationEnum.TOP"/>，平面定位 0.0，深度定位 0.0</param>
        /// <param name="startOffset">起点偏移值，默认值 new Offset()</param>
        /// <param name="endOffset">终点偏移值，默认值 new Offset()</param>
        /// <param name="boltStandard">螺栓等级，默认值 "HS10.9"</param>
        /// <param name="boltSize">螺栓尺寸，默认值 20.0</param>
        /// <param name="boltType">车间(true)或现场(false)，默认值 true</param>
        /// <param name="tolerance">孔公差，默认值 2.0</param>
        /// <param name="bolt">螺栓(true)或孔(false)，默认值 true</param>
        /// <param name="washer1">是否使用垫圈1，默认值 true</param>
        /// <param name="washer2">是否使用垫圈2，默认值 true</param>
        /// <param name="washer3">是否使用垫圈3，默认值 true</param>
        /// <param name="nut1">是否使用螺母1，默认值 true</param>
        /// <param name="nut2">是否使用螺母1，默认值 true</param>
        /// <returns>创建的阵列螺栓组。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="boltDistX"/> 或 <paramref name="boltDistY"/>
        /// 中元素数量为 0 时引发。</exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static BoltArray CreatBoltArray(
            Part boltTo, Part beBolted, IEnumerable<Part> otherBeBolted, Point firstPosition, Point secondPosition,
            IEnumerable<Distance> boltDistX,
            IEnumerable<Distance> boltDistY,
            Position position = null, Offset startOffset = null, Offset endOffset = null,
            string boltStandard = "HS10.9", double boltSize = 20.0,
            BoltGroup.BoltTypeEnum boltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE,
            double tolerance = 2.0,
            bool bolt = true, bool washer1 = true, bool washer2 = true, bool washer3 = true, bool nut1 = true,
            bool nut2 = true) {
            if (boltTo is null) {
                throw new ArgumentNullException(nameof(boltTo));
            }

            if (firstPosition is null) {
                throw new ArgumentNullException(nameof(firstPosition));
            }

            if (secondPosition is null) {
                throw new ArgumentNullException(nameof(secondPosition));
            }

            var boltDistXList = boltDistX as IList<Distance> ?? boltDistX.ToList();
            if (!boltDistXList.Any())
                throw new ArgumentException($"“{nameof(boltDistX)}”中项目数至少需要1个。");

            var boltDistYList = boltDistY as IList<Distance> ?? boltDistY.ToList();
            if (!boltDistYList.Any())
                throw new ArgumentException($"“{nameof(boltDistY)}”中项目数至少需要1个。");

            position ??= new Position { Rotation = Position.RotationEnum.TOP };

            startOffset ??= new Offset();

            endOffset ??= new Offset();

            BoltArray boltArray = new BoltArray {
                PartToBoltTo = boltTo,
                PartToBeBolted = beBolted,
                FirstPosition = firstPosition,
                SecondPosition = secondPosition,
                Position = position,
                StartPointOffset = startOffset,
                EndPointOffset = endOffset,
                BoltStandard = boltStandard,
                BoltSize = boltSize,
                BoltType = boltType,
                Tolerance = tolerance,
                Bolt = bolt,
                Washer1 = washer1,
                Washer2 = washer2,
                Washer3 = washer3,
                Nut1 = nut1,
                Nut2 = nut2,
            };
            if (otherBeBolted != null) {
                foreach (var part in otherBeBolted) {
                    boltArray.AddOtherPartToBolt(part);
                }
            }

            foreach (var d in boltDistXList) {
                boltArray.AddBoltDistX(d.Value);
            }

            foreach (var d in boltDistYList) {
                boltArray.AddBoltDistY(d.Value);
            }

            return boltArray.Insert() ? boltArray : throw new Exception("Failed to insert BoltArray.");
        }

        /// <summary>
        /// 创建环形螺栓组。
        /// </summary>
        /// <param name="boltTo">栓接到的零件</param>
        /// <param name="beBolted">要栓接的零件</param>
        /// <param name="otherBeBolted">其他要栓接的零件集合</param>
        /// <param name="firstPosition">第一定位点</param>
        /// <param name="secondPosition">第二定位点</param>
        /// <param name="num">螺栓数量，默认值 8</param>
        /// <param name="diameter">环形直径，默认值 200.0</param>
        /// <param name="position">螺栓组定位，默认值旋转定位 <see cref="Position.RotationEnum.TOP"/>，平面定位 0.0，深度定位 0.0</param>
        /// <param name="boltStandard">螺栓等级，默认值 "HS10.9"</param>
        /// <param name="boltSize">螺栓尺寸，默认值 20.0</param>
        /// <param name="boltType">车间(true)或现场(false)，默认值 true</param>
        /// <param name="tolerance">孔公差，默认值 2.0</param>
        /// <param name="bolt">螺栓(true)或孔(false)，默认值 true</param>
        /// <param name="washer1">是否使用垫圈1，默认值 true</param>
        /// <param name="washer2">是否使用垫圈2，默认值 true</param>
        /// <param name="washer3">是否使用垫圈3，默认值 true</param>
        /// <param name="nut1">是否使用螺母1，默认值 true</param>
        /// <param name="nut2">是否使用螺母1，默认值 true</param>
        /// <returns>创建的环形螺栓组。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static BoltCircle CreatBoltCircle(
            Part boltTo, Part beBolted, IEnumerable<Part> otherBeBolted, Point firstPosition, Point secondPosition,
            int num = 8, double diameter = 200.0, Position position = null,
            string boltStandard = "HS10.9", double boltSize = 20.0,
            BoltGroup.BoltTypeEnum boltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE,
            double tolerance = 2.0,
            bool bolt = true, bool washer1 = true, bool washer2 = true, bool washer3 = true, bool nut1 = true,
            bool nut2 = true) {
            if (boltTo is null) {
                throw new ArgumentNullException(nameof(boltTo));
            }

            if (firstPosition is null) {
                throw new ArgumentNullException(nameof(firstPosition));
            }

            if (secondPosition is null) {
                throw new ArgumentNullException(nameof(secondPosition));
            }

            position ??= new Position { Rotation = Position.RotationEnum.TOP };

            var boltCircle = new BoltCircle {
                PartToBoltTo = boltTo,
                PartToBeBolted = beBolted,
                FirstPosition = firstPosition,
                SecondPosition = secondPosition,
                NumberOfBolts = num,
                Diameter = diameter,
                Position = position,
                BoltStandard = boltStandard,
                BoltSize = boltSize,
                BoltType = boltType,
                Tolerance = tolerance,
                Bolt = bolt,
                Washer1 = washer1,
                Washer2 = washer2,
                Washer3 = washer3,
                Nut1 = nut1,
                Nut2 = nut2,
            };

            if (otherBeBolted != null) {
                foreach (var part in otherBeBolted) {
                    boltCircle.AddOtherPartToBolt(part);
                }
            }

            return boltCircle.Insert() ? boltCircle : throw new Exception("Failed to insert BoltCircle.");
        }

        /// <summary>
        /// 创建列表螺栓组。
        /// </summary>
        /// <param name="boltTo">栓接到的零件</param>
        /// <param name="beBolted">要栓接的零件</param>
        /// <param name="otherBeBolted">其他要栓接的零件集合</param>
        /// <param name="firstPosition">第一定位点</param>
        /// <param name="secondPosition">第二定位点</param>
        /// <param name="boltDistX">X方向距离列</param>
        /// <param name="boltDistY">Y方向距离列</param>
        /// <param name="position">螺栓组定位，默认值旋转定位 <see cref="Position.RotationEnum.TOP"/>，平面定位 0.0，深度定位 0.0</param>
        /// <param name="startOffset">起点偏移值，默认值 new Offset()</param>
        /// <param name="endOffset">终点偏移值，默认值 new Offset()</param>
        /// <param name="boltStandard">螺栓等级，默认值 "HS10.9"</param>
        /// <param name="boltSize">螺栓尺寸，默认值 20.0</param>
        /// <param name="boltType">车间(true)或现场(false)，默认值 true</param>
        /// <param name="tolerance">孔公差，默认值 2.0</param>
        /// <param name="bolt">螺栓(true)或孔(false)，默认值 true</param>
        /// <param name="washer1">是否使用垫圈1，默认值 true</param>
        /// <param name="washer2">是否使用垫圈2，默认值 true</param>
        /// <param name="washer3">是否使用垫圈3，默认值 true</param>
        /// <param name="nut1">是否使用螺母1，默认值 true</param>
        /// <param name="nut2">是否使用螺母1，默认值 true</param>
        /// <returns>创建的阵列螺栓组。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="boltDistX"/> 或 <paramref name="boltDistY"/>
        /// 中元素数量为 0 时引发。</exception>
        /// <exception cref="Exception">对象插入数据库失败时引发。</exception>
        public static BoltXYList CreatBoltXYList(
            Part boltTo, Part beBolted, IEnumerable<Part> otherBeBolted, Point firstPosition, Point secondPosition,
            IEnumerable<Distance> boltDistX,
            IEnumerable<Distance> boltDistY,
            Position position = null, Offset startOffset = null, Offset endOffset = null,
            string boltStandard = "HS10.9", double boltSize = 20.0,
            BoltGroup.BoltTypeEnum boltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE,
            double tolerance = 2.0,
            bool bolt = true, bool washer1 = true, bool washer2 = true, bool washer3 = true, bool nut1 = true,
            bool nut2 = true) {
            if (boltTo is null) {
                throw new ArgumentNullException(nameof(boltTo));
            }

            if (firstPosition is null) {
                throw new ArgumentNullException(nameof(firstPosition));
            }

            if (secondPosition is null) {
                throw new ArgumentNullException(nameof(secondPosition));
            }

            var boltDistXList = boltDistX as IList<Distance> ?? boltDistX.ToList();
            if (!boltDistXList.Any())
                throw new ArgumentException($"“{nameof(boltDistX)}”中项目数至少需要1个。");

            var boltDistYList = boltDistX as IList<Distance> ?? boltDistY.ToList();
            if (!boltDistYList.Any())
                throw new ArgumentException($"“{nameof(boltDistY)}”中项目数至少需要1个。");

            position ??= new Position { Rotation = Position.RotationEnum.TOP };

            startOffset ??= new Offset();

            endOffset ??= new Offset();

            var boltXYList = new BoltXYList {
                PartToBoltTo = boltTo,
                PartToBeBolted = beBolted,
                FirstPosition = firstPosition,
                SecondPosition = secondPosition,
                Position = position,
                StartPointOffset = startOffset,
                EndPointOffset = endOffset,
                BoltStandard = boltStandard,
                BoltSize = boltSize,
                BoltType = boltType,
                Tolerance = tolerance,
                Bolt = bolt,
                Washer1 = washer1,
                Washer2 = washer2,
                Washer3 = washer3,
                Nut1 = nut1,
                Nut2 = nut2,
            };
            if (otherBeBolted != null) {
                foreach (var part in otherBeBolted) {
                    boltXYList.AddOtherPartToBolt(part);
                }
            }

            foreach (var d in boltDistXList) {
                boltXYList.AddBoltDistX(d.Value);
            }

            foreach (var d in boltDistYList) {
                boltXYList.AddBoltDistY(d.Value);
            }

            return boltXYList.Insert() ? boltXYList : throw new Exception("Failed to insert BoltXYList.");
        }

        /// <summary>
        /// 创建锚杆。
        /// </summary>
        /// <remarks>参考1047系统组件，锚杆、垫圈、螺母等均用零件模拟。</remarks>
        /// <param name="firstPosition">锚杆第一控制点，一般为底板底标高处</param>
        /// <param name="secondPosition">锚杆第二控制点</param>
        /// <param name="length1">锚杆上端至第一控制点长度</param>
        /// <param name="length2">锚杆第一控制点到螺纹截止处长度</param>
        /// <param name="length3">螺纹截止处到锚杆终点或第一弯折点长度</param>
        /// <param name="length4">锚杆第一弯折点到锚杆终点或第二弯折点长度，默认值 0.0</param>
        /// <param name="length5">锚杆第二弯折点到锚杆终点长度，默认值 0.0</param>
        /// <param name="hookDirection">弯钩朝向，不应与锚杆控制方向平行，默认值 null</param>
        /// <param name="bendRadiusFactor">弯曲半径相对于锚杆尺寸的系数，默认值 3.5</param>
        /// <param name="material">锚杆材质，默认值 "Q235B"</param>
        /// <param name="size">锚杆尺寸，默认值 20.0</param>
        /// <param name="tolerance">孔公差，默认值 2.0</param>
        /// <param name="class">等级，默认值 "0"</param>
        /// <param name="useWasherPlate">是否使用垫板，默认值 true</param>
        /// <param name="washerPlateThickness">垫板厚度，默认值 10.0</param>
        /// <param name="washerPlateWidth">垫板宽度，默认值 70.0</param>
        /// <param name="washerPlatePosition">垫板到锚杆第一控制点的距离，默认值 20.0</param>
        /// <param name="washerPlateHoleDiameter">垫板开孔孔径，默认值 26.0</param>
        /// <param name="useWasher1">是否使用垫圈1，默认值 true</param>
        /// <param name="useWasher2">是否使用垫圈2，默认值 true</param>
        /// <param name="useWasher3">是否使用垫圈3，默认值 true</param>
        /// <param name="useNut1">是否使用螺母1，默认值 true</param>
        /// <param name="useNut2">是否使用螺母2，默认值 true</param>
        /// <param name="useNut3">是否使用螺母3，默认值 true</param>
        /// <returns>创建的锚杆。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="firstPosition"/> 和 
        /// <paramref name="secondPosition"/> 构成的锚杆控制方向为零向量，
        /// 或 <paramref name="hookDirection"/> 为零向量，
        /// 或 <paramref name="hookDirection"/> 与锚杆控制方向平行时引发。
        /// 或 <paramref name="length1"/>、<paramref name="length2"/>、<paramref name="length3"/> 
        /// <paramref name="size"/> 任一小于等于 0.0，
        /// 或 <paramref name="length4"/>、<paramref name="length5"/>、<paramref name="tolerance"/> 
        /// 任一小于 0.0，
        /// 或 当 <paramref name="useWasherPlate"/> 为 true，且 <paramref name="washerPlateThickness"/>、
        /// <paramref name="washerPlateWidth"/>、<paramref name="washerPlateHoleDiameter"/> 
        /// 任一小于等于 0.0 时引发。</exception>
        public static List<Part> CreatAnchorRod(
            Point firstPosition, Point secondPosition,
            double length1, double length2, double length3, double length4 = 0.0, double length5 = 0.0,
            Vector hookDirection = null, double bendRadiusFactor = 3.5,
            string material = "Q235B", double size = 20.0, double tolerance = 2.0, string @class = "0",
            bool useWasherPlate = true, double washerPlateThickness = 10.0, double washerPlateWidth = 70.0,
            double washerPlatePosition = 20.0, double washerPlateHoleDiameter = 26.0,
            bool useWasher1 = true, bool useWasher2 = true, bool useWasher3 = true, bool useNut1 = true,
            bool useNut2 = true, bool useNut3 = true) {
            #region 参数检查

            if (firstPosition is null) {
                throw new ArgumentNullException(nameof(firstPosition));
            }

            if (secondPosition is null) {
                throw new ArgumentNullException(nameof(secondPosition));
            }

            var anchorDirection = new Vector(secondPosition - firstPosition).GetNormal();
            if (anchorDirection.IsZero()) {
                throw new ArgumentException("锚杆控制方向不能为零向量。");
            }

            if (length4 > 0.0 || length5 > 0.0) {
                if (hookDirection is null) {
                    throw new ArgumentNullException(nameof(hookDirection));
                }

                if (hookDirection.IsZero()) {
                    throw new ArgumentException("弯钩方向不能为零向量。");
                }

                var cross = anchorDirection.Cross(hookDirection);
                if (cross.IsZero()) {
                    throw new ArgumentException("弯钩方向不能与锚杆控制方向平行。");
                }

                hookDirection = cross.Cross(anchorDirection).GetNormal();
            }

            material ??= string.Empty;

            if (length1 <= 0.0) {
                throw new ArgumentException($"“{nameof(length1)}”不应小于等于 0。", nameof(length1));
            }

            if (length2 <= 0.0) {
                throw new ArgumentException($"“{nameof(length2)}”不应小于等于 0。", nameof(length2));
            }

            if (length3 <= 0.0) {
                throw new ArgumentException($"“{nameof(length3)}”不应小于等于 0。", nameof(length3));
            }

            if (length4 < 0.0) {
                throw new ArgumentException($"“{nameof(length4)}”不应小于 0。", nameof(length4));
            }

            if (length5 < 0.0) {
                throw new ArgumentException($"“{nameof(length5)}”不应小于 0。", nameof(length5));
            }

            if (size <= 0.0) {
                throw new ArgumentException($"“{nameof(size)}”不应小于等于 0。", nameof(size));
            }

            if (tolerance < 0.0) {
                throw new ArgumentException($"“{nameof(tolerance)}”不应小于 0。", nameof(tolerance));
            }

            if (useWasherPlate) {
                if (washerPlateThickness <= 0.0) {
                    throw new ArgumentException($"“{nameof(washerPlateThickness)}”不应小于等于 0。",
                        nameof(washerPlateThickness));
                }

                if (washerPlateWidth <= 0.0) {
                    throw new ArgumentException($"“{nameof(washerPlateWidth)}”不应小于等于 0。", nameof(washerPlateWidth));
                }

                if (washerPlateHoleDiameter <= 0.0) {
                    throw new ArgumentException($"“{nameof(washerPlateHoleDiameter)}”不应小于等于 0。",
                        nameof(washerPlateHoleDiameter));
                }
            }

            if (!useWasher1 && useWasher2) {
                useWasher1 = true;
                useWasher2 = false;
            }

            if (!useNut1 && useNut2) {
                useNut1 = true;
                useNut2 = false;
            }

            #endregion

            if (length5 > 0.0) {
                if (length5 < bendRadiusFactor * size) {
                    length5 = bendRadiusFactor * size;
                }

                if (length4 < 2 * bendRadiusFactor * size) {
                    length4 = 2 * bendRadiusFactor * size;
                }
            } else {
                if (length4 < bendRadiusFactor * size) {
                    length4 = bendRadiusFactor * size;
                }
            }

            Part anchorRod = null,
                screw = null,
                washerPlate = null,
                washer1 = null,
                washer2 = null,
                washer3 = null,
                nut1 = null,
                nut2 = null,
                nut3 = null;

            var point1 = firstPosition - anchorDirection * length1;
            var point2 = firstPosition + anchorDirection * length2;
            screw = CreatBeam(point1, point2, "SCREW", $"D{size - 1}", material,
                partPrefix: "D", @class: @class);

            var point3 = point2 + anchorDirection * length3;
            Point point4 = null, point5 = null, point6 = null;
            if (length4 == 0.0) {
                anchorRod = CreatBeam(point2, point3, "ANCHORROD", $"D{size}", material,
                    partPrefix: "D", @class: @class);
            } else {
                var chamferNone = new Chamfer();
                var chamferRounding = new Chamfer {
                    Type = Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING,
                    X = bendRadiusFactor * size,
                    Y = bendRadiusFactor * size,
                };
                ContourPoint cp2, cp3, cp4, cp5;
                cp2 = new ContourPoint(point2, chamferNone);
                cp3 = new ContourPoint(point3, chamferRounding);

                point4 = point3 + hookDirection * length4;
                if (length5 == 0.0) {
                    cp4 = new ContourPoint(point4, chamferNone);

                    anchorRod = CreatPolyBeam(
                        new Contour { ContourPoints = new ArrayList { cp2, cp3, cp4 } },
                        "ANCHORROD", $"D{size}", material, partPrefix: "D", @class: @class);
                } else {
                    point5 = point4 - anchorDirection * length5;
                    cp4 = new ContourPoint(point4, chamferRounding);
                    cp5 = new ContourPoint(point5, chamferNone);

                    anchorRod = CreatPolyBeam(
                        new Contour { ContourPoints = new ArrayList { cp2, cp3, cp4, cp5 } },
                        "ANCHORROD", $"D{size}", material, partPrefix: "D", @class: @class);
                }
            }

            ApplyBooleanOperation(anchorRod, screw, BooleanPart.BooleanTypeEnum.BOOLEAN_ADD);

            if (!useWasherPlate)
                goto SkipWasherPlate;
            point1 = firstPosition - anchorDirection * washerPlatePosition + hookDirection * (washerPlateWidth * 0.5);
            point2 = point1 - hookDirection * washerPlateWidth;
            washerPlate = CreatBeam(
                point1, point2, "WASHERPLATE", $"PL{washerPlateThickness}*{washerPlateWidth}", material,
                @class: @class, depthEnum: Position.DepthEnum.FRONT);

            point1 = firstPosition - anchorDirection * washerPlatePosition;
            point2 = point1 - anchorDirection * washerPlateThickness;
            var hole = CreatBeam(
                point1, point2, "HOLE", $"D{washerPlateHoleDiameter}", "ANTIMATERIAL",
                partPrefix: "D", @class: BooleanPart.BooleanOperativeClassName);
            ApplyBooleanOperation(washerPlate, hole);
            hole.Delete();
            SkipWasherPlate:

            if (!useWasher1)
                goto SkipWasher1;
            point1 = firstPosition -
                     anchorDirection * (washerPlatePosition + (useWasherPlate ? washerPlateThickness : 0.0));
            point2 = point1 - anchorDirection * (size * 0.5);
            washer1 = CreatBeam(
                point1, point2, "WASHER", $"O{size * 2 + 6}*{size * 0.5 + 3}", material,
                partPrefix: "O", @class: @class);
            SkipWasher1:

            if (!useWasher2)
                goto SkipWasher2;
            point1 = firstPosition - anchorDirection *
                (washerPlatePosition + (useWasherPlate ? washerPlateThickness : 0.0) + size * 0.5);
            point2 = point1 - anchorDirection * (size * 0.5);
            washer2 = CreatBeam(
                point1, point2, "WASHER", $"O{size * 2 + 6}*{size * 0.5 + 3}", material,
                partPrefix: "O", @class: @class);
            SkipWasher2:

            if (!useWasher3)
                goto SkipWasher3;
            point1 = firstPosition;
            point2 = point1 + anchorDirection * (size * 0.5);
            washer3 = CreatBeam(
                point1, point2, "WASHER", $"O{size * 2 + 6}*{size * 0.5 + 3}", material,
                partPrefix: "O", @class: @class);
            SkipWasher3:

            var matrix = MatrixFactoryExtension.Rotate(new Line(firstPosition, secondPosition), Math.PI / 3);
            if (!useNut1)
                goto SkipNut1;
            point1 = firstPosition - anchorDirection *
                (washerPlatePosition + (useWasherPlate ? washerPlateThickness : 0.0) +
                 (useWasher1 ? (useWasher2 ? size : size * 0.5) : 0.0)) + hookDirection * size;
            point2 = matrix.Transform(point1);
            point3 = matrix.Transform(point2);
            point4 = matrix.Transform(point3);
            point5 = matrix.Transform(point4);
            point6 = matrix.Transform(point5);
            nut1 = CreatContourPlate(
                [point1, point2, point3, point4, point5, point6], "NUT", $"PL{size}", material,
                @class: @class, depthEnum: Position.DepthEnum.FRONT);
            SkipNut1:

            if (!useNut2)
                goto SkipNut2;
            point1 = firstPosition - anchorDirection *
                (washerPlatePosition + (useWasherPlate ? washerPlateThickness : 0.0) +
                 (useWasher1 ? (useWasher2 ? size : size * 0.5) : 0.0) +
                 (useNut1 ? size : 0.0)) + hookDirection * size;
            point2 = matrix.Transform(point1);
            point3 = matrix.Transform(point2);
            point4 = matrix.Transform(point3);
            point5 = matrix.Transform(point4);
            point6 = matrix.Transform(point5);
            nut2 = CreatContourPlate(
                [point1, point2, point3, point4, point5, point6], "NUT", $"PL{size}", material,
                @class: @class, depthEnum: Position.DepthEnum.FRONT);
            SkipNut2:

            if (!useNut3)
                goto SkipNut3;
            point1 = firstPosition + anchorDirection * (useWasher3 ? size * 0.5 : 0.0) + hookDirection * size;
            point2 = matrix.Transform(point1);
            point3 = matrix.Transform(point2);
            point4 = matrix.Transform(point3);
            point5 = matrix.Transform(point4);
            point6 = matrix.Transform(point5);
            nut3 = CreatContourPlate(
                [point1, point2, point3, point4, point5, point6], "NUT", $"PL{size}", material,
                @class: @class, depthEnum: Position.DepthEnum.BEHIND);
            SkipNut3:

            var parts = new List<Part> { anchorRod };
            if (washerPlate != null) parts.Add(washerPlate);

            hole = CreatBeam(
                firstPosition + anchorDirection * (size * 1.5),
                firstPosition - anchorDirection * (washerPlatePosition + washerPlateThickness + size * 3),
                "HOLE", $"D{size}", "ANTIMATERIAL", @class: BooleanPart.BooleanOperativeClassName);
            if (washer1 != null) {
                ApplyBooleanOperation(washer1, hole);
                parts.Add(washer1);
            }

            if (washer2 != null) {
                ApplyBooleanOperation(washer2, hole);
                parts.Add(washer2);
            }

            if (washer3 != null) {
                ApplyBooleanOperation(washer3, hole);
                parts.Add(washer3);
            }

            if (nut1 != null) {
                ApplyBooleanOperation(nut1, hole);
                parts.Add(nut1);
            }

            if (nut2 != null) {
                ApplyBooleanOperation(nut2, hole);
                parts.Add(nut2);
            }

            if (nut3 != null) {
                ApplyBooleanOperation(nut3, hole);
                parts.Add(nut3);
            }

            hole.Delete();

            return parts;
        }

        /// <summary>
        /// 求零件实体(<seealso cref="Solid"/>)与加劲板前后表面所在的平面的交集。
        /// </summary>
        /// <remarks>
        /// 返回的数组有两个元素，第一个元素是加劲板前表面所在的平面与零件实体的交集，第二个元素是加劲板后表面所在的平面与零件实体的交集。
        /// 以在零件坐标系X轴上相对方向区分前后，正向为前，负向为后。
        /// 以 <see cref="Solid.IntersectAllFaces(Point, Point, Point)"/> 方法计算交集。
        /// </remarks>
        /// <param name="part"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[1]"/>
        /// </param>
        /// <param name="position"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[2]"/>
        /// </param>
        /// <param name="thickness"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[3]"/>
        /// </param>
        /// <param name="partCS">加劲板所处位置处映射的零件坐标系</param>
        /// <param name="stifPlane">加劲板中心平面</param>
        /// <param name="rotationAroundY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[6]"/>
        /// </param>
        /// <param name="rotationAroundZ"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[7]"/>
        /// </param>
        /// <returns>
        /// 零件实体与加劲板前后表面所在的平面的交集。
        /// 使用方法参考 <see cref="Solid.IntersectAllFaces(Point, Point, Point)"/> 方法的官方示例文档。
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="rotationAroundY"/> 或 
        /// <paramref name="rotationAroundZ"/> 超出角度制 -85~85 度时引发。</exception>
        /// <exception cref="ArgumentException"><paramref name="part"/> 
        /// 不是 <see cref="Beam"/> 或 <see cref="PolyBeam"/> 时引发。</exception>
        private static IEnumerator[] IntersectionWithStiffenerSurfacePlane(
            Part part, Point position, double thickness, out CoordinateSystem partCS, out GeometricPlane stifPlane,
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            const double radiansOf85 = 85.0 / 180.0 * Math.PI;
            if (Math.Abs(rotationAroundY) > radiansOf85) {
                throw new ArgumentOutOfRangeException(
                    $"Rotation angle \"{nameof(rotationAroundY)}\" out of range, only supports in range of -85~85 degrees.",
                    nameof(rotationAroundY));
            }

            if (Math.Abs(rotationAroundZ) > radiansOf85) {
                throw new ArgumentOutOfRangeException(
                    $"Rotation angle \"{nameof(rotationAroundY)}\" out of range, only supports in range of -85~85 degrees.",
                    nameof(rotationAroundZ));
            }

            var centerLine = part.GetCenterLine(false).Cast<Point>().ToList();
            var segLines = centerLine.Take(centerLine.Count() - 1)
                .Zip(centerLine.Skip(1), (p1, p2) => new Line(p1, p2));
            var nearestSegLine = segLines
                .OrderBy(line => Tekla.Structures.Geometry3d.Distance.PointToLine(position, line)).First();

            CoordinateSystem coordinateSystem;
            switch (part) {
            case Beam beam:
                coordinateSystem = beam.GetCoordinateSystem();
                break;
            case PolyBeam polyBeam: {
                var css = polyBeam.GetPolybeamCoordinateSystems().Cast<CoordinateSystem>();
                coordinateSystem = css.First(cs => nearestSegLine.Origin.Equals(cs.Origin));
                break;
            }
            default:
                throw new ArgumentException($"\"{nameof(part)}\" is neither \"Beam\" nor \"PolyBeam\".", nameof(part));
            }

            partCS = new CoordinateSystem(Projection.PointToLine(position, nearestSegLine),
                coordinateSystem.AxisX, coordinateSystem.AxisY);

            var axisY = new Vector(0, 1000, 0);
            var axisZ = new Vector(0, 0, 1000);
            var matrixRotationY = MatrixFactory.Rotate(-rotationAroundY, axisY);
            var matrixRotationZ = MatrixFactory.Rotate(-rotationAroundZ, axisZ);
            var matrix = matrixRotationZ * matrixRotationY;

            var planeAxisX = MatrixExtension.Transform(matrix, -1 * axisZ).TransformFrom(partCS);
            var planeAxisY = MatrixExtension.Transform(matrix, axisY).TransformFrom(partCS);
            var planeNormal = planeAxisX.Cross(planeAxisY).GetNormal();
            stifPlane = new GeometricPlane(partCS.Origin, planeNormal);

            var offsetVector = planeNormal * (thickness * 0.5);

            var planeFrontOrigin = partCS.Origin + offsetVector;
            var planeFrontPointX = planeFrontOrigin + planeAxisX;
            var planeFrontPointY = planeFrontOrigin + planeAxisY;

            var planeBehindOrigin = partCS.Origin - offsetVector;
            var planeBehindPointX = planeBehindOrigin + planeAxisX;
            var planeBehindPointY = planeBehindOrigin + planeAxisY;

            //  Solid.SolidCreationTypeEnum.RAW 基本轮廓
            //  Solid.SolidCreationTypeEnum.HIGH_ACCURACY 高精度轮廓
            var solid = part.GetSolid(Solid.SolidCreationTypeEnum.RAW);
            var faceEnumFront = solid.IntersectAllFaces(planeFrontOrigin, planeFrontPointX, planeFrontPointY);
            var faceEnumBehind = solid.IntersectAllFaces(planeBehindOrigin, planeBehindPointX, planeBehindPointY);

            return [faceEnumFront, faceEnumBehind];
        }

        /// <summary>
        /// 将集合中的顶点按规则排序。
        /// </summary>
        /// <remarks>
        /// 根据各顶点在零件坐标系 <paramref name="partCS"/> 中的坐标值（不改变当前坐标值）进行排序，
        /// 先按 Z 坐标从小到大排序，再按 Y 坐标从大到小排序，以序列第一点为起点，
        /// 其余点绕 X 轴按右手螺旋法则依次排序。
        /// </remarks>
        /// <param name="vertices">原顶点集合</param>
        /// <param name="partCS">零件坐标系</param>
        /// <returns>排序后的顶点集合。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static IEnumerable<Point> OrderVertices(IEnumerable<Point> vertices, CoordinateSystem partCS) {
            if (vertices == null) {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (partCS == null) {
                throw new ArgumentNullException(nameof(partCS));
            }

            var vertexList = vertices as IList<Point> ?? [.. vertices];

            var cnt = vertexList.Count();

            var transformedVertices = vertexList.Select(p => p.TransformTo(partCS));
            var firstIndex = transformedVertices
                .Select((p, i) => (p, i))
                .OrderBy(item => item.p.Z)
                .ThenByDescending(item => item.p.Y)
                .First()
                .i;
            var preIndex = firstIndex == 0 ? cnt - 1 : firstIndex - 1;
            var nxtIndex = firstIndex == cnt - 1 ? 0 : firstIndex + 1;
            var reverse = partCS.AxisX.Dot(
                new Vector(vertexList.ElementAt(nxtIndex) - vertexList.ElementAt(firstIndex))
                    .Cross(new Vector(vertexList.ElementAt(preIndex) - vertexList.ElementAt(firstIndex)))) < 0;

            if (reverse) {
                vertexList = vertexList.Reverse().ToList();
                vertexList = vertexList.Skip(cnt - 1 - firstIndex)
                    .Concat(vertexList.Take(cnt - 1 - firstIndex)).ToList();
            } else {
                vertexList = vertexList.Skip(firstIndex).Concat(vertexList.Take(firstIndex)).ToList();
            }

            return vertexList;
        }

        /// <summary>
        /// 为 H 型钢创建加劲板。
        /// </summary>
        /// <param name="part"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[1]"/></param>
        /// <param name="position"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[2]"/></param>
        /// <param name="thickness"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[3]"/></param>
        /// <param name="material"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[4]"/></param>
        /// <param name="class"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[5]"/></param>
        /// <param name="rotationAroundY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[6]"/></param>
        /// <param name="rotationAroundZ"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[7]"/></param>
        /// <param name="indent"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[8]"/></param>
        /// <param name="clearance"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[10]"/></param>
        /// <param name="chamferType"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[11]"/></param>
        /// <param name="chamferSizeX"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[12]"/></param>
        /// <param name="chamferSizeY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[13]"/></param>
        /// <param name="chamferDz1"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[14]"/></param>
        /// <param name="chamferDz2"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[15]"/></param>
        /// <returns>成功创建的加劲板集合，
        /// 第一个元素在零件坐标系 Z 轴正向侧，第二个元素在零件坐标系 Z 轴负向侧。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        private static IEnumerable<ContourPlate> CreatStiffenersForTypeI(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double indent = 0.0, double clearance = 2.0,
            Chamfer.ChamferTypeEnum chamferType = 0, double chamferSizeX = 0.0, double chamferSizeY = 0.0,
            double chamferDz1 = 0.0, double chamferDz2 = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var faceEnumArr = IntersectionWithStiffenerSurfacePlane(
                part, position, thickness, out var partCS, out var stifPlane, rotationAroundY,
                rotationAroundZ);
            var faceEnumFront = faceEnumArr[0];
            var faceEnumBehind = faceEnumArr[1];

            var verticesFront = GetVertices(faceEnumFront);
            var verticesBehind = GetVertices(faceEnumBehind);
            verticesFront = OrderVertices(verticesFront, partCS).ToList();
            verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
            verticesFront = verticesFront.Skip(2).Take(4).Concat(verticesFront.Skip(8)).ToList();
            verticesBehind = verticesBehind.Skip(2).Take(4).Concat(verticesBehind.Skip(8)).ToList();

            var offsetZIndent = new Vector(0, 0, indent).TransformFrom(partCS);
            var offsetZClearance = new Vector(0, 0, clearance).TransformFrom(partCS);
            var offsetYClearance = new Vector(0, clearance, 0).TransformFrom(partCS);
            var arrList = new List<Point[]> { verticesFront.ToArray(), verticesBehind.ToArray() };
            foreach (var arr in arrList) {
                arr[0] -= offsetZIndent;
                arr[3] -= offsetZIndent;
                arr[4] += offsetZIndent;
                arr[7] += offsetZIndent;
                arr[1] += offsetZClearance;
                arr[2] += offsetZClearance;
                arr[5] -= offsetZClearance;
                arr[6] -= offsetZClearance;
                arr[0] -= offsetYClearance;
                arr[1] -= offsetYClearance;
                arr[2] += offsetYClearance;
                arr[3] += offsetYClearance;
                arr[4] += offsetYClearance;
                arr[5] += offsetYClearance;
                arr[6] -= offsetYClearance;
                arr[7] -= offsetYClearance;
            }

            var lines = arrList[0].Zip(arrList[1], (p1, p2) => new Line(p1, p2)).ToList();
            var stifFrontPlane = new GeometricPlane(stifPlane.Origin + stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);
            var stifBehindPlane = new GeometricPlane(stifPlane.Origin - stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);

            var partZXPlane = new GeometricPlane(partCS.Origin, partCS.AxisY);
            var partXYPlane = new GeometricPlane(partCS.Origin, partCS.AxisX.Cross(partCS.AxisY));
            var shearAxisX = Intersection.PlaneToPlane(partZXPlane, stifPlane).Direction;
            var shearAxisY = Intersection.PlaneToPlane(partXYPlane, stifPlane).Direction;
            var shearAxisZ = shearAxisX.Cross(shearAxisY);
            var matrixToStif =
                MatrixFactoryExtension.ToCoordinateSystem(shearAxisX, shearAxisY, shearAxisZ, stifPlane.Origin);
            var matrixFromStif = matrixToStif.Inverse();

            verticesFront = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifFrontPlane)))
                .ToList();
            verticesBehind = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifBehindPlane)))
                .ToList();
            Point[] corners1 = [
                TopLeft(verticesFront.ElementAt(0), verticesBehind.ElementAt(0)),
                TopRight(verticesFront.ElementAt(1), verticesBehind.ElementAt(1)),
                BottomRight(verticesFront.ElementAt(2), verticesBehind.ElementAt(2)),
                BottomLeft(verticesFront.ElementAt(3), verticesBehind.ElementAt(3))
            ];
            Point[] corners2 = [
                BottomRight(verticesFront.ElementAt(4), verticesBehind.ElementAt(4)),
                BottomLeft(verticesFront.ElementAt(5), verticesBehind.ElementAt(5)),
                TopLeft(verticesFront.ElementAt(6), verticesBehind.ElementAt(6)),
                TopRight(verticesFront.ElementAt(7), verticesBehind.ElementAt(7))
            ];
            for (int i = 0; i < 4; i++) {
                corners1[i] = matrixFromStif.Transform(corners1[i]);
                corners2[i] = matrixFromStif.Transform(corners2[i]);
            }

            var contourPlates = new ContourPlate[2];
            contourPlates[0] = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(corners1[0], new Chamfer()),
                    new ContourPoint(
                        corners1[1],
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners1[2],
                        new Chamfer(chamferSizeY, chamferSizeX, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(corners1[3], new Chamfer())
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);
            contourPlates[1] = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(corners2[3], new Chamfer()),
                    new ContourPoint(
                        corners2[2], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners2[1], 
                        new Chamfer(chamferSizeY, chamferSizeX, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(corners2[0], new Chamfer())
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);

            return contourPlates;


            static Point BottomRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point BottomLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point TopLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);
            static Point TopRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);

            static List<Point> GetVertices(IEnumerator faceEnum) {
                var vertices = new List<Point>();

                var faceCnt = 0;
                while (faceEnum.MoveNext()) {
                    if (++faceCnt > 1) throw new Exception(UnknownSectionType);

                    var face = faceEnum.Current as ArrayList;
                    var loopEnum = face!.GetEnumerator();

                    var loopCnt = 0;
                    while (loopEnum.MoveNext()) {
                        if (++loopCnt > 1) throw new Exception(UnknownSectionType);

                        var loop = loopEnum.Current as ArrayList;
                        var vertexEnum = loop!.GetEnumerator();

                        var verticesCnt = 0;
                        while (vertexEnum.MoveNext()) {
                            ++verticesCnt;

                            var vertex = vertexEnum.Current as Point;
                            vertices.Add(vertex);
                        }

                        (vertexEnum as IDisposable)?.Dispose();

                        //  使用 RAW 选项是 12 个顶点，使用 HIGH_ACCURACY 选项对于不同类型的截面有不同数量的顶点
                        if (verticesCnt != 12) throw new Exception(UnknownSectionType);
                    }

                    (loopEnum as IDisposable)?.Dispose();
                }

                return vertices;
            }
        }

        /// <summary>
        /// 为 T 型钢创建加劲板。
        /// </summary>
        /// <param name="part"></param>
        /// <param name="position"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <param name="class"></param>
        /// <param name="rotationAroundY"></param>
        /// <param name="rotationAroundZ"></param>
        /// <param name="indent"></param>
        /// <param name="indent2"></param>
        /// <param name="clearance"></param>
        /// <param name="chamferType"></param>
        /// <param name="chamferSizeX"></param>
        /// <param name="chamferSizeY"></param>
        /// <param name="chamferDz1"></param>
        /// <param name="chamferDz2"></param>
        /// <returns>成功创建的加劲板集合，
        /// 第一个元素在零件坐标系 Z 轴正向侧，第二个元素在零件坐标系 Z 轴负向侧。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        private static IEnumerable<ContourPlate> CreatStiffenersForTypeT(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double indent = 0.0, double indent2 = 0.0,
            double clearance = 2.0,
            Chamfer.ChamferTypeEnum chamferType = 0, double chamferSizeX = 0.0, double chamferSizeY = 0.0,
            double chamferDz1 = 0.0, double chamferDz2 = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var faceEnumArr = IntersectionWithStiffenerSurfacePlane(
                part, position, thickness, out var partCS, out var stifPlane, rotationAroundY,
                rotationAroundZ);
            var faceEnumFront = faceEnumArr[0];
            var faceEnumBehind = faceEnumArr[1];

            var verticesFront = GetVertices(faceEnumFront);
            var verticesBehind = GetVertices(faceEnumBehind);
            verticesFront = OrderVertices(verticesFront, partCS).ToList();
            verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
            verticesFront = verticesFront.Skip(2).ToList();
            verticesBehind = verticesBehind.Skip(2).ToList();

            var offsetZIndent = new Vector(0, 0, indent).TransformFrom(partCS);
            var offsetZClearance = new Vector(0, 0, clearance).TransformFrom(partCS);
            var offsetYIndent2 = new Vector(0, indent2, 0).TransformFrom(partCS);
            var offsetYClearance = new Vector(0, clearance, 0).TransformFrom(partCS);
            var arrList = new List<Point[]> { verticesFront.ToArray(), verticesBehind.ToArray() };
            foreach (var arr in arrList) {
                arr[0] -= offsetZIndent;
                arr[5] += offsetZIndent;
                arr[2] += offsetYIndent2;
                arr[3] += offsetYIndent2;
                arr[1] += offsetZClearance;
                arr[2] += offsetZClearance;
                arr[3] -= offsetZClearance;
                arr[4] -= offsetZClearance;
                arr[0] -= offsetYClearance;
                arr[1] -= offsetYClearance;
                arr[4] -= offsetYClearance;
                arr[5] -= offsetYClearance;
            }

            var lines = arrList[0].Zip(arrList[1], (p1, p2) => new Line(p1, p2)).ToList();
            var stifFrontPlane = new GeometricPlane(stifPlane.Origin + stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);
            var stifBehindPlane = new GeometricPlane(stifPlane.Origin - stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);

            var partZXPlane = new GeometricPlane(partCS.Origin, partCS.AxisY);
            var partXYPlane = new GeometricPlane(partCS.Origin, partCS.AxisX.Cross(partCS.AxisY));
            var shearAxisX = Intersection.PlaneToPlane(partZXPlane, stifPlane).Direction;
            var shearAxisY = Intersection.PlaneToPlane(partXYPlane, stifPlane).Direction;
            var shearAxisZ = shearAxisX.Cross(shearAxisY);
            var matrixToStif =
                MatrixFactoryExtension.ToCoordinateSystem(shearAxisX, shearAxisY, shearAxisZ, stifPlane.Origin);
            var matrixFromStif = matrixToStif.Inverse();

            verticesFront = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifFrontPlane)))
                .ToList();
            verticesBehind = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifBehindPlane)))
                .ToList();

            var corners1 = new[] {
                TopLeft(verticesFront.ElementAt(0), verticesBehind.ElementAt(0)),
                TopRight(verticesFront.ElementAt(1), verticesBehind.ElementAt(1)),
                BottomRight(verticesFront.ElementAt(2), verticesBehind.ElementAt(2)), new()
            };
            var corners2 = new[] {
                new(), BottomLeft(verticesFront.ElementAt(3), verticesBehind.ElementAt(3)),
                TopLeft(verticesFront.ElementAt(4), verticesBehind.ElementAt(4)),
                TopRight(verticesFront.ElementAt(5), verticesBehind.ElementAt(5))
            };
            for (int i = 0; i < 4; i++) {
                corners1[i] = matrixFromStif.Transform(corners1[i]);
                corners2[i] = matrixFromStif.Transform(corners2[i]);
            }

            corners1[3] = corners1[0] + corners1[2] - corners1[1];
            corners2[0] = corners2[1] + corners2[3] - corners2[2];

            var contourPlates = new ContourPlate[2];
            contourPlates[0] = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(corners1[0], new Chamfer()),
                    new ContourPoint(
                        corners1[1], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(corners1[2], new Chamfer()),
                    new ContourPoint(corners1[3], new Chamfer())
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);
            contourPlates[1] = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(corners2[3], new Chamfer()),
                    new ContourPoint(
                        corners2[2], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(corners2[1], new Chamfer()),
                    new ContourPoint(corners2[0], new Chamfer())
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);

            return contourPlates;

            static Point BottomRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point BottomLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point TopLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);
            static Point TopRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);

            static List<Point> GetVertices(IEnumerator faceEnum) {
                var vertices = new List<Point>();

                var faceCnt = 0;
                while (faceEnum.MoveNext()) {
                    if (++faceCnt > 1) throw new Exception(UnknownSectionType);

                    var face = faceEnum.Current as ArrayList;
                    var loopEnum = face!.GetEnumerator();

                    var loopCnt = 0;
                    while (loopEnum.MoveNext()) {
                        if (++loopCnt > 1) throw new Exception(UnknownSectionType);

                        var loop = loopEnum.Current as ArrayList;
                        var vertexEnum = loop!.GetEnumerator();

                        var verticesCnt = 0;
                        while (vertexEnum.MoveNext()) {
                            ++verticesCnt;

                            var vertex = vertexEnum.Current as Point;
                            vertices.Add(vertex);
                        }

                        (vertexEnum as IDisposable)?.Dispose();

                        //  使用 RAW 选项是 8 个顶点，
                        //  使用 HIGH_ACCURACY 选项，对于 T, TN, TM, TW 类型的截面是 16 个顶点，
                        //  对于 B_WLD_E 类型的截面是 8 个顶点
                        if (verticesCnt != 8) throw new Exception(UnknownSectionType);
                    }

                    (loopEnum as IDisposable)?.Dispose();
                }

                return vertices;
            }
        }

        /// <summary>
        /// 为槽钢创建加劲板。
        /// </summary>
        /// <param name="part"></param>
        /// <param name="position"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <param name="class"></param>
        /// <param name="rotationAroundY"></param>
        /// <param name="rotationAroundZ"></param>
        /// <param name="indent"></param>
        /// <param name="clearance"></param>
        /// <param name="chamferType"></param>
        /// <param name="chamferSizeX"></param>
        /// <param name="chamferSizeY"></param>
        /// <param name="chamferDz1"></param>
        /// <param name="chamferDz2"></param>
        /// <returns>成功创建的加劲板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        private static ContourPlate CreatStiffenersForTypeU(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double indent = 0.0, double clearance = 2.0,
            Chamfer.ChamferTypeEnum chamferType = 0, double chamferSizeX = 0.0, double chamferSizeY = 0.0,
            double chamferDz1 = 0.0, double chamferDz2 = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var faceEnumArr = IntersectionWithStiffenerSurfacePlane(
                part, position, thickness, out var partCS, out var stifPlane, rotationAroundY,
                rotationAroundZ);
            var faceEnumFront = faceEnumArr[0];
            var faceEnumBehind = faceEnumArr[1];

            var verticesFront = GetVertices(faceEnumFront);
            var verticesBehind = GetVertices(faceEnumBehind);
            verticesFront = OrderVertices(verticesFront, partCS).ToList();
            verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
            verticesFront = verticesFront.Skip(2).Take(4).ToList();
            verticesBehind = verticesBehind.Skip(2).Take(4).ToList();

            var offsetZIndent = new Vector(0, 0, indent).TransformFrom(partCS);
            var offsetZClearance = new Vector(0, 0, clearance).TransformFrom(partCS);
            var offsetYClearance = new Vector(0, clearance, 0).TransformFrom(partCS);
            var arrList = new List<Point[]> { verticesFront.ToArray(), verticesBehind.ToArray() };
            foreach (var arr in arrList) {
                arr[0] -= offsetZIndent;
                arr[3] -= offsetZIndent;
                arr[1] += offsetZClearance;
                arr[2] += offsetZClearance;
                arr[0] -= offsetYClearance;
                arr[1] -= offsetYClearance;
                arr[2] += offsetYClearance;
                arr[3] += offsetYClearance;
            }

            var lines = arrList[0].Zip(arrList[1], (p1, p2) => new Line(p1, p2)).ToList();
            var stifFrontPlane = new GeometricPlane(stifPlane.Origin + stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);
            var stifBehindPlane = new GeometricPlane(stifPlane.Origin - stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);

            var partZXPlane = new GeometricPlane(partCS.Origin, partCS.AxisY);
            var partXYPlane = new GeometricPlane(partCS.Origin, partCS.AxisX.Cross(partCS.AxisY));
            var shearAxisX = Intersection.PlaneToPlane(partZXPlane, stifPlane).Direction;
            var shearAxisY = Intersection.PlaneToPlane(partXYPlane, stifPlane).Direction;
            var shearAxisZ = shearAxisX.Cross(shearAxisY);
            var matrixToStif =
                MatrixFactoryExtension.ToCoordinateSystem(shearAxisX, shearAxisY, shearAxisZ, stifPlane.Origin);
            var matrixFromStif = matrixToStif.Inverse();

            verticesFront = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifFrontPlane)))
                .ToList();
            verticesBehind = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifBehindPlane)))
                .ToList();

            var corners = new[] {
                TopLeft(verticesFront.ElementAt(0), verticesBehind.ElementAt(0)),
                TopRight(verticesFront.ElementAt(1), verticesBehind.ElementAt(1)),
                BottomRight(verticesFront.ElementAt(2), verticesBehind.ElementAt(2)),
                BottomLeft(verticesFront.ElementAt(3), verticesBehind.ElementAt(3)),
            };
            for (int i = 0; i < 4; i++) {
                corners[i] = matrixFromStif.Transform(corners[i]);
            }

            var contourPlate = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(corners[0], new Chamfer()),
                    new ContourPoint(
                        corners[1], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners[2], 
                        new Chamfer(chamferSizeY, chamferSizeX, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(corners[3], new Chamfer())
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);

            return contourPlate;

            static Point BottomRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point BottomLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point TopLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);
            static Point TopRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);

            static List<Point> GetVertices(IEnumerator faceEnum) {
                var vertices = new List<Point>();

                var faceCnt = 0;
                while (faceEnum.MoveNext()) {
                    if (++faceCnt > 1) throw new Exception(UnknownSectionType);

                    var face = faceEnum.Current as ArrayList;
                    var loopEnum = face!.GetEnumerator();

                    var loopCnt = 0;
                    while (loopEnum.MoveNext()) {
                        if (++loopCnt > 1) throw new Exception(UnknownSectionType);

                        var loop = loopEnum.Current as ArrayList;
                        var vertexEnum = loop!.GetEnumerator();

                        var verticesCnt = 0;
                        while (vertexEnum.MoveNext()) {
                            ++verticesCnt;

                            var vertex = vertexEnum.Current as Point;
                            vertices.Add(vertex);
                        }

                        (vertexEnum as IDisposable)?.Dispose();

                        //  使用 RAW 选项是 8 个顶点；
                        //  使用 HIGH_ACCURACY 选项，
                        //  对于 C 前缀单参数（如 C22A ）截面是 24 个顶点，
                        //  对于 C 前缀三参数（如 C200*100*5 ）、BLC、BLU、U 前缀截面是 16 个顶点，
                        //  对于 B_WLD_D、C_BUILT、C_VAR_A、C_VAR_B、C_VAR_C、C_VAR_D 前缀截面是 8 个顶点
                        if (verticesCnt != 8) throw new Exception(UnknownSectionType);
                    }

                    (loopEnum as IDisposable)?.Dispose();
                }

                return vertices;
            }
        }

        /// <summary>
        /// 为箱型截面创建加劲板。
        /// </summary>
        /// <param name="part"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[1]"/></param>
        /// <param name="position"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[2]"/></param>
        /// <param name="thickness"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[3]"/></param>
        /// <param name="material"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[4"/></param>
        /// <param name="class"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[5]"/></param>
        /// <param name="rotationAroundY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[6]"/></param>
        /// <param name="rotationAroundZ"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[7]"/></param>
        /// <param name="clearance"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[10]"/></param>
        /// <param name="chamferType"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[11]"/></param>
        /// <param name="chamferSizeX"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[12]"/></param>
        /// <param name="chamferSizeY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[13]"/></param>
        /// <param name="chamferDz1"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[14]"/></param>
        /// <param name="chamferDz2"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[15]"/></param>
        /// <returns>成功创建的加劲板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        private static ContourPlate CreatStiffenersForTypeM(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double clearance = 2.0,
            Chamfer.ChamferTypeEnum chamferType = 0, double chamferSizeX = 0.0, double chamferSizeY = 0.0,
            double chamferDz1 = 0.0, double chamferDz2 = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var faceEnumArr = IntersectionWithStiffenerSurfacePlane(
                part, position, thickness, out var partCS, out var stifPlane, rotationAroundY,
                rotationAroundZ);
            var faceEnumFront = faceEnumArr[0];
            var faceEnumBehind = faceEnumArr[1];

            var verticesFront = GetVertices(faceEnumFront);
            var verticesBehind = GetVertices(faceEnumBehind);
            verticesFront = OrderVertices(verticesFront, partCS).ToList();
            verticesBehind = OrderVertices(verticesBehind, partCS).ToList();

            /*
             *  顶点顺序与调用 IntersectAllFaces 方法时传入的 3 个点顺序有关
             *  ========================================================================================
             *  RAW 选项：
             *  ----------------------------------------------------------------------------------------
             *  F, TUB, CFRHS, P, RHS, SHS, [], B_WLD_J 前缀截面，内外 2 个 Loop，每个 Loop 各 4 个顶点
             *  B_WLD_F 前缀截面，有些情况是内外 2 个 Loop，每个 Loop 各 4 个顶点，有些情况是 1 个 Loop，10 个顶点
             *      1------------------------2
             *      | 4--------------------3  8
             *  	| |       Y .           7 |
             *      | |        /|\          | |
             *      | |         |           | |
             *      | |         ---->       | |
             *      | |             Z       | |
             *      | |                     | |
             *      | 5---------------------6 |
             *      0-------------------------9
             *  B_VAR_A, B_VAR_B, B_VAR_C 前缀截面，1 个 Loop， 10 个顶点
             *      1-------------------------2
             *      | 8---------------------7 |
             *  	| |       Y .           | |
             *      | |        /|\          | |
             *      | |         |           | |
             *      | |         ---->       | |
             *      | |             Z       | |
             *      | |                     | |
             *      0 9 5-------------------6 |
             *      4-------------------------3
             *  B_BUILT 前缀截面，1 个 Loop，22 个顶点
             *      1-------------------------2
             *     22-2116---------------15 4-3
             *     19-20|     Y .         | 5-6
             *      |   |      /|\        |   |
             *      |   |       |         |   |
             *      |   |       ---->     |   |
             *      |   |           Z     |   |
             *     18---17                | 8-7
             *     13--------------------14 9-10
             *     12-------------------------11
             *
             *  ========================================================================================
             *  HIGH_ACCURACY 选项：
             *  ----------------------------------------------------------------------------------------
             *  B_WLD_J 前缀截面，内外 2 个 Loop，每个 Loop 各 4 个顶点
             *  F, TUB, CFRHS, P, RHS, SHS, [] 前缀截面，内外 2 个 Loop，每个 Loop 各 20 个顶点
             *  B_WLD_F 前缀截面，同 RAW 选项
             *  B_VAR_A, B_VAR_B, B_VAR_C 前缀截面，同 RAW 选项
             *  B_BUILT 前缀截面，同 RAW 选项
             */

            var prefix = GetProfilePrefix(part.Profile.ProfileString);
            switch (prefix) {
            case "B_WLD_F":
                if (verticesFront.Count > 4)
                    verticesFront = verticesFront.Skip(3).Take(3)
                        .Append(verticesFront.ElementAt(3) + verticesFront.ElementAt(5) - verticesFront.ElementAt(4))
                        .ToList();
                if (verticesBehind.Count > 4)
                    verticesBehind = verticesBehind.Skip(3).Take(3)
                        .Append(verticesBehind.ElementAt(3) + verticesBehind.ElementAt(5) - verticesBehind.ElementAt(4))
                        .ToList();
                verticesFront = OrderVertices(verticesFront, partCS).ToList();
                verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
                break;
            case "B_VAR_A":
            case "B_VAR_B":
            case "B_VAR_C":
                verticesFront = verticesFront.Skip(5).Take(4).ToList();
                verticesBehind = verticesBehind.Skip(5).Take(4).ToList();
                verticesFront = OrderVertices(verticesFront, partCS).ToList();
                verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
                break;
            case "B_BUILT":
                verticesFront = verticesFront.Skip(13).Take(3)
                    .Append(verticesFront.ElementAt(13) + verticesFront.ElementAt(15) - verticesFront.ElementAt(14))
                    .ToList();
                verticesBehind = verticesBehind.Skip(13).Take(3)
                    .Append(verticesBehind.ElementAt(13) + verticesBehind.ElementAt(15) - verticesBehind.ElementAt(14))
                    .ToList();
                verticesFront = OrderVertices(verticesFront, partCS).ToList();
                verticesBehind = OrderVertices(verticesBehind, partCS).ToList();
                break;
            }

            var offsetZ = new Vector(0, 0, clearance).TransformFrom(partCS);
            var offsetY = new Vector(0, clearance, 0).TransformFrom(partCS);
            var arrList = new List<Point[]> { verticesFront.ToArray(), verticesBehind.ToArray() };
            foreach (var arr in arrList) {
                arr[0] += offsetZ;
                arr[1] -= offsetZ;
                arr[2] -= offsetZ;
                arr[3] += offsetZ;
                arr[0] -= offsetY;
                arr[1] -= offsetY;
                arr[2] += offsetY;
                arr[3] += offsetY;
            }

            var lines = arrList[0].Zip(arrList[1], (p1, p2) => new Line(p1, p2)).ToList();
            var stifFrontPlane = new GeometricPlane(stifPlane.Origin + stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);
            var stifBehindPlane = new GeometricPlane(stifPlane.Origin - stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);

            var partZXPlane = new GeometricPlane(partCS.Origin, partCS.AxisY);
            var partXYPlane = new GeometricPlane(partCS.Origin, partCS.AxisX.Cross(partCS.AxisY));
            var shearAxisX = Intersection.PlaneToPlane(partZXPlane, stifPlane).Direction;
            var shearAxisY = Intersection.PlaneToPlane(partXYPlane, stifPlane).Direction;
            var shearAxisZ = shearAxisX.Cross(shearAxisY);
            var matrixToStif =
                MatrixFactoryExtension.ToCoordinateSystem(shearAxisX, shearAxisY, shearAxisZ, stifPlane.Origin);
            var matrixFromStif = matrixToStif.Inverse();

            verticesFront = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifFrontPlane)))
                .ToList();
            verticesBehind = lines.Select(l => matrixToStif.Transform(Intersection.LineToPlane(l, stifBehindPlane)))
                .ToList();

            var corners = new[] {
                TopRight(verticesFront.ElementAt(0), verticesBehind.ElementAt(0)),
                TopLeft(verticesFront.ElementAt(1), verticesBehind.ElementAt(1)),
                BottomLeft(verticesFront.ElementAt(2), verticesBehind.ElementAt(2)),
                BottomRight(verticesFront.ElementAt(3), verticesBehind.ElementAt(3)),
            };
            for (int i = 0; i < 4; i++) {
                corners[i] = matrixFromStif.Transform(corners[i]);
            }

            var contourPlate = CreatContourPlate(
                new ArrayList {
                    new ContourPoint(
                        corners[0], 
                        new Chamfer(chamferSizeY, chamferSizeX, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners[1], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners[2], 
                        new Chamfer(chamferSizeY, chamferSizeX, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2}),
                    new ContourPoint(
                        corners[3], 
                        new Chamfer(chamferSizeX, chamferSizeY, chamferType){DZ1 = chamferDz1, DZ2 = chamferDz2})
                }, "STIFFENER", $"PL{thickness}", material, @class: @class);

            return contourPlate;

            static Point BottomRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point BottomLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0.0);
            static Point TopLeft(Point p1, Point p2) => new(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);
            static Point TopRight(Point p1, Point p2) => new(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0.0);

            static List<Point> GetVertices(IEnumerator faceEnum) {
                var vertices = new List<Point>();

                var faceCnt = 0;
                while (faceEnum.MoveNext()) {
                    if (++faceCnt > 1) throw new Exception(UnknownSectionType);

                    var face = faceEnum.Current as ArrayList;
                    var loopEnum = face!.GetEnumerator();

                    var loopCnt = 0;
                    while (loopEnum.MoveNext()) {
                        if (++loopCnt > 2) throw new Exception(UnknownSectionType);
                        //  存在内外两个 loop 时，不需要外层 loop
                        if (loopCnt == 2) vertices.Clear();

                        var loop = loopEnum.Current as ArrayList;
                        var vertexEnum = loop!.GetEnumerator();

                        while (vertexEnum.MoveNext()) {
                            var vertex = vertexEnum.Current as Point;
                            vertices.Add(vertex);
                        }

                        (vertexEnum as IDisposable)?.Dispose();
                    }

                    (loopEnum as IDisposable)?.Dispose();
                }

                return vertices;
            }
        }

        /// <summary>
        /// 为圆管截面创建加劲板。
        /// </summary>
        /// <param name="part"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[1]"/></param>
        /// <param name="position"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[2]"/></param>
        /// <param name="thickness"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[3]"/></param>
        /// <param name="material"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[4]"/></param>
        /// <param name="class"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[5]"/></param>
        /// <param name="rotationAroundY"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[6]"/></param>
        /// <param name="rotationAroundZ"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[7]"/></param>
        /// <param name="clearance"><inheritdoc 
        /// cref="CreatStiffeners(Part, Point, double, string, string, double, double, double, double, double, Chamfer.ChamferTypeEnum, double, double, double, double)" 
        /// path="/param[10]"/></param>
        /// <returns>成功创建的加劲板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        private static ContourPlate CreatStiffenersForTypeRO(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double clearance = 2.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            var faceEnumArr = IntersectionWithStiffenerSurfacePlane(
                part, position, thickness, out var partCS, out var stifPlane, rotationAroundY,
                rotationAroundZ);
            var faceEnumFront = faceEnumArr[0];
            var faceEnumBehind = faceEnumArr[1];

            var verticesFront = GetVertices(faceEnumFront);
            var verticesBehind = GetVertices(faceEnumBehind);
            verticesFront = OrderVertices(verticesFront, partCS).ToList();
            verticesBehind = OrderVertices(verticesBehind, partCS).ToList();

            var centerLine = new Line(partCS.Origin, partCS.AxisX);
            var arrFront = verticesFront.ToArray();
            var arrBehind = verticesBehind.ToArray();
            for (int i = 0; i < arrFront.Length; ++i) {
                arrFront[i] -=
                    new Vector(arrFront[i] - Projection.PointToLine(arrFront[i], centerLine)).GetNormal(clearance);
                arrBehind[i] -=
                    new Vector(arrBehind[i] - Projection.PointToLine(arrBehind[i], centerLine)).GetNormal(clearance);
            }

            var lines = arrFront.Zip(arrBehind, (p1, p2) => new Line(p1, p2)).ToList();
            var stifFrontPlane = new GeometricPlane(stifPlane.Origin + stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);
            var stifBehindPlane = new GeometricPlane(stifPlane.Origin - stifPlane.Normal.GetNormal(thickness * 0.5),
                stifPlane.Normal);

            verticesFront = lines.Select(l => Intersection.LineToPlane(l, stifFrontPlane)).ToList();
            verticesBehind = lines.Select(l => Intersection.LineToPlane(l, stifBehindPlane)).ToList();
            verticesFront.Concat(verticesBehind).Aggregate((p1, p2) => p1 + p2)
                .Multiply(verticesFront.Count() * 2);

            var contourPoints = verticesFront.Select(p => Projection.PointToPlane(p, stifPlane))
                .Zip(verticesBehind.Select(p => Projection.PointToPlane(p, stifPlane)), (p1, p2) =>
                    Tekla.Structures.Geometry3d.Distance.PointToLine(p1, centerLine) <
                    Tekla.Structures.Geometry3d.Distance.PointToLine(p2, centerLine)
                        ? p1
                        : p2);

            var contourPlate = CreatContourPlate(contourPoints, "STIFFENER", $"PL{thickness}", material,
                @class: @class);

            return contourPlate;

            static List<Point> GetVertices(IEnumerator faceEnum) {
                var vertices = new List<Point>();

                var faceCnt = 0;
                while (faceEnum.MoveNext()) {
                    if (++faceCnt > 1) throw new Exception(UnknownSectionType);

                    var face = faceEnum.Current as ArrayList;
                    var loopEnum = face!.GetEnumerator();

                    var loopCnt = 0;
                    while (loopEnum.MoveNext()) {
                        if (++loopCnt == 1) continue; //  跳过外层 Loop
                        if (loopCnt > 2) throw new Exception(UnknownSectionType);

                        var loop = loopEnum.Current as ArrayList;
                        var vertexEnum = loop!.GetEnumerator();

                        while (vertexEnum.MoveNext()) {
                            var vertex = vertexEnum.Current as Point;
                            vertices.Add(vertex);
                        }

                        (vertexEnum as IDisposable)?.Dispose();
                    }

                    (loopEnum as IDisposable)?.Dispose();
                }

                return vertices;
            }
        }

        /// <summary>
        /// 创建加劲板。
        /// </summary>
        /// <param name="part">要创建加劲板的零件</param>
        /// <param name="position">创建加劲板的位置</param>
        /// <param name="thickness">加劲板厚度</param>
        /// <param name="material">加劲板材质</param>
        /// <param name="class">加劲板等级</param>
        /// <param name="rotationAroundY">加劲板绕零件坐标系Y轴旋转角度，弧度制</param>
        /// <param name="rotationAroundZ">加劲板绕零件坐标系Z轴旋转角度，弧度制</param>
        /// <param name="indent">加劲板缩进长度，仅适用于 H型钢、T型钢、工字钢、槽钢</param>
        /// <param name="indent2">加劲板另一个方向的缩进长度，仅适用于 T型钢</param>
        /// <param name="clearance">加劲板与零件表面的净距</param>
        /// <param name="chamferType">加劲板倒角类型，仅适用于 H型钢、T型钢、工字钢、槽钢、矩形管</param>
        /// <param name="chamferSizeX">加劲板倒角尺寸X，仅适用于 H型钢、T型钢、工字钢、槽钢、矩形管</param>
        /// <param name="chamferSizeY">加劲板倒角尺寸Y，仅适用于 H型钢、T型钢、工字钢、槽钢、矩形管</param>
        /// <param name="chamferDz1">加劲板倒角尺寸Dz1，仅适用于 H型钢、T型钢、工字钢、槽钢、矩形管</param>
        /// <param name="chamferDz2">加劲板倒角尺寸Dz2，仅适用于 H型钢、T型钢、工字钢、槽钢、矩形管</param>
        /// <returns>成功创建的加劲板。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception">不支持的截面类型引发。</exception>
        public static IEnumerable<ContourPlate> CreatStiffeners(
            Part part, Point position, double thickness, string material = "Q235B", string @class = "99",
            double rotationAroundY = 0.0, double rotationAroundZ = 0.0, double indent = 0.0, double indent2 = 0.0,
            double clearance = 2.0,
            Chamfer.ChamferTypeEnum chamferType = 0, double chamferSizeX = 0.0, double chamferSizeY = 0.0,
            double chamferDz1 = 0.0, double chamferDz2 = 0.0) {
            if (part is null) {
                throw new ArgumentNullException(nameof(part));
            }

            if (position is null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (material is null) {
                throw new ArgumentNullException(nameof(material));
            }

            if (string.IsNullOrEmpty(@class)) {
                @class = "99";
            }

            const string propertyName = "PROFILE_TYPE";
            var profilesI = new[] { "B_WLD_A", "B_WLD_H", "B_WLD_K" };
            var profilesT = new[] { "B_WLD_E" };
            var profilesU = new[] { "B_WLD_D", "C_BUILT", "C_VAR_A", "C_VAR_B", "C_VAR_C", "C_VAR_D" };
            var profilesM = new[] { "B_WLD_F", "B_WLD_J", "B_BUILT", "B_VAR_A", "B_VAR_B", "B_VAR_C" };

            var profileType = string.Empty;
            if (!part.GetReportProperty(propertyName, ref profileType)) {
                throw new Exception($"Failed to get \"{propertyName}\" for {part.Identifier}.");
            }

            var profileText = part.Profile.ProfileString;

            switch (profileType) {
            case "I":
                return CreatStiffenersForTypeI(part, position, thickness, material, @class,
                    rotationAroundY, rotationAroundZ, indent, clearance,
                    chamferType, chamferSizeX, chamferSizeY, chamferDz1, chamferDz2);
            case "T":
                return CreatStiffenersForTypeT(part, position, thickness, material, @class,
                    rotationAroundY, rotationAroundZ, indent, indent2, clearance,
                    chamferType, chamferSizeX, chamferSizeY, chamferDz1, chamferDz2);
            case "U":
                return [
                    CreatStiffenersForTypeU(part, position, thickness, material, @class,
                        rotationAroundY, rotationAroundZ, indent, clearance,
                        chamferType, chamferSizeX, chamferSizeY, chamferDz1, chamferDz2)
                ];
            case "M":
                return [
                    CreatStiffenersForTypeM(part, position, thickness, material, @class,
                        rotationAroundY, rotationAroundZ, clearance,
                        chamferType, chamferSizeX, chamferSizeY, chamferDz1, chamferDz2)
                ];
            case "RO":
                return [
                    CreatStiffenersForTypeRO(part, position, thickness, material, @class,
                        rotationAroundY, rotationAroundZ, clearance)
                ];
            case "Z":
                var profilePrefix = GetProfilePrefix(profileText);
                if (profilesI.Contains(profilePrefix)) {
                    goto case "I";
                }

                if (profilesT.Contains(profilePrefix)) {
                    goto case "T";
                }

                if (profilesU.Contains(profilePrefix)) {
                    goto case "U";
                }

                if (profilesM.Contains(profilePrefix)) {
                    goto case "M";
                }

                goto default;
            default:
                throw new Exception($"Profile \"{profileText}\" not supported yet.");
            }
        }

        /// <summary>
        /// 沿给定轴线每隔给定角度旋转复制一份对象。
        /// </summary>
        /// <param name="obj">要旋转复制的对象</param>
        /// <param name="axisOrigin">旋转轴起点</param>
        /// <param name="axisDirection">旋转轴方向</param>
        /// <param name="radians">旋转角度，弧度制</param>
        /// <param name="num">要复制的数量，默认值 1</param>
        /// <returns>成功旋转复制的对象集合（不包括初始对象）。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="num"/> &lt;= 0 时引发。</exception>
        [Obsolete(
            "应改为使用 Muggle.TsExtensions.Model.ModelOperation.CopyObject(ModelObject obj, Matrix matrix, int num)方法",
            true)]
        public static List<ModelObject> Copy_Rotate(
            ModelObject obj,
            Point axisOrigin,
            Vector axisDirection,
            double radians,
            int num = 1) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            if (axisOrigin is null) {
                throw new ArgumentNullException(nameof(axisOrigin));
            }

            if (axisDirection is null) {
                throw new ArgumentNullException(nameof(axisDirection));
            }

            if (num <= 0)
                throw new ArgumentException($"“{nameof(num)}”不应小于等于 0。");

            var objs = new List<ModelObject>();

            for (int i = 1; i <= num; i++) {
                var copy = Tekla.Structures.Model.Operations.Operation.CopyObject(obj, new Vector());
                if (Move_Rotate(copy, axisOrigin, axisDirection, radians * i)) objs.Add(copy);
            }

            return objs;
        }

        /// <summary>
        /// 沿给定轴线和角度旋转移动对象。
        /// </summary>
        /// <param name="obj">要旋转移动的对象</param>
        /// <param name="axisOrigin">旋转轴起点</param>
        /// <param name="axisDirection">旋转轴方向</param>
        /// <param name="radians">旋转角度，弧度制</param>
        /// <returns>成功返回 True，失败返回 False。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete("应改为使用 Muggle.TsExtensions.Model.ModelOperation.MoveObject(ModelObject obj, Matrix matrix)方法", true)]
        public static bool Move_Rotate(
            ModelObject obj,
            Point axisOrigin,
            Vector axisDirection,
            double radians) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            if (axisOrigin is null) {
                throw new ArgumentNullException(nameof(axisOrigin));
            }

            if (axisDirection is null) {
                throw new ArgumentNullException(nameof(axisDirection));
            }

            var matrix = MatrixFactoryExtension.Rotate(new Line(axisOrigin, axisDirection), radians);
            var currentCS = new CoordinateSystem();
            var targetCS = matrix.Transform(currentCS);

            return Tekla.Structures.Model.Operations.Operation.MoveObject(obj, currentCS, targetCS);
        }

        /// <summary>
        /// 按给定矩阵移动模型对象。
        /// </summary>
        /// <param name="obj">要移动的模型对象</param>
        /// <param name="matrix">给定矩阵</param>
        /// <returns>成功返回 True，失败返回 False。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>由于 Tekla Open API 底层限制，
        /// 诸如镜像、缩放、切变等“变形”矩阵无法产生（完全）作用，
        /// 仅平移、旋转矩阵可产生作用。</remarks>
        public static bool MoveObject(ModelObject obj, Matrix matrix) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            if (matrix is null) {
                throw new ArgumentNullException(nameof(matrix));
            }

            var currentCS = new CoordinateSystem();
            var targetCS = matrix.Transform(currentCS);

            return Tekla.Structures.Model.Operations.Operation.MoveObject(obj, currentCS, targetCS);
        }

        /// <summary>
        /// 按给定矩阵连续复制模型对象。
        /// </summary>
        /// <param name="obj">要复制的模型对象</param>
        /// <param name="matrix">给定矩阵</param>
        /// <param name="num">复制份数，默认值 1</param>
        /// <returns>成功复制的对象集合（不包括初始对象）。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="num"/> &lt;= 0 时引发。</exception>
        /// <remarks>由于 Tekla Open API 底层限制，
        /// 诸如镜像、缩放、切变等“变形”矩阵无法产生（完全）作用，
        /// 仅平移、旋转矩阵可产生作用。</remarks>
        public static List<ModelObject> CopyObject(ModelObject obj, Matrix matrix, int num = 1) {
            if (obj is null) {
                throw new ArgumentNullException(nameof(obj));
            }

            if (matrix is null) {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (num <= 0)
                throw new ArgumentException($"“{nameof(num)}”应大于 0。", nameof(num));

            var objs = new List<ModelObject>();
            var zeroVector = new Vector();
            ModelObject copy = obj;

            for (int i = 0; i < num; i++) {
                copy = Tekla.Structures.Model.Operations.Operation.CopyObject(copy, zeroVector);
                if (MoveObject(copy, matrix)) objs.Add(copy);
            }

            return objs;
        }
    }
}