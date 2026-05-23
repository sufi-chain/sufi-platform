using System.Threading.Tasks;
using SufiChain.SufiAbp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Uow;
using Volo.Abp.Uow;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class RoleUpdateEventHandler :
    IDistributedEventHandler<IdentityRoleNameChangedEto>,
    ITransientDependency
{
    protected IPermissionManager PermissionManager { get; }
    protected IPermissionGrantRepository PermissionGrantRepository { get; }
    protected IResourcePermissionManager ResourcePermissionManager { get; }
    protected IResourcePermissionGrantRepository ResourcePermissionGrantRepository { get; }

    public RoleUpdateEventHandler(
        IPermissionManager permissionManager,
        IPermissionGrantRepository permissionGrantRepository,
        IResourcePermissionManager resourcePermissionManager,
        IResourcePermissionGrantRepository resourcePermissionGrantRepository)
    {
        PermissionManager = permissionManager;
        PermissionGrantRepository = permissionGrantRepository;
        ResourcePermissionManager = resourcePermissionManager;
        ResourcePermissionGrantRepository = resourcePermissionGrantRepository;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(IdentityRoleNameChangedEto eventData)
    {
        if (eventData.Name == eventData.OldName)
        {
            return;
        }

        var permissionGrants = await PermissionGrantRepository.GetListAsync(RolePermissionValueProvider.ProviderName, eventData.OldName!);
        foreach (var permissionGrant in permissionGrants)
        {
            await PermissionManager.UpdateProviderKeyAsync(permissionGrant, eventData.Name);
        }

        var resourcePermissionGrants = await ResourcePermissionGrantRepository.GetListAsync(RolePermissionValueProvider.ProviderName, eventData.OldName!);
        foreach (var resourcePermissionGrant in resourcePermissionGrants)
        {
            await ResourcePermissionManager.UpdateProviderKeyAsync(resourcePermissionGrant, eventData.Name);
        }
    }
}
