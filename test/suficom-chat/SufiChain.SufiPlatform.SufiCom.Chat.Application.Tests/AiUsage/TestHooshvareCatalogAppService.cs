using SufiChain.SufiPlatform.SufiAI.Hooshvare;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

using SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;

using Volo.Abp;



namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;



/// <summary>

/// In-memory catalog for Chat application tests. The host does not include Hooshvares EF,

/// so the real <c>HooshvareCatalogAppService</c> cannot resolve <c>IHooshvareDefinitionRepository</c>.

/// </summary>

public class TestHooshvareCatalogAppService : IHooshvareCatalogAppService

{

    public static readonly Guid PublicAssistantId = Guid.Parse("c0a10000-0000-4000-8000-000000000001");

    public static readonly Guid PublicAssistantWorkspaceId = Guid.Parse("c0a10000-0000-4000-8000-000000000002");



    private readonly List<HooshvareCatalogItemDto> _items =

    [

        new HooshvareCatalogItemDto

        {

            Id = PublicAssistantId,

            Key = ChatHooshvareKeys.PublicAssistant.Key,

            SourceModule = "SufiCom.Chat",

            DisplayName = "Everyday Assistant",

            WorkspaceId = PublicAssistantWorkspaceId,

            WorkspaceName = ChatTestData.DefaultWorkspaceName,

            Kind = HooshvareKind.Assistant,

            Purpose = "PublicAssistant",

            PersistChatSession = true,

            IsPublic = true,

            EntityVersion = ChatHooshvareKeys.EntityVersion

        }

    ];



    public Task<HooshvareCatalogItemDto> GetAsync(Guid id)

    {

        var item = _items.FirstOrDefault(x => x.Id == id);

        if (item == null)

        {

            throw new Volo.Abp.BusinessException(AIHooshvareErrorCodes.HooshvareNotFound)

                .WithData("Id", id);

        }



        return Task.FromResult(item);

    }



    public Task<HooshvareCatalogItemDto> GetByKeyAsync(string key)

    {

        Check.NotNullOrWhiteSpace(key, nameof(key));



        var item = _items.FirstOrDefault(x =>

            string.Equals(x.Key, key, StringComparison.Ordinal));

        if (item == null)

        {

            throw new Volo.Abp.BusinessException(AIHooshvareErrorCodes.HooshvareNotFound)

                .WithData("Key", key);

        }



        return Task.FromResult(item);

    }



    public Task<List<HooshvareCatalogItemDto>> GetListAsync(GetHooshvareCatalogInput input)

    {

        IEnumerable<HooshvareCatalogItemDto> items = _items;

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

