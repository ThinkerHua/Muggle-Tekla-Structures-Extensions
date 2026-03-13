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
 *  PointsInterval.cs: a solution to the points interval problem
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// 点的取值区间（闭区间，不支持开区间）。
    /// </summary>
    /// <remarks>虽然可以直接用 [<see cref="PointsInterval.StartPoint"/>, <see cref="PointsInterval.EndPoint"/>]
    /// 来表示点区间，但实际应用中可能存在精度问题。
    /// 本实现旨在提供一种解决方案，规避这种误差，因此在本类内部，不采用容许误差的设计。
    /// <para>具体方案为：使用原点 <see cref="PointsInterval.Origin"/> 和<b>单位向量</b>
    /// <see cref="PointsInterval.Direction"/> 建立数轴，用实数 <see cref="PointsInterval.Start"/> 和
    /// <see cref="PointsInterval.End"/> 来表示区间。将点转化成在此数轴上对应的实数
    /// （用与原点之间带有方向的距离表示，也即 <see cref="PointsInterval.Direction"/> 的倍数）。
    /// 在具体问题中，使用实数来讨论，得出结论后，再将实数转换成点。</para>
    /// <para>同时本类还实现了枚举，在枚举前应先设置枚举间隔 <see cref="PointsInterval.EnumInterval"/>。</para>
    /// </remarks>
    public class PointsInterval : IEnumerable, IEnumerable<Point> {
        private readonly Point _origin = new Point();
        private readonly Vector _direction = new Vector(1, 0, 0);
        private double _start = 0;
        private double _end = 0;
        private double _enumInterval = GeometryConstants.DISTANCE_EPSILON;

        /// <summary>
        /// 点区间的枚举器。
        /// </summary>
        public struct Enumerator : IEnumerator<Point> {
            private readonly Point _origin;
            private readonly Vector _direction;
            private readonly double _width;
            private int _index;
            private readonly double _interval;

            /// <summary>
            /// 使用给定点区间的属性初始化枚举器。
            /// </summary>
            /// <param name="itvl">给定点区间</param>
            public Enumerator(PointsInterval itvl) {
                _origin = new Point(itvl.Origin);
                _direction = new Vector(itvl.Direction);
                _width = itvl.Width;
                _index = -1;
                _interval = itvl.EnumInterval;
            }

            /// <inheritdoc cref="IEnumerator{T}.Current"/>
            public readonly Point Current => _origin + _direction * (_index * _interval);

            readonly object IEnumerator.Current => Current;

            /// <inheritdoc cref="IDisposable.Dispose"/>
            public readonly void Dispose() {

            }

            /// <inheritdoc cref="IEnumerator.MoveNext"/>
            public bool MoveNext() {
                _index++;
                return _index * _interval <= _width;
            }

            /// <inheritdoc cref="IEnumerator.Reset"/>
            public void Reset() {
                _index = -1;
            }
        }

        /// <summary>
        /// 数轴的原点。
        /// </summary>
        /// <value>默认值 (0.0, 0.0, 0.0)。</value>
        public Point Origin {
            get => _origin;
            set => _origin.Copy(value);
        }

        /// <summary>
        /// 数轴的方向。
        /// </summary>
        /// <remarks>不接受零向量（赋值为零向量时不处理）。自动设置成单位向量。</remarks>
        /// <value>默认值 (1.0, 0.0, 0.0)。</value>
        public Vector Direction {
            get => _direction;
            set {
                //  不接受零向量
                if (!value.IsZero()) {
                    _direction.Copy(value);
                    _direction.Normalize();
                }
            }
        }

        /// <summary>
        /// 区间起点值。
        /// </summary>
        /// <remarks>将始终保持 <see cref="Start"/> &lt;= <see cref="End"/>，
        /// 若赋值大于 <see cref="End"/>，则自动将 <see cref="End"/> 
        /// 设置为等于 <see cref="Start"/>。</remarks>
        public double Start {
            get => _start;
            set {
                _start = value;
                if (_start > _end) _end = _start;
            }
        }

        /// <summary>
        /// 区间终点值。
        /// </summary>
        /// <remarks>将始终保持 <see cref="Start"/> &lt;= <see cref="End"/>，
        /// 若赋值小于 <see cref="Start"/>，则自动将 <see cref="Start"/> 
        /// 设置为等于 <see cref="End"/>。</remarks>
        public double End {
            get => _end;
            set {
                _end = value;
                if (_start > _end) _start = _end;
            }
        }

        /// <summary>
        /// 区间的宽度。
        /// </summary>
        public double Width => _end - _start;

        /// <summary>
        /// 区间起点。
        /// </summary>
        public Point StartPoint => _origin + _start * _direction;

        /// <summary>
        /// 区间终点。
        /// </summary>
        public Point EndPoint => _origin + _end * _direction;

        /// <summary>
        /// 枚举间隔。
        /// </summary>
        /// <value>默认值 <see cref="GeometryConstants.DISTANCE_EPSILON"/>。</value>
        public double EnumInterval {
            get => _enumInterval;
            set {
                if (value >= 0) _enumInterval = value;
            }
        }

        /// <summary>
        /// 使用默认值构造点区间，<see cref="Origin"/> 为零点，
        /// <see cref="Direction"/> 为单位 X 向量，<see cref="Start"/> 和 <see cref="End"/> 均为 0.0。
        /// </summary>
        public PointsInterval() { }

        /// <summary>
        /// 使用给定属性值构造点区间。
        /// </summary>
        /// <param name="origin">原点</param>
        /// <param name="direction">方向</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PointsInterval(Point origin, Vector direction, double start, double end) {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
            Direction = direction ?? throw new ArgumentNullException(nameof(direction));
            Start = start;
            End = end;
        }

        /// <summary>
        /// 用一条直线构造点区间，<see cref="Start"/> 为 <see cref="double.NegativeInfinity"/>，
        /// <see cref="End"/> 为 <see cref="double.PositiveInfinity"/>。
        /// </summary>
        /// <param name="line">给定直线</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="line"/>.Direction 不应为零向量。</exception>
        public PointsInterval(Line line) {
            if (line is null) {
                throw new ArgumentNullException(nameof(line));
            }
            if (line.Direction.IsZero())
                throw new ArgumentException($"“{nameof(line)}”.Direction 不应为零向量。");

            Origin = line.Origin;
            Direction = line.Direction;
            Start = double.NegativeInfinity;
            End = double.PositiveInfinity;
        }

        /// <summary>
        /// 用一条线段构造点区间，<see cref="Start"/> 为 0.0，<see cref="End"/> 为线段长度。
        /// </summary>
        /// <param name="lineSegment">给定线段</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="lineSegment"/>的长度不应等于0.0。</exception>
        public PointsInterval(LineSegment lineSegment) {
            if (lineSegment is null) {
                throw new ArgumentNullException(nameof(lineSegment));
            }

            Direction = lineSegment.GetDirectionVector();
            if (Direction.IsZero())
                throw new ArgumentException($"“{nameof(lineSegment)}”长度不应等于0.0。");

            Origin = lineSegment.StartPoint;
            End = lineSegment.Length();
        }

        /// <summary>
        /// 使用给定点区间构造新实例。
        /// </summary>
        /// <param name="itvl">给定点区间</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PointsInterval(PointsInterval itvl) {
            if (itvl is null) {
                throw new ArgumentNullException(nameof(itvl));
            }
            Origin = itvl.Origin;
            Direction = itvl.Direction;
            Start = itvl.Start;
            End = itvl.End;
            EnumInterval = itvl.EnumInterval;
        }

        /// <summary>
        /// 获取当前点区间的字符串表示形式。
        /// </summary>
        /// <remarks>有关数字格式字符串的详细信息，请参阅
        /// <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings">标准数字格式字符串</a>
        /// 和 <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-numeric-format-strings">自定义数字格式字符串</a>。
        /// </remarks>
        /// <param name="format">复合格式字符串</param>
        /// <returns>当前点区间的字符串表示形式。</returns>
        public string ToString(string format = default) {
            return $"Origin = {_origin.ToString(format)},\n" +
                $"Direction = {_direction.ToString(format)},\n" +
                $"Start = {_start.ToString(format)}, StartPoint = {StartPoint.ToString(format)},\n" +
                $"End = {_end.ToString(format)}, EndPoint = {EndPoint.ToString(format)}\n";
        }

        /// <summary>
        /// 判断区间是否包含给定点。
        /// </summary>
        /// <param name="point">给定点</param>
        /// <param name="entireLine">是否用整个实数轴判断，默认 false。</param>
        /// <returns>包含返回 true，不包含返回 false。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete("由于计算精度问题，可能不能得出正确结果，谨慎使用。" +
            "应优先转换思路，使用“PointsInterval.Contains(double value)”方法。", false)]
        public bool Contains(Point point, bool entireLine = false) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            var value = GetValue(point);

            if (double.IsNaN(value) || !entireLine && (value < Start || value > End))
                return false;

            return true;
        }

        /// <summary>
        /// 判断区间是否包含给定实数。
        /// </summary>
        /// <param name="value">给定实数。</param>
        /// <returns>包含返回 true，不包含返回 false。</returns>
        public bool Contains(double value) {
            if (double.IsNaN(value) || value < Start || value > End) return false;
            return true;
        }

        /// <summary>
        /// 获取给定点在整个数轴上对应的实数值。
        /// </summary>
        /// <param name="point">给定点</param>
        /// <returns>给定点在整个数轴上对应的实数值。如果不在数轴上，则返回 <see cref="double.NaN"/>。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete("由于计算精度问题，可能不能得出正确结果，谨慎使用。" +
            "应优先转换思路，使用“PointsInterval.GetPoint(double value)”方法。", false)]
        public double GetValue(Point point) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }
            if (double.IsNaN(point.X) || double.IsNaN(point.Y) || double.IsNaN(point.Z))
                return double.NaN;

            var vector = new Vector(point - Origin);
            if (vector.IsZero()) return 0;

            //  不共线
            if (!Vector.Cross(vector, Direction).IsZero()) return double.NaN;

            //  完整表达式应该是 Vector.Dot(vector, Direction) / Vector.Dot(Direction, Direction)
            //  由于 Direction 是单位向量，其自身的点积必定等于1，所以除数可以简化掉
            return Vector.Dot(vector, Direction);
        }

        /// <summary>
        /// 获取给定实数值在整个数轴上对应的点。
        /// </summary>
        /// <param name="value">给定实数值</param>
        /// <returns>实数值对应的点。</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/> 应当是有效数值，不应是 <see cref="double.NaN"/>。</exception>
        public Point GetPoint(double value) {
            if (double.IsNaN(value)) {
                throw new ArgumentException($"“{nameof(value)} = NaN”，不是有效数值。");
            }

            return Origin + value * Direction;
        }

        /// <summary>
        /// 获取<b>区间内</b>给定实数值左右各数个连续间距的值的集合。
        /// </summary>
        /// <param name="value">给定实数值。</param>
        /// <param name="dis">要获取的连续值之间的间距，正数。</param>
        /// <param name="num">左右各指定数量个值，正整数。</param>
        /// <returns>区间内给定实数值左右各数个连续间距的值的集合。</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/>不在区间内。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dis"/> &lt;= 0.0 或 <paramref name="num"/> &lt;= 0.0 时引发。</exception>
        public IEnumerable<double> GetValuesArround(double value, double dis, int num) {
            if (double.IsNaN(value) || value < _start || value > _end) {
                var msg = ToString(null);
                msg = msg.Replace("\n", " ");
                msg = msg.Remove(msg.Length - 1, 1);
                throw new ArgumentException($"“{nameof(value)} = {value}”不在区间“{msg}”内。");
            }
            if (dis <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(dis)} = {dis}”不应小于等于0.0。");
            }
            if (num <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(num)} = {num}”不应小于等于0。");
            }


            var num_Left = (value - num * dis) > _start ? num : (int)((value - _start) / dis);
            var num_Right = (value + num * dis) < _end ? num : (int)((_end - value) / dis);
            value -= dis * num_Left;
            for (int i = 0; i <= num_Left + num_Right; i++) {
                yield return value + i * dis;
            }
        }

        /// <summary>
        /// 获取<b>区间内</b>给定点左右各数个连续间距的点的集合。
        /// </summary>
        /// <param name="point">区间内给定点。</param>
        /// <param name="dis">要获取的连续点之间的间距，正数。</param>
        /// <param name="num">左右各指定数量个点，正整数。</param>
        /// <returns>区间内给定点左右各数个连续间距的点的集合。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="point"/>不在区间内。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dis"/> &lt;= 0.0 或 <paramref name="num"/> &lt;= 0.0 时引发。</exception>
        [Obsolete("由于计算精度问题，可能出现误判参数“point”不在区间内，谨慎使用。" +
            "应优先转换思路，使用“PointsInterval.GetPointsArround(double value, double dis, int num)”方法。", false)]
        public IEnumerable<Point> GetPointsArround(Point point, double dis, int num) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            var value = GetValue(point);
            if (double.IsNaN(value) || value < _start || value > _end) {
                var msg = ToString(null);
                msg = msg.Replace("\n", " ");
                msg = msg.Remove(msg.Length - 1, 1);
                throw new ArgumentException($"“{nameof(point)} = {point.ToString(null)}”不在区间“{msg}”内。");
            }
            if (dis <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(dis)} = {dis}”不应小于等于0.0。");
            }
            if (num <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(num)} = {num}”不应小于等于0。");
            }

            return GetPointsArround(value, dis, num);
        }

        /// <summary>
        /// 获取<b>区间内</b>给定实数值对应的点左右各数个连续间距的点的集合。
        /// </summary>
        /// <param name="value">给定实数值。</param>
        /// <param name="dis">要获取的连续点之间的间距，正数。</param>
        /// <param name="num">左右各指定数量个点，正整数。</param>
        /// <returns>区间内给定实数值对应的点左右各数个连续间距的点的集合。</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/>不在区间内。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dis"/> &lt;= 0.0 或 <paramref name="num"/> &lt;= 0.0 时引发。</exception>
        public IEnumerable<Point> GetPointsArround(double value, double dis, int num) {
            if (double.IsNaN(value) || value < _start || value > _end) {
                var msg = ToString(null);
                msg = msg.Replace("\n", " ");
                msg = msg.Remove(msg.Length - 1, 1);
                throw new ArgumentException($"“{nameof(value)} = {value}”不在区间“{msg}”内。");
            }
            if (dis <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(dis)} = {dis}”不应小于等于0.0。");
            }
            if (num <= 0) {
                throw new ArgumentOutOfRangeException($"“{nameof(num)} = {num}”不应小于等于0。");
            }


            var num_Left = (value - num * dis) > _start ? num : (int)((value - _start) / dis);
            var num_Right = (value + num * dis) < _end ? num : (int)((_end - value) / dis);
            value -= dis * num_Left;
            for (int i = 0; i <= num_Left + num_Right; i++) {
                yield return GetPoint(value + i * dis);
            }
        }

        /// <summary>
        /// 用当前点区间的原点和方向变换给定点区间。
        /// </summary>
        /// <param name="itvl">给定点区间</param>
        /// <returns>
        ///     如果给定点区间与当前点区间在同一数轴上，则返回true，同时变换给定点区间；
        ///     否则返回false，不处理给定点区间。
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        public bool Transform(PointsInterval itvl) {
            if (itvl is null) {
                throw new ArgumentNullException(nameof(itvl));
            }

            //  不平行
            if (!Vector.Cross(Direction, itvl.Direction).IsZero()) return false;
            //  平行但不共线
            if (!Contains(itvl.Origin, true)) return false;

            var startPoint = itvl.GetPoint(itvl.Start);
            var endPoint = itvl.GetPoint(itvl.End);

            var start = GetValue(startPoint);
            var end = GetValue(endPoint);
            if (start > end) (start, end) = (end, start);

            itvl.Origin = Origin;
            itvl.Direction = Direction;
            itvl.Start = start;
            itvl.End = end;

            return true;
        }

        /// <summary>
        /// 获取当前点区间与给定点区间的交集。交集原点和方向与当前点区间相同。
        /// </summary>
        /// <param name="itvl">给定点区间</param>
        /// <returns>当前点区间与给定点区间的交集。不存在交集则返回 null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PointsInterval Intersect(PointsInterval itvl) {
            if (itvl is null) {
                throw new ArgumentNullException(nameof(itvl));
            }

            return Intersect(this, itvl);
        }

        /// <summary>
        /// 获取两个给定点区间的交集，其原点和方向与 <paramref name="itvl1"/> 相同。
        /// </summary>
        /// <param name="itvl1">给定点区间1</param>
        /// <param name="itvl2">给定点区间2</param>
        /// <returns>两个给定点区间的交集。不存在交集则返回 null。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static PointsInterval Intersect(PointsInterval itvl1, PointsInterval itvl2) {
            if (itvl1 is null) {
                throw new ArgumentNullException(nameof(itvl1));
            }
            if (itvl2 is null) {
                throw new ArgumentNullException(nameof(itvl2));
            }

            var normal = Vector.Cross(itvl1.Direction, itvl2.Direction);
            if (normal.IsZero()) {
                //  平行

                //  不共线
#pragma warning disable CS0618 // 类型或成员已过时
                if (!itvl1.Contains(itvl2.Origin, true)) return null;
#pragma warning restore CS0618 // 类型或成员已过时

                //  共线
                var p1 = itvl2.GetPoint(itvl2.Start);
                var p2 = itvl2.GetPoint(itvl2.End);
#pragma warning disable CS0618 // 类型或成员已过时
                var v1 = itvl1.GetValue(p1);
                var v2 = itvl1.GetValue(p2);
#pragma warning restore CS0618 // 类型或成员已过时
                if (v1 > v2) (v1, v2) = (v2, v1);

                if (v1 > itvl1.End || v2 < itvl1.Start) return null;

                return new PointsInterval(itvl1.Origin, itvl1.Direction,
                    itvl1.Start > v1 ? itvl1.Start : v1,
                    itvl1.End < v2 ? itvl1.End : v2);
            } else {
                //  不平行，转变成求两条直线交点的问题

                var vector = new Vector(itvl2.Origin - itvl1.Origin);
                //  交点就是原点
                if (vector.IsZero()) return new PointsInterval(itvl1.Origin, itvl1.Direction, 0, 0);

                var dot = Vector.Dot(vector, normal);
                //  不相交
                if (dot != 0) return null;

                //  公式 s = ((P2 - P1)×V2)∙(V1×V2)/|| V1×V2 || ^2
                //  参见IntersectionExtension.LineToLine注释
                var value = Vector.Dot(Vector.Cross(vector, itvl2.Direction), normal) / Math.Pow(normal.GetLength(), 2);
                return new PointsInterval(itvl1.Origin, itvl1.Direction, value, value);
            }
        }

        /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
        public IEnumerator GetEnumerator() {
            return new Enumerator(this);
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        IEnumerator<Point> IEnumerable<Point>.GetEnumerator() {
            return new Enumerator(this);
        }
    }
}
