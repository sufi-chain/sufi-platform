namespace SufiChain.SufiAbp.UI.MultiTenancy;

/// <summary>
/// Provides tenant selector visibility and mode for the account layout.
/// When multi-tenancy is enabled and the feature allows, the tenant selector can be shown.
/// </summary>
public interface ITenantSelectorVisibilityService
{
    /// <summary>
    /// Returns whether to show the tenant selector and which mode to use.
    /// </summary>
    /// <returns>Show = true when multi-tenant and feature allows; Mode = None, InputName, SelectFromList, or Search.</returns>
    Task<(bool Show, string Mode)> GetOptionsAsync();
}
