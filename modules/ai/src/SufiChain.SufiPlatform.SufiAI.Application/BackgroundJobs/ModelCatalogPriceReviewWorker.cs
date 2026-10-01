using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.BackgroundJobs;

public class ModelCatalogPriceReviewWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ModelCatalogPriceReviewWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 1000 * 60 * 60 * 24;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        try
        {
            using var scope = workerContext.ServiceProvider.CreateScope();
            var catalog = scope.ServiceProvider.GetRequiredService<IModelCatalogSource>();
            var markup = scope.ServiceProvider.GetRequiredService<HostPriceMarkup>();
            var percent = await markup.GetPercentAsync();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
            var tenantRepository = scope.ServiceProvider.GetService<ITenantRepository>();
            var updated = 0;

            using (currentTenant.Change(null))
            {
                updated += await ReviewCurrentTenantAsync(scope.ServiceProvider, catalog, percent, workerContext.CancellationToken);
            }

            if (tenantRepository != null)
            {
                var tenants = await tenantRepository.GetListAsync(includeDetails: false);
                foreach (var tenant in tenants)
                {
                    try
                    {
                        using (currentTenant.Change(tenant.Id))
                        {
                            updated += await ReviewCurrentTenantAsync(scope.ServiceProvider, catalog, percent, workerContext.CancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Model catalog price review failed for tenant {TenantId}.", tenant.Id);
                    }
                }
            }

            Logger.LogInformation("Model catalog price review updated {Count} routes.", updated);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Model catalog price review failed.");
        }
    }

    private static async Task<int> ReviewCurrentTenantAsync(
        IServiceProvider serviceProvider,
        IModelCatalogSource catalog,
        decimal percent,
        CancellationToken cancellationToken)
    {
        var workspaceRepository = serviceProvider.GetRequiredService<IWorkspaceRepository>();
        var repository = serviceProvider.GetRequiredService<IAIModelConfigurationRepository>();
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var profiles = serviceProvider.GetRequiredService<IEnumerable<IAiProviderProfile>>();
        var count = await workspaceRepository.GetCountAsync();
        var workspaces = count == 0
            ? new List<Workspace>()
            : await workspaceRepository.GetListAsync(maxResultCount: (int)Math.Min(count, int.MaxValue));
        var catalogs = new Dictionary<string, IReadOnlyList<ModelCatalogEntry>?>(StringComparer.OrdinalIgnoreCase);
        var updated = 0;
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
        foreach (var workspace in workspaces)
        {
            var catalogName = AiProviderProfiles.Find(profiles, workspace.Provider)?.CatalogName;
            IReadOnlyList<ModelCatalogEntry>? models = null;
            if (!string.IsNullOrWhiteSpace(catalogName))
            {
                if (!catalogs.TryGetValue(catalogName, out models))
                {
                    models = await catalog.GetModelsAsync(catalogName, cancellationToken);
                    catalogs[catalogName] = models;
                }
            }

            var routes = await repository.GetByWorkspaceIdAsync(workspace.Id);
            foreach (var route in routes)
            {
                if (!ModelCatalogPriceReview.TryGetReplacement(route, models, percent, out var quote))
                {
                    continue;
                }

                route.ReplaceCatalogPrices(quote);
                await repository.UpdateAsync(route);
                updated++;
            }
        }

        await unitOfWork.CompleteAsync();
        return updated;
    }
}
