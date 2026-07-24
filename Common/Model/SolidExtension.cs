using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;

namespace Muggle.TsExtensions.Common.Model;

/// <summary>
/// <see cref="Tekla.Structures.Model"/>.<see cref="Solid"/> 的扩展。
/// </summary>
public static class SolidExtension {
    
    /// <summary>
    /// 获取此 <see cref="Solid"/> 的所有 <see cref="Face"/>。
    /// </summary>
    /// <param name="solid">此 <see cref="Solid"/></param>
    /// <returns>此 <see cref="Solid"/> 的所有 <see cref="Face"/>。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Face> GetAllFaces(this Solid solid) {
        if (solid == null) {
            throw new ArgumentNullException(nameof(solid));
        }

        var result = new List<Face>();
        var faceEnumerator = solid.GetFaceEnumerator();
        while (faceEnumerator.MoveNext()) result.Add(faceEnumerator.Current);
        
        return result;
    }

    /// <summary>
    /// 获取此 <see cref="Solid"/> 的所有 <see cref="Edge"/>。
    /// </summary>
    /// <param name="solid">此 <see cref="Solid"/></param>
    /// <returns>此 <see cref="Solid"/> 的所有 <see cref="Edge"/>。</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<Edge> GetAllEdges(this Solid solid) {
        if (solid == null) {
            throw new ArgumentNullException(nameof(solid));
        }

        var result = new List<Edge>();
        var edgeEnumerator = solid.GetEdgeEnumerator();
        while (edgeEnumerator.MoveNext()) result.Add(edgeEnumerator.Current as Edge);
        
        return result;
    }
}