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
 *  SolidExtension.cs: extension of Tekla.Structures.Model.Solid
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

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