/*==============================================================================
 *  Muggle TsExtensions - extensions for Tekla Structures
 *
 *  Copyright © 2026 Huang YongXing.
 *
 *  This library is free software, licensed under the terms of the GNU
 *  General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or (at your option) any later version.
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <http://www.gnu.org/licenses/>.
 *==============================================================================
 *  LoopExtension.cs: extension of Tekla.Structures.Solid.Loop
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

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