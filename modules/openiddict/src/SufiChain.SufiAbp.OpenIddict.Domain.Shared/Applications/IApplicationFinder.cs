using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.OpenIddict.Applications;

public interface IApplicationFinder
{
    Task<List<ApplicationFinderResult>> SearchAsync(string filter, int page = 1);
}
