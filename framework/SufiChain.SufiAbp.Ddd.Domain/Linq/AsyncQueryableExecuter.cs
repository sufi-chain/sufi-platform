using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;

namespace SufiChain.SufiAbp.Linq;

[ExposeServices(typeof(IAsyncQueryableExecuter))]
public class AsyncQueryableExecuter : Volo.Abp.Linq.AsyncQueryableExecuter, IAsyncQueryableExecuter, ISingletonDependency
{
    public AsyncQueryableExecuter(IEnumerable<IAsyncQueryableProvider> providers)
        : base(providers)
    {
    }
}
