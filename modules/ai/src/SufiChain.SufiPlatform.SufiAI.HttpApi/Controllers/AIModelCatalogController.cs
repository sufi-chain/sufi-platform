using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/model-catalog")]
public class AIModelCatalogController : AIController, IAIModelCatalogAppService
{
    private readonly IAIModelCatalogAppService _catalogAppService;

    public AIModelCatalogController(IAIModelCatalogAppService catalogAppService)
    {
        _catalogAppService = catalogAppService;
    }

    [HttpGet("selectable-routes")]
    public virtual Task<List<AIModelRouteDto>> GetSelectableRoutesAsync([FromQuery] GetSelectableModelRoutesInput input)
    {
        return _catalogAppService.GetSelectableRoutesAsync(input);
    }
}
