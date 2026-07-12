using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Permissions;

public interface IPermissionFinder
{
    Task<List<IsGrantedResponse>> IsGrantedAsync(List<IsGrantedRequest> requests);
}
