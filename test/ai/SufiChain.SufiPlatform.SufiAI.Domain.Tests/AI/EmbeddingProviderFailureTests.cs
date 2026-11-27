using System.Net;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class EmbeddingProviderFailureTests
{
    [Fact]
    public async Task CompatibleEmbeddingGenerator_410_EndOfLife_ThrowsRetiredModel()
    {
        var exception = await AssertRetiredAsync(
            HttpStatusCode.Gone,
            """{"error":{"message":"model reached end of life"}}""");

        exception.Model.ShouldBe("private/openrouter/nvidia/nemotron-3-embed-1b");
        exception.Capability.ShouldBe(EmbeddingProviderFailures.CapabilityEmbedding);
        exception.Message.ShouldNotContain("end of life");
        exception.Message.ShouldContain("nemotron-3-embed-1b");
    }

    [Fact]
    public async Task CompatibleEmbeddingGenerator_404_ModelNotFound_ThrowsRetiredModel()
    {
        var exception = await AssertRetiredAsync(
            HttpStatusCode.NotFound,
            """{"error":{"message":"model not found"}}""");

        exception.Capability.ShouldBe(EmbeddingProviderFailures.CapabilityEmbedding);
    }

    [Fact]
    public async Task CompatibleEmbeddingGenerator_500_KeepsGenericError()
    {
        using var handler = new ScriptedHandler(HttpStatusCode.InternalServerError, "upstream blew up");
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            WorkspaceConfigurationHelper.GenerateEmbeddingsForTestsAsync(
                handler,
                "some-model",
                ["hello"]));

        exception.ShouldNotBeOfType<EmbeddingModelRetiredException>();
        exception.Message.ShouldContain("HTTP 500");
        exception.Message.ShouldContain("upstream blew up");
    }

    [Fact]
    public void UnknownModelWithoutDimensions_IsRejected()
    {
        var exception = Should.Throw<Volo.Abp.BusinessException>(() =>
            EmbeddingProviderFailures.ResolveDimensions("brand-new-embed", null));

        exception.Code.ShouldBe(AIErrorCodes.EmbeddingDimensionsRequired);
    }

    [Fact]
    public void KnownModelWithoutDimensions_UsesTheTable()
    {
        EmbeddingProviderFailures.ResolveDimensions("nvidia/nemotron-3-embed-1b", null).ShouldBe(2048);
        EmbeddingProviderFailures.ResolveDimensions("custom-embed", 768).ShouldBe(768);
    }

    [Fact]
    public void ChatClient_OpenRouterNoEndpoints_ThrowsRetiredModel_Chat()
    {
        var error = LlmCallFailure.Sanitize(new LlmProviderCallException(
            "No endpoints found for cohere/north-mini-code:free",
            statusCode: 404));

        error.Code.ShouldBe(AIErrorCodes.EmbeddingModelRetired);
        error.Data["Capability"].ShouldBe(EmbeddingProviderFailures.CapabilityChat);
        error.Message.ShouldNotContain("No endpoints found");
    }

    [Fact]
    public void ChatClient_404ModelNotFound_ThrowsRetiredModel_Chat()
    {
        var error = LlmCallFailure.Sanitize(new Volo.Abp.BusinessException(AIErrorCodes.ProviderRequestFailed)
            .WithData("StatusCode", 404)
            .WithData("ProviderMessage", "model not found")
            .WithData("ModelId", "retired-chat"));

        error.Code.ShouldBe(AIErrorCodes.EmbeddingModelRetired);
        error.Data["Model"].ShouldBe("retired-chat");
        error.Data.Contains("ProviderMessage").ShouldBeFalse();
    }

    [Fact]
    public void ChatClient_429_IsNotRetired()
    {
        var error = LlmCallFailure.Sanitize(new LlmProviderCallException("slow down", statusCode: 429));
        error.Code.ShouldBe(AIErrorCodes.ProviderRateLimited);
    }

    [Fact]
    public void DecisionClient_RetiredModel_ThrowsRetiredModel_Decision()
    {
        var error = EmbeddingProviderFailures.AsRetiredBusinessException(
            410,
            "end of life",
            "decision-model",
            EmbeddingProviderFailures.CapabilityDecision);

        error.ShouldNotBeNull();
        error!.Code.ShouldBe(AIErrorCodes.EmbeddingModelRetired);
        error.Data["Capability"].ShouldBe(EmbeddingProviderFailures.CapabilityDecision);
        error.Message.ShouldNotContain("end of life");
    }

    [Theory]
    [InlineData(429, "rate limit")]
    [InlineData(404, "gateway timeout")]
    public void Other_statuses_are_not_retired(int statusCode, string body)
    {
        EmbeddingProviderFailures.IsRetired(statusCode, body).ShouldBeFalse();
    }

    private static async Task<EmbeddingModelRetiredException> AssertRetiredAsync(HttpStatusCode status, string body)
    {
        using var handler = new ScriptedHandler(status, body);
        return await Should.ThrowAsync<EmbeddingModelRetiredException>(() =>
            WorkspaceConfigurationHelper.GenerateEmbeddingsForTestsAsync(
                handler,
                "private/openrouter/nvidia/nemotron-3-embed-1b",
                ["hello"]));
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public ScriptedHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body)
            });
        }
    }
}
