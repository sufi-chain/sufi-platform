using System;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiAbp.AI.Application.Tests.Workspaces;

public class WorkspaceAppServiceTests : AITestBase<AIApplicationTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceAppServiceTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Workspaces()
    {
        // Arrange
        await WithUnitOfWorkAsync(async () =>
        {
            await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "workspace-1",
                AIProviderType.OpenAI,
                "gpt-4"
            ));

            await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "workspace-2",
                AIProviderType.OpenAI,
                "gpt-4o-mini"
            ));
        });

        // Act
        var result = await _workspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldContain(x => x.Name == "workspace-1");
        result.Items.ShouldContain(x => x.Name == "workspace-2");
    }

    [Fact]
    public async Task Should_Create_Workspace()
    {
        // Arrange
        var input = new CreateWorkspaceDto
        {
            Name = "new-workspace",
            
            Provider = AIProviderType.OpenAI,
            Model = "gpt-4",
            ApiKey = "sk-test-key",
            ApiBaseUrl = "https://api.openai.com/v1"
        };

        // Act
        var result = await _workspaceAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        
        result.Provider.ShouldBe(input.Provider);
        result.Model.ShouldBe(input.Model);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Create_Workspace_With_Duplicate_Name()
    {
        // Arrange
        await WithUnitOfWorkAsync(async () =>
        {
            await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "duplicate-name",
                AIProviderType.OpenAI,
                "gpt-4"
            ));
        });

        var input = new CreateWorkspaceDto
        {
            Name = "duplicate-name",
            Provider = AIProviderType.OpenAI,
            Model = "gpt-4"
        };

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _workspaceAppService.CreateAsync(input);
        });
    }

    [Fact]
    public async Task Should_Update_Workspace()
    {
        // Arrange
        Guid workspaceId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "update-test",
                AIProviderType.OpenAI,
                "gpt-3.5-turbo"
            ));
            workspaceId = workspace.Id;
        });

        var input = new UpdateWorkspaceDto
        {
            
            Provider = AIProviderType.OpenAI,
            Model = "gpt-4",
            ApiKey = "new-key",
            ApiBaseUrl = "https://new-endpoint.com",
            IsActive = false
        };

        // Act
        var result = await _workspaceAppService.UpdateAsync(workspaceId, input);

        // Assert
        
        result.Model.ShouldBe(input.Model);
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Delete_Workspace()
    {
        // Arrange
        Guid workspaceId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "delete-test",
                AIProviderType.OpenAI,
                "gpt-4"
            ));
            workspaceId = workspace.Id;
        });

        // Act
        await _workspaceAppService.DeleteAsync(workspaceId);

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.FindAsync(workspaceId);
            workspace.ShouldBeNull();
        });
    }

    [Fact]
    public async Task Should_Get_Workspace_By_Id()
    {
        // Arrange
        Guid workspaceId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.InsertAsync(new Workspace(
                Guid.NewGuid(),
                "get-test",
                AIProviderType.OpenAI,
                "gpt-4"
            ));
            workspaceId = workspace.Id;
        });

        // Act
        var result = await _workspaceAppService.GetAsync(workspaceId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(workspaceId);
        result.Name.ShouldBe("get-test");
    }
}
