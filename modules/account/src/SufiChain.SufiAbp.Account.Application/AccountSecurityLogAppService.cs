using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Account;

/// <summary>
/// Application service implementation for account security logging.
/// Delegates to Identity domain's IdentitySecurityLogManager.
/// </summary>
public class AccountSecurityLogAppService : SufiAbpApplicationService, IAccountSecurityLogAppService
{
    private readonly IdentitySecurityLogManager _securityLogManager;

    public AccountSecurityLogAppService(IdentitySecurityLogManager securityLogManager)
    {
        _securityLogManager = securityLogManager;
    }

    public async Task SaveLoginEventAsync(string identity, string action, string? userName = null)
    {
        await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
        {
            Identity = identity,
            Action = action,
            UserName = userName
        });
    }
}
