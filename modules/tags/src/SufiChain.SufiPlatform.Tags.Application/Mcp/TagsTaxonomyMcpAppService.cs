using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Tags.Hooshvare;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Tags;

namespace SufiChain.SufiPlatform.Tags.Mcp;

[Authorize]
public class TagsTaxonomyMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected ITagAppService Tags { get; }
    protected ITagLinkAppService Links { get; }

    public TagsTaxonomyMcpAppService(ITagAppService tags, ITagLinkAppService links)
    {
        Tags = tags;
        Links = links;
    }

    [SufiAiMcpTool(TagsTaxonomyHooshvareKeys.Tools.Search,
        "Searches tags. Does not create or change tags.", ReadOnly = true)]
    [Authorize(TagsPermissions.Tags.Default)]
    public virtual async Task<object> SearchAsync(string? scope = null, string? filter = null)
    {
        var result = await Tags.SearchAsync(new SearchTagsInput
        {
            Scope = scope,
            Filter = filter,
            MaxResultCount = MaxResults
        });
        return result.Items.Select(MapTag).ToList();
    }

    [SufiAiMcpTool(TagsTaxonomyHooshvareKeys.Tools.Get,
        "Returns one tag. Does not change it.", ReadOnly = true)]
    [Authorize(TagsPermissions.Tags.Default)]
    public virtual async Task<object> GetAsync(Guid id) => MapTag(await Tags.GetAsync(id));

    [SufiAiMcpTool(TagsTaxonomyHooshvareKeys.Tools.ListByScope,
        "Lists tags in one scope. Does not change them.", ReadOnly = true)]
    [Authorize(TagsPermissions.Tags.Default)]
    public virtual async Task<object> ListByScopeAsync(string scope)
    {
        var result = await Tags.GetListByScopeAsync(scope);
        return result.Items.Take(MaxResults).Select(MapTag).ToList();
    }

    [SufiAiMcpTool(TagsTaxonomyHooshvareKeys.Tools.GetLinksByTag,
        "Lists entity links for a tag. Does not assign or unassign.", ReadOnly = true)]
    [Authorize(TagsPermissions.TagLinks.Default)]
    public virtual async Task<object> GetLinksByTagAsync(Guid tagId)
    {
        var links = await Links.GetLinksByTagAsync(tagId);
        return links.Take(MaxResults).Select(link => new
        {
            link.Id,
            link.TagId,
            link.EntityType,
            link.EntityId
        }).ToList();
    }

    [SufiAiMcpTool(TagsTaxonomyHooshvareKeys.Tools.GetTagsByEntity,
        "Lists tags assigned to one entity. Does not assign or unassign.", ReadOnly = true)]
    [Authorize(TagsPermissions.TagLinks.Default)]
    public virtual async Task<object> GetTagsByEntityAsync(string entityType, Guid entityId)
    {
        var tags = await Links.GetTagsByEntityAsync(new EntityTagQueryInput
        {
            EntityType = entityType,
            EntityId = entityId
        });
        return tags.Take(MaxResults).Select(MapTag).ToList();
    }

    private static object MapTag(TagDto tag) => new
    {
        tag.Id,
        tag.Name,
        tag.Scope,
        tag.Color
    };
}
