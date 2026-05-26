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
 *  Geometry3dOperation.cs: solver of geometric problems
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Task = System.Threading.Tasks.Task;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// 三维几何操作相关功能
    /// </summary>
    public static class Geometry3dOperation {
        /// <summary>
        /// 获取一个向量，其在XY平面上的投影与X轴之间的夹角，以及其与XY平面之间的夹角为给定值。
        /// </summary>
        /// <param name="angle_between_X">在XY平面上的投影与X轴之间的夹角，弧度制</param>
        /// <param name="angle_between_XY">与XY平面之间的夹角，弧度制</param>
        /// <returns>向量，其在XY平面上的投影与X轴之间的夹角，以及其与XY平面之间的夹角为给定值。</returns>
        public static Vector GetDirectionByAngle(double angle_between_X, double angle_between_XY = 0) {
            Vector direction = new Vector {
                X = Math.Cos(angle_between_X),
                Y = Math.Sin(angle_between_X),
                Z = Math.Sin(angle_between_XY)
            };

            return direction;
        }

        /// <summary>
        /// 对点进行镜像。
        /// </summary>
        /// <param name="point">要镜像的点</param>
        /// <param name="byPlane">镜像平面</param>
        /// <returns>镜像后的点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point Mirror(Point point, GeometricPlane byPlane) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            if (byPlane is null) {
                throw new ArgumentNullException(nameof(byPlane));
            }

            var p = Projection.PointToPlane(point, byPlane);
            var v = new Vector(p - point);
            p.Translate(v);

            return p;
        }

        /// <summary>
        /// 对轮廓点进行镜像。
        /// </summary>
        /// <param name="contourPoint">要镜像的轮廓点</param>
        /// <param name="byPlane">镜像平面</param>
        /// <returns>镜像后的轮廓点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ContourPoint Mirror(ContourPoint contourPoint, GeometricPlane byPlane) {
            if (contourPoint is null) {
                throw new ArgumentNullException(nameof(contourPoint));
            }

            if (byPlane is null) {
                throw new ArgumentNullException(nameof(byPlane));
            }

            var point = new Point(contourPoint.X, contourPoint.Y, contourPoint.Z);
            point = Mirror(point, byPlane);

            return new ContourPoint(point, contourPoint.Chamfer);
        }

        /// <summary>
        /// 对 ContourPoints 进行镜像。
        /// </summary>
        /// <param name="contourPoints">要镜像的ContourPoints</param>
        /// <param name="byPlane">镜像平面</param>
        /// <returns>镜像后的ContourPoints</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ArrayList Mirror(ArrayList contourPoints, GeometricPlane byPlane) {
            if (contourPoints is null) {
                throw new ArgumentNullException(nameof(contourPoints));
            }

            if (byPlane is null) {
                throw new ArgumentNullException(nameof(byPlane));
            }

            ArrayList arrayList;
            try {
                arrayList = new ArrayList();
                foreach (ContourPoint cp in contourPoints) {
                    arrayList.Add(Mirror(cp, byPlane));
                }
            } catch {
                arrayList = null;
            }

            return arrayList;
        }

        /// <summary>
        /// 在平面上，有一个固定形状的三角形（位置不固定）和三条已知直线（位置固定）。求当三角形三个顶点分别落在三条直线上时的位置（即三个顶点值）。
        /// </summary>
        /// <remarks><para><b>* 本方法仅处理二维平面情形。</b></para>
        /// <para>作如下约定：</para>
        /// <list type="bullet">
        ///     <item>
        ///         三条直线分别为 <paramref name="lines"/>.L1, <paramref name="lines"/>.L2, <paramref name="lines"/>.L3。
        ///     </item>
        ///     <item>  
        ///         三角形三条边分别为 <paramref name="edges"/>.E1, <paramref name="edges"/>.E2, <paramref name="edges"/>.E3。
        ///     </item>
        ///     <item>
        ///         <para>三角形三个顶点分别为 P1, P2, P3, 分别落在 L1, L2, L3上。</para>
        ///         <para>E1 = [P1, P3], E2 = [P2, P3], E3 = [P1, P2]</para>
        ///     </item>
        /// </list></remarks>
        /// <param name="lines">给定的三条直线</param>
        /// <param name="edges">给定的三角形三条边</param>
        /// <param name="samplingSpacingAtStart">
        ///     初始采样间隔长度。
        ///     输入正值以指定初始采样间隔，或输入非正值由方法自动确定一个适当的值。
        ///     默认值 0。
        /// </param>
        /// <param name="epsilon">
        ///     容许误差，小于此差异的距离值当作相等处理。默认值为
        ///     <see cref="GeometryConstants.DISTANCE_EPSILON"/>。
        /// </param>
        /// <returns>符合要求的三角形三个顶点的集合。如无解则集合中元素数量为 0。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="lines"/> 的元素的Direction属性为零向量或不在同一个平面上，
        ///     或 <paramref name="edges"/> 的元素不构成三角形。
        /// </exception>
        public static List<(Point P1, Point P2, Point P3)> PositionOfTriangleOnLines(
            (Line L1, Line L2, Line L3) lines,
            (double E1, double E2, double E3) edges,
            double samplingSpacingAtStart = 0,
            double epsilon = GeometryConstants.DISTANCE_EPSILON) {

            var l1 = lines.L1; var l2 = lines.L2; var l3 = lines.L3;
            var e1 = edges.E1; var e2 = edges.E2; var e3 = edges.E3;

            #region 验证数据有效性
            if (l1 is null) {
                throw new ArgumentNullException($"{nameof(lines)}.{nameof(lines.L1)}");
            }
            if (l2 is null) {
                throw new ArgumentNullException($"{nameof(lines)}.{nameof(lines.L2)}");
            }
            if (l3 is null) {
                throw new ArgumentNullException($"{nameof(lines)}.{nameof(lines.L3)}");
            }
            if (l1.Direction.IsZero()) {
                throw new ArgumentException(
                    $"“{nameof(lines)}”的元素" +
                    $"“L1 = {{" +
                    $"Origin = {l1.Origin.ToString(null)}, " +
                    $"Direction = {l1.Direction.ToString(null)} }}”" +
                    $"的Direction属性不应为零向量。");
            }
            if (l2.Direction.IsZero()) {
                throw new ArgumentException(
                    $"“{nameof(lines)}”的元素" +
                    $"“L2 = {{" +
                    $"Origin = {l2.Origin.ToString(null)}, " +
                    $"Direction = {l2.Direction.ToString(null)} }}”" +
                    $"的Direction属性不应为零向量。");
            }
            if (l3.Direction.IsZero()) {
                throw new ArgumentException(
                    $"“{nameof(lines)}”的元素" +
                    $"“L3 = {{" +
                    $"Origin = {l3.Origin.ToString(null)}, " +
                    $"Direction = {l3.Direction.ToString(null)} }}”" +
                    $"的Direction属性不应为零向量。");
            }

            //不构成三角形
            if (e1 <= 0 || e2 <= 0 || e3 <= 0) {
                throw new ArgumentException($"“{nameof(edges)}”的元素" +
                    $"E1 = “{e1}, E2 = {e2}, E3 = {e3}”不应小于等于0.0。");
            }
            if ((e1 + e2) <= e3 || (e1 + e3) <= e2 || (e2 + e3) <= e1) {
                throw new ArgumentException($"“{nameof(edges)}”的元素" +
                    $"E1 = “{e1}, E2 = {e2}, E3 = {e3}”不构成三角形。");
            }

            //任意一对不共面 或 均共线
            var sign12 = GeometricPlaneFactory.ByLines(l1, l2, out GeometricPlane gPlane12);
            var sign13 = GeometricPlaneFactory.ByLines(l1, l3, out GeometricPlane gPlane13);
            var sign23 = GeometricPlaneFactory.ByLines(l2, l3, out _);
            if (sign12 == 0 || sign13 == 0 || sign23 == 0 || sign12 + sign13 + sign23 == -3) {
                throw new ArgumentException($"“{nameof(lines)}”的元素不在同一个平面上。");
            }

            //共同平面法向
            //只可能有一对线共线，不需要判断两次
            var normal = sign12 == 1 ? gPlane12.Normal : gPlane13.Normal;
            #endregion

            #region 求p1, p2, p3取值范围，并验证是否有解
            var itvlP1 = new PointsInterval(l1);
            var itvlP1_e1_left = new PointsInterval(itvlP1);
            var itvlP1_e1_right = new PointsInterval(itvlP1);
            var itvlP1_e3_left = new PointsInterval(itvlP1);
            var itvlP1_e3_right = new PointsInterval(itvlP1);
            var itvlP2 = new PointsInterval(l2);
            var itvlP2_e2_left = new PointsInterval(itvlP2);
            var itvlP2_e2_right = new PointsInterval(itvlP2);
            var itvlP2_e3_left = new PointsInterval(itvlP2);
            var itvlP2_e3_right = new PointsInterval(itvlP2);
            var itvlP3 = new PointsInterval(l3);
            var itvlP3_e1_left = new PointsInterval(itvlP3);
            var itvlP3_e1_right = new PointsInterval(itvlP3);
            var itvlP3_e2_left = new PointsInterval(itvlP3);
            var itvlP3_e2_right = new PointsInterval(itvlP3);

            var transVector = Vector.Cross(normal, itvlP1.Direction);
            transVector.Normalize(e1);
            itvlP1_e1_left.Origin += transVector;
            itvlP1_e1_right.Origin -= transVector;
            transVector.Normalize(e3);
            itvlP1_e3_left.Origin += transVector;
            itvlP1_e3_right.Origin -= transVector;

            transVector = Vector.Cross(normal, itvlP2.Direction);
            transVector.Normalize(e2);
            itvlP2_e2_left.Origin += transVector;
            itvlP2_e2_right.Origin -= transVector;
            transVector.Normalize(e3);
            itvlP2_e3_left.Origin += transVector;
            itvlP2_e3_right.Origin -= transVector;

            transVector = Vector.Cross(normal, itvlP3.Direction);
            transVector.Normalize(e1);
            itvlP3_e1_left.Origin += transVector;
            itvlP3_e1_right.Origin -= transVector;
            transVector.Normalize(e2);
            itvlP3_e2_left.Origin += transVector;
            itvlP3_e2_right.Origin -= transVector;

            double value1, value2, value3, value4;
            //  P1取值范围
            if (sign12 == -1) {
                value1 = double.NegativeInfinity;
                value2 = double.PositiveInfinity;
            } else {
                value1 = itvlP1.Intersect(itvlP2_e3_left).Start;
                value2 = itvlP1.Intersect(itvlP2_e3_right).Start;
                if (value1 > value2) (value1, value2) = (value2, value1);
            }
            if (sign13 == -1) {
                value3 = double.NegativeInfinity;
                value4 = double.PositiveInfinity;
            } else {
                value3 = itvlP1.Intersect(itvlP3_e1_left).Start;
                value4 = itvlP1.Intersect(itvlP3_e1_right).Start;
                if (value3 > value4) (value3, value4) = (value4, value3);
            }
            if (value1 > value4 || value2 < value3) return null;

            //  P2取值范围
            if (sign12 == -1) {
                value1 = double.NegativeInfinity;
                value2 = double.PositiveInfinity;
            } else {
                value1 = itvlP2.Intersect(itvlP1_e3_left).Start;
                value2 = itvlP2.Intersect(itvlP1_e3_right).Start;
                if (value1 > value2) (value1, value2) = (value2, value1);
            }
            if (sign23 == -1) {
                value3 = double.NegativeInfinity;
                value4 = double.PositiveInfinity;
            } else {
                value3 = itvlP2.Intersect(itvlP3_e2_left).Start;
                value4 = itvlP2.Intersect(itvlP3_e2_right).Start;
                if (value3 > value4) (value3, value4) = (value4, value3);
            }
            if (value1 > value4 || value2 < value3) return null;

            //  P3取值范围
            if (sign13 == -1) {
                value1 = double.NegativeInfinity;
                value2 = double.PositiveInfinity;
            } else {
                value1 = itvlP3.Intersect(itvlP1_e1_left).Start;
                value2 = itvlP3.Intersect(itvlP1_e1_right).Start;
                if (value1 > value2) (value1, value2) = (value2, value1);
            }
            if (sign23 == -1) {
                value3 = double.NegativeInfinity;
                value4 = double.PositiveInfinity;
            } else {
                value3 = itvlP3.Intersect(itvlP2_e2_left).Start;
                value4 = itvlP3.Intersect(itvlP2_e2_right).Start;
                if (value3 > value4) (value3, value4) = (value4, value3);
            }
            if (value1 > value4 || value2 < value3) return null;
            itvlP3.Start = value1 < value3 ? value3 : value1;
            itvlP3.End = value2 < value4 ? value2 : value4;

            #endregion

            #region 确定一个适当的初始采样间距
            if (samplingSpacingAtStart <= 0) {
                samplingSpacingAtStart = new double[] { itvlP3.Width, e1, e2, e3, 100 }.Min() * 0.01;
            }
            if (samplingSpacingAtStart < epsilon)
                samplingSpacingAtStart = epsilon;

            itvlP3.EnumInterval = samplingSpacingAtStart;
            #endregion

            var initialVertex3Values_11 = new List<double>();
            for (int i = 0; i < itvlP3.Width / samplingSpacingAtStart; i++) {
                initialVertex3Values_11.Add(itvlP3.Start + i * samplingSpacingAtStart);
            }
            if (itvlP3.Width % samplingSpacingAtStart >= epsilon) {
                initialVertex3Values_11.Add(itvlP3.End);
            }
            var initialVertex3Values_12 = new List<double>(initialVertex3Values_11);
            var initialVertex3Values_21 = new List<double>(initialVertex3Values_11);
            var initialVertex3Values_22 = new List<double>(initialVertex3Values_11);

            var initialVertices = GetVertices(itvlP3, initialVertex3Values_11, l1, l2, e1, e2);

            var solution = new Func<List<double>, int, IEnumerable<(Point vertex1, Point vertex2, Point vertex3)>>(
                (initialVertex3Values, combinationEnum) => {
                    var initialCombinations = GetSpecificCombinationsOfVertices(initialVertices, combinationEnum);
                    return FinalSolutionOfVertices(
                        IterativeGetVertex3MinExtremeValues(initialCombinations, initialVertex3Values,
                            itvlP3, l1, l2, e1, e2, e3, combinationEnum, samplingSpacingAtStart, epsilon),
                        itvlP3, l1, l2, e1, e2, e3, combinationEnum, epsilon);
                }
            );

            var task11 = new Task<IEnumerable<(Point vertex1, Point vertex2, Point vertex3)>>(
                () => solution(initialVertex3Values_11, 11));
            var task12 = new Task<IEnumerable<(Point vertex1, Point vertex2, Point vertex3)>>(
                () => solution(initialVertex3Values_12, 12));
            var task21 = new Task<IEnumerable<(Point vertex1, Point vertex2, Point vertex3)>>(
                () => solution(initialVertex3Values_21, 21));
            var task22 = new Task<IEnumerable<(Point vertex1, Point vertex2, Point vertex3)>>(
                () => solution(initialVertex3Values_22, 22));
            var taskList = new[] { task11, task12, task21, task22 };
            foreach (var task in taskList) {
                task.Start();
            }
            Task.WaitAll(taskList);

            var results = new List<(Point vertex1, Point vertex2, Point vertex3)>();
            foreach (var task in taskList) {
                if (task.IsCompleted && task.Result != null) {
                    results.AddRange(task.Result);
                }
            }

            return results;
        }

        /// <summary>
        /// 根据3#顶点区间和给定数值，求1#和2#顶点（未分组）集合。
        /// </summary>
        /// <param name="vertex3Interval">3#顶点区间</param>
        /// <param name="vertex3Values">3#顶点的数值</param>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <param name="edge1">边长1</param>
        /// <param name="edge2">边长2</param>
        /// <returns>1#和2#顶点（未分组）集合。</returns>
        internal static IEnumerable<((Point X1, Point X2) intersection_l1, (Point X1, Point X2) intersection_l2)> GetVertices(
            PointsInterval vertex3Interval,
            IEnumerable<double> vertex3Values,
            Line line1,
            Line line2,
            double edge1,
            double edge2) {

            if (vertex3Interval is null) {
                throw new ArgumentNullException(nameof(vertex3Interval));
            }

            if (vertex3Values is null) {
                throw new ArgumentNullException(nameof(vertex3Values));
            }

            if (vertex3Values.Count() == 0) {
                throw new ArgumentException($"“{nameof(vertex3Values)}”的元素数量不应为0。");
            }

            (Point X1, Point X2) intersection_l1, intersection_l2;
            foreach (var value in vertex3Values) {
                var p = vertex3Interval.GetPoint(value);
                intersection_l1 = IntersectionExtension.CircleToLine_2D(p, edge1, line1);
                intersection_l2 = IntersectionExtension.CircleToLine_2D(p, edge2, line2);
                yield return (intersection_l1, intersection_l2);
            }
        }

        /// <summary>
        /// 从给定的顶点集合中，获取指定组合的顶点。
        /// </summary>
        /// <param name="vertices">1#和2#顶点集合</param>
        /// <param name="combinationEnum">指定组合：<br/>
        /// 11 - 1#顶点取直线1上的第1个交点，2#顶点取直线2上的第1个交点 <br/>
        /// 12 - 1#顶点取直线1上的第1个交点，2#顶点取直线2上的第2个交点 <br/>
        /// 21 - 1#顶点取直线1上的第2个交点，2#顶点取直线2上的第1个交点 <br/>
        /// 22 - 1#顶点取直线1上的第2个交点，2#顶点取直线2上的第2个交点 <br/>
        /// </param>
        /// <returns>指定顶点组合的集合。</returns>
        internal static IEnumerable<(Point vertex1, Point vertex2)> GetSpecificCombinationsOfVertices(
            IEnumerable<((Point X1, Point X2) intersection_l1, (Point X1, Point X2) intersection_l2)> vertices,
            int combinationEnum) {
            if (vertices is null) {
                throw new ArgumentNullException(nameof(vertices));
            }

            if (vertices.Count() == 0) {
                throw new ArgumentException($"“{nameof(vertices)}”的元素数量不应为0。");
            }

            foreach (var (intersection_l1, intersection_l2) in vertices) {
                switch (combinationEnum) {
                case 11:
                    yield return (intersection_l1.X1, intersection_l2.X1);
                    break;
                case 12:
                    yield return (intersection_l1.X1, intersection_l2.X2);
                    break;
                case 21:
                    yield return (intersection_l1.X2, intersection_l2.X1);
                    break;
                case 22:
                    yield return (intersection_l1.X2, intersection_l2.X2);
                    break;
                default:
                    break;
                }
            }
        }

        /// <summary>
        /// 根据给定的1#和2#顶点组合，求出最接近给定的边长3时对应的3#顶点数值。
        /// </summary>
        /// <param name="combinations">1#和2#顶点组合的集合，
        /// 元素数量应与 <paramref name="vertex3Values"/> 一致</param>
        /// <param name="vertex3Values">与1#和2#顶点组合对应的3#顶点数值集合，
        /// 元素数量应与 <paramref name="combinations"/> 一致</param>
        /// <param name="edge3">给定边长3</param>
        /// <returns>最接近给定的边长3时对应的3#顶点数值集合。</returns>
        internal static IEnumerable<double> GetVertex3MinExtremeValues(
            IEnumerable<(Point vertex1, Point vertex2)> combinations,
            IEnumerable<double> vertex3Values,
            double edge3) {
            if (combinations is null) {
                throw new ArgumentNullException(nameof(combinations));
            }

            if (vertex3Values is null) {
                throw new ArgumentNullException(nameof(vertex3Values));
            }

            var cntC = combinations.Count();
            var cntV = vertex3Values.Count();

            if (cntC == 0) {
                throw new ArgumentException($"“{nameof(combinations)}”的元素数量不应为0。");
            }

            if (cntV == 0) {
                throw new ArgumentException($"“{nameof(vertex3Values)}”的元素数量不应为0。");
            }

            if (cntC != cntV) {
                throw new ArgumentException(
                    $"“{nameof(combinations)}”的元素数量" +
                    $"与“{nameof(vertex3Values)}”的元素数量不一致。");
            }

            var distances = new List<double>();
            foreach (var (vertex1, vertex2) in combinations) {
                distances.Add(Math.Abs(Distance.PointToPoint(vertex1, vertex2) - edge3));
            }

            var minExtremeIndexes = Operation.GetLocalExtremeIndexes(distances, Operation.ExtremeTypeEnum.LocalMinimum);

            if (minExtremeIndexes.Count > 10) {
                minExtremeIndexes = distances.Select((dis, index) => (dis, index))
                    .OrderBy(x => x.dis).Take(10).OrderBy(x => x.index)
                    .Select(x => x.index).ToList();
            }

            foreach (var index in minExtremeIndexes) {
                yield return vertex3Values.ElementAt(index);
            }
        }

        /// <summary>
        /// 迭代求解最接近给定的边长3时对应的3#顶点数值集合。
        /// </summary>
        /// <param name="initialCombinations">初始1#和2#顶点组合集合</param>
        /// <param name="initialVertex3Values">与初始1#和2#顶点组合对应的初始3#顶点数值集合</param>
        /// <param name="vertex3Interval">3#顶点区间</param>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <param name="edge1">边长1</param>
        /// <param name="edge2">边长2</param>
        /// <param name="edge3">边长3</param>
        /// <param name="combinationEnum">
        /// <inheritdoc cref="GetSpecificCombinationsOfVertices(
        /// IEnumerable{ValueTuple{ValueTuple{Point, Point}, ValueTuple{Point, Point}}}, int)" 
        /// path="/param[2]"/></param>
        /// <param name="samplingSpacingAtStart">初始采样间距</param>
        /// <param name="epsilon">容许误差</param>
        /// <returns>最接近给定的边长3时对应的3#顶点数值集合。</returns>
        internal static IEnumerable<double> IterativeGetVertex3MinExtremeValues(
            IEnumerable<(Point vertex1, Point vertex2)> initialCombinations,
            IEnumerable<double> initialVertex3Values,
            PointsInterval vertex3Interval,
            Line line1,
            Line line2,
            double edge1,
            double edge2,
            double edge3,
            int combinationEnum,
            double samplingSpacingAtStart,
            double epsilon = GeometryConstants.DISTANCE_EPSILON) {
            if (initialCombinations is null) {
                throw new ArgumentNullException(nameof(initialCombinations));
            }

            if (initialVertex3Values is null) {
                throw new ArgumentNullException(nameof(initialVertex3Values));
            }

            var cntC = initialCombinations.Count();
            var cntV = initialVertex3Values.Count();

            if (cntC == 0) {
                throw new ArgumentException($"“{nameof(initialCombinations)}”的元素数量不应为0。");
            }

            if (cntV == 0) {
                throw new ArgumentException($"“{nameof(initialVertex3Values)}”的元素数量不应为0。");
            }

            if (cntC != cntV) {
                throw new ArgumentException(
                    $"“{nameof(initialCombinations)}”的元素数量" +
                    $"与“{nameof(initialVertex3Values)}”的元素数量不一致。");
            }

            if (vertex3Interval is null) {
                throw new ArgumentNullException(nameof(vertex3Interval));
            }

            if (line1 is null) {
                throw new ArgumentNullException(nameof(line1));
            }

            if (line2 is null) {
                throw new ArgumentNullException(nameof(line2));
            }

            var minExtremeValues = GetVertex3MinExtremeValues(initialCombinations, initialVertex3Values, edge3);

            var spacing = samplingSpacingAtStart;
            var cnt = 0;
            while (spacing > epsilon) {
                spacing *= 0.1;
                cnt++;

                var vertex3Values = new List<double>();
                foreach (var value in minExtremeValues) {
                    foreach (var v in vertex3Interval.GetValuesAround(value, spacing, 10)) {
                        if (vertex3Values.Any() && v <= vertex3Values.Last()) continue;
                        vertex3Values.Add(v);
                    }
                }

                var vertices = GetVertices(vertex3Interval, vertex3Values, line1, line2, edge1, edge2);

                var combinations = GetSpecificCombinationsOfVertices(vertices, combinationEnum);
                minExtremeValues = GetVertex3MinExtremeValues(combinations, vertex3Values, edge3);
            }

            return minExtremeValues;
        }

        /// <summary>
        /// 求最终解。
        /// </summary>
        /// <param name="vertex3MinExtremeValues">最接近给定的边长3时对应的3#顶点数值</param>
        /// <param name="vertex3Interval">3#顶点区间</param>
        /// <param name="line1">直线1</param>
        /// <param name="line2">直线2</param>
        /// <param name="edge1">边长1</param>
        /// <param name="edge2">边长2</param>
        /// <param name="edge3">边长3</param>
        /// <param name="combinationEnum">
        /// <inheritdoc cref="GetSpecificCombinationsOfVertices(
        /// IEnumerable{ValueTuple{ValueTuple{Point, Point}, ValueTuple{Point, Point}}}, int)" 
        /// path="/param[2]"/></param>
        /// <param name="epsilon">容许误差</param>
        /// <returns>最终有效解的三个顶点组合的集合。</returns>
        internal static IEnumerable<(Point vertex1, Point vertex2, Point vertex3)> FinalSolutionOfVertices(
            IEnumerable<double> vertex3MinExtremeValues,
            PointsInterval vertex3Interval,
            Line line1,
            Line line2,
            double edge1,
            double edge2,
            double edge3,
            int combinationEnum,
            double epsilon = GeometryConstants.DISTANCE_EPSILON) {

            if (vertex3MinExtremeValues is null) {
                throw new ArgumentNullException(nameof(vertex3MinExtremeValues));
            }

            if (vertex3MinExtremeValues.Count() == 0) {
                yield break;
            }

            if (vertex3Interval is null) {
                throw new ArgumentNullException(nameof(vertex3Interval));
            }

            if (line1 is null) {
                throw new ArgumentNullException(nameof(line1));
            }

            if (line2 is null) {
                throw new ArgumentNullException(nameof(line2));
            }

            var count = -1;
            var vertices = GetVertices(vertex3Interval, vertex3MinExtremeValues, line1, line2, edge1, edge2);

            foreach (var (vertex1, vertex2) in GetSpecificCombinationsOfVertices(vertices, combinationEnum)) {

                count++;
                if (Math.Abs(Distance.PointToPoint(vertex1, vertex2) - edge3) <= epsilon) {
                    yield return (vertex1, vertex2, vertex3Interval.GetPoint(vertex3MinExtremeValues.ElementAt(count)));
                }
            }
        }

        /// <summary>
        /// 根据任意四个不相同的点求球体中心点。
        /// </summary>
        /// <param name="p0">给定任意点0</param>
        /// <param name="p1">给定任意点1</param>
        /// <param name="p2">给定任意点2</param>
        /// <param name="p3">给定任意点3</param>
        /// <returns>球体的中心点。如果给定四点共面，则无解，返回 null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">给定的四个点必须均不相等。</exception>
        public static Point CenterOfSphere(Point p0, Point p1, Point p2, Point p3) {
            if (p0 is null) {
                throw new ArgumentNullException(nameof(p0));
            }

            if (p1 is null) {
                throw new ArgumentNullException(nameof(p1));
            }

            if (p2 is null) {
                throw new ArgumentNullException(nameof(p2));
            }

            if (p3 is null) {
                throw new ArgumentNullException(nameof(p3));
            }

            if (p0 == p1 || p0 == p2 || p0 == p3 || p1 == p2 || p1 == p3 || p2 == p3) {
                throw new ArgumentException("给定的四个点必须均不相等。");
            }

            var m = new Matrix();
            m[0, 0] = p0.X - p1.X; m[0, 1] = p0.Y - p1.Y; m[0, 2] = p0.Z - p1.Z;
            m[1, 0] = p2.X - p3.X; m[1, 1] = p2.Y - p3.Y; m[1, 2] = p2.Z - p3.Z;
            m[2, 0] = p1.X - p2.X; m[2, 1] = p1.Y - p2.Y; m[2, 2] = p1.Z - p2.Z;
            m[3, 0] = (p0.X * p0.X - p1.X * p1.X + p0.Y * p0.Y - p1.Y * p1.Y + p0.Z * p0.Z - p1.Z * p1.Z) * 0.5;
            m[3, 1] = (p2.X * p2.X - p3.X * p3.X + p2.Y * p2.Y - p3.Y * p3.Y + p2.Z * p2.Z - p3.Z * p3.Z) * 0.5;
            m[3, 2] = (p1.X * p1.X - p2.X * p2.X + p1.Y * p1.Y - p2.Y * p2.Y + p1.Z * p1.Z - p2.Z * p2.Z) * 0.5;

            return m.Cramer();
        }
    }
}
