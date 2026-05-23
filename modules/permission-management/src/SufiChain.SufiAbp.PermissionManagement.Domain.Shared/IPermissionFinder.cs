using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.PermissionManagement;

public interface IPermissionFinder
{
    Task<List<IsGrantedResponse>> IsGrantedAsync(List<IsGrantedRequest> requests);
}
