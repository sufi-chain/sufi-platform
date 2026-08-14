using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class WorkspaceAppServiceTests : SufiAITestBase<SufiAIApplicationTestModule>
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
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "gpt-3.5-turbo",
                openAIApiMode: OpenAIApiMode.ChatCompletions);
            await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
            workspaceId = workspace.Id;
        });

        var input = new UpdateWorkspaceDto
        {
            Name = "update-test",
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

        var updated = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
        var chatConfiguration = updated.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!;
        chatConfiguration.ModelId.ShouldBe(input.Model);
        chatConfiguration.ApiEndpoint.ShouldBe(input.ApiBaseUrl);
        chatConfiguration.OpenAIApiMode.ShouldBe(input.OpenAIApiMode);
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

    [Fact]
    public async Task Should_Resolve_Effective_Chat_Model_Configuration_For_Readiness()
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "readiness-overrides",
                AIProviderType.OpenAI,
                "workspace-model");
            workspace.UpdateConfiguration(
                "workspace-model",
                "workspace-key",
                "https://workspace.example/v1",
                null,
                0.7f,
                200000,
                OpenAIApiMode.ChatCompletions);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "configured-model",
                apiEndpoint: "https://model.example/v1",
                apiKey: null,
                openAIApiMode: OpenAIApiMode.Responses);
            await _workspaceRepository.InsertAsync(workspace);
        });

        var result = await _workspaceAppService.GetReadinessAsync(workspaceId);
        var chat = result.Capabilities.Single(
            item => item.CapabilityType == AICapabilityType.ChatCompletion);

        result.IsConfigured.ShouldBeTrue();
        result.IsReady.ShouldBeTrue();
        chat.ModelId.ShouldBe("configured-model");
        chat.OpenAIApiMode.ShouldBe(OpenAIApiMode.Responses);
        chat.HasApiEndpoint.ShouldBeTrue();
        chat.HasApiKey.ShouldBeTrue();
        chat.UsesWorkspaceFallback.ShouldBeFalse();
        result.Mcp.IsReady.ShouldBeFalse();
        result.Mcp.FailureCode.ShouldBe(WorkspaceRuntimeFailureCodes.McpApiModeNotSupported);
    }

    [Fact]
    public async Task Should_Report_Missing_Optional_Capabilities_Without_Chat_Fallback()
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "readiness-optional",
                AIProviderType.OpenAI,
                "chat-model");
            workspace.UpdateConfiguration(
                "chat-model",
                "workspace-key",
                "https://api.openai.com/v1",
                null,
                0.7f,
                200000);
            await _workspaceRepository.InsertAsync(workspace);
        });

        var result = await _workspaceAppService.GetReadinessAsync(workspaceId);
        var embeddings = result.Capabilities.Single(
            item => item.CapabilityType == AICapabilityType.Embeddings);

        embeddings.IsConfigured.ShouldBeFalse();
        embeddings.IsReady.ShouldBeFalse();
        embeddings.ModelId.ShouldBeNull();
        embeddings.FailureCode.ShouldBe(WorkspaceRuntimeFailureCodes.ModelNotConfigured);
    }

    [Fact]
    public async Task Should_Report_Invalid_Effective_Endpoint()
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "readiness-invalid-endpoint",
                AIProviderType.OpenAI,
                "chat-model");
            workspace.UpdateConfiguration(
                "chat-model",
                "workspace-key",
                "not-a-valid-endpoint",
                null,
                0.7f,
                200000);
            await _workspaceRepository.InsertAsync(workspace);
        });

        var result = await _workspaceAppService.GetReadinessAsync(workspaceId);

        result.IsReady.ShouldBeFalse();
        result.Capabilities.Single(
                item => item.CapabilityType == AICapabilityType.ChatCompletion)
            .FailureCode.ShouldBe(WorkspaceRuntimeFailureCodes.EndpointInvalid);
    }

    [Fact]
    public async Task Readiness_Should_Not_Expose_Credentials_Or_Endpoint_Values()
    {
        var workspaceId = Guid.NewGuid();
        const string secret = "super-secret-api-key";
        const string endpoint = "https://private-provider.example/v1";
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "readiness-redaction",
                AIProviderType.OpenAI,
                "chat-model");
            workspace.UpdateConfiguration(
                "chat-model",
                secret,
                endpoint,
                null,
                0.7f,
                200000);
            await _workspaceRepository.InsertAsync(workspace);
        });

        var result = await _workspaceAppService.GetReadinessAsync(workspaceId);
        var json = JsonSerializer.Serialize(result);

        json.ShouldNotContain(secret);
        json.ShouldNotContain(endpoint);
        json.ShouldContain("\"HasApiKey\":true");
        json.ShouldContain("\"HasApiEndpoint\":true");
    }
}
