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
 *  Internal.cs: class used for testing during development
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Muggle.TsExtensions.Common.Geometry3d;
using Muggle.TsExtensions.Common.ModelUI;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.Common.Internal {
    /// <summary>
    /// 内部使用的类。主要用于开发过程中的测试。
    /// </summary>
    public static class Internal {
        /// <summary>
        /// 显示给定变换平面。
        /// </summary>
        /// <param name="tp">给定变换平面</param>
        /// <param name="width">轴线要显示的宽度，默认 1。根据官方文档 <see cref="GraphicPolyLine.Width"/>，当前有效值为 1 或 2 或 4。</param>
        /// <param name="lineType">轴线要显示的线型，默认 <see cref="GraphicPolyLine.LineType.Solid"/></param>
        /// <param name="axisX_Color">X轴要显示的颜色，默认为 <see cref="ColorExtension.Red"/></param>
        /// <param name="axisY_Color">Y轴要显示的颜色，默认为 <see cref="ColorExtension.Green"/></param>
        /// <param name="axisZ_Color">Z轴要显示的颜色，默认为 <see cref="ColorExtension.Blue"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">当 Tekla Structures 不在运行时引发。</exception>
        public static void ShowTransformationPlane(
            TransformationPlane tp,
            int width = 1,
            GraphicPolyLine.LineType lineType = GraphicPolyLine.LineType.Solid,
            Color axisX_Color = default,
            Color axisY_Color = default,
            Color axisZ_Color = default) {

            if (tp is null) {
                throw new ArgumentNullException(nameof(tp));
            }

            var model = new Tekla.Structures.Model.Model();
            if (!model.GetConnectionStatus()) throw new Exception("Tekla Structures 不在运行。");
            var currentTP = model.GetWorkPlaneHandler().GetCurrentTransformationPlane();

            var origin = new Point(0, 0, 0).Transform(tp, currentTP);
            var axisX = new Point(500, 0, 0).Transform(tp, currentTP);
            var axisY = new Point(0, 500, 0).Transform(tp, currentTP);
            var axisZ = new Point(0, 0, 500).Transform(tp, currentTP);

            var plX = new PolyLine(new Point[] { origin, axisX });
            var plY = new PolyLine(new Point[] { origin, axisY });
            var plZ = new PolyLine(new Point[] { origin, axisZ });

            if (axisX_Color == null) axisX_Color = ColorExtension.Red;
            if (axisY_Color == null) axisY_Color = ColorExtension.Green;
            if (axisZ_Color == null) axisZ_Color = ColorExtension.Blue;

            var gplX = new GraphicPolyLine(plX, axisX_Color, width, lineType);
            var gplY = new GraphicPolyLine(plY, axisY_Color, width, lineType);
            var gplZ = new GraphicPolyLine(plZ, axisZ_Color, width, lineType);

            var drawer = new GraphicsDrawer();
            drawer.DrawPolyLine(gplX);
            drawer.DrawPolyLine(gplY);
            drawer.DrawPolyLine(gplZ);
        }
        /// <summary>
        /// 显示给定坐标系。
        /// </summary>
        /// <param name="cs">给定坐标系</param>
        /// <param name="width">轴线要显示的宽度，默认 1。根据官方文档 <see cref="GraphicPolyLine.Width"/>，当前有效值为 1 或 2 或 4。</param>
        /// <param name="lineType">轴线要显示的线型，默认 <see cref="GraphicPolyLine.LineType.Solid"/></param>
        /// <param name="axisX_Color">X轴要显示的颜色，默认为 <see cref="ColorExtension.Red"/></param>
        /// <param name="axisY_Color">Y轴要显示的颜色，默认为 <see cref="ColorExtension.Green"/></param>
        /// <param name="axisZ_Color">Z轴要显示的颜色，默认为 <see cref="ColorExtension.Blue"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception">当 Tekla Structures 不在运行时引发。</exception>
        public static void ShowCoordinateSystem(
            CoordinateSystem cs,
            int width = 1,
            GraphicPolyLine.LineType lineType = GraphicPolyLine.LineType.Solid,
            Color axisX_Color = default,
            Color axisY_Color = default,
            Color axisZ_Color = default) {

            if (cs is null) {
                throw new ArgumentNullException(nameof(cs));
            }

            var model = new Tekla.Structures.Model.Model();
            if (!model.GetConnectionStatus()) throw new Exception("Tekla Structures 不在运行。");

            var axisX = new Vector(cs.AxisX);
            var axisY = new Vector(cs.AxisY);
            var axisZ = axisX.Cross(axisY);
            axisX.Normalize(500);
            axisY.Normalize(500);
            axisZ.Normalize(500);

            var plX = new PolyLine(new Point[] { cs.Origin, cs.Origin + axisX });
            var plY = new PolyLine(new Point[] { cs.Origin, cs.Origin + axisY });
            var plZ = new PolyLine(new Point[] { cs.Origin, cs.Origin + axisZ });

            if (axisX_Color == null) axisX_Color = ColorExtension.Red;
            if (axisY_Color == null) axisY_Color = ColorExtension.Green;
            if (axisZ_Color == null) axisZ_Color = ColorExtension.Blue;

            var gplX = new GraphicPolyLine(plX, axisX_Color, width, lineType);
            var gplY = new GraphicPolyLine(plY, axisY_Color, width, lineType);
            var gplZ = new GraphicPolyLine(plZ, axisZ_Color, width, lineType);

            var drawer = new GraphicsDrawer();
            drawer.DrawPolyLine(gplX);
            drawer.DrawPolyLine(gplY);
            drawer.DrawPolyLine(gplZ);
        }
    }
}
