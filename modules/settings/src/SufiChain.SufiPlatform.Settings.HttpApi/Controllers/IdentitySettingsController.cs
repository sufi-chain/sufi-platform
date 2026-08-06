using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings.Controllers;

[Area(SettingsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingsRemoteServiceConsts.RemoteServiceName)]
[Route("api/settings/identity")]
public class IdentitySettingsController : SufiControllerBase, IIdentitySettingsAppService
{
    private readonly IIdentitySettingsAppService _identitySettingsAppService;

    public IdentitySettingsController(IIdentitySettingsAppService identitySettingsAppService)
    {
        _identitySettingsAppService = identitySettingsAppService;
    }

    [HttpGet]
    public virtual Task<IdentitySettingsDto> GetAsync()
    {
        return _identitySettingsAppService.GetAsync();
    }

    [HttpPost]
    public virtual Task UpdateAsync(UpdateIdentitySettingsDto input)
    {
        return _identitySettingsAppService.UpdateAsync(input);
    }
}
