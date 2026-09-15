namespace SufiChain.SufiPlatform.SufiAI.RAG;

/// <summary>
/// Keeps embedding inputs inside the model window. Character packing is conservative
/// (about two tokens per character) so multilingual text does not 422 the provider.
/// </summary>
public static class EmbeddingInputGuard
{
    public const string ChunkIndexMetadataKey = "chunkIndex";
    public const string ChunkCountMetadataKey = "chunkCount";

    public static int GetCharacterBudget(int maxInputTokens)
    {
        var tokens = Math.Max(256, maxInputTokens);
        return Math.Max(256, tokens / 2);
    }

    public static string FitToBudget(string text, int maxInputTokens)
    {
        ArgumentNullException.ThrowIfNull(text);
        var budget = GetCharacterBudget(maxInputTokens);
        return text.Length <= budget ? text : text[..budget];
    }

    public static List<DocumentChunk> SplitDocuments(
        IReadOnlyList<DocumentChunk> documents,
        int maxInputTokens)
    {
        ArgumentNullException.ThrowIfNull(documents);
        var budget = GetCharacterBudget(maxInputTokens);
        var overlap = Math.Clamp(budget / 8, 32, 256);
        var result = new List<DocumentChunk>();
        foreach (var document in documents)
        {
            result.AddRange(SplitDocument(document, budget, overlap));
        }

        return result;
    }

    private static List<DocumentChunk> SplitDocument(DocumentChunk document, int budget, int overlap)
    {
        var content = document.Content ?? string.Empty;
        var pieces = SplitText(content, budget, overlap);
        var chunks = new List<DocumentChunk>(pieces.Count);
        for (var index = 0; index < pieces.Count; index++)
        {
            var id = pieces.Count == 1
                ? document.Id
                : $"{document.Id}#{index}";
            chunks.Add(CloneChunk(document, id, pieces[index], index, pieces.Count));
        }

        return chunks;
    }

    private static List<string> SplitText(string content, int budget, int overlap)
    {
        if (content.Length <= budget)
        {
            return new List<string> { content };
        }

        var pieces = new List<string>();
        var start = 0;
        while (start < content.Length)
        {
            if (content.Length - start <= budget)
            {
                AddPiece(pieces, content[start..]);
                break;
            }

            var end = FindSplitEnd(content, start, start + budget);
            if (end <= start)
            {
                end = Math.Min(content.Length, start + budget);
            }

            AddPiece(pieces, content[start..end]);
            if (end >= content.Length)
            {
                break;
            }

            var next = end - overlap;
            start = next <= start ? end : next;
        }

        return pieces.Count == 0 ? new List<string> { content } : pieces;
    }

    private static void AddPiece(List<string> pieces, string piece)
    {
        if (string.IsNullOrWhiteSpace(piece) && pieces.Count > 0)
        {
            return;
        }

        pieces.Add(piece);
    }

    private static int FindSplitEnd(string text, int start, int proposedEnd)
    {
        if (proposedEnd >= text.Length)
        {
            return text.Length;
        }

        var minEnd = start + Math.Max(32, (proposedEnd - start) / 2);
        var paragraph = text.LastIndexOf("\n\n", proposedEnd - 1, StringComparison.Ordinal);
        if (paragraph >= minEnd)
        {
            return paragraph + 2;
        }

        var newline = text.LastIndexOf('\n', proposedEnd - 1);
        if (newline >= minEnd)
        {
            return newline + 1;
        }

        for (var i = proposedEnd - 1; i >= minEnd; i--)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                return i + 1;
            }
        }

        return proposedEnd;
    }

    private static DocumentChunk CloneChunk(
        DocumentChunk source,
        string id,
        string content,
        int index,
        int count)
    {
        var metadata = source.Metadata == null
            ? new Dictionary<string, object>()
            : new Dictionary<string, object>(source.Metadata);
        metadata[ChunkIndexMetadataKey] = index;
        metadata[ChunkCountMetadataKey] = count;

        return new DocumentChunk
        {
            Id = id,
            WorkspaceName = source.WorkspaceName,
            SourceName = source.SourceName,
            SourceId = source.SourceId,
            Content = content,
            Metadata = metadata,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            Score = source.Score
        };
    }
}
