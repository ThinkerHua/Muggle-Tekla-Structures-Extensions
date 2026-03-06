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
 *  ArcExtension.cs: extension of Tekla.Structures.Geometry3d.Arc
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections.Generic;

using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Arc"/> 的扩展。
    /// </summary>
    public static class ArcExtension {
        /// <summary>
        /// 获取圆弧上指定方向的点。
        /// </summary>
        /// <param name="arc">当前圆弧</param>
        /// <param name="vector">指定方向，从圆弧中心点指出。不应平行于圆弧法向，也不应为零向量。</param>
        /// <returns>指定方向的点，如指定方向不在圆弧区间内，则返回null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">参数 <paramref name="vector"/> 是零向量或与 <paramref name="arc"/> 的法向平行时引发。</exception>
        public static Point GetPointOnDirection(this Arc arc, Vector vector) {
            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }

            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            if (vector.IsZero() || Parallel.VectorToVector(vector, arc.Normal))
                throw new ArgumentException($"{nameof(vector)} 不应是零向量，且不应与 {nameof(arc)} 的法向平行。", nameof(vector));

            var v = ProjectionExtension.VectorToPlane(vector, new GeometricPlane(arc.CenterPoint, arc.Normal));
            var angle = arc.StartDirection.GetAngleBetween_WithDirection(v, arc.Normal);
            if (angle > arc.Angle) return null;

            v.Normalize(arc.Radius);
            v.Translate(arc.CenterPoint);

            return v;
        }
        /// <summary>
        /// 获取弧线上在 <paramref name="startPoint"/> 与 <paramref name="endPoint"/> 之间每隔 <paramref name="stepRadians"/> 角度的点的集合。
        /// </summary>
        /// <remarks><b>* 约定：计算夹角时遵循圆弧的方向。
        /// 如果 <paramref name="startPoint"/> 与 <paramref name="endPoint"/> 之间夹角为0，则按2π处理。
        /// </b></remarks>
        /// <param name="arc">当前圆弧</param>
        /// <param name="startPoint">
        ///     起始计算点。（在弧平面上的投影）不应与弧中心点重合。
        ///     如果不在弧线上，则按弧中心点与该点构成的射线在完整弧线（圆）上投影的点计算。
        /// </param>
        /// <param name="endPoint">
        ///     终止计算点。（在弧平面上的投影）不应与弧中心点重合。
        ///     如果不在弧线上，则按弧中心点与该点构成的射线在完整弧线（圆）上投影的点计算。
        /// </param>
        /// <param name="stepRadians">每隔给定角度收集弧线上的点。弧度制。</param>
        /// <returns>
        ///     弧线上的点的集合，是弧线上区间 [ <paramref name="startPoint"/>, <paramref name="endPoint"/> ] 与 
        ///     [ <see cref="Arc.StartPoint"/>, <see cref="Arc.EndPoint"/> ] 的<b>交集</b>的<b>子集</b>。
        ///     没有有效值则返回null。
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">输入点在圆弧平面上投影与弧中心点重合。</exception>
        public static List<Point> GetPoints(this Arc arc, Point startPoint, Point endPoint, double stepRadians) {
            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }

            if (startPoint is null) {
                throw new ArgumentNullException(nameof(startPoint));
            }

            if (endPoint is null) {
                throw new ArgumentNullException(nameof(endPoint));
            }

            var gPlane = new GeometricPlane(arc.CenterPoint, arc.Normal);

            if (Projection.PointToPlane(startPoint, gPlane) == arc.CenterPoint)
                throw new ArgumentException($"“{nameof(startPoint)}”在圆弧平面上的投影，不应与弧中心点重合。");

            if (Projection.PointToPlane(endPoint, gPlane) == arc.CenterPoint)
                throw new ArgumentException($"“{nameof(endPoint)}”在圆弧平面上的投影，不应与弧中心点重合。");

            //  0半径特解
            if (arc.Radius == 0) return new List<Point> { arc.StartPoint };

            //  取值区间
            var startVector = new Vector(startPoint - arc.CenterPoint);
            var endVector = new Vector(endPoint - arc.CenterPoint);
            startVector = ProjectionExtension.VectorToPlane(startVector, gPlane);
            endVector = ProjectionExtension.VectorToPlane(endVector, gPlane);
            startVector.Normalize(arc.Radius);
            endVector.Normalize(arc.Radius);
            var angleStart = arc.StartDirection.GetAngleBetween_WithDirection(startVector, arc.Normal);
            var angleEnd = arc.StartDirection.GetAngleBetween_WithDirection(endVector, arc.Normal);

            //  情形1：angleStart <= angleEnd, 区间为 [ angleStart, Min(angleEnd, arc.Angle) ]
            //  情形2：angleStart >  angleEnd, 区间为 [ 0, Min(angleEnd, arc.Angle) ] + [ angleStart, arc.Angle ]
            //  此情形第一个区间段必然有效，第二个区间段当angleStart<arc.Angle时存在

            //  无有效区间
            if (angleStart <= angleEnd && angleStart > arc.Angle) return null;

            double angleCurrent;
            Point point;
            bool inAngleRange;
            double angleRange = startVector.GetAngleBetween_WithDirection(endVector, gPlane.Normal);
            angleRange += angleRange == 0 ? Math.PI * 2 : 0;
            var uBound = angleEnd < arc.Angle ? angleEnd : arc.Angle;
            var rotationMatrix = MatrixFactory.Rotate(-stepRadians, gPlane.Normal);
            var currentVector = new Vector(startVector);
            var points = new List<Point>();
            for (int i = 0; i <= angleRange / stepRadians; i++) {

                inAngleRange = false;

                angleCurrent = arc.StartDirection.GetAngleBetween_WithDirection(currentVector, gPlane.Normal);
                if (angleStart > angleEnd) {
                    //  不需要判断是否大于等于0
                    if (angleCurrent <= uBound || angleCurrent >= angleStart && angleCurrent <= arc.Angle)
                        inAngleRange = true;
                } else {
                    if (angleCurrent >= angleStart && angleCurrent <= uBound)
                        inAngleRange = true;
                }

                if (inAngleRange) {
                    point = new Point(currentVector);
                    point.Translate(arc.CenterPoint);
                    points.Add(point);
                }

                currentVector = new Vector(rotationMatrix.Transform(currentVector));
            }

            return points;
        }
        /// <summary>
        /// 获取弧线上定数等分点集合。
        /// </summary>
        /// <param name="arc">当前圆弧</param>
        /// <param name="num">等分数</param>
        /// <returns>定数等分点集合。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="num"/>必须大于0。</exception>
        public static List<Point> GetPointsDivide(this Arc arc, int num) {
            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }
            if (num <= 0) throw new ArgumentOutOfRangeException(nameof(num));

            var radians = arc.Angle / num;
            return arc.GetPoints(arc.StartPoint, arc.EndPoint, radians);
        }
        /// <summary>
        /// 获取弧线上定距等分点集合。
        /// </summary>
        /// <param name="arc">当前圆弧</param>
        /// <param name="length">等分弧长</param>
        /// <returns>定距等分点集合。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/>必须大于0。</exception>
        public static List<Point> GetPointsMeasure(this Arc arc, double length) {
            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            if (arc.Radius == 0) return new List<Point> { arc.StartPoint };

            var radians = length / arc.Radius;
            return arc.GetPoints(arc.StartPoint, arc.EndPoint, radians);
        }
    }
}
