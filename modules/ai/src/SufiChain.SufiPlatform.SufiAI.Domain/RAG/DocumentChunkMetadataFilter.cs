namespace SufiChain.SufiPlatform.SufiAI.RAG;

/// <summary>
/// Exact-match helpers for RAG document metadata filters (index harvest and search prep).
/// </summary>
public static class DocumentChunkMetadataFilter
{
    public static Dictionary<string, string> Normalize(IReadOnlyDictionary<string, string>? metadataFilters)
    {
        if (metadataFilters == null || metadataFilters.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return metadataFilters
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(
                pair => pair.Key.Trim(),
                pair => pair.Value.Trim(),
                StringComparer.OrdinalIgnoreCase);
    }

    public static List<DocumentChunk> Filter(
        List<DocumentChunk> documents,
        IReadOnlyDictionary<string, string> metadataFilters)
    {
        if (metadataFilters.Count == 0)
        {
            return documents;
        }

        return documents
            .Where(document => Matches(document, metadataFilters))
            .ToList();
    }

    public static bool Matches(
        DocumentChunk document,
        IReadOnlyDictionary<string, string> metadataFilters)
    {
        foreach (var (key, expected) in metadataFilters)
        {
            if (document.Metadata == null ||
                !TryGetMetadataValue(document.Metadata, key, out var actual) ||
                actual == null)
            {
                return false;
            }

            if (!string.Equals(actual.Trim(), expected, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetMetadataValue(
        IReadOnlyDictionary<string, object> metadata,
        string key,
        out string? value)
    {
        value = null;

        if (metadata.TryGetValue(key, out var raw) && raw != null)
        {
            value = FormatMetadataValue(raw);
            return !string.IsNullOrWhiteSpace(value);
        }

        foreach (var pair in metadata)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase) && pair.Value != null)
            {
                value = FormatMetadataValue(pair.Value);
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        return false;
    }

    private static string? FormatMetadataValue(object raw)
    {
        return raw switch
        {
            string s => s,
            Guid guid => guid.ToString("D"),
            _ => raw.ToString()
        };
    }
}
