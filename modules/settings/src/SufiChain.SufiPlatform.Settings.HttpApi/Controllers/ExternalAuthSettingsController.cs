using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings.Controllers;

[Area(SettingsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingsRemoteServiceConsts.RemoteServiceName)]
[Route("api/settings/external-auth")]
public class ExternalAuthSettingsController : SufiControllerBase, IExternalAuthSettingsAppService
{
    private readonly IExternalAuthSettingsAppService _externalAuthSettingsAppService;

    public ExternalAuthSettingsController(IExternalAuthSettingsAppService externalAuthSettingsAppService)
    {
        _externalAuthSettingsAppService = externalAuthSettingsAppService;
    }

    [HttpGet]
    public virtual Task<ExternalAuthSettingsDto> GetAsync()
    {
        return _externalAuthSettingsAppService.GetAsync();
    }

    [HttpPost]
    public virtual Task UpdateAsync(UpdateExternalAuthSettingsDto input)
    {
        return _externalAuthSettingsAppService.UpdateAsync(input);
    }
}
