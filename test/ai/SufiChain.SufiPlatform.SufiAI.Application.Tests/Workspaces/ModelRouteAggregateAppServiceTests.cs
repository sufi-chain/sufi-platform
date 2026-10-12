using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class ModelRouteAggregateAppServiceTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IAIAppService _aiAppService;
    private readonly IWorkspaceRepository _workspaceRepository;

    public ModelRouteAggregateAppServiceTests()
    {
        _aiAppService = GetRequiredService<IAIAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
    }

    [Fact]
    public async Task Create_appends_contiguous_priorities_per_capability()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-append");

        var first = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-a",
            IsUserSelectable = true
        });
        var second = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-b",
            IsUserSelectable = true
        });

        first.Priority.ShouldBe(0);
        first.IsDefault.ShouldBeTrue();
        second.Priority.ShouldBe(1);
        second.IsDefault.ShouldBeFalse();
        first.ConcurrencyStamp.ShouldNotBeNullOrWhiteSpace();

        var listed = await _aiAppService.GetModelConfigurationsAsync(workspaceId);
        listed.Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .Select(route => route.Priority)
            .ShouldBe([0, 1]);
        listed.Count(route => route.CapabilityType == AICapabilityType.ChatCompletion && route.IsDefault)
            .ShouldBe(1);
    }

    [Fact]
    public async Task Create_with_position_zero_inserts_as_new_default()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-insert");
        await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-old",
            IsUserSelectable = true
        });

        var inserted = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-new",
            Priority = 0,
            IsUserSelectable = true
        });

        inserted.Priority.ShouldBe(0);
        inserted.IsDefault.ShouldBeTrue();
        inserted.ModelId.ShouldBe("chat-new");

        var chat = (await _aiAppService.GetModelConfigurationsAsync(workspaceId))
            .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .OrderBy(route => route.Priority)
            .ToList();
        chat.Select(route => route.ModelId).ShouldBe(["chat-new", "chat-old"]);
        chat.Select(route => route.Priority).ShouldBe([0, 1]);
    }

    [Fact]
    public async Task Update_can_move_a_route_and_SetDefault_works_for_embeddings()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-move");
        var stamp = await ReadStampAsync(workspaceId);

        var embA = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.Embeddings,
            ModelId = "emb-a",
            Dimensions = 1536,
            ConcurrencyStamp = stamp
        });
        stamp = embA.ConcurrencyStamp;
        var embB = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.Embeddings,
            ModelId = "emb-b",
            Dimensions = 1536,
            ConcurrencyStamp = stamp
        });

        embA.IsDefault.ShouldBeTrue();
        embB.Priority.ShouldBe(1);

        await _aiAppService.SetWorkspaceDefaultModelConfigurationAsync(
            embB.Id,
            new ModelConfigurationConcurrencyDto { ConcurrencyStamp = embB.ConcurrencyStamp });

        var listed = await _aiAppService.GetModelConfigurationsAsync(workspaceId);
        var embeddings = listed
            .Where(route => route.CapabilityType == AICapabilityType.Embeddings)
            .OrderBy(route => route.Priority)
            .ToList();
        embeddings[0].ModelId.ShouldBe("emb-b");
        embeddings[0].IsDefault.ShouldBeTrue();
        embeddings[1].ModelId.ShouldBe("emb-a");
        embeddings[1].IsDefault.ShouldBeFalse();
    }

    [Fact]
    public async Task Move_and_Delete_keep_priorities_contiguous()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-delete");
        var a = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-a",
            IsUserSelectable = true
        });
        var b = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-b",
            IsUserSelectable = true,
            ConcurrencyStamp = a.ConcurrencyStamp
        });
        var c = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-c",
            IsUserSelectable = true,
            ConcurrencyStamp = b.ConcurrencyStamp
        });

        await _aiAppService.MoveModelConfigurationAsync(c.Id, new MoveAIModelConfigurationDto
        {
            Position = 0,
            ConcurrencyStamp = c.ConcurrencyStamp
        });

        var afterMove = await _aiAppService.GetModelConfigurationsAsync(workspaceId);
        var stamp = afterMove[0].ConcurrencyStamp;
        afterMove.Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .OrderBy(route => route.Priority)
            .Select(route => route.ModelId)
            .ShouldBe(["chat-c", "chat-a", "chat-b"]);

        var middle = afterMove.Single(route => route.ModelId == "chat-a");
        await _aiAppService.DeleteModelConfigurationAsync(
            middle.Id,
            new ModelConfigurationConcurrencyDto { ConcurrencyStamp = stamp });

        var afterDelete = (await _aiAppService.GetModelConfigurationsAsync(workspaceId))
            .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .OrderBy(route => route.Priority)
            .ToList();
        afterDelete.Select(route => route.ModelId).ShouldBe(["chat-c", "chat-b"]);
        afterDelete.Select(route => route.Priority).ShouldBe([0, 1]);
        afterDelete[0].IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Stale_concurrency_stamp_returns_a_friendly_message()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-concurrency");
        var created = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-a",
            IsUserSelectable = true
        });

        var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
            _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
            {
                WorkspaceId = workspaceId,
                CapabilityType = AICapabilityType.ChatCompletion,
                ModelId = "chat-b",
                IsUserSelectable = true,
                ConcurrencyStamp = "stale-stamp"
            }));

        exception.Message.ShouldContain("list changed", Case.Insensitive);
        created.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Delete_rejects_removing_the_only_enabled_default()
    {
        var workspaceId = await InsertWorkspaceAsync("routes-only-default");
        var only = await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ModelId = "chat-only",
            IsUserSelectable = true
        });

        var exception = await Should.ThrowAsync<global::Volo.Abp.BusinessException>(() =>
            _aiAppService.DeleteModelConfigurationAsync(
                only.Id,
                new ModelConfigurationConcurrencyDto { ConcurrencyStamp = only.ConcurrencyStamp }));
        exception.Code.ShouldBe(AIErrorCodes.CannotDeleteWorkspaceDefault);
    }

    private async Task<Guid> InsertWorkspaceAsync(string name)
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                name,
                AIProviderType.OpenAICompatible,
                "base-chat");
            workspace.UpdateConfiguration("base-chat", "sk-test", "https://example.test/v1");
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });
        return workspaceId;
    }

    private async Task<string> ReadStampAsync(Guid workspaceId)
    {
        string stamp = string.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId);
            stamp = workspace.ConcurrencyStamp;
        });
        return stamp;
    }
}
