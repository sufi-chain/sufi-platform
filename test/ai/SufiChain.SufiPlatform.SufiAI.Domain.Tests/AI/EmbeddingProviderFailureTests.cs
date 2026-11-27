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
