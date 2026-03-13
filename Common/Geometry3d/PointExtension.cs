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
 *  PointExtension.cs: extension of Tekla.Structures.Geometry3d.Point
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Point"/> 的扩展。
    /// </summary>
    public static class PointExtension {
        /// <summary>
        /// 获取点的字符串表示形式。
        /// <para></para>
        /// </summary>
        /// <remarks><b>* 官方实现的 <see cref="Point.ToString()"/> 方法，
        /// 输出的是保留3位有效数字的字符串，不能输出其他样式。本实现可自定义输出样式。</b>
        /// <para>有关数字格式字符串的详细信息，请参阅
        /// <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings">标准数字格式字符串</a>
        /// 和 <a href="https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/custom-numeric-format-strings">自定义数字格式字符串</a>。
        /// </para></remarks>
        /// <param name="p">当前点</param>
        /// <param name="format">复合格式字符串。默认 null。</param>
        /// <returns>点的字符串表示形式。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToString(this Point p, string format = default) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            return $"({p.X.ToString(format)}, {p.Y.ToString(format)}, {p.Z.ToString(format)})";
        }
        /// <summary>
        /// 判断给定点是否是零点。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="epsilon">容许误差，小于等于此误差当做 0 处理。默认值 0。</param>
        /// <returns>是零点则返回 true, 否则返回 false。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsZero(this Point p, double epsilon = 0) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (Math.Abs(p.X) <= epsilon && Math.Abs(p.Y) <= epsilon && Math.Abs(p.Z) <= epsilon)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 点与标量的乘法。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="multiplier">乘数</param>
        /// <returns>点与标量的乘积。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point Multiply(this Point p, double multiplier) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            return new Point(p.X * multiplier, p.Y * multiplier, p.Z * multiplier);
        }
        /// <summary>
        /// 点与标量的除法。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="divisor">除数</param>
        /// <returns>点与标量的商。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">除数 <paramref name="divisor"/> 不应为 0.0。</exception>
        public static Point Divide(this Point p, double divisor) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (divisor == 0.0) {
                throw new ArgumentException($"除数“{nameof(divisor)}”不应为 0.0。");
            }

            return new Point(p.X / divisor, p.Y / divisor, p.Z / divisor);
        }
        /// <summary>
        /// 从给定点复制字段值。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="point">给定点</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Copy(this Point p, Point point) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            p.X = point.X;
            p.Y = point.Y;
            p.Z = point.Z;
        }
        /// <summary>
        /// 使用给定点平移当前点。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="point">给定点</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Translate(this Point p, Point point) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            p.X += point.X;
            p.Y += point.Y;
            p.Z += point.Z;
        }
        /// <summary>
        /// 将点从一个变换平面转换到另一个变换平面。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="sourceTP">源变换平面</param>
        /// <param name="targetTP">目标变换平面</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point Transform(this Point p, TransformationPlane sourceTP, TransformationPlane targetTP) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (sourceTP is null) {
                throw new ArgumentNullException(nameof(sourceTP));
            }

            if (targetTP is null) {
                throw new ArgumentNullException(nameof(targetTP));
            }

            return new Point(targetTP.TransformationMatrixToLocal.Transform(sourceTP.TransformationMatrixToGlobal.Transform(p)));
        }
        /// <summary>
        /// 将点从源变换平面转换到当前变换平面。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="sourceTP">源变换平面</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point TransformFrom(this Point p, TransformationPlane sourceTP) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (sourceTP is null) {
                throw new ArgumentNullException(nameof(sourceTP));
            }

            var currentTP = new TransformationPlane(new CoordinateSystem());
            return p.Transform(sourceTP, currentTP);
        }
        /// <summary>
        /// 将点从当前变换平面转换到目标变换平面。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="targetTP">目标变换平面</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point TransformTo(this Point p, TransformationPlane targetTP) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (targetTP is null) {
                throw new ArgumentNullException(nameof(targetTP));
            }

            var currentTP = new TransformationPlane(new CoordinateSystem());
            return p.Transform(currentTP, targetTP);
        }
        /// <summary>
        /// 将点从一个坐标系转换到另一个坐标系。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="sourceCS">源坐标系</param>
        /// <param name="targetCS">目标坐标系</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point Transform(this Point p, CoordinateSystem sourceCS, CoordinateSystem targetCS) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (sourceCS is null) {
                throw new ArgumentNullException(nameof(sourceCS));
            }

            if (targetCS is null) {
                throw new ArgumentNullException(nameof(targetCS));
            }

            var matrix = MatrixFactory.ByCoordinateSystems(sourceCS, targetCS);
            return matrix.Transform(p);
        }
        /// <summary>
        /// 将点从源坐标系转换到当前坐标系。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="sourceCS">源坐标系</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point TransformFrom(this Point p, CoordinateSystem sourceCS) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (sourceCS is null) {
                throw new ArgumentNullException(nameof(sourceCS));
            }

            return p.Transform(sourceCS, new CoordinateSystem());
        }
        /// <summary>
        /// 将点从当前坐标系转换到目标坐标系。
        /// </summary>
        /// <param name="p">当前点</param>
        /// <param name="targetCS">目标坐标系</param>
        /// <returns>转换后的新点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point TransformTo(this Point p, CoordinateSystem targetCS) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            if (targetCS is null) {
                throw new ArgumentNullException(nameof(targetCS));
            }

            return p.Transform(new CoordinateSystem(), targetCS);
        }
    }
}
