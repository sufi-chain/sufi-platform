using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.SecurityLog;

namespace SufiChain.SufiAbp.Identity;

public class IdentitySecurityLogManager : DomainService
{
    protected IIdentitySecurityLogRepository SecurityLogRepository { get; }
    protected ISecurityLogManager SecurityLogManager { get; }

    public IdentitySecurityLogManager(
        IIdentitySecurityLogRepository securityLogRepository,
        ISecurityLogManager securityLogManager)
    {
        SecurityLogRepository = securityLogRepository;
        SecurityLogManager = securityLogManager;
    }

    public virtual async Task SaveAsync(IdentitySecurityLogContext context)
    {
        await SecurityLogManager.SaveAsync(securityLog =>
        {
            securityLog.Identity = context.Identity;
            securityLog.Action = context.Action;

            if (!context.UserName.IsNullOrWhiteSpace())
            {
                securityLog.UserName = context.UserName;
            }

            if (!context.ClientId.IsNullOrWhiteSpace())
            {
                securityLog.ClientId = context.ClientId;
            }

            foreach (var property in context.ExtraProperties)
            {
                securityLog.ExtraProperties[property.Key] = property.Value;
            }
        });
    }

    public virtual async Task<IdentitySecurityLog> CreateAsync(
        string? applicationName = null,
        string? identity = null,
        string? action = null,
        Guid? userId = null,
        string? userName = null,
        string? tenantName = null,
        string? clientId = null,
        string? correlationId = null,
        string? clientIpAddress = null,
        string? browserInfo = null)
    {
        var securityLog = new IdentitySecurityLog(GuidGenerator.Create(), CurrentTenant.Id)
        {
            ApplicationName = applicationName,
            Identity = identity,
            Action = action,
            UserId = userId,
            UserName = userName,
            TenantName = tenantName,
            ClientId = clientId,
            CorrelationId = correlationId,
            ClientIpAddress = clientIpAddress,
            BrowserInfo = browserInfo
        };

        await SecurityLogRepository.InsertAsync(securityLog);

        return securityLog;
    }
}
