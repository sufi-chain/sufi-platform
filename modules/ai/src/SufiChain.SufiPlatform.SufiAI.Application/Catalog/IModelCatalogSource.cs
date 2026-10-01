using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public interface IModelCatalogSource
{
    Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(string? catalogName, CancellationToken cancellationToken = default);

    Task<ModelCatalogEntry?> FindAsync(string modelId, CancellationToken cancellationToken = default);

    Task<ModelCatalogEntry?> FindAsync(string modelId, string? catalogName, CancellationToken cancellationToken = default);
}
