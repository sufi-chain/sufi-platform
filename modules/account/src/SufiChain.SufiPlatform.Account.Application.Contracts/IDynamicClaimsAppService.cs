using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Account;

public interface IDynamicClaimsAppService : IApplicationService
{
    Task RefreshAsync();
}
