using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Catalog;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Fixed OpenRouter catalog for application tests. No network.
/// </summary>
internal sealed class FakeOpenRouterModelCatalogProvider : IModelCatalogProvider
{
    public static readonly IReadOnlyList<ModelCatalogEntry> FixedModels = new[]
    {
        new ModelCatalogEntry
        {
            Id = "openrouter/free",
            Name = "OpenRouter Free",
            Mode = "chat",
            SupportedEndpoints = new List<string> { "/v1/chat/completions" },
            InputModalities = new List<string> { "text", "image" },
            OutputModalities = new List<string> { "text" },
            SupportedParameters = new List<string> { "reasoning" },
            ReasoningEfforts = new List<string> { "low", "high" },
            DefaultReasoningEffort = "low"
        }
    };

    public string Name => OpenRouterModelCatalogSource.NameValue;

    public Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ModelCatalogEntry>?>(FixedModels);
    }

    public Task<ModelCatalogLookup> TryFindAsync(string modelId, CancellationToken cancellationToken = default)
    {
        var match = ModelCatalogMatcher.Match(FixedModels, modelId);
        return Task.FromResult(match == null
            ? new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unknown }
            : new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Found, Entry = match });
    }
}
