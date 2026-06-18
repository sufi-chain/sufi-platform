using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.Http.Modeling;

namespace SufiChain.SufiAbp.AspNetCore.Mvc.ApiExploring;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/api-definition")]
public class SufiAbpApiDefinitionController : SufiAbpControllerBase, IRemoteService
{
    protected IApiDescriptionModelProvider ModelProvider { get; }

    public SufiAbpApiDefinitionController(IApiDescriptionModelProvider modelProvider)
    {
        ModelProvider = modelProvider;
    }

    [HttpGet]
    public virtual async Task<ApplicationApiDescriptionModel> GetAsync(ApplicationApiDescriptionModelRequestDto model)
    {
        return await ModelProvider.CreateApiModelAsync(model);
    }
}
