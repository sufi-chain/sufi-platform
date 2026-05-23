using SufiChain.SufiAbp.UI.MultiTenancy;

namespace SufiChain.SufiAbp.UI.Services.MultiTenancy;

/// <summary>
/// Default implementation: never show tenant selector.
/// Replace with an ABP-backed implementation when using multi-tenancy.
/// </summary>
public class DefaultTenantSelectorVisibilityService : ITenantSelectorVisibilityService
{
    public Task<(bool Show, string Mode)> GetOptionsAsync() =>
        Task.FromResult((false, "None"));
}
