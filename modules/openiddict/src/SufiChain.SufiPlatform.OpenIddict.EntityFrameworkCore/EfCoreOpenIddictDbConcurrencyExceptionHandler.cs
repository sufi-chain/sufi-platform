using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Resets EF Core change-tracker state after OpenIddict store concurrency failures.
/// </summary>
public class EfCoreOpenIddictDbConcurrencyExceptionHandler : IOpenIddictDbConcurrencyExceptionHandler, ITransientDependency
{
    public virtual Task HandleAsync(AbpDbConcurrencyException exception)
    {
        if (exception != null &&
            exception.InnerException is DbUpdateConcurrencyException updateConcurrencyException)
        {
            foreach (var entry in updateConcurrencyException.Entries)
            {
                // Reset the state of the entity to prevent future calls to SaveChangesAsync() from failing.
                entry.State = EntityState.Unchanged;
            }
        }

        return Task.CompletedTask;
    }
}
