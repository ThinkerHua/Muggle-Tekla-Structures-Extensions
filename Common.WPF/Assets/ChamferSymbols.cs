using System;
using System.Windows;
using System.Windows.Media;
using Muggle.TsExtensions.Common.WPF.DataTemplates;
using Muggle.TsExtensions.Common.WPF.Localization;

namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class ChamferSymbols {
    private static readonly ResourceDictionary Dict = new() {
        Source = new Uri("/Common.WPF;component/Assets/ChamferSymbols.xaml", UriKind.Relative)
    };

    public static Symbol[] ChamferTypeSet => [
        new(TranslationService.Instance["albl_ChamferType_None"], Dict["Chamfer_None"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_Line"], Dict["Chamfer_Line"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_Rounding"], Dict["Chamfer_Rounding"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_Arc"], Dict["Chamfer_Arc"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_ArcPoint"], Dict["Chamfer_ArcPoint"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_Square"], Dict["Chamfer_Square"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_SquareParallel"], Dict["Chamfer_SquareParallel"] as DrawingGroup),
        new(TranslationService.Instance["albl_ChamferType_LineAndArc"], Dict["Chamfer_LineAndArc"] as DrawingGroup),
    ];
}
