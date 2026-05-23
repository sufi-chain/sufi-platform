using System.Threading.Tasks;
using SufiChain.SufiAbp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Uow;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class RoleDeletedEventHandler :
    IDistributedEventHandler<EntityDeletedEto<IdentityRoleEto>>,
    ITransientDependency
{
    protected IPermissionManager PermissionManager { get; }
    protected IResourcePermissionManager ResourcePermissionManager { get; }

    public RoleDeletedEventHandler(IPermissionManager permissionManager, IResourcePermissionManager resourcePermissionManager)
    {
        PermissionManager = permissionManager;
        ResourcePermissionManager = resourcePermissionManager;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityDeletedEto<IdentityRoleEto> eventData)
    {
        await PermissionManager.DeleteAsync(RolePermissionValueProvider.ProviderName, eventData.Entity.Name);
        await ResourcePermissionManager.DeleteAsync(RolePermissionValueProvider.ProviderName, eventData.Entity.Name);
    }
}
