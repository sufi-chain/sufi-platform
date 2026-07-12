using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.SecurityLog;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Identity;

[Dependency(ReplaceServices = true)]
public class IdentitySecurityLogStore : ISecurityLogStore, ITransientDependency
{
    protected AbpSecurityLogOptions SecurityLogOptions { get; }
    protected IIdentitySecurityLogRepository SecurityLogRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected ILogger<IdentitySecurityLogStore> Logger { get; }

    public IdentitySecurityLogStore(
        IOptions<AbpSecurityLogOptions> securityLogOptions,
        IIdentitySecurityLogRepository securityLogRepository,
        IGuidGenerator guidGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<IdentitySecurityLogStore> logger)
    {
        SecurityLogOptions = securityLogOptions.Value;
        SecurityLogRepository = securityLogRepository;
        GuidGenerator = guidGenerator;
        UnitOfWorkManager = unitOfWorkManager;
        Logger = logger;
    }

    public virtual async Task SaveAsync(SecurityLogInfo securityLogInfo)
    {
        if (!SecurityLogOptions.IsEnabled)
        {
            return;
        }

        using var uow = UnitOfWorkManager.Begin(requiresNew: true);
        await SecurityLogRepository.InsertAsync(new IdentitySecurityLog(GuidGenerator.Create(), securityLogInfo));
        await uow.CompleteAsync();

        Logger.LogDebug("Saved identity security log {Identity}/{Action} for user {UserName}.",
            securityLogInfo.Identity,
            securityLogInfo.Action,
            securityLogInfo.UserName);
    }
}
