using SufiChain.SufiAbp.TenantManagement.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.TenantManagement.Features;

/// <summary>
/// Defines tenant-related features (e.g. tenant selector mode on account layout).
/// Host-level only. Appears in Feature Management when Tenant Management module is loaded.
/// </summary>
public class TenantSelectorFeatureDefinitionProvider : FeatureDefinitionProvider
{
    /// <summary>
    /// Feature name. Used by theme and account layout to control tenant selector behavior.
    /// </summary>
    public const string TenantSelectorModeFeatureName = "SufiAbpUI.TenantSelector.Mode";
    private const string TenantManagementResourceName = "TenantManagement";

    /// <summary>
    /// Tenant selector mode: SelectFromList, Search, InputName.
    /// </summary>
    public static class TenantSelectorMode
    {
        public const string SelectFromList = "SelectFromList";
        public const string Search = "Search";
        public const string InputName = "InputName";
    }

    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup("TenantManagement", L("Menu:TenantManagement"));

        group.AddFeature(
            TenantSelectorModeFeatureName,
            defaultValue: TenantSelectorMode.InputName,
            displayName: L("TenantSelectorMode"),
            description: L("TenantSelectorModeDescription"),
            valueType: new SelectionStringValueType
            {
                ItemSource = new StaticSelectionStringValueItemSource(
                    new LocalizableSelectionStringValueItem
                    {
                        Value = TenantSelectorMode.SelectFromList,
                        DisplayText = new LocalizableStringInfo(TenantManagementResourceName, "TenantSelectorMode_SelectFromList")
                    },
                    new LocalizableSelectionStringValueItem
                    {
                        Value = TenantSelectorMode.Search,
                        DisplayText = new LocalizableStringInfo(TenantManagementResourceName, "TenantSelectorMode_Search")
                    },
                    new LocalizableSelectionStringValueItem
                    {
                        Value = TenantSelectorMode.InputName,
                        DisplayText = new LocalizableStringInfo(TenantManagementResourceName, "TenantSelectorMode_InputName")
                    })
            },
            isAvailableToHost: true)
            .WithProperty("HostOnly", true);
    }

    private static LocalizableString L(string name)
    {
        return new LocalizableString(typeof(SufiAbpTenantManagementResource), name);
    }
}
