using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public enum ModelCatalogLookupStatus
{
    Found = 0,
    Unknown = 1,
    Unavailable = 2
}

public sealed class ModelCatalogLookup
{
    public ModelCatalogLookupStatus Status { get; init; }

    public ModelCatalogEntry? Entry { get; init; }
}

/// <summary>
/// One external catalog. Load Models and the price job never call this directly.
/// </summary>
public interface IModelCatalogProvider
{
    string Name { get; }

    Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default);

    Task<ModelCatalogLookup> TryFindAsync(string modelId, CancellationToken cancellationToken = default);
}
