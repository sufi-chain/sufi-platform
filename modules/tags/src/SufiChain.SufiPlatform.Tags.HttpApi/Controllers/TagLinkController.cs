using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Tags.Controllers;

[Area("SufiTags")]
[RemoteService(Name = "SufiTags")]
[Route("api/tags/tag-links")]
public class TagLinkController : SufiControllerBase, ITagLinkAppService
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

    [HttpGet("by-tag/{tagId:guid}")]
    public virtual Task<List<TagLinkDto>> GetLinksByTagAsync(Guid tagId) => _tagLinkAppService.GetLinksByTagAsync(tagId);
}