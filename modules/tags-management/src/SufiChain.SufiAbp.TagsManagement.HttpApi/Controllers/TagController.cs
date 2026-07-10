using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp;

namespace SufiChain.SufiAbp.TagsManagement.Controllers;

[Area("tags-management")]
[RemoteService(Name = "tags-management")]
[Route("api/tags-management/tags")]
public class TagController : SufiAbpControllerBase, ITagAppService
{
    private readonly ITagAppService _tagAppService;

    public TagController(ITagAppService tagAppService)
    {
        _tagAppService = tagAppService;
    }

    [HttpGet("{id:guid}")]
    public virtual Task<TagDto> GetAsync(Guid id) => _tagAppService.GetAsync(id);

    [HttpGet]
    public virtual Task<PagedResultDto<TagDto>> GetListAsync(PagedAndSortedResultRequestDto input) => _tagAppService.GetListAsync(input);

    [HttpGet("scope/{scope}")]
    public virtual Task<ListResultDto<TagDto>> GetListByScopeAsync(string scope) => _tagAppService.GetListByScopeAsync(scope);

    [HttpGet("search")]
    public virtual Task<ListResultDto<TagDto>> SearchAsync([FromQuery] SearchTagsInput input) => _tagAppService.SearchAsync(input);

    [HttpPost]
    public virtual Task<TagDto> CreateAsync(CreateTagDto input) => _tagAppService.CreateAsync(input);

    [HttpPut("{id:guid}")]
    public virtual Task<TagDto> UpdateAsync(Guid id, UpdateTagDto input) => _tagAppService.UpdateAsync(id, input);

    [HttpDelete("{id:guid}")]
    public virtual Task DeleteAsync(Guid id) => _tagAppService.DeleteAsync(id);
}

