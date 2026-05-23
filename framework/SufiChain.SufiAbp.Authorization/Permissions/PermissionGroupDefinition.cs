namespace SufiChain.SufiAbp.Authorization.Permissions;

public class PermissionGroupDefinition
{
    protected Volo.Abp.Authorization.Permissions.PermissionGroupDefinition Inner { get; }

    public PermissionGroupDefinition(Volo.Abp.Authorization.Permissions.PermissionGroupDefinition inner)
    {
        Inner = inner;
    }

    public virtual PermissionDefinition AddPermission(string name, object displayName = null!)
    {
        return new PermissionDefinition(Inner.AddPermission(name, LocalizableStringConverter.ToVolo(displayName)));
    }
}
