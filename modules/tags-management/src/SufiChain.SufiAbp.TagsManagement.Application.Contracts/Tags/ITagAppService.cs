using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public interface ITagAppService : IApplicationService
{
    Task<TagDto> GetAsync(Guid id);
    Task<PagedResultDto<TagDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ListResultDto<TagDto>> GetListByScopeAsync(string scope);
    Task<ListResultDto<TagDto>> SearchAsync(SearchTagsInput input);
    Task<TagDto> CreateAsync(CreateTagDto input);
    Task<TagDto> UpdateAsync(Guid id, UpdateTagDto input);
    Task DeleteAsync(Guid id);
}

