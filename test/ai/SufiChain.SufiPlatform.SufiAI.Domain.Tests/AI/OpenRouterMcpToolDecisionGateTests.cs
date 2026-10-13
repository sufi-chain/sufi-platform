using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class OpenRouterMcpToolDecisionGateTests
{
    [Fact]
    public async Task Unsure_off_task_then_clear_yes_destructive_blocks_with_destructive()
    {
        var gate = CreateGate(Answers(
            ("off_task", 0.5),
            ("destructive", 0.95),
            ("untrusted_input", 0.02)));

        var reason = await gate.GetBlockReasonAsync(Context(), "delete", new Dictionary<string, object?>());

        reason.ShouldNotBeNull();
        reason.ShouldContain("destructive");
    }

    [Fact]
    public async Task All_clear_no_allows()
    {
        var gate = CreateGate(Answers(
            ("off_task", 0.05),
            ("destructive", 0.02),
            ("untrusted_input", 0.01)));

        var reason = await gate.GetBlockReasonAsync(Context(), "read", new Dictionary<string, object?>());

        reason.ShouldBeNull();
    }

    [Fact]
    public async Task All_unsure_allows()
    {
        var gate = CreateGate(Answers(
            ("off_task", 0.5),
            ("destructive", 0.4),
            ("untrusted_input", 0.55)));

        var reason = await gate.GetBlockReasonAsync(Context(), "read", new Dictionary<string, object?>());

        reason.ShouldBeNull();
    }

    [Fact]
    public async Task Missing_untrusted_input_with_clear_no_others_allows()
    {
        var answers = Answers(
            ("off_task", 0.05),
            ("destructive", 0.02));
        var gate = CreateGate(answers);

        var reason = await gate.GetBlockReasonAsync(Context(), "read", new Dictionary<string, object?>());

        reason.ShouldBeNull();
    }

    [Fact]
    public async Task Client_null_allows()
    {
        var workspaces = Substitute.For<IWorkspaceRepository>();
        workspaces.FindByNameAsync("demo", Arg.Any<CancellationToken>())
            .Returns(new Workspace(Guid.NewGuid(), "demo", AIProviderType.OpenRouter, "model"));
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns((SystemOneDecisionResult?)null);
        var gate = new OpenRouterMcpToolDecisionGate(client, workspaces);

        var reason = await gate.GetBlockReasonAsync(Context(), "read", new Dictionary<string, object?>());

        reason.ShouldBeNull();
    }

    [Fact]
    public async Task Workspace_not_found_allows_without_calling_client()
    {
        var workspaces = Substitute.For<IWorkspaceRepository>();
        workspaces.FindByNameAsync("demo", Arg.Any<CancellationToken>())
            .Returns((Workspace?)null);
        var client = Substitute.For<ISystemOneDecisionClient>();
        var gate = new OpenRouterMcpToolDecisionGate(client, workspaces);

        var reason = await gate.GetBlockReasonAsync(Context(), "read", new Dictionary<string, object?>());

        reason.ShouldBeNull();
        await client.DidNotReceive().TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>());
    }

    private static OpenRouterMcpToolDecisionGate CreateGate(Dictionary<string, SystemOneAnswer> answers)
    {
        var workspaces = Substitute.For<IWorkspaceRepository>();
        workspaces.FindByNameAsync("demo", Arg.Any<CancellationToken>())
            .Returns(new Workspace(Guid.NewGuid(), "demo", AIProviderType.OpenRouter, "model"));
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SystemOneDecisionResult { Answers = answers });
        return new OpenRouterMcpToolDecisionGate(client, workspaces);
    }

    private static Dictionary<string, SystemOneAnswer> Answers(params (string Name, double Noul)[] items)
    {
        return items.ToDictionary(
            item => item.Name,
            item => new SystemOneAnswer { Type = "noul", Noul = item.Noul });
    }

    private static WorkspaceContext Context() => new() { WorkspaceName = "demo" };
}
