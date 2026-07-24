using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;

namespace Muggle.TsExtensions.Common.Model;

/// <summary>
/// <see cref="Tekla.Structures.Solid"/>.<see cref="Face"/> 的扩展。
/// </summary>
public static class FaceExtension {

    /// <summary>
    /// 获取此 <see cref="Face"/> 的所有 <see cref="Loop"/>。
    /// </summary>
    /// <param name="face">此 <see cref="Face"/></param>
    /// <returns>此 <see cref="Face"/> 的所有 <see cref="Loop"/>。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Loop> GetAllLoops(this Face face) {
        if (face == null) {
            throw new ArgumentNullException(nameof(face));
        }

        var result = new List<Loop>();
        var loopEnumerator = face.GetLoopEnumerator();
        while (loopEnumerator.MoveNext()) result.Add(loopEnumerator.Current);

        return result;
    }

    /// <summary>
    /// 获取此 <see cref="Face"/> 的所有 Vertex (<see cref="Point"/>)。
    /// </summary>
    /// <param name="face">此 <see cref="Face"/></param>
    /// <returns>此 <see cref="Face"/> 的所有 Vertex (<see cref="Point"/>)。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Point> GetAllVertexes(this Face face) {
        if (face == null) {
            throw new ArgumentNullException(nameof(face));
        }

        return face.GetAllLoops().SelectMany(l => l.GetAllVertexes()).ToList();
    }

    /// <summary>
    /// 将此 <see cref="Face"/> 转换为 <see cref="GeometricPlane"/>。
    /// </summary>
    /// <param name="face">此 <see cref="Face"/></param>
    /// <returns>成功转换的 <see cref="GeometricPlane"/>，
    /// Origin 为此 <see cref="Face"/> 的第一个顶点，
    /// Normal 为此 <see cref="Face"/> 的 Normal。
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static GeometricPlane ToGeometricPlane(this Face face) {
        if (face == null) {
            throw new ArgumentNullException(nameof(face));
        }

        var loopEnumerator = face.GetLoopEnumerator();
        if (loopEnumerator.MoveNext()) {
            var loop = loopEnumerator.Current;

            var vertexEnumerator = loop.GetVertexEnumerator();
            if (vertexEnumerator.MoveNext()) {
                var p1 = vertexEnumerator.Current;
                return new GeometricPlane(p1, face.Normal);
            }
        }

        throw new InvalidOperationException("Cannot convert face to GeometricPlane");
    }
}