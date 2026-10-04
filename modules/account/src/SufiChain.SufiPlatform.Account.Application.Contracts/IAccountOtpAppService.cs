using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Account;

public interface IAccountOtpAppService : IApplicationService
{
    Task<OtpOptionsDto> GetOtpOptionsAsync();

    Task<OtpSendResultDto> SendLoginOtpAsync(SendOtpInput input);

    Task<VerifyLoginOtpResultDto> VerifyLoginOtpAsync(VerifyLoginOtpInput input);

    Task<OtpSendResultDto> SendRegistrationOtpAsync(SendOtpInput input);

    /// <summary>
    /// Sends a registration OTP after the caller has already checked captcha.
    /// This method is not an HTTP endpoint. HTTP callers use <see cref="SendRegistrationOtpAsync"/>.
    /// </summary>
    Task<OtpSendResultDto> SendVerifiedRegistrationOtpAsync(SendOtpInput input);

    Task<VerifyRegistrationOtpResultDto> VerifyRegistrationOtpAsync(VerifyOtpInput input);

    Task<AccountRegistrationResultDto> RegisterWithOtpAsync(RegisterWithOtpDto input);
}
