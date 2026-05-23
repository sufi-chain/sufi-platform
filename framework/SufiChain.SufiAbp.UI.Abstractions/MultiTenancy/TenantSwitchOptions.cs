namespace SufiChain.SufiAbp.UI.MultiTenancy;

/// <summary>
/// Options for tenant switching in the UI.
/// The cookie name defaults to the standard SufiAbp tenant key.
/// </summary>
public class TenantSwitchOptions
{
    /// <summary>
    /// The cookie name used to persist the selected tenant.
    /// Default tenant cookie name.
    /// </summary>
    public string TenantCookieName { get; set; } = "__tenant";
}
