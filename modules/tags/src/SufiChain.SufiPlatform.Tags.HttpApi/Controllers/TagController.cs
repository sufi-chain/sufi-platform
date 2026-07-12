using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Tags.Controllers;

[Area("SufiTags")]
[RemoteService(Name = "SufiTags")]
[Route("api/tags/tags")]
public class TagController : SufiControllerBase, ITagAppService
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