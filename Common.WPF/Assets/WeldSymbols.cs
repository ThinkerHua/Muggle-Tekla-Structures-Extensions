using System;
using System.Windows;
using System.Windows.Media;
using Muggle.TsExtensions.Common.WPF.DataTemplates;
using Muggle.TsExtensions.Common.WPF.Localization;

namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class WeldSymbols {
    private static readonly ResourceDictionary Dict = new() {
        Source = new Uri("/Common.WPF;component/Assets/WeldSymbols.xaml", UriKind.Relative)
    };

    public static Symbol[] AroundWeldSet => [
        new(TranslationService.Instance["albl_Weld_AroundWeld_Edge"], Dict["AroundWeld_Edge"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_AroundWeld_Around"], Dict["AroundWeld_Around"] as DrawingGroup),
    ];

    public static Symbol[] ShopWeldSet => [
        new(TranslationService.Instance["albl_Weld_ShopWeld_Workshop"], Dict["ShopWeld_Workshop"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_ShopWeld_Site"], Dict["ShopWeld_Site"] as DrawingGroup),
    ];

    public static Symbol[] WeldContourSet => [
        new(TranslationService.Instance["albl_Weld_Contour_None"], Dict["WeldContour_None"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Contour_Flush"], Dict["WeldContour_Flush"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Contour_Convex"], Dict["WeldContour_Convex"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Contour_Concave"], Dict["WeldContour_Concave"] as DrawingGroup),
    ];

    public static Symbol[] WeldIntermittentSet => [
        new(TranslationService.Instance["albl_Weld_Intermittent_Continuous"], Dict["WeldIntermittent_Continuous"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Intermittent_ChainIntermittent"], Dict["WeldIntermittent_ChainIntermittent"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Intermittent_StaggeredIntermittent"], Dict["WeldIntermittent_StaggeredIntermittent"] as DrawingGroup),
    ];

    public static Symbol[] WeldFinishSet => [
        new(TranslationService.Instance["albl_Weld_Finish_None"], Dict["WeldFinish_None"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Finish_Grind"], Dict["WeldFinish_Grind"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Finish_Machine"], Dict["WeldFinish_Machine"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Finish_Chip"], Dict["WeldFinish_Chip"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Finish_FinishedWeld"], Dict["WeldFinish_FinishedWeld"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Finish_SmoothTransition"], Dict["WeldFinish_SmoothTransition"] as DrawingGroup),
    ];

    public static Symbol[] WeldTypeSet => [
        new(TranslationService.Instance["albl_Weld_Type_None"], Dict["WeldType_None"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_EdgeFlange"], Dict["WeldType_EdgeFlange"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_SquareGrooveSquareButt"], Dict["WeldType_SquareGrooveSquareButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_BevelGrooveSingleVButt"], Dict["WeldType_BevelGrooveSingleVButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_BevelGrooveSingleBevelButt"], Dict["WeldType_BevelGrooveSingleBevelButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_SingleVButtWithBroadRootFace"], Dict["WeldType_SingleVButtWithBroadRootFace"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_SingleBevelButtWithBroadRootFace"], Dict["WeldType_SingleBevelButtWithBroadRootFace"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_UGrooveSingleUButt"], Dict["WeldType_UGrooveSingleUButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_JGrooveJButt"], Dict["WeldType_JGrooveJButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_BevelBacking"], Dict["WeldType_BevelBacking"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Fillet"], Dict["WeldType_Fillet"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Plug"], Dict["WeldType_Plug"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Spot"], Dict["WeldType_Spot"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Seam"], Dict["WeldType_Seam"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Slot"], Dict["WeldType_Slot"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_FlareBevelGroove"], Dict["WeldType_FlareBevelGroove"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_FlareVGroove"], Dict["WeldType_FlareVGroove"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_CornerFlange"], Dict["WeldType_CornerFlange"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_PartialPenetrationSingleBevelButtPlusFillet"], Dict["WeldType_PartialPenetrationSingleBevelButtPlusFillet"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_PartialPenetrationSquareGroovePlusFillet"], Dict["WeldType_PartialPenetrationSquareGroovePlusFillet"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_MeltThrough"], Dict["WeldType_MeltThrough"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_SteepFlankedBevelGrooveSingleVButt"], Dict["WeldType_SteepFlankedBevelGrooveSingleVButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_SteepFlankedBevelGrooveSingleBevelButt"], Dict["WeldType_SteepFlankedBevelGrooveSingleBevelButt"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Edge"], Dict["WeldType_Edge"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_IsoSurfacing"], Dict["WeldType_IsoSurfacing"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Fold"], Dict["WeldType_Fold"] as DrawingGroup),
        new(TranslationService.Instance["albl_Weld_Type_Inclined"], Dict["WeldType_Inclined"] as DrawingGroup),
    ];

    public static string[] WeldConnectAsSet => [
        TranslationService.Instance["albl_Weld_ConnectAs_AsSecondaryPart"],
        TranslationService.Instance["albl_Weld_ConnectAs_AsSubAssembly"],
    ];

    public static string[] WeldPlacementSet => [
        TranslationService.Instance["albl_Weld_Placement_Auto"],
        TranslationService.Instance["albl_Weld_Placement_MainPart"],
        TranslationService.Instance["albl_Weld_Placement_SecondaryPart"],
    ];

    public static string[] WeldPreparationSet => [
        TranslationService.Instance["albl_Weld_Preparation_None"],
        TranslationService.Instance["albl_Weld_Preparation_Auto"],
        TranslationService.Instance["albl_Weld_Preparation_MainPart"],
        TranslationService.Instance["albl_Weld_Preparation_SecondaryPart"],
    ];
}
