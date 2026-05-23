using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// HTTP API controller for short-link generator settings.
/// </summary>
[Area(ShortLinkGeneratorRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinkGeneratorRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/settings")]
public class ShortLinkGeneratorSettingsController : SufiAbpControllerBase, IShortLinkGeneratorSettingsAppService
{
    private readonly IShortLinkGeneratorSettingsAppService _service;

    public ShortLinkGeneratorSettingsController(IShortLinkGeneratorSettingsAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<ShortLinkGeneratorSettingsDto> GetAsync()
    {
        return _service.GetAsync();
    }

    [HttpPut]
    public Task UpdateAsync(ShortLinkGeneratorSettingsDto input)
    {
        return _service.UpdateAsync(input);
    }
}
