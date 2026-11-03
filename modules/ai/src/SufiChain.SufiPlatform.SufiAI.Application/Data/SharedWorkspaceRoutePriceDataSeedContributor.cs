using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Writes prices onto the host shared workspace. Tenants inherit those routes
/// through the live view, so this seed does not copy prices onto projections.
/// </summary>
public class SharedWorkspaceRoutePriceDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IDefaultAiWorkspaceSeeder _workspaceSeeder;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ISharedWorkspaceRoutePriceSource _prices;
    private readonly IModelCatalogSource _catalog;
    private readonly HostPriceMarkup _markup;
    private readonly IEnumerable<IAiProviderProfile> _profiles;
    private readonly IOptions<AIOptions> _options;
    private readonly ILogger<SharedWorkspaceRoutePriceDataSeedContributor> _logger;

    public SharedWorkspaceRoutePriceDataSeedContributor(
        IDefaultAiWorkspaceSeeder workspaceSeeder,
        IWorkspaceRepository workspaceRepository,
        ICurrentTenant currentTenant,
        ISharedWorkspaceRoutePriceSource prices,
        IModelCatalogSource catalog,
        HostPriceMarkup markup,
        IEnumerable<IAiProviderProfile> profiles,
        IOptions<AIOptions> options,
        ILogger<SharedWorkspaceRoutePriceDataSeedContributor> logger)
    {
        _workspaceSeeder = workspaceSeeder;
        _workspaceRepository = workspaceRepository;
        _currentTenant = currentTenant;
        _prices = prices;
        _catalog = catalog;
        _markup = markup;
        _profiles = profiles;
        _options = options;
        _logger = logger;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId != null)
        {
            return;
        }

        if (_currentTenant.Id != null)
        {
            _logger.LogDebug(
                "Skipping shared workspace route prices. Tenant {TenantId} is ambient and the seed context did not name the host.",
                _currentTenant.Id);
            return;
        }

        await _workspaceSeeder.EnsureDefaultWorkspaceAsync(tenantId: null);
        var schedule = await _prices.GetAsync();
        var percent = await _markup.GetPercentAsync();
        var configuredName = _options.Value.DefaultWorkspace?.Name;

        using (_currentTenant.Change(null))
        {
            var count = await _workspaceRepository.GetCountAsync(filter: null);
            if (count <= 0)
            {
                return;
            }

            var max = count > int.MaxValue ? int.MaxValue : (int)count;
            var workspaces = await _workspaceRepository.GetListAsync(maxResultCount: max);
            var shared = HostDefaultWorkspaceMarker.Select(
                workspaces.Where(workspace => workspace.TenantId == null && !workspace.IsInherited).ToList(),
                configuredName);
            if (shared == null || InheritedWorkspaceLiveView.IsLiveView(shared))
            {
                _logger.LogWarning("Shared AI workspace was not found. Route prices were not seeded.");
                return;
            }

            var catalog = await TryLoadCatalogAsync(shared);
            var updated = 0;
            foreach (var route in shared.ModelConfigurations)
            {
                if (!SharedWorkspaceRoutePriceReview.TryGetReplacement(
                        route,
                        catalog,
                        percent,
                        schedule.For(route.CapabilityType),
                        out var quote))
                {
                    continue;
                }

                route.ReplaceCatalogPrices(quote);
                updated++;
            }

            if (updated == 0)
            {
                _logger.LogDebug(
                    "Shared AI workspace {WorkspaceId} already has route prices.",
                    shared.Id);
                return;
            }

            await _workspaceRepository.UpdateAsync(shared, autoSave: true);
            _logger.LogInformation(
                "Seeded prices on {Count} route(s) of shared AI workspace {WorkspaceId}.",
                updated,
                shared.Id);
        }
    }

    private async Task<IReadOnlyList<ModelCatalogEntry>?> TryLoadCatalogAsync(Workspace workspace)
    {
        var catalogName = AiProviderProfiles.Find(_profiles, workspace.Provider)?.CatalogName;
        if (string.IsNullOrWhiteSpace(catalogName))
        {
            return null;
        }

        try
        {
            return await _catalog.GetModelsAsync(catalogName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Model catalog {CatalogName} was unavailable while seeding shared workspace prices. Host setting floors will be used where the route has no positive price.",
                catalogName);
            return null;
        }
    }
}
