using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// HTTP API controller for short-link generator settings.
/// </summary>
[Area(ShortLinksRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinksRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/settings")]
public class ShortLinksSettingsController : SufiControllerBase, IShortLinksSettingsAppService
{
    private readonly IShortLinksSettingsAppService _service;

    public ShortLinksSettingsController(IShortLinksSettingsAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<ShortLinksSettingsDto> GetAsync()
    {
        return _service.GetAsync();
    }

    [HttpPut]
    public Task UpdateAsync(ShortLinksSettingsDto input)
    {
        return _service.UpdateAsync(input);
    }
}
