using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Adapters;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Adapters;

public class SufiAIChatServiceAdapterTests
{
    [Fact]
    public async Task Should_Map_ModelConfigurationId_Onto_ChatCompletionRequest()
    {
        var routeId = Guid.NewGuid();
        var aiService = Substitute.For<IAIService>();
        ChatCompletionRequest? mapped = null;
        aiService.SendChatMessageAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                mapped = call.Arg<ChatCompletionRequest>();
                return new ChatCompletionResponse { Content = "ok", ModelId = "gpt-4" };
            });
        var adapter = new SufiAIChatServiceAdapter(aiService);

        await adapter.CompleteAsync(new SufiAIChatRequest
        {
            WorkspaceName = "workspace",
            ModelConfigurationId = routeId
        });

        mapped.ShouldNotBeNull();
        mapped!.WorkspaceName.ShouldBe("workspace");
        mapped.ModelConfigurationId.ShouldBe(routeId);
    }
}
