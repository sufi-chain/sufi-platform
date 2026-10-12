using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

[DependsOn(typeof(SufiAIApplicationTestModule))]
public class SufiAIWorkspaceSavedKeyTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Deterministic provider traffic: the recorder returns a chat model list for GETs
        // and a 200 for connection probes, and exposes each call's Authorization token.
        context.Services.AddSingleton<Recorder>();
        context.Services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory>(serviceProvider =>
            new RecordingClientFactory(serviceProvider.GetRequiredService<Recorder>())));

        // Make every test observe its own provider request instead of a cached list.
        context.Services.Configure<AIOptions>(options => options.ProviderModelDiscoveryCacheSeconds = 0);
    }
}

[DependsOn(typeof(SufiAIWorkspaceSavedKeyTestModule))]
public class SufiAIWorkspaceSavedKeyDenyTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Always-allow authorization hides the saved-key permission gate. Swap in a
        // denying service so the explicit IsGrantedAsync check in WorkspaceAppService
        // actually evaluates and denies.
        var deny = new DenyAllAuthorization();
        context.Services.Replace(ServiceDescriptor.Singleton<IAuthorizationService>(deny));
        context.Services.Replace(ServiceDescriptor.Singleton<IAbpAuthorizationService>(deny));
    }
}

public class WorkspaceSavedKeyFallbackTests : SufiAITestBase<SufiAIWorkspaceSavedKeyTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly Recorder _recorder;

    public WorkspaceSavedKeyFallbackTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _recorder = GetRequiredService<Recorder>();
    }

    [Fact]
    public async Task Load_models_with_a_blank_key_uses_the_saved_workspace_key()
    {
        const string savedKey = "sk-saved-workspace-key";
        var workspaceId = await InsertWorkspaceAsync("blank-key-saved", savedKey, "https://blank-key.example/v1");

        var models = await _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion
        });

        models.ShouldContain(model => model.Id == "saved-key-model");
        LastAuthorizationToken().ShouldBe(savedKey);
    }

    [Fact]
    public async Task Load_models_with_a_blank_key_falls_back_to_the_primary_capability_configuration_key()
    {
        var workspaceId = await InsertWorkspaceAsync("config-key", apiKey: null, "https://config-key.example/v1");
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "gpt-4",
                apiEndpoint: "https://config-key.example/v1",
                apiKey: "sk-configuration-key");
            await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        });

        var models = await _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion
        });

        models.ShouldContain(model => model.Id == "saved-key-model");
        LastAuthorizationToken().ShouldBe("sk-configuration-key");
    }

    [Fact]
    public async Task A_typed_key_wins_over_the_saved_workspace_key()
    {
        var workspaceId = await InsertWorkspaceAsync("typed-wins", "sk-saved-workspace-key", "https://typed-wins.example/v1");

        var models = await _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ApiKey = "sk-typed-key"
        });

        models.ShouldContain(model => model.Id == "saved-key-model");
        LastAuthorizationToken().ShouldBe("sk-typed-key");
    }

    [Fact]
    public async Task Load_models_without_any_key_fails_with_the_localized_message()
    {
        var workspaceId = await InsertWorkspaceAsync("no-key-anywhere", apiKey: null, "https://no-key.example/v1");

        var exception = await Should.ThrowAsync<UserFriendlyException>(() =>
            _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
            {
                WorkspaceId = workspaceId,
                CapabilityType = AICapabilityType.ChatCompletion
            }));

        exception.Message.ShouldBe("API key is required to load models.");
        _recorder.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Test_connection_with_a_blank_key_uses_the_saved_workspace_key()
    {
        const string savedKey = "sk-saved-workspace-key";
        var workspaceId = await InsertWorkspaceAsync("connection-saved", savedKey, "https://connection.example/v1");

        await _workspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
        {
            WorkspaceId = workspaceId,
            Model = "gpt-4",
            CapabilityType = AICapabilityType.ChatCompletion,
            OpenAIApiMode = OpenAIApiMode.ChatCompletions
        });

        LastAuthorizationToken().ShouldBe(savedKey);
    }

    [Fact]
    public async Task Saved_key_is_never_exposed_in_the_workspace_dto()
    {
        const string savedKey = "sk-plain-secret-key";
        var workspaceId = await InsertWorkspaceAsync("redaction", savedKey, "https://redaction.example/v1");

        var dto = await _workspaceAppService.GetAsync(workspaceId);
        var json = JsonSerializer.Serialize(dto);

        json.ShouldNotContain(savedKey);
        dto.HasApiKey.ShouldBeTrue();
    }

    private async Task<Guid> InsertWorkspaceAsync(string name, string? apiKey, string baseUrl)
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(workspaceId, name, AIProviderType.OpenAI, "gpt-4");
            workspace.UpdateConfiguration("gpt-4", apiKey, baseUrl);
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });
        return workspaceId;
    }

    private string? LastAuthorizationToken() => _recorder.AuthorizationTokens.LastOrDefault();
}

