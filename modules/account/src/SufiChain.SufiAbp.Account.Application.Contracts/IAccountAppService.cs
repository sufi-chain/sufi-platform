using SufiChain.SufiAbp.Identity;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface IAccountAppService : IApplicationService
{
    Task<IdentityUserDto> RegisterAsync(RegisterDto input);

    Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input);

    Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input);

    Task ResetPasswordAsync(ResetPasswordDto input);

    Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input);

    Task ConfirmEmailAsync(ConfirmEmailDto input);

    Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input);
}
