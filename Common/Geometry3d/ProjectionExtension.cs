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
 *  ProjectionExtension.cs: extension of Tekla.Structures.Geometry3d.Projection
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using Tekla.Structures.Geometry3d;

namespace Muggle.TsExtensions.Common.Geometry3d {
    /// <summary>
    /// <see cref="Tekla.Structures.Geometry3d"/>.<see cref="Projection"/> 的扩展。
    /// </summary>
    public static class ProjectionExtension {
        /// <summary>
        /// 获取向量在平面上投影的向量。
        /// </summary>
        /// <remarks>根据以下公式计算：<code>P = V - (V∙N / ||N||^2) * N</code></remarks>
        /// <param name="vector">要投影的向量</param>
        /// <param name="gPlane">要投影的平面</param>
        /// <returns>投影的向量。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector VectorToPlane(Vector vector, GeometricPlane gPlane) {
            if (vector is null) {
                throw new ArgumentNullException(nameof(vector));
            }

            if (gPlane is null) {
                throw new ArgumentNullException(nameof(gPlane));
            }

            // GeometricPlane.Normal 不可能是零向量，不需要验证
            return new Vector(vector - Vector.Dot(vector, gPlane.Normal) / Math.Pow(gPlane.Normal.GetLength(), 2) * gPlane.Normal);
        }

        /// <summary>
        /// 获取点在线段或其延长线上的投影点。
        /// </summary>
        /// <param name="point">给定点</param>
        /// <param name="lineSegment">给定线段</param>
        /// <returns>点在线段或其延长线上的投影点。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Point PointToLineSegment(Point point, LineSegment lineSegment) {
            if (point is null) {
                throw new ArgumentNullException(nameof(point));
            }
            if (lineSegment is null) {
                throw new ArgumentNullException(nameof(lineSegment));
            }
            var line = new Line(lineSegment.Point1, lineSegment.Point2);
            var projection = Projection.PointToLine(point, line);
            return projection;
        }
    }
}
