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
 *  GraphicsDrawerExtension.cs: extension of Tekla.Structures.Model.UI.GraphicsDrawer
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;

namespace Muggle.TsExtensions.Common.ModelUI {
    /// <summary>
    /// <see cref="Tekla.Structures.Model.UI"/>.<see cref="GraphicsDrawer"/> 的扩展。
    /// </summary>
    public static class GraphicsDrawerExtension {
        /// <summary>
        /// 在视图中画一个圆弧（用碎线条拟合，线条越短越接近圆形）。
        /// </summary>
        /// <param name="drawer">当前绘图器。</param>
        /// <param name="arc">要画出的圆弧。</param>
        /// <param name="color">线条颜色，默认值 <see cref="ColorExtension.Black"/>。</param>
        /// <param name="width">线条宽度，默认值 1。根据官方文档 <see cref="GraphicPolyLine.Width"/>，当前有效值为 1 或 2 或 4。</param>
        /// <param name="type">线型，默认值 <see cref="GraphicPolyLine.LineType.Solid"/>。</param>
        /// <param name="accuracy">模拟圆弧的精细程度，即每多少弧长画一条直线，默认值 10.0。不建议设置为太小的数值，会十分影响性能。</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="width"/> 是有效值"1, 2, 4"以外的数值，
        /// 或 <paramref name="accuracy"/> 小于等于 0.0 时引发。</exception>
        public static void DrawArc(this GraphicsDrawer drawer,
            Arc arc,
            Color color = default,
            int width = 1,
            GraphicPolyLine.LineType type = GraphicPolyLine.LineType.Solid,
            double accuracy = 10.0) {
            if (drawer is null) {
                throw new ArgumentNullException(nameof(drawer));
            }

            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }

            if (width != 1 && width != 2 && width != 4)
                throw new ArgumentException($"根据官方文档，“{nameof(width)}”有效值仅为 1 或 2 或 4。");

            if (accuracy <= 0)
                throw new ArgumentException($"“{nameof(accuracy)}”不应小于等于 0.0。");

            var points = arc.GetPointsMeasure(accuracy);
            //必须包含圆弧终点，避免产生缺口
            if (points[points.Count - 1] != arc.EndPoint) points.Add(arc.EndPoint);
            var polyLine = new PolyLine(points);
            if (color == default) color = new Color();
            var graphicPolyLine = new GraphicPolyLine(polyLine, color, width, type);

            drawer.DrawPolyLine(graphicPolyLine);
        }
        /// <summary>
        /// 在视图中画一个点。
        /// </summary>
        /// <param name="drawer">当前绘图器。</param>
        /// <param name="point">要画出的点。</param>
        /// <param name="color">线条颜色，默认值 <see cref="ColorExtension.Black"/>。</param>
        /// <param name="width">线条宽度，默认值 1。根据官方文档 <see cref="GraphicPolyLine.Width"/>，当前有效值为 1 或 2 或 4。</param>
        /// <param name="type">线型，默认值 <see cref="GraphicPolyLine.LineType.Solid"/>。</param>
        /// <param name="size">点大小，默认值 50.0。</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="width"/> 是有效值"1, 2, 4"以外的数值，
        /// 或 <paramref name="size"/> 小于等于 0.0 时引发。</exception>
        public static void DrawPoint(this GraphicsDrawer drawer,
            Point point,
            Color color = default,
            int width = 1,
            GraphicPolyLine.LineType type = GraphicPolyLine.LineType.Solid,
            double size = 50.0) {
            if (drawer is null) {
                throw new ArgumentNullException(nameof(drawer));
            }

            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            if (width != 1 && width != 2 && width != 4)
                throw new ArgumentException($"根据官方文档，“{nameof(width)}”有效值仅为 1 或 2 或 4。");

            if (size <= 0)
                throw new ArgumentException($"“{nameof(size)}”不应小于等于0.0。");

            Point outLT = new Point(-size, size, 0), outRT = new Point(size, size, 0);
            Point outLB = new Point(-size, -size, 0), outRB = new Point(size, -size, 0);
            Point inLT = new Point(-size * 0.5, size * 0.5, 0), inRT = new Point(size * 0.5, size * 0.5, 0);
            Point inLB = new Point(-size * 0.5, -size * 0.5, 0), inRB = new Point(size * 0.5, -size * 0.5, 0);

            outLT.Translate(point); outRT.Translate(point);
            outLB.Translate(point); outRB.Translate(point);
            inLT.Translate(point); inRT.Translate(point);
            inLB.Translate(point); inRB.Translate(point);

            var pl1 = new PolyLine(new Point[] { outLT, outRB });
            var pl2 = new PolyLine(new Point[] { outLB, outRT });
            var pl3 = new PolyLine(new Point[] { inLT, inRT, inRB, inLB, inLT });

            if (color == default) color = new Color();
            var graphicPolyLine1 = new GraphicPolyLine(pl1, color, width, type);
            var graphicPolyLine2 = new GraphicPolyLine(pl2, color, width, type);
            var graphicPolyLine3 = new GraphicPolyLine(pl3, color, width, type);

            drawer.DrawPolyLine(graphicPolyLine1);
            drawer.DrawPolyLine(graphicPolyLine2);
            drawer.DrawPolyLine(graphicPolyLine3);
        }
        /// <summary>
        /// 在视图中画一条直线。由于直线是延伸到无限远的，所以此方法仅画出其一部分长度
        /// （以 <paramref name="point"/> 在直线上的投影点为基准，沿直线向前后共 <paramref name="length"/> 长度）。
        /// </summary>
        /// <param name="drawer">当前绘图器。</param>
        /// <param name="line">要画的直线。</param>
        /// <param name="color">线条颜色，默认值 <see cref="ColorExtension.Black"/>。</param>
        /// <param name="width">线条宽度，默认值 1。根据官方文档 <see cref="GraphicPolyLine.Width"/>，当前有效值为 1 或 2 或 4。</param>
        /// <param name="type">线型，默认值 <see cref="GraphicPolyLine.LineType.Solid"/>。</param>
        /// <param name="point">基准控制点，默认值为直线原点。</param>
        /// <param name="length">要画出的长度，默认值 5000.0。</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="line"/> 的 <see cref="Line.Direction"/> 属性为零向量，
        /// 或 <paramref name="length"/> 小于等于 0.0，或 <paramref name="width"/> 是有效值"1, 2, 4"以外的数值时引发。</exception>
        public static void DrawLine(this GraphicsDrawer drawer,
            Line line,
            Color color = default,
            int width = 1,
            GraphicPolyLine.LineType type = GraphicPolyLine.LineType.Solid,
            Point point = default,
            double length = 5000.0) {
            if (drawer is null) {
                throw new ArgumentNullException(nameof(drawer));
            }

            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line)}”.Direction 不应为零向量。");

            if (length <= 0)
                throw new ArgumentException($"“{nameof(length)}”不应小于等于0.0。");

            if (width != 1 && width != 2 && width != 4)
                throw new ArgumentException($"根据官方文档，“{nameof(width)}”有效值仅为 1 或 2 或 4。");

            if (point == default) point = line.Origin;
            var p1 = Projection.PointToLine(point, line);
            var p2 = new Point(p1);

            var v = new Vector(line.Direction);
            v.Normalize();
            p1.Translate(v * length * 0.5);
            p2.Translate(v * length * -0.5);

            var pl = new PolyLine(new Point[] { p1, p2 });
            if (color == default) color = new Color();
            var graphicPolyLine = new GraphicPolyLine(pl, color, width, type);

            drawer.DrawPolyLine(graphicPolyLine);
        }
    }
}
