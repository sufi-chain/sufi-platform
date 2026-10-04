using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Account;

public interface IAccountAppService : IApplicationService
{
    Task<AccountRegistrationResultDto> RegisterAsync(RegisterDto input);

    Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input);

    Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input);

    Task ResetPasswordAsync(ResetPasswordDto input);

    Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input);

    Task ConfirmEmailAsync(ConfirmEmailDto input);

    Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input);

    Task<PhoneConfirmationStateDto> GetPhoneConfirmationStateAsync(string? sessionToken);

    Task<OtpSendResultDto> SendPhoneConfirmationCodeAsync(SendPhoneConfirmationCodeDto input);

    Task<ConfirmPhoneNumberResultDto> ConfirmPhoneNumberAsync(ConfirmPhoneNumberDto input);
}
