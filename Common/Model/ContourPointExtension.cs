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
 *  ContourPointExtension.cs: extension of Tekla.Structures.Model.ContourPoint
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Muggle.TsExtensions.Common.Geometry3d;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace Muggle.TsExtensions.Common.Model;

/// <summary>
/// <see cref="Tekla.Structures.Model"/>.<see cref="ContourPoint"/> 的扩展。
/// </summary>
public static class ContourPointExtension {
    
    /// <summary>
    /// 克隆一个实例。
    /// </summary>
    /// <param name="contourPoint">当前实例</param>
    /// <returns>克隆的实例。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ContourPoint Clone(this ContourPoint contourPoint) {
        if (contourPoint is null) {
            throw new ArgumentNullException(nameof(contourPoint));
        }

        return new ContourPoint {
            X = contourPoint.X,
            Y = contourPoint.Y,
            Z = contourPoint.Z,
            Chamfer = new Chamfer {
                Type = contourPoint.Chamfer.Type,
                X = contourPoint.Chamfer.X,
                Y = contourPoint.Chamfer.Y,
                DZ1 = contourPoint.Chamfer.DZ1,
                DZ2 = contourPoint.Chamfer.DZ2,
            }
        };
    }

    /// <summary>
    /// 计算此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的和。
    /// </summary>
    /// <param name="cp">此 <see cref="ContourPoint"/></param>
    /// <param name="vector">给定 <see cref="Vector"/></param>
    /// <returns>此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的和。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ContourPoint Addition(this ContourPoint cp, Vector vector) {
        if (cp == null) {
            throw new ArgumentNullException(nameof(cp));
        }

        if (vector == null) {
            throw new ArgumentNullException(nameof(vector));
        }

        var cp2 = cp.Clone();
        cp2.X += vector.X;
        cp2.Y += vector.Y;
        cp2.Z += vector.Z;
        return cp2;
    }

    /// <summary>
    /// 计算此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的差。
    /// </summary>
    /// <param name="cp">此 <see cref="ContourPoint"/></param>
    /// <param name="vector">给定 <see cref="Vector"/></param>
    /// <returns>此 <see cref="ContourPoint"/> 与 给定 <see cref="Vector"/> 的差。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ContourPoint Subtraction(this ContourPoint cp, Vector vector) {
        if (cp == null) {
            throw new ArgumentNullException(nameof(cp));
        }

        if (vector == null) {
            throw new ArgumentNullException(nameof(vector));
        }

        var cp2 = cp.Clone();
        cp2.X -= vector.X;
        cp2.Y -= vector.Y;
        cp2.Z -= vector.Z;
        return cp2;
    }

    /// <summary>
    /// 获取用零碎点模拟的轮廓点集合。
    /// </summary>
    /// <remarks>需自行判断元素是否与 <paramref name="previous"/> 或 <paramref name="next"/> 重合。</remarks>
    /// <param name="current">当前轮廓点</param>
    /// <param name="previous">前一个轮廓点</param>
    /// <param name="next">下一下轮廓点</param>
    /// <param name="epsilon">轮廓线最小线段长</param>
    /// <returns>模拟的轮廓点集合。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static List<ContourPoint> GetSimulatedContourPoints(this ContourPoint current, 
        Point previous, Point next, double epsilon = 5.0) {
        
        if (current == null) {
            throw new ArgumentNullException(nameof(current));
        }

        if (previous == null) {
            throw new ArgumentNullException(nameof(previous));
        }

        if (next == null) {
            throw new ArgumentNullException(nameof(next));
        }

        if (epsilon < GeometryConstants.DISTANCE_EPSILON) {
            throw new ArgumentOutOfRangeException(nameof(epsilon));
        }

        var chamfer = current.Chamfer;
        
        var axisX = new Vector(current - previous).GetNormal();
        var axisY = new Vector(next - current).GetNormal();
        var axisZ = axisX.Cross(axisY).GetNormal();
        var zCrossX = axisZ.Cross(axisX).GetNormal();
        var zCrossY = axisZ.Cross(axisY).GetNormal();
        var angle = axisX.GetAngleBetweenPrecisely(axisY);

        var line1 = new Line(current, axisX);
        var line2 = new Line(current, axisY);

        List<Point> result;
        switch (chamfer.Type) {
        case Chamfer.ChamferTypeEnum.CHAMFER_NONE:
            result = [current];
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_LINE:
            result = [
                current - axisX * chamfer.X,
                current + axisY * chamfer.Y,
            ];
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING:
            var line1Offset = line1.Offset(zCrossX * chamfer.X);
            var line2Offset = line2.Offset(zCrossY * chamfer.X);
            
            var center = IntersectionExtension.LineToLine(line1Offset, line2Offset).StartPoint;
            var startDirection = new Vector(Projection.PointToLine(center, line1) - center);
            var arc = new Arc(center, startDirection, axisX, chamfer.X, angle);
            
            result = GetPointsOnArc(arc);
            if (!result.Last().Equals(arc.EndPoint)) result.Add(arc.EndPoint);
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_ARC:
            var startPoint = current - axisX * chamfer.X;
            arc = new Arc(current, startPoint, axisZ * -1, Math.PI - angle);
            
            result = GetPointsOnArc(arc);
            if (!result.Last().Equals(arc.EndPoint)) result.Add(arc.EndPoint);
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT:
            arc = new Arc(previous, next, current);
            result = GetPointsOnArc(arc);
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_SQUARE:
            line1Offset = line1.Offset(zCrossX * chamfer.Y);
            line2Offset = line2.Offset(zCrossY * chamfer.X);
            var cross = IntersectionExtension.LineToLine(line1Offset, line2Offset).StartPoint;
            result = [
                Projection.PointToLine(cross, line1),
                cross,
                Projection.PointToLine(cross, line2),
            ];
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_SQUARE_PARALLEL:
            line1Offset = line1.Offset(zCrossX * chamfer.Y);
            line2Offset = line2.Offset(zCrossY * chamfer.X);
            result = [
                IntersectionExtension.LineToLine(line1, line2Offset).StartPoint,
                IntersectionExtension.LineToLine(line1Offset, line2Offset).StartPoint,
                IntersectionExtension.LineToLine(line1Offset, line2).StartPoint,
            ];
            break;
        case Chamfer.ChamferTypeEnum.CHAMFER_LINE_AND_ARC:
            line1Offset = line1.Offset(zCrossX * chamfer.Y);
            line2Offset = line2.Offset(zCrossY * chamfer.X);
            if (chamfer.X >= chamfer.Y) {
                startPoint = IntersectionExtension.LineToLine(line1, line2Offset).StartPoint;
                center = startPoint + axisX * chamfer.Y;
                arc = new Arc(center, startPoint, axisZ * -1, Math.PI / 2);
                
                result = GetPointsOnArc(arc);
                if (!result.Last().Equals(arc.EndPoint)) result.Add(arc.EndPoint);
                result.Add(IntersectionExtension.LineToLine(line1Offset, line2).StartPoint);
            } else {
                var endPoint = IntersectionExtension.LineToLine(line1Offset, line2).StartPoint;
                center = endPoint - axisY * chamfer.X;
                startPoint = Projection.PointToLine(center, line2Offset);
                arc = new Arc(center, startPoint, axisZ * -1, Math.PI / 2);
                
                result = [
                    IntersectionExtension.LineToLine(line1, line2Offset).StartPoint,
                    .. GetPointsOnArc(arc)
                ];
                if (!result.Last().Equals(arc.EndPoint)) result.Add(arc.EndPoint);
            }

            break;
        default:
            throw new ArgumentOutOfRangeException();
        }

        /*if (result.Last().Equals(next)) result.RemoveAt(result.Count - 1);
        if (result.First().Equals(previous)) {
            return [.. result.Take(result.Count - 1)];
        } else {
            return [.. result];
        }*/
        return [.. result.Select(p => new ContourPoint(p, new Chamfer()))];

        List<Point> GetPointsOnArc(Arc arc) {
            var cnt = (int)(arc.Length / epsilon);
            if (arc.Length % epsilon != 0) cnt++;
            var startPoint = arc.StartPoint;
            var endPoint = arc.EndPoint;
            var stepRadians = arc.Angle / cnt;

            arc = new Arc(arc.CenterPoint, arc.StartDirection, arc.StartTangent, arc.Radius, Math.PI * 2);

            return arc.GetPoints(startPoint, endPoint, stepRadians);
        }
    }

}