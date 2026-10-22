using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.SimpleStateChecking;
namespace SufiChain.SufiPlatform.Authorization.Permissions;

public class PermissionDefinition
{
    protected Volo.Abp.Authorization.Permissions.PermissionDefinition Inner { get; }

    public PermissionDefinition(Volo.Abp.Authorization.Permissions.PermissionDefinition inner)
    {
        Inner = inner;
    }

    public virtual PermissionDefinition AddChild(string name, ILocalizableString? displayName = null)
    {
        return new PermissionDefinition(Inner.AddChild(name, displayName));
    }

    /// <summary>
    /// sufi-chain#490: declares that this permission is only meaningful while every given feature is enabled.
    /// ABP attaches a <c>RequireFeaturesSimpleStateChecker</c> to the permission, so when a feature is off
    /// (edition or tenant override) the permission reads as not granted for the current user. That prunes the
    /// menu items and Blazor pages that require the permission, and refuses the app-service / HTTP API calls
    /// guarded by <c>[Authorize]</c> on it, without a per-page check.
    /// </summary>
    public virtual PermissionDefinition RequireFeatures(params string[] features)
    {
        Inner.RequireFeatures(features);
        return this;
    }

    /// <summary>
    /// sufi-chain#490: <see cref="RequireFeatures(string[])"/> with an explicit <paramref name="requiresAll"/> flag.
    /// </summary>
    public virtual PermissionDefinition RequireFeatures(bool requiresAll, params string[] features)
    {
        Inner.RequireFeatures(requiresAll, features);
        return this;
    }
}
