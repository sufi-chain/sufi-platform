using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings.Controllers;

[Area(SettingsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingsRemoteServiceConsts.RemoteServiceName)]
[Route("api/settings/my-language")]
public class CurrentUserLanguagePreferenceController
    : SufiControllerBase, ICurrentUserLanguagePreferenceAppService
{
    private readonly ICurrentUserLanguagePreferenceAppService _appService;

    public CurrentUserLanguagePreferenceController(
        ICurrentUserLanguagePreferenceAppService appService)
    {
        _appService = appService;
    }

    [HttpPut]
    public virtual Task UpdateAsync(UpdateCurrentUserLanguagePreferenceInput input)
    {
        return _appService.UpdateAsync(input);
    }
}
