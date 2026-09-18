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
 *  WinFormUiGenerator.cs: Implement a WinForm-type user interface generator.
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/

using Microsoft.CodeAnalysis;
using Muggle.TsExtensions.CodingHelper.Generators.Information;

namespace Muggle.TsExtensions.CodingHelper.Generators {
    internal class WinFormUiGenerator : IUserInterfaceGenerator {
        public static WinFormUiGenerator Instance { get; } = new WinFormUiGenerator();
        
        private WinFormUiGenerator() { }

        public void Generate(SourceProductionContext context, UserInterfaceInfo info, string projectDir) {
            throw new System.NotImplementedException();
        }
    }
}