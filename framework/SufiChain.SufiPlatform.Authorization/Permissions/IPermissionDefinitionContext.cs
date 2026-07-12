using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Authorization.Permissions;

public interface IPermissionDefinitionContext
{
    PermissionGroupDefinition AddGroup(string name, ILocalizableString? displayName = null);

    PermissionGroupDefinition? GetGroupOrNull(string name);
}
