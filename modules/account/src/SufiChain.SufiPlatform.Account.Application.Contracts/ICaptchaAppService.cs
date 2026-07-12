using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Account;

public interface ICaptchaAppService : IApplicationService
{
    Task<CaptchaChallengeDto> GetChallengeAsync();

    Task<CaptchaOptionsDto> GetOptionsAsync();
}
