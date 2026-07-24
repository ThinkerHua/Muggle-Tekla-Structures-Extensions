using System;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;

namespace Muggle.TsExtensions.Common.Model;

/// <summary>
/// <see cref="Tekla.Structures.Solid"/>.<see cref="Loop"/> 的扩展。
/// </summary>
public static class LoopExtension {
    
    /// <summary>
    /// 获取此 <see cref="Loop"/> 的所有 Vertex (<see cref="Point"/>)。
    /// </summary>
    /// <param name="loop">此 <see cref="Loop"/></param>
    /// <returns>此 <see cref="Loop"/> 的所有 Vertex (<see cref="Point"/>)。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Point> GetAllVertexes(this Loop loop) {
        if (loop == null) {
            throw new ArgumentNullException(nameof(loop));
        }

        var result = new List<Point>();
        var vertexEnumerator = loop.GetVertexEnumerator();
        while (vertexEnumerator.MoveNext()) result.Add(vertexEnumerator.Current);
        
        return result;
    }
}