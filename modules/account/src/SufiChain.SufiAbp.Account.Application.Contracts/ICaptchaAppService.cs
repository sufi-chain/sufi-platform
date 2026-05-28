using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface ICaptchaAppService : IApplicationService
{
    Task<CaptchaChallengeDto> GetChallengeAsync();

    Task<CaptchaOptionsDto> GetOptionsAsync();
}
