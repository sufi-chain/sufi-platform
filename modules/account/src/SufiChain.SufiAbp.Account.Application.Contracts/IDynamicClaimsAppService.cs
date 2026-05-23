using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface IDynamicClaimsAppService : IApplicationService
{
    Task RefreshAsync();
}
