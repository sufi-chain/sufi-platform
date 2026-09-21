using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.OpenIddict;

public class OpenIddictCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public OpenIddictCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OpenIddictCleanupOptions> options)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = Math.Max(60_000, options.Value.CleanupPeriodMilliseconds);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var options = workerContext.ServiceProvider
            .GetRequiredService<IOptions<OpenIddictCleanupOptions>>()
            .Value;

        if (!options.IsCleanupEnabled)
        {
            return;
        }

        var threshold = DateTimeOffset.UtcNow - options.MinimumAge;

        try
        {
            if (!options.DisableTokenCleanup)
            {
                var tokenManager = workerContext.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
                var tokenCount = await tokenManager.PruneAsync(threshold);
                Logger.LogInformation("OpenIddict token prune removed {Count} entries older than {Threshold}.", tokenCount, threshold);
            }

            if (!options.DisableAuthorizationCleanup)
            {
                var authorizationManager = workerContext.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
                var authorizationCount = await authorizationManager.PruneAsync(threshold);
                Logger.LogInformation("OpenIddict authorization prune removed {Count} entries older than {Threshold}.", authorizationCount, threshold);
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "OpenIddict token and authorization prune failed.");
        }
    }
}
