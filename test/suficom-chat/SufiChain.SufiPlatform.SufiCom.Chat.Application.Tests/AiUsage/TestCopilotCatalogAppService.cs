using SufiChain.SufiPlatform.SufiAI.Copilots;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Copilots;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

/// <summary>
/// In-memory catalog for Chat application tests. The host does not include Copilots EF,
/// so the real <c>CopilotCatalogAppService</c> cannot resolve <c>ICopilotDefinitionRepository</c>.
/// </summary>
public class TestCopilotCatalogAppService : ICopilotCatalogAppService
{
    public static readonly Guid PublicAssistantId = Guid.Parse("c0a10000-0000-4000-8000-000000000001");
    public static readonly Guid PublicAssistantWorkspaceId = Guid.Parse("c0a10000-0000-4000-8000-000000000002");

    private readonly List<CopilotCatalogItemDto> _items =
    [
        new CopilotCatalogItemDto
        {
            Id = PublicAssistantId,
            Key = ChatCopilotKeys.PublicAssistant.Key,
            SourceModule = "SufiCom.Chat",
            DisplayName = "Everyday Assistant",
            WorkspaceId = PublicAssistantWorkspaceId,
            WorkspaceName = ChatTestData.DefaultWorkspaceName,
            Kind = CopilotKind.Assistant,
            Purpose = "PublicAssistant",
            PersistChatSession = true,
            IsPublic = true,
            EntityVersion = ChatCopilotKeys.EntityVersion
        }
    ];

    public Task<CopilotCatalogItemDto> GetAsync(Guid id)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            throw new Volo.Abp.BusinessException(AICopilotsErrorCodes.CopilotNotFound)
                .WithData("Id", id);
        }

        return Task.FromResult(item);
    }

    public Task<CopilotCatalogItemDto> GetByKeyAsync(string key)
    {
        Check.NotNullOrWhiteSpace(key, nameof(key));

        var item = _items.FirstOrDefault(x =>
            string.Equals(x.Key, key, StringComparison.Ordinal));
        if (item == null)
        {
            throw new Volo.Abp.BusinessException(AICopilotsErrorCodes.CopilotNotFound)
                .WithData("Key", key);
        }

        return Task.FromResult(item);
    }

    public Task<List<CopilotCatalogItemDto>> GetListAsync(GetCopilotCatalogInput input)
    {
        IEnumerable<CopilotCatalogItemDto> items = _items;
        if (input.IncludePublicOnly)
        {
            items = items.Where(x => x.IsPublic);
        }

        if (input.Kind.HasValue)
        {
            items = items.Where(x => x.Kind == input.Kind.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Purpose))
        {
            items = items.Where(x => x.Purpose == input.Purpose);
        }

        return Task.FromResult(items.OrderBy(x => x.DisplayName).ToList());
    }
}
