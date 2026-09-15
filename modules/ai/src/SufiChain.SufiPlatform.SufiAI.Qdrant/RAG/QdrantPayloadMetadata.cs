using Qdrant.Client.Grpc;

namespace SufiChain.SufiPlatform.SufiAI.Qdrant;

/// <summary>
/// Shapes document metadata for Qdrant payloads and filters.
/// Qdrant resolves a dotted filter key such as <c>meta.projectId</c> as a nested path
/// (object <c>meta</c>, field <c>projectId</c>). A flat top-level key that literally
/// contains a dot is never matched by that path, so metadata must be stored as a nested
/// <c>meta</c> object and filtered through the same dotted path.
/// </summary>
public static class QdrantPayloadMetadata
{
    public const string RootKey = "meta";

    private static readonly char[] PathSeparators = ['.', '[', ']'];

    /// <summary>
    /// Builds the nested <c>meta</c> payload value for a document.
    /// </summary>
    public static Value BuildPayloadValue(IReadOnlyDictionary<string, object>? metadata)
    {
        var fields = new Struct();
        foreach (var (key, value) in Flatten(metadata))
        {
            fields.Fields[key] = value;
        }

        return new Value { StructValue = fields };
    }

    /// <summary>
    /// Builds the filter key for a metadata key, for example <c>meta.projectId</c>.
    /// </summary>
    public static string ToFilterKey(string metadataKey)
    {
        return $"{RootKey}.{NormalizeKey(metadataKey)}";
    }

    /// <summary>
    /// Normalizes a metadata key so it can be addressed by a Qdrant nested path.
    /// Path separator characters are replaced because they would otherwise be parsed as deeper nesting.
    /// </summary>
    public static string NormalizeKey(string metadataKey)
    {
        var trimmed = metadataKey.Trim();
        return trimmed.IndexOfAny(PathSeparators) < 0
            ? trimmed
            : string.Create(trimmed.Length, trimmed, static (span, source) =>
            {
                for (var index = 0; index < source.Length; index++)
                {
                    var character = source[index];
                    span[index] = Array.IndexOf(PathSeparators, character) >= 0 ? '_' : character;
                }
            });
    }

    /// <summary>
    /// Flattens metadata to normalized string key/value pairs. Null, empty, and whitespace values are skipped.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> Flatten(IReadOnlyDictionary<string, object>? metadata)
    {
        if (metadata == null || metadata.Count == 0)
        {
            yield break;
        }

        foreach (var (key, raw) in metadata)
        {
            if (string.IsNullOrWhiteSpace(key) || raw == null)
            {
                continue;
            }

            var text = raw switch
            {
                string s => s,
                Guid guid => guid.ToString("D"),
                _ => raw.ToString()
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return new KeyValuePair<string, string>(NormalizeKey(key), text.Trim());
            }
        }
    }
}
