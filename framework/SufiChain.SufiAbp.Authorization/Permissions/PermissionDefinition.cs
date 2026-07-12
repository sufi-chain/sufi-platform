using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.Authorization.Permissions;

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
}