public class WorkspaceSavedKeyAuthorizationTests : SufiAITestBase<SufiAIWorkspaceSavedKeyDenyTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly Recorder _recorder;

    public WorkspaceSavedKeyAuthorizationTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _recorder = GetRequiredService<Recorder>();
    }

    [Fact]
    public async Task A_caller_without_workspace_or_configuration_access_cannot_use_the_saved_key()
    {
        var workspaceId = await InsertWorkspaceAsync("denied-saved-key");

        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
            {
                WorkspaceId = workspaceId,
                CapabilityType = AICapabilityType.ChatCompletion
            }));

        _recorder.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_typed_key_still_works_when_the_saved_key_is_not_accessible()
    {
        var workspaceId = await InsertWorkspaceAsync("denied-but-typed");

        var models = await _workspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
        {
            WorkspaceId = workspaceId,
            CapabilityType = AICapabilityType.ChatCompletion,
            ApiKey = "sk-typed-key"
        });

        models.ShouldContain(model => model.Id == "saved-key-model");
        _recorder.AuthorizationTokens.Last().ShouldBe("sk-typed-key");
    }

    private async Task<Guid> InsertWorkspaceAsync(string name)
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(workspaceId, name, AIProviderType.OpenAI, "gpt-4");
            workspace.UpdateConfiguration("gpt-4", "sk-saved-workspace-key", "https://denied.example/v1");
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });
        return workspaceId;
    }
}

/// <summary>Captures every provider call and the Authorization token it carried.</summary>
public sealed class Recorder
{
    public List<HttpRequestMessage> Requests { get; } = new();

    public List<string?> AuthorizationTokens { get; } = new();
}

public sealed class RecordingClientFactory : IHttpClientFactory
{
    private readonly RecordingHandler _handler;

    public RecordingClientFactory(Recorder recorder)
    {
        _handler = new RecordingHandler(recorder);
    }

    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Recorder _recorder;

        public RecordingHandler(Recorder recorder)
        {
            _recorder = recorder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _recorder.Requests.Add(request);
            _recorder.AuthorizationTokens.Add(request.Headers.Authorization?.Parameter);

            var payload = request.Method == HttpMethod.Get
                ? """{"data":[{"id":"saved-key-model"}]}"""
                : "{}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload),
                RequestMessage = request
            });
        }
    }
}

/// <summary>Denies every authorization check so the explicit permission gate can be asserted.</summary>
internal sealed class DenyAllAuthorization : IAbpAuthorizationService
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();

    public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity("test"));

    public IServiceProvider ServiceProvider => _serviceProvider;

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user,
        object? resource,
        IEnumerable<IAuthorizationRequirement> requirements) =>
        Task.FromResult(AuthorizationResult.Failed());

    public Task<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user,
        object? resource,
        string policyName) =>
        Task.FromResult(AuthorizationResult.Failed());
}
