using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface IAccountTwoFactorAppService : IApplicationService
{
    Task<TwoFactorLoginOptionsDto> GetLoginOptionsAsync();

    Task<TwoFactorInfoDto> GetTwoFactorInfoAsync();

    Task<AuthenticatorSetupDto> GenerateAuthenticatorSetupAsync();

    Task<RecoveryCodesDto> EnableTwoFactorAsync(EnableTwoFactorInput input);

    Task DisableTwoFactorAsync(DisableTwoFactorInput input);

    Task<RecoveryCodesDto> GenerateRecoveryCodesAsync();

    Task SendTwoFactorCodeAsync(SendTwoFactorCodeInput input);

    Task<CompleteTwoFactorLoginResultDto> CompleteTwoFactorLoginAsync(CompleteTwoFactorLoginInput input);

    /// <summary>
    /// Returns a redirect URL when the user must set up two-factor before continuing, or null.
    /// </summary>
    Task<string?> GetPostLoginRedirectUrlAsync(Guid userId, string? returnUrl);
}
