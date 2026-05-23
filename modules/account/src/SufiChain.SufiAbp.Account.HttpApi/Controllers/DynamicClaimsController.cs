using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiAbp.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/account/dynamic-claims")]
public class DynamicClaimsController : SufiAbpControllerBase, IDynamicClaimsAppService
{
    private readonly IDynamicClaimsAppService _dynamicClaimsAppService;

    public DynamicClaimsController(IDynamicClaimsAppService dynamicClaimsAppService)
    {
        _dynamicClaimsAppService = dynamicClaimsAppService;
    }

    [HttpPost]
    [Route("refresh")]
    public virtual Task RefreshAsync()
    {
        return _dynamicClaimsAppService.RefreshAsync();
    }
}
