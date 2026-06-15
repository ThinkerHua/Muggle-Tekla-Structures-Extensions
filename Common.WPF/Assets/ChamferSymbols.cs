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
 *  ChamferSymbols.cs: sets of chamfer symbol
 *  written by Huang YongXing - thinkerhua@hotmail.com
 *==============================================================================*/
using System;
using System.Windows;
using System.Windows.Media;
using Muggle.TsExtensions.Common.WPF.DataTemplates;

namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class ChamferSymbols {
    private static readonly ResourceDictionary Dict = new() {
        Source = new Uri("/Common.WPF;component/Assets/ChamferSymbols.xaml", UriKind.Relative)
    };

    public static Symbol[] ChamferTypeSet => ItemsProvider.SymbolArray([
        ("albl_ChamferType_None", Dict["Chamfer_None"] as DrawingGroup),
        ("albl_ChamferType_Line", Dict["Chamfer_Line"] as DrawingGroup),
        ("albl_ChamferType_Rounding", Dict["Chamfer_Rounding"] as DrawingGroup),
        ("albl_ChamferType_Arc", Dict["Chamfer_Arc"] as DrawingGroup),
        ("albl_ChamferType_ArcPoint", Dict["Chamfer_ArcPoint"] as DrawingGroup),
        ("albl_ChamferType_Square", Dict["Chamfer_Square"] as DrawingGroup),
        ("albl_ChamferType_SquareParallel", Dict["Chamfer_SquareParallel"] as DrawingGroup),
        ("albl_ChamferType_LineAndArc", Dict["Chamfer_LineAndArc"] as DrawingGroup),
    ]);
}