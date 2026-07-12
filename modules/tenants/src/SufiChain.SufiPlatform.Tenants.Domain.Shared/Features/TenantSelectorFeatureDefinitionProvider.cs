using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Tenants.Features;

/// <summary>
/// Defines tenant-related features (e.g. tenant selector mode on account layout).
/// Host-level only. Appears in Feature Management when Tenant Management module is loaded.
/// </summary>
public class TenantSelectorFeatureDefinitionProvider : FeatureDefinitionProvider
{
    /// <summary>
    /// Feature name. Used by theme and account layout to control tenant selector behavior.
    /// </summary>
    public const string TenantSelectorModeFeatureName = "SufiUI.TenantSelector.Mode";
    private const string TenantsResourceName = "SufiTenants";

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
        var group = context.AddGroup("SufiTenants", L("Menu:Tenants"));

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
                        DisplayText = new LocalizableStringInfo(TenantsResourceName, "TenantSelectorMode_SelectFromList")
                    },
                    new LocalizableSelectionStringValueItem
                    {
                        Value = TenantSelectorMode.Search,
                        DisplayText = new LocalizableStringInfo(TenantsResourceName, "TenantSelectorMode_Search")
                    },
                    new LocalizableSelectionStringValueItem
                    {
                        Value = TenantSelectorMode.InputName,
                        DisplayText = new LocalizableStringInfo(TenantsResourceName, "TenantSelectorMode_InputName")
                    })
            },
            isAvailableToHost: true)
            .WithProperty("HostOnly", true);
    }

    private static LocalizableString L(string name)
    {
        return new LocalizableString(typeof(SufiTenantsResource), name);
    }
}
