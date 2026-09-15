using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIModelCatalogAppService : IApplicationService
{
    Task<List<AIModelRouteDto>> GetSelectableRoutesAsync(GetSelectableModelRoutesInput input);
}
