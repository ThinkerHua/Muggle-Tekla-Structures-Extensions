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
 *  MatrixFactoryExtension.cs: extension of Tekla.Structures.Geometry3d.MatrixFactory
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="MatrixFactory"/> 的扩展。
    /// </summary>
    /// <remarks><b>* Tekla Open API 内部，矩阵操作对象为坐标系。
    /// 参考官方库的 <see cref="MatrixFactory.Rotate(double, Vector)"/> 方法，
    /// 产生的矩阵对点进行变换操作，实际得到的结果是反方向旋转的。
    /// 本类中的扩展方法创建的矩阵，保持与官方实现行为一致。</b></remarks>
    public static class MatrixFactoryExtension {
        /// <summary>
        /// 使用给定向量创建平移矩阵。
        /// </summary>
        /// <param name="vector">给定向量</param>
        /// <returns>平移矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Matrix Translate(Vector vector) {
            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            var matrix = new Matrix();
            matrix[3, 0] = -vector.X;
            matrix[3, 1] = -vector.Y;
            matrix[3, 2] = -vector.Z;

            return matrix;
        }

        /// <summary>
        /// 以给定直线为旋转轴创建旋转矩阵。
        /// </summary>
        /// <remarks>官方实现的 <see cref="MatrixFactory.Rotate(double, Vector)"/> 方法，
        /// 只能以穿过当前坐标原点的旋转轴创建旋转矩阵。本实现可根据任意旋转轴创建旋转矩阵。</remarks>
        /// <param name="axis">旋转轴</param>
        /// <param name="radians">旋转角度，弧度制</param>
        /// <returns>旋转矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="axis"/> 的 <see cref="Line.Direction"/> 属性不应为零向量。</exception>
        public static Matrix Rotate(Line axis, double radians) {
            if (axis is null) {
                throw new ArgumentNullException(nameof(axis));
            }

            if (axis.Direction.IsZero()) {
                throw new ArgumentException($"{nameof(axis)} 的 Direction 属性不应为零向量。", nameof(axis));
            }

            var origin = new Point();
            var rM = MatrixFactory.Rotate(radians, axis.Direction);

            var projectedPoint = Projection.PointToLine(origin, axis);
            var currentVector = new Vector(origin - projectedPoint);
            var rotatedVector = MatrixExtension.Transform(rM.GetTranspose(), currentVector);

            // currentVector + translateVector = rotatedVector
            var tM = MatrixFactoryExtension.Translate(new Vector(rotatedVector - currentVector));

            return rM * tM;
        }

        /// <summary>
        /// 使用镜像平面的法向及镜像平面上的任意一点创建镜像矩阵。
        /// </summary>
        /// <param name="normal">镜像平面的法向</param>
        /// <param name="point">镜像平面上的任意一点</param>
        /// <returns>镜像矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="normal"/> 不应是零向量。</exception>
        public static Matrix Mirror(Vector normal, Point point) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }

            if (normal is null) {
                throw new ArgumentNullException(nameof(normal));
            }

            if (normal.IsZero()) {
                throw new ArgumentException($"{nameof(normal)} 不应是零向量", nameof(normal));
            }

            normal = normal.GetNormal();

            var v = Vector.Dot(normal, new Vector(point)) * normal;
            var translate = MatrixFactoryExtension.Translate(v * 2);
            var mirror = new Matrix();
            mirror[0, 0] = 1 - 2 * normal.X * normal.X; mirror[0, 1] = -2 * normal.X * normal.Y; mirror[0, 2] = -2 * normal.X * normal.Z;
            mirror[1, 0] = -2 * normal.Y * normal.X; mirror[1, 1] = 1 - 2 * normal.Y * normal.Y; mirror[1, 2] = -2 * normal.Y * normal.Z;
            mirror[2, 0] = -2 * normal.Z * normal.X; mirror[2, 1] = -2 * normal.Z * normal.Y; mirror[2, 2] = 1 - 2 * normal.Z * normal.Z;

            return mirror * translate;
        }

        /// <summary>
        /// 使用镜像平面的法向及坐标系原点与镜像平面之间的距离创建镜像矩阵。
        /// </summary>
        /// <param name="normal">镜像平面的法向</param>
        /// <param name="distance">坐标系原点与镜像平面之间的距离。正值表示坐标系原点在平面之上，负值表示坐标系原点在平面之下。</param>
        /// <returns>镜像矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"><paramref name="normal"/> 不应是零向量。</exception>
        public static Matrix Mirror(Vector normal, double distance) {
            if (normal is null) {
                throw new ArgumentNullException(nameof(normal));
            }

            if (normal.IsZero()) {
                throw new ArgumentException($"{nameof(normal)} 不应是零向量", nameof(normal));
            }

            normal = normal.GetNormal();
            ;
            var translate = MatrixFactoryExtension.Translate(normal * distance * 2);
            var mirror = new Matrix();
            mirror[0, 0] = 1 - 2 * normal.X * normal.X; mirror[0, 1] = -2 * normal.X * normal.Y; mirror[0, 2] = -2 * normal.X * normal.Z;
            mirror[1, 0] = -2 * normal.Y * normal.X; mirror[1, 1] = 1 - 2 * normal.Y * normal.Y; mirror[1, 2] = -2 * normal.Y * normal.Z;
            mirror[2, 0] = -2 * normal.Z * normal.X; mirror[2, 1] = -2 * normal.Z * normal.Y; mirror[2, 2] = 1 - 2 * normal.Z * normal.Z;

            return mirror * translate;
        }

        /// <summary>
        /// 创建切变矩阵。
        /// </summary>
        /// <param name="xY">沿X轴切变，Y轴切变值</param>
        /// <param name="xZ">沿X轴切变，Z轴切变值</param>
        /// <param name="yX">沿Y轴切变，X轴切变值</param>
        /// <param name="yZ">沿Y轴切变，Z轴切变值</param>
        /// <param name="zX">沿Z轴切变，X轴切变值</param>
        /// <param name="zY">沿Z轴切变，Y轴切变值</param>
        /// <returns>切变矩阵。</returns>
        public static Matrix Shear(double xY = 0, double xZ = 0, double yX = 0, double yZ = 0, double zX = 0, double zY = 0) {
            var matrix = new Matrix();
            matrix[0, 0] = 1.0; matrix[0, 1] = yX; matrix[0, 2] = zX;
            matrix[1, 0] = xY; matrix[1, 1] = 1.0; matrix[1, 2] = zY;
            matrix[2, 0] = xZ; matrix[2, 1] = yZ; matrix[2, 2] = 1.0;

            return matrix;
        }

        /// <summary>
        /// 根据给定坐标系的原点和轴向量创建变换矩阵，使用此矩阵可将点从当前工作平面坐标系变换到目标坐标系。
        /// </summary>
        /// <remarks>官方提供的 <see cref="MatrixFactory.ToCoordinateSystem(CoordinateSystem)"/> 方法，
        /// 只能创建正交非缩放矩阵，此方法可突破此限制（如切变、缩放等）。</remarks>
        /// <param name="axisX">给定坐标系的 X 轴</param>
        /// <param name="axisY">给定坐标系的 Y 轴</param>
        /// <param name="axisZ">给定坐标系的 Z 轴</param>
        /// <param name="origin">给定坐标系的原点</param>
        /// <returns>成功创建的矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Matrix ToCoordinateSystem(Vector axisX, Vector axisY, Vector axisZ, Point origin) {
            if (origin is null) {
                throw new ArgumentNullException(nameof(origin));
            }

            if (axisX is null) {
                throw new ArgumentNullException(nameof(axisX));
            }

            if (axisY is null) {
                throw new ArgumentNullException(nameof(axisY));
            }

            if (axisZ is null) {
                throw new ArgumentNullException(nameof(axisZ));
            }

            var matrix = new Matrix();
            matrix[0, 0] = axisX.X; matrix[0, 1] = axisY.X; matrix[0, 2] = axisZ.X;
            matrix[1, 0] = axisX.Y; matrix[1, 1] = axisY.Y; matrix[1, 2] = axisZ.Y;
            matrix[2, 0] = axisX.Z; matrix[2, 1] = axisY.Z; matrix[2, 2] = axisZ.Z;
            matrix[3, 0] = -(origin.X * axisX.X + origin.Y * axisX.Y + origin.Z * axisX.Z);
            matrix[3, 1] = -(origin.X * axisY.X + origin.Y * axisY.Y + origin.Z * axisY.Z);
            matrix[3, 2] = -(origin.X * axisZ.X + origin.Y * axisZ.Y + origin.Z * axisZ.Z);

            return matrix;
        }

        /// <summary>
        /// 根据给定坐标系的原点和轴向量创建变换矩阵，使用此矩阵可将点从目标坐标系变换到当前工作平面坐标系。
        /// </summary>
        /// <remarks>官方提供的 <see cref="MatrixFactory.FromCoordinateSystem(CoordinateSystem)"/> 方法，
        /// 只能创建正交非缩放矩阵，此方法可突破此限制（如切变、缩放等）。</remarks>
        /// <param name="axisX">给定坐标系的 X 轴</param>
        /// <param name="axisY">给定坐标系的 Y 轴</param>
        /// <param name="axisZ">给定坐标系的 Z 轴</param>
        /// <param name="origin">给定坐标系的原点</param>
        /// <returns>成功创建的矩阵。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Matrix FromCoordinateSystem(Vector axisX, Vector axisY, Vector axisZ, Point origin) {
            return ToCoordinateSystem(axisX, axisY, axisZ, origin).Inverse();
        }
    }
}
