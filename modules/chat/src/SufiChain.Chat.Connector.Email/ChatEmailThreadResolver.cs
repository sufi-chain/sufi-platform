namespace SufiChain.Chat.Connectors.Email;

public static class ChatEmailThreadResolver
{
    public static string ResolveExternalThreadId(
        string messageId,
        string? inReplyTo,
        string? references)
    {
        var lookupIds = BuildLookupIds(messageId, inReplyTo, references);
        return lookupIds[0];
    }

    public static IReadOnlyList<string> BuildLookupIds(
        string messageId,
        string? inReplyTo,
        string? references)
    {
        var ids = new List<string>();

        AddId(ids, NormalizeMessageId(inReplyTo));
        AddId(ids, NormalizeMessageId(messageId));

        foreach (var reference in ParseReferences(references))
        {
            AddId(ids, reference);
        }

        if (ids.Count == 0)
        {
            ids.Add(NormalizeMessageId(messageId));
        }

        return ids;
    }

    public static string NormalizeMessageId(string? messageId)
    {
        if (messageId.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        var normalized = messageId.Trim();

        if (normalized.StartsWith('<') && normalized.EndsWith('>'))
        {
            normalized = normalized[1..^1];
        }

        return normalized;
    }

    private static IEnumerable<string> ParseReferences(string? references)
    {
        if (references.IsNullOrWhiteSpace())
        {
            yield break;
        }

        foreach (var token in references.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = NormalizeMessageId(token);
            if (!normalized.IsNullOrWhiteSpace())
            {
                yield return normalized;
            }
        }
    }

    private static void AddId(List<string> ids, string? value)
    {
        if (value.IsNullOrWhiteSpace() || ids.Contains(value))
        {
            return;
        }

        ids.Add(value);
    }
}
