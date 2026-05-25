using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp;

namespace SufiChain.SufiAbp.TagsManagement.Controllers;

[Area("tags-management")]
[RemoteService(Name = "tags-management")]
[Route("api/tags-management/tag-links")]
public class TagLinkController : SufiAbpControllerBase, ITagLinkAppService
{
    private readonly ITagLinkAppService _tagLinkAppService;

    public TagLinkController(ITagLinkAppService tagLinkAppService)
    {
        _tagLinkAppService = tagLinkAppService;
    }

    [HttpPost("assign")]
    public virtual Task AssignAsync(AssignTagDto input) => _tagLinkAppService.AssignAsync(input);

    [HttpPost("unassign")]
    public virtual Task UnassignAsync(AssignTagDto input) => _tagLinkAppService.UnassignAsync(input);

    [HttpGet("by-entity")]
    public virtual Task<List<TagDto>> GetTagsByEntityAsync([FromQuery] EntityTagQueryInput input) => _tagLinkAppService.GetTagsByEntityAsync(input);
}

