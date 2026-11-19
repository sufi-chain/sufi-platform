using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareReplyCheckTests
{
    [Fact]
    public async Task A_failed_decisions_call_keeps_the_reply()
    {
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SystemOneDecisionResult?>(new HttpRequestException("SystemOneRequestFailed")));
        var chat = Substitute.For<ISufiAIChatService>();
        var markup = new HooshvareTextToolCallMarkupHandler(
            Substitute.For<IMCPToolExecutor>(),
            chat,
            new PassThroughLocalizer(),
            NullLogger<HooshvareTextToolCallMarkupHandler>.Instance);
        var check = new HooshvareReplyCheck([client], chat, markup);
        var response = new SufiAIChatResponse { Content = "The main reply stays." };
        var orchestration = new HooshvareOrchestrationResult
        {
            WorkspaceBinding = new HooshvareWorkspaceBinding
            {
                WorkspaceId = Guid.NewGuid(),
                WorkspaceName = "demo"
            },
            Citations =
            [
                new HooshvareRagCitationDto { DocumentId = "doc-1", Title = "Guide" }
            ],
            Request = new SufiAIChatRequest
            {
                WorkspaceName = "demo",
                Messages = []
            }
        };

        var result = await check.ApplyAsync(orchestration, response);

        result.Content.ShouldBe("The main reply stays.");
        await chat.DidNotReceive().CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>());
    }

    private sealed class PassThroughLocalizer : IStringLocalizer<AIResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
