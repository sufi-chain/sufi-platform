using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Workspaces;

public class DecisionsRoutePriceTests
{
    [Fact]
    public void Decisions_route_without_price_does_not_inherit_chat_workspace_price()
    {
        var workspace = new Workspace(Guid.NewGuid(), "decisions-price", AIProviderType.OpenRouter, "chat");
        workspace.UpdateConfiguration(
            "chat",
            "sk-workspace",
            "https://openrouter.ai/api/v1",
            inputCostPer1MTokens: 3.5m,
            outputCostPer1MTokens: 7m);
        var decisions = workspace.AddModelConfiguration(
            AICapabilityType.Decisions,
            "typesafe/jev-1.13",
            apiKey: "sk-route");

        var resolver = new WorkspaceRuntimeConfigurationResolver(
            Substitute.For<IWorkspaceRepository>(),
            Array.Empty<IAIProvider>(),
            Array.Empty<IAiProviderProfile>(),
            Substitute.For<IAICredentialResolver>());

        var resolved = resolver.Resolve(workspace, AICapabilityType.Decisions, decisions);

        resolved.InputPrice.ShouldBeNull();
        resolved.OutputPrice.ShouldBeNull();
    }

    [Fact]
    public void Chat_route_still_inherits_workspace_token_price()
    {
        var workspace = new Workspace(Guid.NewGuid(), "decisions-price", AIProviderType.OpenRouter, "chat");
        workspace.UpdateConfiguration(
            "chat",
            "sk-workspace",
            "https://openrouter.ai/api/v1",
            inputCostPer1MTokens: 3.5m,
            outputCostPer1MTokens: 7m);
        var chat = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "openrouter/auto",
            apiKey: "sk-route");

        var resolver = new WorkspaceRuntimeConfigurationResolver(
            Substitute.For<IWorkspaceRepository>(),
            Array.Empty<IAIProvider>(),
            Array.Empty<IAiProviderProfile>(),
            Substitute.For<IAICredentialResolver>());

        var resolved = resolver.Resolve(workspace, AICapabilityType.ChatCompletion, chat);

        resolved.InputPrice.ShouldBe(3.5m);
        resolved.OutputPrice.ShouldBe(7m);
    }

    [Fact]
    public void Decisions_route_keeps_its_own_input_price()
    {
        var workspace = new Workspace(Guid.NewGuid(), "decisions-price", AIProviderType.OpenRouter, "chat");
        workspace.UpdateConfiguration(
            "chat",
            "sk-workspace",
            "https://openrouter.ai/api/v1",
            inputCostPer1MTokens: 3.5m);
        var decisions = workspace.AddModelConfiguration(
            AICapabilityType.Decisions,
            "typesafe/jev-1.13",
            apiKey: "sk-route",
            inputPrice: 0.042m);

        var resolver = new WorkspaceRuntimeConfigurationResolver(
            Substitute.For<IWorkspaceRepository>(),
            Array.Empty<IAIProvider>(),
            Array.Empty<IAiProviderProfile>(),
            Substitute.For<IAICredentialResolver>());

        var resolved = resolver.Resolve(workspace, AICapabilityType.Decisions, decisions);

        resolved.InputPrice.ShouldBe(0.042m);
    }
}
