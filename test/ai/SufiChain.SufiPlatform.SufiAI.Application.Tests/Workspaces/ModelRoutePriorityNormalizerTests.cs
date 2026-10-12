using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class ModelRoutePriorityNormalizerTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly ModelRoutePriorityNormalizer _normalizer;
    private readonly IWorkspaceRepository _workspaceRepository;

    public ModelRoutePriorityNormalizerTests()
    {
        _normalizer = GetRequiredService<ModelRoutePriorityNormalizer>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
    }

    [Fact]
    public async Task Seed_rewrites_tie_gap_and_negative_priorities_keeping_the_winner()
    {
        var workspaceId = await InsertMessyWorkspaceAsync("normalize-messy");

        await WithUnitOfWorkAsync(() => _normalizer.SeedAsync(new DataSeedContext()));

        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            var ordered = workspace.ModelConfigurations
                .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
                .OrderBy(route => route.Priority)
                .ToList();

            ordered.Select(route => route.Priority).ShouldBe(new[] { 0, 1, 2 });
            ordered[0].ModelId.ShouldBe("late");
            workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.ModelId.ShouldBe("late");
        });
    }

    [Fact]
    public async Task Second_seed_run_is_a_no_op()
    {
        var workspaceId = await InsertMessyWorkspaceAsync("normalize-idempotent");
        await WithUnitOfWorkAsync(() => _normalizer.SeedAsync(new DataSeedContext()));

        string stampAfterFirst = string.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId);
            stampAfterFirst = workspace.ConcurrencyStamp;
        });

        await WithUnitOfWorkAsync(() => _normalizer.SeedAsync(new DataSeedContext()));

        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            workspace.ConcurrencyStamp.ShouldBe(stampAfterFirst);
            workspace.ModelConfigurations
                .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
                .Select(route => route.Priority)
                .OrderBy(priority => priority)
                .ShouldBe(new[] { 0, 1, 2 });
        });
    }

    [Fact]
    public async Task Clean_workspace_stamp_is_unchanged()
    {
        var workspaceId = Guid.NewGuid();
        string stampBefore = string.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "normalize-clean",
                AIProviderType.OpenAICompatible,
                "chat-a");
            workspace.UpdateConfiguration("chat-a", "sk-test", "https://example.test/v1");
            workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "chat-a");
            workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "chat-b");
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            stampBefore = workspace.ConcurrencyStamp;
        });

        await WithUnitOfWorkAsync(() => _normalizer.SeedAsync(new DataSeedContext()));

        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId);
            workspace.ConcurrencyStamp.ShouldBe(stampBefore);
        });
    }

    private async Task<Guid> InsertMessyWorkspaceAsync(string name)
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                name,
                AIProviderType.OpenAICompatible,
                "late");
            workspace.UpdateConfiguration("late", "sk-test", "https://example.test/v1");
            var early = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "early");
            var mid = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "mid");
            var late = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "late");

            ObjectHelper.TrySetProperty(early, item => item.CreationTime, () => new DateTime(2020, 1, 1));
            ObjectHelper.TrySetProperty(mid, item => item.CreationTime, () => new DateTime(2020, 1, 2));
            ObjectHelper.TrySetProperty(late, item => item.CreationTime, () => new DateTime(2020, 1, 3));

            early.SetPriority(5);
            mid.SetPriority(5);
            late.SetPriority(-3);

            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });
        return workspaceId;
    }
}
