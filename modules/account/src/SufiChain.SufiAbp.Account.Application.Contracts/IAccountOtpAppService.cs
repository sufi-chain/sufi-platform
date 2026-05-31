using System.Threading.Tasks;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface IAccountOtpAppService : IApplicationService
{
    Task<OtpOptionsDto> GetOtpOptionsAsync();

    Task SendLoginOtpAsync(SendOtpInput input);

    Task<VerifyLoginOtpResultDto> VerifyLoginOtpAsync(VerifyLoginOtpInput input);

    Task SendRegistrationOtpAsync(SendOtpInput input);

    Task<VerifyRegistrationOtpResultDto> VerifyRegistrationOtpAsync(VerifyOtpInput input);

    Task<IdentityUserDto> RegisterWithOtpAsync(RegisterWithOtpDto input);
}
