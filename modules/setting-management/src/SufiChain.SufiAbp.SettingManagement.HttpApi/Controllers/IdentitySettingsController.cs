using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiAbp.SettingManagement.Controllers;

[Area(SettingManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/setting-management/identity")]
public class IdentitySettingsController : SufiAbpControllerBase, IIdentitySettingsAppService
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
