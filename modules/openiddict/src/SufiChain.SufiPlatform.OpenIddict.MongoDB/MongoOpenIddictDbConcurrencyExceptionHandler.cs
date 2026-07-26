using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// No-op MongoDB concurrency handler for OpenIddict stores (matches ABP).
/// </summary>
public class MongoOpenIddictDbConcurrencyExceptionHandler : IOpenIddictDbConcurrencyExceptionHandler, ITransientDependency
{
    public virtual Task HandleAsync(AbpDbConcurrencyException exception)
    {
        return Task.CompletedTask;
    }
}
