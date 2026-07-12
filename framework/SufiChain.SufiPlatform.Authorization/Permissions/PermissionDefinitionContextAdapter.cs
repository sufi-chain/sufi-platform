using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Authorization.Permissions;

public class PermissionDefinitionContextAdapter : IPermissionDefinitionContext
{
    protected Volo.Abp.Authorization.Permissions.IPermissionDefinitionContext InnerContext { get; }

    public PermissionDefinitionContextAdapter(Volo.Abp.Authorization.Permissions.IPermissionDefinitionContext innerContext)
    {
        InnerContext = innerContext;
    }

    public virtual PermissionGroupDefinition AddGroup(string name, ILocalizableString? displayName = null)
    {
        return new PermissionGroupDefinition(InnerContext.AddGroup(name, displayName));
    }

    public virtual PermissionGroupDefinition? GetGroupOrNull(string name)
    {
        var group = InnerContext.GetGroupOrNull(name);
        return group == null ? null : new PermissionGroupDefinition(group);
    }
}
