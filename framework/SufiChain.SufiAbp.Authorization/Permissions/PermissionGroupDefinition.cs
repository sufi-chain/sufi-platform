using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.Authorization.Permissions;

public class PermissionGroupDefinition
{
    protected Volo.Abp.Authorization.Permissions.PermissionGroupDefinition Inner { get; }

    public PermissionGroupDefinition(Volo.Abp.Authorization.Permissions.PermissionGroupDefinition inner)
    {
        Inner = inner;
    }

    public virtual PermissionDefinition AddPermission(string name, ILocalizableString? displayName = null)
    {
        return new PermissionDefinition(Inner.AddPermission(name, displayName));
    }
}
