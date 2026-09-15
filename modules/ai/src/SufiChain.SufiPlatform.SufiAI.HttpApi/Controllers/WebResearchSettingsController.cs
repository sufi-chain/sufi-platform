using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Volo.Abp;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.SufiAI;

[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/web-research-settings")]
public class WebResearchSettingsController(IWebResearchSettingsAppService service) : AIController, IWebResearchSettingsAppService
{
    [HttpGet] public Task<WebResearchSettingsDto> GetAsync() => service.GetAsync();
    [HttpPut, DisableAuditing] public Task UpdateAsync(UpdateWebResearchSettingsInput input) => service.UpdateAsync(input);
    [HttpPost("test")] public Task<WebResearchConnectionDto> TestAsync() => service.TestAsync();
}
