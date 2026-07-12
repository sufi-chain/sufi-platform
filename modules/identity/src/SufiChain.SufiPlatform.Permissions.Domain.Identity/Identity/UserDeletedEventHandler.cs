using System.Threading.Tasks;
using SufiChain.SufiPlatform.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Permissions.Identity;

public class UserDeletedEventHandler :
    IDistributedEventHandler<EntityDeletedEto<UserEto>>,
    ITransientDependency
{
    protected IPermissionManager PermissionManager { get; }
    protected IResourcePermissionManager ResourcePermissionManager { get; }

    public UserDeletedEventHandler(IPermissionManager permissionManager, IResourcePermissionManager resourcePermissionManager)
    {
        PermissionManager = permissionManager;
        ResourcePermissionManager = resourcePermissionManager;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityDeletedEto<UserEto> eventData)
    {
        await PermissionManager.DeleteAsync(UserPermissionValueProvider.ProviderName, eventData.Entity.Id.ToString());
        await ResourcePermissionManager.DeleteAsync(UserPermissionValueProvider.ProviderName, eventData.Entity.Id.ToString());
    }
}
