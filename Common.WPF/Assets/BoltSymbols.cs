using Muggle.TsExtensions.Common.WPF.Localization;

namespace Muggle.TsExtensions.Common.WPF.Assets;

public static class BoltSymbols {
    public static string[] BoltTypeSet => [
        TranslationService.Instance["albl_Bolt_Type_Site"],
        TranslationService.Instance["albl_Bolt_Type_Workshop"],
    ];

    public static string[] BoltThreadInMaterialSet => [
        TranslationService.Instance["albl_Bolt_ThreadInMaterial_No"],
        TranslationService.Instance["albl_Bolt_ThreadInMaterial_Yes"],
    ];

    public static string[] BoltPlainHoleTypeSet => [
        TranslationService.Instance["albl_Bolt_PlainHoleType_Through"],
        TranslationService.Instance["albl_Bolt_PlainHoleType_Blind"],
    ];

    public static string[] BoltSpecialHoleTypeSet => [
        TranslationService.Instance["albl_Bolt_SpecialHoleType_Oversized"],
        TranslationService.Instance["albl_Bolt_SpecialHoleType_Slotted"],
        TranslationService.Instance["albl_Bolt_SpecialHoleType_NoHole"],
#if D2024 || R2024
        TranslationService.Instance["albl_Bolt_SpecialHoleType_Tapped"]
#endif
    ];

    public static string[] BoltRotateSlotsSet => [
        TranslationService.Instance["albl_Bolt_RotateSlots_Odd"],
        TranslationService.Instance["albl_Bolt_RotateSlots_Even"],
        TranslationService.Instance["albl_Bolt_RotateSlots_Parallel"],
    ];
}
