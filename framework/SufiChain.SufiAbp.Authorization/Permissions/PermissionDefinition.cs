namespace SufiChain.SufiAbp.Authorization.Permissions;

public class PermissionDefinition
{
    protected Volo.Abp.Authorization.Permissions.PermissionDefinition Inner { get; }

    public PermissionDefinition(Volo.Abp.Authorization.Permissions.PermissionDefinition inner)
    {
        Inner = inner;
    }

    public virtual PermissionDefinition AddChild(string name, object displayName = null!)
    {
        return new PermissionDefinition(Inner.AddChild(name, LocalizableStringConverter.ToVolo(displayName)));
    }
}
