using System;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Account;

public interface IAppUrlProvider
{
    Task<string> GetEmailConfirmationUrlAsync(
        string appName,
        Guid userId,
        string confirmationToken,
        string? returnUrl = null,
        string? returnUrlHash = null);

    Task<string> GetPasswordResetUrlAsync(
        string appName,
        Guid userId,
        string resetToken,
        string? returnUrl = null,
        string? returnUrlHash = null);
}
