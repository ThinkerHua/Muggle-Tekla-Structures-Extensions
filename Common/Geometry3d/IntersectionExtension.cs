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
 *  IntersectionExtension.cs: extension of Tekla.Structures.Geometry3d.Intersection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Operation;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Intersection"/> 的扩展。
    /// </summary>
    public static class IntersectionExtension {
        /// <summary>
        /// 点与直线间最短线段。
        /// </summary>
        /// <param name="point">给定点。</param>
        /// <param name="line">给定直线。</param>
        /// <returns>最短线段。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LineSegment PointToLine(Point point, Line line) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.Direction.IsZero()) return new LineSegment(point, line.Origin);

            return new LineSegment(point, Projection.PointToLine(point, line));
        }
        /// <summary>
        /// 两条直线间最短线段。
        /// </summary>
        /// <remarks><para>本实现旨在解决在两直线相交的情况下，官方实现求得的线段长度不等于0的问题。
        /// 同时也实现了求直线退化成点，即 <see cref="Line.Direction"/> 为零向量时的解。</para>
        /// <para>主要求解公式推导过程如下(<a href="https://math.stackexchange.com/a/4764188">参考</a>)：<br/>
        /// L1上的点方程：P=P1+s*V1, L2上的点方程：P=P2+t*V2，最短线段所在直线上的点方程：P=P3+r*V3<br/>
        /// 由于最短线段两端分别落在L1、L2上，则有：(P2+t*V2)+r*V3=P1+s*V1<br/>
        /// 可令V3=(P2+t*V2)-(P1+s*V1)，代入上述方程，则有：<br/>
        /// <i><![CDATA[  ]]>(1) P1+s*V1=P2+t*V2，即s*V1-t*V2=P2-P1</i><br/>
        /// 由于向量与其自身叉积为0，则有：<br/>
        /// <i><![CDATA[  ]]>(2) s*V1×V2=(P2-P1)×V2</i><br/>
        /// 这是一个向量方程，要得到实数方程，可以对两边用V1×V2做点积：
        /// s*(V1×V2)∙(V1×V2)=((P2-P1)×V2)∙(V1×V2)，求解出：<br/>
        /// <i><![CDATA[  ]]>(3) s=((P2-P1)×V2)∙(V1×V2)/||V1×V2||^2</i><br/>
        /// 再将s代入步骤(1)方程即可求解出：<br/>
        /// <i><![CDATA[  ]]>(4) t=(s*V1-(P2-P1))∙V2/(V2∙V2)</i> - 由于向量没有除法，所以此处需用点积形式做除法<br/>
        /// 据此即可算出最短线段的两个端点。</para>
        /// <para>上述方程中，P1、P2可分别取值为L1、L2的 <see cref="Line.Origin"/> 属性，
        /// V1、V2分别为L1、L2的 <see cref="Line.Direction"/> 属性。</para>
        /// <para>另外，最短距离可用公式 <i>d = (P2-P1)∙(V1×V2)/||V1×V2||</i> 求得。</para></remarks>
        /// <param name="line1">给定直线1</param>
        /// <param name="line2">给定直线2</param>
        /// <returns>两条直线之间的最短线段。如果直线平行，则为 null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LineSegment LineToLine(Line line1, Line line2) {
            if (line1 is null) {
                throw new ArgumentNullException(nameof(line1));
            }

            if (line2 is null) {
                throw new ArgumentNullException(nameof(line2));
            }

            var v = new Vector(line2.Origin - line1.Origin);
            if (v.IsZero()) return new LineSegment(line1.Origin, line2.Origin);

            var s = 0.0;
            var t = 0.0;
            var situation = 0b00;
            situation |= line1.Direction.IsZero() ? 0b01 : 0b00;
            situation |= line2.Direction.IsZero() ? 0b10 : 0b00;
            switch (situation) {
            case 0b00:
                var n = Vector.Cross(line1.Direction, line2.Direction);
                if (n.IsZero()) return null;//  平行

                s = Vector.Dot(Vector.Cross(v, line2.Direction), n) / Math.Pow(n.GetLength(), 2);
                t = Vector.Dot(new Vector(line1.Direction * s - v), line2.Direction) / Vector.Dot(line2.Direction, line2.Direction);
                break;
            case 0b01:
                t = Vector.Dot(-1 * v, line2.Direction) / Vector.Dot(line2.Direction, line2.Direction);
                break;
            case 0b10:
                s = Vector.Dot(v, line1.Direction) / Vector.Dot(line1.Direction, line1.Direction);
                break;
            case 0b11:
            default:
                break;
            }

            return new LineSegment(line1.Origin + s * line1.Direction, line2.Origin + t * line2.Direction);
        }
        /// <summary>
        /// 圆弧与直线间最短线段的集合。
        /// </summary>
        /// <remarks><b>* 有可能不是最优解。</b>
        /// 本方法是在圆弧上循环采样（逐步缩小采样间隔）取点计算最短距离，采样区域有可能错过最优解。
        /// 可以设置更小的 <paramref name="samplingSpacingAtStart"/> 值以获得更精确的采样间隔。
        /// 但不宜设置过小的值，一是影响计算速度，二是可能导致大批距离值相等影响判断。</remarks>
        /// <param name="arc">给定的圆弧</param>
        /// <param name="line">给定的直线</param>
        /// <param name="samplingSpacingAtStart">
        ///     初始采样间隔弧长。
        ///     输入正值以指定初始采样间隔弧长，
        ///     或输入非正值由方法自动确定一个适当的值。
        ///     默认值 0。
        /// </param>
        /// <param name="epsilon">
        ///     容许误差，小于此差异的距离值当作相等处理。默认值为
        ///     <see cref="GeometryConstants.DISTANCE_EPSILON"/>。
        /// </param>
        /// <returns>最短线段集合。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="line"/>.Direction 不应为零向量。</exception>
        public static List<LineSegment> ArcToLine(
            Arc arc,
            Line line,
            double samplingSpacingAtStart = 0,
            double epsilon = GeometryConstants.DISTANCE_EPSILON) {

            if (arc is null) {
                throw new ArgumentNullException(nameof(arc));
            }

            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line)}”.Direction 不应为零向量。");

            //  0半径圆弧
            if (arc.Radius == 0)
                return new List<LineSegment> {
                    new LineSegment(arc.CenterPoint, Projection.PointToLine(arc.CenterPoint, line))
                };

            //  直线垂直圆弧平面穿过弧心的情形
            if (Distance.PointToLine(arc.CenterPoint, line) == 0 && Parallel.VectorToVector(arc.Normal, line.Direction))
                return null;

            //  以下为一般情形
            if (samplingSpacingAtStart <= 0) {
                //  取一个适当的采样间隔
                //  采样太密集影响计算速度，太宽松可能会错过最优解
                samplingSpacingAtStart = arc.Length / 360 > 0.1 ? arc.Length / 360 : 0.1;
            }

            List<LineSegment> segList;
            var spacing = samplingSpacingAtStart;//  采样弧长
            var angle = spacing / arc.Radius;//  采样角度

            #region 初始极值点
            var points = arc.GetPoints(arc.StartPoint, arc.EndPoint, angle);
            var disList = new List<double>();
            foreach (var point in points) {
                disList.Add(Distance.PointToLine(point, line));
            }
            var minExtremeIndexs = CommonOperation.GetLocalExtremeIndexes(disList, CommonOperation.ExtremeTypeEnum.LocalMinimum);
            var minExtremePoints = new List<Point>();//  极小值点集合
            foreach (var index in minExtremeIndexs) {
                minExtremePoints.Add(points[index]);
            }
            var comparer = new PointComparer();
            List<Point> tmpList;
            tmpList = Enumerable.Distinct(minExtremePoints, comparer).ToList();//  剔除重复值
            minExtremePoints.Clear();
            minExtremePoints.AddRange(tmpList);
            #endregion

            Point vector, leftPoint, rightPoint;
            Matrix rotationMatrix;
            while (spacing > epsilon) {
                rotationMatrix = MatrixFactory.Rotate(angle, arc.Normal);

                spacing *= 0.1;//  步进弧长，每次缩小一个数量级
                angle = spacing / arc.Radius;//  步进角度

                points.Clear();
                //  以极小值点左右相邻点为新起止点，取得新细分等分点集合
                foreach (var point in minExtremePoints) {
                    vector = point - arc.CenterPoint;
                    leftPoint = arc.GetPointOnDirection(new Vector(rotationMatrix.Transform(vector)));
                    rightPoint = arc.GetPointOnDirection(new Vector(rotationMatrix.GetTranspose().Transform(vector)));

                    points.AddRange(arc.GetPoints(leftPoint, rightPoint, angle));
                }

                disList.Clear();
                foreach (var point in points) {
                    disList.Add(Distance.PointToLine(point, line));
                }

                minExtremeIndexs = CommonOperation.GetLocalExtremeIndexes(disList, CommonOperation.ExtremeTypeEnum.LocalMinimum);
                minExtremePoints.Clear();
                foreach (var index in minExtremeIndexs) {
                    minExtremePoints.Add(points[index]);
                }
                tmpList = Enumerable.Distinct(minExtremePoints, comparer).ToList();//  剔除重复值
                minExtremePoints.Clear();
                minExtremePoints.AddRange(tmpList);
            }

            segList = new List<LineSegment>();
            foreach (var point in minExtremePoints) {
                segList.Add(new LineSegment(point, Projection.PointToLine(point, line)));
            }

            var cnt = segList.Count;
            double minLength = double.MaxValue;
            for (int i = 0; i < cnt; i++) {
                if (segList[i].Length() < minLength) {
                    minLength = segList[i].Length();
                }
            }
            minLength += epsilon;//  容许误差
            for (int i = 0; i < cnt; i++) {
                if (segList[i].Length() > minLength) {
                    segList.RemoveAt(i);
                    i--; cnt--;
                }
            }

            return segList;
        }
        /// <summary>
        /// 圆与直线间最短线段的集合。
        /// </summary>
        /// <param name="circle">给定的圆（弧），不是整圆依然当成整圆处理。</param>
        /// <param name="line">给定的直线。</param>
        /// <param name="samplingIntervalAtStart">用于情形4.2下调用关联方法时输入的参数，
        /// 详情参见 <see cref="ArcToLine(Arc, Line, double, double)"/> 。</param>
        /// <param name="epsilon">容许误差，小于此差异的距离值当作相等处理。
        /// 默认值为 <see cref="GeometryConstants.DISTANCE_EPSILON"/> 。</param>
        /// <returns>最短线段集合。符合条件的解有多种情形：
        /// <list type="number">
        ///     <item>圆半径为0：此时相当于求圆中心点与直线间的最短线段。</item>
        ///     <item>直线垂直于圆平面：
        ///         <list type="bullet">
        ///             <item>穿过圆中心点：此时有无数个解，返回null。</item>
        ///             <item>不穿过圆中心点：此时仅有一个解。</item>
        ///         </list>
        ///     </item>
        ///     <item>直线平行于圆平面：
        ///         <list type="bullet">
        ///             <item>直线在圆平面上投影与圆不相交或相切：有一个解。</item>
        ///             <item>直线在圆平面上投影在与圆相交：有两个解。</item>
        ///         </list>
        ///     </item>
        ///     <item>与穿过的圆直径所在直线垂直：
        ///         <list type="bullet">
        ///             <item>直线与圆平面交点在圆内时，有两个解；</item>
        ///             <item>在圆上或圆外时，有一个解。</item>
        ///         </list>
        ///         此情形下最优解可通过求以下方程最小值时的 α 得到：
        ///         <code>
        /// y = ((d*tan(α)/tan(θ))^2 + (r-d/cos(α))^2)^0.5
        /// y - 圆上取点到直线的距离, y >= 0
        /// r - 圆半径, r > 0
        /// d - 圆心到直线的距离, 0 &lt;= d &lt;= r
        /// θ - 直线方向与圆平面法向的夹角, 0 &lt; θ &lt; 0.5π
        /// α - 圆上取点与圆心的连线 和 直线与圆平面的交点与圆心的连线之间的夹角, 0 &lt; α &lt; 0.5π
        ///         </code>
        ///         <b>* 上述方程不会解，暂时调用采样方法实现</b>，参见 <see cref="ArcToLine(Arc, Line, double, double)"/>。
        ///     </item>
        ///     <item>与穿过的圆直径所在直线不垂直：只有一个解。
        ///         <br/><b>* 此情形下是通过取样逼近最小值的方法实现。</b>
        ///         参见 <see cref="ArcToLine(Arc, Line, double, double)"/> 。
        ///     </item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="line"/>.Direction 不应为零向量。</exception>
        public static List<LineSegment> CircleToLine(
            Arc circle,
            Line line,
            double samplingIntervalAtStart = 0,
            double epsilon = GeometryConstants.DISTANCE_EPSILON) {

            if (circle is null) {
                throw new ArgumentNullException(nameof(circle));
            }

            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line)}”.Direction 不应为零向量。");

            var segList = new List<LineSegment>();
            LineSegment seg;
            //  情形1：圆半径为0
            if (circle.Radius == 0) {
                seg = PointToLine(circle.CenterPoint, line);
                segList.Add(seg);
                return segList;
            }

            var cFull = new Arc(circle.CenterPoint, circle.StartPoint, circle.Normal, 2 * Math.PI);
            var gPlane = new GeometricPlane(cFull.CenterPoint, cFull.Normal);
            var lineProjected = Projection.LineToPlane(line, gPlane);
            var disCenterToLineProjected = Distance.PointToLine(cFull.CenterPoint, lineProjected);

            var angleLineWithNormal = line.Direction.GetAngleBetween(cFull.Normal);
            if (angleLineWithNormal == 0) {
                //  情形2.1：直线垂直于圆平面，穿过圆中心点
                if (Distance.PointToLine(cFull.CenterPoint, line) == 0) return null;

                //  情形2.2：直线垂直于圆平面，不穿过圆中心点
                var p1 = new Point(cFull.CenterPoint);
                var p2 = Projection.PointToLine(p1, line);
                var v = new Vector(p2 - p1);
                p1 = cFull.GetPointOnDirection(v);
                seg = new LineSegment(p1, p2);
                segList.Add(seg);
            } else if (angleLineWithNormal == Math.PI * 0.5) {

                var p1 = new Point(cFull.CenterPoint);
                var p2 = Projection.PointToLine(p1, line);
                var v = new Vector(p2 - p1);

                if (disCenterToLineProjected >= cFull.Radius) {
                    //  情形3.1：直线平行于圆平面，直线在圆平面上投影与圆不相交或相切
                    p1 = cFull.GetPointOnDirection(v);
                    seg = new LineSegment(p1, p2);
                    segList.Add(seg);
                } else {
                    //  情形3.2：直线平行于圆平面，直线在圆平面上投影在与圆相交
                    var angle = Math.Acos(disCenterToLineProjected / cFull.Radius);
                    var rotationMatrix = MatrixFactory.Rotate(angle, cFull.Normal);

                    v = new Vector(rotationMatrix.Transform(v));
                    p1 = cFull.GetPointOnDirection(v);
                    p2 = Projection.PointToLine(p1, line);
                    seg = new LineSegment(p1, p2);
                    segList.Add(seg);

                    rotationMatrix = MatrixFactory.Rotate(-2 * angle, cFull.Normal);
                    v = new Vector(rotationMatrix.Transform(v));
                    p1 = cFull.GetPointOnDirection(v);
                    p2 = Projection.PointToLine(p1, line);
                    seg = new LineSegment(p1, p2);
                    segList.Add(seg);
                }
            } else {
                var pointLineIntersectPlane = Intersection.LineToLine(line, lineProjected).StartPoint;
                var vDiameter = new Vector(pointLineIntersectPlane - cFull.CenterPoint);

                var angleDiameterWithLine = vDiameter.GetAngleBetween(line.Direction);
                if (angleDiameterWithLine == Math.PI * 0.5) {
                    //  情形4.1：直线既不垂直也不平行于圆平面，与穿过的圆直径所在直线垂直

                    /*
                        //  ==========此段代码不正确，不是最优解==========
                     
                        //  边界条件 d == r * (1 - cos(θ)^2) / (1 + cos(θ)^2)
                        var disBoundary = cFull.Radius * (1 - Math.Pow(Math.Cos(angleLineWithNormal), 2))
                                                        / (1 + Math.Pow(Math.Cos(angleLineWithNormal), 2));
                        var disCenterToIntersection = vDiameter.GetLength();
                        if (disCenterToIntersection <= disBoundary) {
                            var angle = Math.Acos(disCenterToIntersection / cFull.Radius);
                            var rotationMatrix = MatrixFactory.Rotate(angle, cFull.Normal);
                            var v = new Vector(rotationMatrix.Transform(vDiameter));
                            var p1 = cFull.GetPointOnDirection(v);
                            var p2 = Projection.PointToLine(p1, line);
                            seg = new LineSegment(p1, p2);
                            segList.Add(seg);
                     
                            rotationMatrix = MatrixFactory.Rotate(-angle, cFull.Normal);
                            v = new Vector(rotationMatrix.Transform(vDiameter));
                            p1 = cFull.GetPointOnDirection(v);
                            p2 = Projection.PointToLine(p1, line);
                            seg = new LineSegment(p1, p2);
                            segList.Add(seg);
                        }
                        if (disCenterToIntersection >= disBoundary) {
                            var p3 = cFull.GetPointOnDirection(vDiameter);
                            seg = new LineSegment(p3, pointLineIntersectPlane);
                            segList.Add(seg);
                        }
                    */

                    //  代数计算法待研究，暂时调用采样方法来实现
                    var arc = new Arc(circle.CenterPoint, circle.StartPoint, circle.Normal, 2 * Math.PI);
                    segList = ArcToLine(arc, line, samplingIntervalAtStart, epsilon);
                } else {
                    //  情形4.2：直线既不垂直也不平行于圆平面，与穿过的圆直径所在直线不垂直
                    var arc = new Arc(circle.CenterPoint, circle.StartPoint, circle.Normal, 2 * Math.PI);
                    segList = ArcToLine(arc, line, samplingIntervalAtStart, epsilon);
                }
            }

            return segList;
        }
        /// <summary>
        /// 圆与直线的交点。适用于二维平面。
        /// </summary>
        /// <param name="centerPoint">圆的中心点</param>
        /// <param name="radius">圆的半径</param>
        /// <param name="line">给定的直线</param>
        /// <param name="epsilon">容许误差，小于此误差判定为相切</param>
        /// <returns>
        ///     圆与直线的交点组成的元组，其中元素 X1, X2 的顺序符合顺直线方向。
        ///     圆与直线相切时，元素相等；圆与直线既不相交也不相切时，元素为 null。
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static (Point X1, Point X2) CircleToLine_2D(
            Point centerPoint,
            double radius,
            Line line,
            double epsilon = 1e-10) {

            if (centerPoint is null) {
                throw new ArgumentNullException(nameof(centerPoint));
            }

            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }

            var d = Distance.PointToLine(centerPoint, line);
            if (d - radius > epsilon) return (null, null);
            if (d > radius) d = radius;

            var X2 = Projection.PointToLine(centerPoint, line);
            var X1 = new Point(X2);

            var t = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(d, 2));
            var v = new Vector(line.Direction);
            v.Normalize(t);
            X2.Translate(v);
            X1.Translate(-1 * v);

            return (X1, X2);
        }
    }
}
