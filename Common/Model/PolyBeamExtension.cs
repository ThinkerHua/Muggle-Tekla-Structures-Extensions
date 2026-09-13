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
 *  PolyBeamExtension.cs: extension of Tekla.Structures.Model.PolyBeam
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model {

    /// <summary>
    /// <see cref="Tekla.Structures.Model"/>.<see cref="PolyBeam"/> 的扩展。
    /// </summary>
    public static class PolyBeamExtension {

        /// <summary>
        /// 获取此 <see cref="PolyBeam"/> 各分段的坐标系。
        /// </summary>
        /// <remarks> 系统提供的 <see cref="PolyBeam.GetPolybeamCoordinateSystems"/> 方法存在以下问题：<br/>
        /// 当 Position 属性为： Plane == MIDDLE, PlaneOffset == 0.0, Depth == MIDDLE, DepthOffset == 0.0 时，
        /// 返回的是空集合；<br/>
        /// 当 Position 属性为： Plane != MIDDLE 或 Plane == MIDDLE, PlaneOffset != 0.0 时，
        /// 坐标系会绕 X 轴旋转一定的角度；<br/>
        /// 当 Position 属性为： Rotation != TOP 时，坐标系 Y 轴不指向截面的 Top face
        /// (参考此<a href="https://support.tekla.com/article/understanding-part-orientation">官方文档</a>)。<br/> 
        /// 本扩展方法可以正确获取到各分段的坐标系。</remarks>
        /// <param name="polyBeam">此 <see cref="PolyBeam"/></param>
        /// <returns>此 <see cref="PolyBeam"/> 各分段的坐标系。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static List<CoordinateSystem> GetPolybeamCoordinateSystemsEx(this PolyBeam polyBeam) {
            if (polyBeam == null) {
                throw new ArgumentNullException(nameof(polyBeam));
            }

            var centerLine = polyBeam.GetCenterLine(false);
            var partCs = polyBeam.GetCoordinateSystem();
            var contours = polyBeam.Contour;

            var newPolyBeam = new PolyBeam {
                Profile = polyBeam.Profile,
                Material = polyBeam.Material,
                Contour = contours,
                Position = {
                    Rotation = polyBeam.Position.Rotation,
                    RotationOffset = polyBeam.Position.RotationOffset,
                    Plane = Position.PlaneEnum.MIDDLE,
                    Depth = Position.DepthEnum.BEHIND
                }
            };
            newPolyBeam.Insert();

            var polyCss = newPolyBeam.GetPolybeamCoordinateSystems().Cast<CoordinateSystem>().ToList();
            newPolyBeam.Delete();

            if (!Parallel.VectorToVector(partCs.AxisY, polyCss[0].AxisY)
                || partCs.AxisY.Dot(polyCss[0].AxisY) < 0.0) {

                var angle = partCs.AxisY.GetAngleBetweenWithDirection(polyCss[0].AxisY, partCs.AxisX);

                foreach (var cs in polyCss) {
                    var matrix = MatrixFactory.Rotate(angle, cs.AxisX);
                    cs.AxisY = MatrixExtension.Transform(matrix, cs.AxisY);
                }
            }

            for (int i = 0; i < polyCss.Count; i++) {
                polyCss[i].Origin = (Point)centerLine[i];
            }

            return polyCss;
        }
    }
}