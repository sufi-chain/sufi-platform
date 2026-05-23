namespace SufiChain.SufiAbp.Authorization.Permissions;

public interface IPermissionDefinitionContext
{
    PermissionGroupDefinition AddGroup(string name, object displayName = null!);

    PermissionGroupDefinition? GetGroupOrNull(string name);
}
