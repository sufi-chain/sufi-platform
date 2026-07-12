using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Application service for account-related security logging.
/// Keeps UI and presentation layers from depending on Identity domain directly (DDD).
/// </summary>
public interface IAccountSecurityLogAppService : IApplicationService
{
    /// <summary>
    /// Saves a login-related security event (success, failure, locked out, etc.).
    /// </summary>
    Task SaveLoginEventAsync(string identity, string action, string? userName = null);
}
