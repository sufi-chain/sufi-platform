using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public interface ITagLinkAppService : IApplicationService
{
    Task AssignAsync(AssignTagDto input);
    Task UnassignAsync(AssignTagDto input);
    Task<List<TagDto>> GetTagsByEntityAsync(EntityTagQueryInput input);
    Task<List<TagLinkDto>> GetLinksByTagAsync(Guid tagId);
}

