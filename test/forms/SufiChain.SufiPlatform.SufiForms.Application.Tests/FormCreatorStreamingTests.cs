using System.Net.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreatorStreamingTests
{
    [Fact]
    public async Task Complete_stream_retains_final_text_and_usage()
    {
        var result = await AIStreamingChatResponseReader.ReadAsync(Chunks());
        result.Content.ShouldBe("{\"message\":\"Ready\"}");
        SemanticKernelChatTokenUsageExtractor.Extract(result).TotalTokens.ShouldBe(15);
    }

    [Fact]
    public async Task Interrupted_stream_never_returns_partial_proposal()
    {
        await Should.ThrowAsync<HttpIOException>(() => AIStreamingChatResponseReader.ReadAsync(Chunks(fail: true)));
    }

    [Fact]
    public async Task Tool_call_preamble_is_not_prefixed_to_the_final_proposal_json()
    {
        static async IAsyncEnumerable<StreamingChatMessageContent> ToolChunks()
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, "I will inspect the catalog.");
            var tool = new StreamingChatMessageContent(AuthorRole.Assistant, null);
            tool.Items.Add(new StreamingFunctionCallUpdateContent("call-1", "forms_get_designer_catalog", "{}", 0));
            yield return tool;
            await Task.Yield();
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, "{\"message\":\"Ready\"}");
        }
        var result = await AIStreamingChatResponseReader.ReadAsync(ToolChunks());
        result.Content.ShouldBe("{\"message\":\"Ready\"}");
    }

    [Fact]
    public async Task Cancelled_stream_never_returns_partial_proposal()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => AIStreamingChatResponseReader.ReadAsync(Chunks(), cancellation.Token));
    }

    private static async IAsyncEnumerable<StreamingChatMessageContent> Chunks(bool fail = false)
    {
        yield return new StreamingChatMessageContent(AuthorRole.Assistant, "{\"message\":");
        await Task.Yield();
        if (fail) throw new HttpIOException(HttpRequestError.ResponseEnded, "response ended");
        yield return new StreamingChatMessageContent(AuthorRole.Assistant, "\"Ready\"}", metadata:
            new Dictionary<string, object?> { ["InputTokens"] = 10, ["OutputTokens"] = 5, ["TotalTokens"] = 15 });
    }
}
