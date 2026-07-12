using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Application service implementation for account security logging.
/// Delegates to Identity domain's IdentitySecurityLogManager.
/// </summary>
public class AccountSecurityLogAppService : SufiApplicationService, IAccountSecurityLogAppService
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
