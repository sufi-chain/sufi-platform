using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Localization;

internal static class SufiLocalizationResourceNameMap
{
    private static readonly HashSet<string> ReplacedAbpResourceNames = new(StringComparer.Ordinal)
    {
        "AbpAuthorization",
        "AbpDddApplicationContracts",
        "AbpEmailing",
        "AbpExceptionHandling",
        "AbpFeature",
        "AbpSettings",
        "AbpGlobalFeature",
        "AbpLocalization",
        "AbpMultiTenancy",
        "AbpTiming",
        "AbpUi",
        "AbpUiNavigation",
        "AbpValidation"
    };

    public static bool IsReplacedAbpResource(string resourceName)
    {
        return ReplacedAbpResourceNames.Contains(resourceName);
    }
}
