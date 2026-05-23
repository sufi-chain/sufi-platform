namespace SufiChain.SufiAbp.Authorization.Permissions;

public class PermissionDefinitionContextAdapter : IPermissionDefinitionContext
{
    protected Volo.Abp.Authorization.Permissions.IPermissionDefinitionContext InnerContext { get; }

    public PermissionDefinitionContextAdapter(Volo.Abp.Authorization.Permissions.IPermissionDefinitionContext innerContext)
    {
        InnerContext = innerContext;
    }

    public virtual PermissionGroupDefinition AddGroup(string name, object displayName = null!)
    {
        return new PermissionGroupDefinition(InnerContext.AddGroup(name, LocalizableStringConverter.ToVolo(displayName)));
    }

    public virtual PermissionGroupDefinition? GetGroupOrNull(string name)
    {
        var group = InnerContext.GetGroupOrNull(name);
        return group == null ? null : new PermissionGroupDefinition(group);
    }
}
