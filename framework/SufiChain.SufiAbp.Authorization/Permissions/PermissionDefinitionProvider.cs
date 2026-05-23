namespace SufiChain.SufiAbp.Authorization.Permissions;

public abstract class PermissionDefinitionProvider : Volo.Abp.Authorization.Permissions.PermissionDefinitionProvider
{
    public sealed override void Define(Volo.Abp.Authorization.Permissions.IPermissionDefinitionContext context)
    {
        Define(new PermissionDefinitionContextAdapter(context));
    }

    public abstract void Define(IPermissionDefinitionContext context);
}
