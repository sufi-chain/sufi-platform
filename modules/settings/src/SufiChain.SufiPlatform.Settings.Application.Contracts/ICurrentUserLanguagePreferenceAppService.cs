using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Settings;

public interface ICurrentUserLanguagePreferenceAppService : IApplicationService
{
    Task UpdateAsync(UpdateCurrentUserLanguagePreferenceInput input);
}
