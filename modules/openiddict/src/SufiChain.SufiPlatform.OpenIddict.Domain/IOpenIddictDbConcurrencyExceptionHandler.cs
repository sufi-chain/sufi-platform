using System.Threading.Tasks;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Handles database concurrency exceptions raised by OpenIddict stores.
/// Implementations live in EF Core / MongoDB packages.
/// </summary>
public interface IOpenIddictDbConcurrencyExceptionHandler
{
    Task HandleAsync(AbpDbConcurrencyException exception);
}
