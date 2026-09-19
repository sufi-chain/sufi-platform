using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>Buffers the final assistant response while the provider connection remains streamed.</summary>
public static class AIStreamingChatResponseReader
{
    public static async Task<ChatMessageContent> ReadAsync(
        IAsyncEnumerable<StreamingChatMessageContent> chunks, CancellationToken cancellationToken = default)
    {
        var text = new StringBuilder();
        var metadata = new Dictionary<string, object?>();
        string? modelId = null;
        await foreach (var chunk in chunks.WithCancellation(cancellationToken))
        {
            if (chunk.ChoiceIndex != 0) continue;
            // An assistant preamble preceding a tool call is not part of the final JSON reply.
            if (chunk.Items.OfType<StreamingFunctionCallUpdateContent>().Any())
            {
                text.Clear();
                metadata.Clear();
                continue;
            }
            text.Append(chunk.Content);
            modelId = chunk.ModelId ?? modelId;
            if (chunk.Metadata != null)
                foreach (var item in chunk.Metadata)
                    // Trailing usage chunks carry a null finish reason. Preserve the terminal value.
                    if (item.Value != null) metadata[item.Key] = item.Value;
        }
        cancellationToken.ThrowIfCancellationRequested();
        // A truncated stream throws during enumeration. Never return the partial buffer or retry tool calls here.
        return new ChatMessageContent(AuthorRole.Assistant, text.ToString(), modelId: modelId, metadata: metadata);
    }
}
