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
 *  VectorExtension.cs: extension of Tekla.Structures.Geometry3d.Vector
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Vector"/> 的扩展。
    /// </summary>
    public static class VectorExtension {
        /// <summary>
        /// 返回当前向量用给定长度标准化后的新向量。
        /// </summary>
        /// <param name="vector">当前向量</param>
        /// <param name="newLength">给定长度</param>
        /// <returns>当前向量标准化后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector GetNormal(this Vector vector, double newLength) {
            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            var v = new Vector(vector);
            v.Normalize(newLength);

            return v;
        }
        /// <summary>
        /// 获取向量之间更精确的角度。
        /// </summary>
        /// <remarks><para><b>* 官方实现的 <see cref="Vector.GetAngleBetween(Vector)"/> 方法，
        /// 对于一些比较小的角度，会按 0 返回。本方法可以返回更精确一些的角度。根据以下计算公式计算：
        /// <code>cos(θ) = U∙V / (||U|| * ||V||)</code></b></para>
        /// <para><b>* 官方实现的 <see cref="Vector.GetAngleBetween(Vector)"/> 方法，
        /// 约定零向量与任意有效向量的角度均为π/2，本方法也依此约定实现。</b></para>
        /// <para><b>* 官方实现的 <see cref="Vector.GetAngleBetween(Vector)"/> 方法，
        /// 输入参数为null时将引发异常：System.NullReferenceException，本方法与此行为一致。</b></para>
        /// </remarks>
        /// <param name="v">当前向量</param>
        /// <param name="vector">给定向量</param>
        /// <returns>向量之间的角度,取值范围 [0.0, π]。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double GetAngleBetween_Precisely(this Vector v, Vector vector) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            if (vector.IsZero() || v.IsZero()) return Math.PI * 0.5;

            return Math.Acos(v.Dot(vector) / (v.GetLength() * vector.GetLength()));
        }
        /// <summary>
        /// 获取向量之间具有方向性的角度。
        /// 正方向由 <paramref name="normal"/> 决定（仅需指示大致方向），
        /// 从当前向量起始，到给定向量终止。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="vector">给定向量</param>
        /// <param name="normal">法向（仅需指示大致方向）</param>
        /// <returns>向量之间具有方向性的角度（弧度制），取值范围 [0.0, 2π)。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double GetAngleBetween_WithDirection(this Vector v, Vector vector, Vector normal) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            if (normal is null) {
                throw new ArgumentNullException(nameof(normal));
            }

            var angle = v.GetAngleBetween_Precisely(vector);
            var cross = v.Cross(vector);
            var dot = Vector.Dot(cross, normal);

            return dot >= 0 ? angle : 2 * Math.PI - angle;
        }
        /// <summary>
        /// 将向量从一个变换平面转换到另一个变换平面。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="sourceTP">源变换平面</param>
        /// <param name="targetTP">目标变换平面</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector Transform(this Vector v, TransformationPlane sourceTP, TransformationPlane targetTP) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (sourceTP is null) {
                throw new ArgumentNullException(nameof(sourceTP));
            }

            if (targetTP is null) {
                throw new ArgumentNullException(nameof(targetTP));
            }

            var head = targetTP.TransformationMatrixToLocal.Transform(sourceTP.TransformationMatrixToGlobal.Transform(v));
            var tail = targetTP.TransformationMatrixToLocal.Transform(sourceTP.TransformationMatrixToGlobal.Transform(new Point()));
            return new Vector(head - tail);
        }
        /// <summary>
        /// 将向量从一个坐标系转换到另一个坐标系。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="sourceCS">源坐标系</param>
        /// <param name="targetCS">目标坐标系</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector Transform(this Vector v, CoordinateSystem sourceCS, CoordinateSystem targetCS) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (sourceCS is null) {
                throw new ArgumentNullException(nameof(sourceCS));
            }

            if (targetCS is null) {
                throw new ArgumentNullException(nameof(targetCS));
            }

            var matrix = MatrixFactory.ByCoordinateSystems(sourceCS, targetCS);
            var head = matrix.Transform(v);
            var tail = matrix.Transform(new Point());
            return new Vector(head - tail);
        }
        /// <summary>
        /// 将向量从源变换平面转换到当前变换平面。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="sourceTp">源变换平面</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector TransformFrom(this Vector v, TransformationPlane sourceTp) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (sourceTp is null) {
                throw new ArgumentNullException(nameof(sourceTp));
            }

            var currentTP = new TransformationPlane(new CoordinateSystem());
            return v.Transform(sourceTp, currentTP);
        }
        /// <summary>
        /// 将向量从源坐标系转换到当前坐标系。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="sourceCS">源坐标系</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector TransformFrom(this Vector v, CoordinateSystem sourceCS) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (sourceCS is null) {
                throw new ArgumentNullException(nameof(sourceCS));
            }

            var currentCS = new CoordinateSystem();
            return v.Transform(sourceCS, currentCS);
        }
        /// <summary>
        /// 将向量从当前变换平面转换到目标变换平面。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="targetTP">目标变换平面</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector TransformTo(this Vector v, TransformationPlane targetTP) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (targetTP is null) {
                throw new ArgumentNullException(nameof(targetTP));
            }

            var currentTP = new TransformationPlane(new CoordinateSystem());
            return v.Transform(currentTP, targetTP);
        }
        /// <summary>
        /// 将向量从当前坐标系转换到目标坐标系。
        /// </summary>
        /// <param name="v">当前向量</param>
        /// <param name="targetCS">目标坐标系</param>
        /// <returns>转换后的新向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector TransformTo(this Vector v, CoordinateSystem targetCS) {
            if (v is null) {
                throw new ArgumentNullException(nameof(v));
            }

            if (targetCS is null) {
                throw new ArgumentNullException(nameof(targetCS));
            }

            var currentCS = new CoordinateSystem();
            return v.Transform(currentCS, targetCS);
        }
    }
}
