using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI;

public static class ModelCatalogMatcher
{
    public static ModelCatalogEntry? Match(IReadOnlyList<ModelCatalogEntry>? catalog, string? modelId)
    {
        if (catalog == null || catalog.Count == 0 || string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        var id = modelId.Trim();
        var exact = catalog.FirstOrDefault(entry => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));
        if (exact != null)
        {
            return exact;
        }

        var lookup = StripNitro(id);
        if (!string.Equals(lookup, id, StringComparison.Ordinal))
        {
            exact = catalog.FirstOrDefault(entry => string.Equals(entry.Id, lookup, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
            {
                return exact;
            }
        }

        var suffix = "/" + lookup;
        var suffixMatches = catalog
            .Where(entry => entry.Id.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();
        return suffixMatches.Count == 1 ? suffixMatches[0] : null;
    }

    public static bool NeedsAuthorSlugLookup(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return false;
        }

        var id = StripNitro(modelId.Trim());
        var slash = id.IndexOf('/');
        return slash > 0 && slash < id.Length - 1 && id.IndexOf('/', slash + 1) < 0;
    }

    public static string StripNitro(string modelId)
    {
        const string suffix = ":nitro";
        return modelId.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? modelId[..^suffix.Length]
            : modelId;
    }
}
