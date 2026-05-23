using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.LocalizationManagement;

internal static class SufiAbpLocalizationResourceNameMap
{
    private static readonly HashSet<string> ReplacedAbpResourceNames = new(StringComparer.Ordinal)
    {
        "AbpAuthorization",
        "AbpDddApplicationContracts",
        "AbpEmailing",
        "AbpExceptionHandling",
        "AbpFeature",
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
