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

    Task<VerifyRegistrationOtpResultDto> VerifyRegistrationOtpAsync(VerifyOtpInput input);

    Task<IdentityUserDto> RegisterWithOtpAsync(RegisterWithOtpDto input);
}
