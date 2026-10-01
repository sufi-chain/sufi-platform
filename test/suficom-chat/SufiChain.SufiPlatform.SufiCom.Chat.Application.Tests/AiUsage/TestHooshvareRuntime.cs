using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

/// <summary>
/// The chat host does not include the Hooshvare and AI persistence layers, so the real
/// resolver, runtime, and workspace catalog cannot resolve their repositories. These doubles
/// keep the chat-side rules (settings, usage guard, input validation) under test.
/// </summary>
public class TestPlatformHooshvareResolver : IPlatformHooshvareResolver
{
    private readonly Dictionary<string, Guid> _ids = new(StringComparer.Ordinal);

    public Task<Guid> GetRuntimeIdByKeyAsync(string hooshvareKey, CancellationToken cancellationToken = default)
        => Task.FromResult(GetOrCreateId(hooshvareKey));

    public Task<Guid> TryGetRuntimeIdByKeyAsync(string hooshvareKey, CancellationToken cancellationToken = default)
        => Task.FromResult(GetOrCreateId(hooshvareKey));

    public Task<PlatformHooshvareRuntimeDto> GetRuntimeDefinitionByKeyAsync(string hooshvareKey, CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDefinition(hooshvareKey));

    public Task<PlatformHooshvareRuntimeDto?> FindDefinitionByKeyAsync(string hooshvareKey, CancellationToken cancellationToken = default)
        => Task.FromResult<PlatformHooshvareRuntimeDto?>(CreateDefinition(hooshvareKey));

    private PlatformHooshvareRuntimeDto CreateDefinition(string hooshvareKey) => new()
    {
        Id = GetOrCreateId(hooshvareKey),
        Key = hooshvareKey,
        SourceModule = "SufiCom.Chat",
        DisplayName = hooshvareKey,
        IsEnabled = true
    };

    private Guid GetOrCreateId(string hooshvareKey)
    {
        lock (_ids)
        {
            if (!_ids.TryGetValue(hooshvareKey, out var id))
            {
                id = Guid.NewGuid();
                _ids[hooshvareKey] = id;
            }

            return id;
        }
    }
}

/// <summary>
/// Forwards runtime requests to <see cref="ISufiAIChatService"/> so tests control the reply
/// through <c>ConfigurableAiService</c>.
/// </summary>
public class TestHooshvareRuntimeAppService : IHooshvareRuntimeAppService
{
    private readonly ISufiAIChatService _chatService;

    public TestHooshvareRuntimeAppService(ISufiAIChatService chatService)
    {
        _chatService = chatService;
    }

    public string WorkspaceName { get; set; } = ChatTestData.DefaultWorkspaceName;

    public async Task<HooshvareRuntimeResultDto> SendAsync(
        HooshvareRuntimeRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var response = await _chatService.CompleteAsync(new SufiAIChatRequest
        {
            WorkspaceName = WorkspaceName,
            Messages =
            {
                new SufiAIChatMessage { Role = SufiAIChatRoles.User, Content = input.Message }
            }
        }, cancellationToken);

        return new HooshvareRuntimeResultDto
        {
            HooshvareId = input.HooshvareId,
            Message = response.Content,
            WorkspaceName = WorkspaceName,
            SessionId = input.SessionId,
            Model = response.ModelId,
            FinishReason = response.FinishReason,
            InputTokens = response.Usage.InputTokens,
            OutputTokens = response.Usage.OutputTokens,
            TotalTokens = response.Usage.TotalTokens
        };
    }

    public async IAsyncEnumerable<HooshvareRuntimeResultDto> StreamAsync(
        HooshvareRuntimeRequestDto input,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return await SendAsync(input, cancellationToken);
    }
}

public class TestSufiAIWorkspaceCatalog : ISufiAIWorkspaceCatalog
{
    public List<SufiAIWorkspaceDescriptor> Workspaces { get; } =
    [
        new SufiAIWorkspaceDescriptor
        {
            Id = TestHooshvareCatalogAppService.PublicAssistantWorkspaceId,
            Name = ChatTestData.DefaultWorkspaceName,
            DisplayName = ChatTestData.DefaultWorkspaceName,
            IsActive = true
        }
    ];

    public Task<List<SufiAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Workspaces.ToList());

    public Task<SufiAIWorkspaceDescriptor?> FindAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(Workspaces.FirstOrDefault(w =>
            string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<SufiAIWorkspaceDescriptor?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Workspaces.FirstOrDefault(w => w.Id == id));
}
