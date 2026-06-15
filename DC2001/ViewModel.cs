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
 *  ViewModel.cs: view model for main window of DC2001
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using Muggle.TsExtensions.CodingHelper.Generators;
using Tekla.Structures.Datatype;

namespace Muggle.TsExtensions.DC2001 {
    [PartProperties("LeftCorbel", profile: "HI530-270-30-30*600", name: "LeftCorbel")]
    [PartProperties("RightCorbel", profile: "HI530-270-30-30*600", name: "RightCorbel")]
    [PlateProperties("PrimStif", 20, name: "ColumnStiffener")]
    [PlateProperties("LCorbelStif", 20, 185, name: "LeftCorbelStiffener")]
    [PlateProperties("RCorbelStif", 20, 185, name: "RightCorbelStiffener")]
    [PlateProperties("LeftPad", 30, 560, 560, name: "LeftPad")]
    [PlateProperties("RightPad", 30, 560, 560, name: "RightPad")]
    [WeldProperties("PrimStif", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
    [WeldProperties("Corbel", typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
    [WeldProperties("CorbelStif", typeAbove: 10, typeBelow: 10, sizeAbove: 6, sizeBelow: 6, shop: 1)]
    [WeldProperties("Pad", typeAbove: 10, sizeAbove: 6, around: 1, shop: 1)]
    [GeneralProperties(typeof(Integer), "RightCorbelCreation", 0, "PrimStifChamferType", 1)]
    [GeneralProperties(typeof(Double), "LeftCorbelLength", 800, "RightCorbelLength", 800,
        "LeftCorbelStifDist", 500, "RightCorbelStifDist", 500, "PrimStifChamferX", 25, "PrimStifChamferY", 25,
        "PrimStifChamferDz1", 0, "PrimStifChamferDz2", 0)]
    public partial class ViewModel : DetailViewModel {
        public ViewModel() {
            DetailType = 1;
            Class = -1;
        }
    }
}