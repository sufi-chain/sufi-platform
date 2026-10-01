using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IModelEndpointLookup))]
public class CatalogModelEndpointLookup : IModelEndpointLookup, ITransientDependency
{
    private readonly IModelCatalogSource _catalog;
    private readonly IEnumerable<IAiProviderProfile> _profiles;
    private readonly ILogger<CatalogModelEndpointLookup> _logger;

    public CatalogModelEndpointLookup(
        IModelCatalogSource catalog,
        IEnumerable<IAiProviderProfile> profiles,
        ILogger<CatalogModelEndpointLookup> logger)
    {
        _catalog = catalog;
        _profiles = profiles;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetSupportedEndpointsAsync(
        AIProviderType provider,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return Array.Empty<string>();
        }

        var catalogName = AiProviderProfiles.Find(_profiles, provider)?.CatalogName;
        if (string.IsNullOrWhiteSpace(catalogName))
        {
            return Array.Empty<string>();
        }

        try
        {
            var entry = await _catalog.FindAsync(modelId, catalogName, cancellationToken);
            return entry?.SupportedEndpoints ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Model catalog endpoints are unavailable. Provider={Provider}, Model={Model}",
                provider,
                modelId);
            return Array.Empty<string>();
        }
    }
}
