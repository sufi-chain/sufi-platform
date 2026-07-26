using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Tags.Caching;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Settings;
using Volo.Abp;
using Volo.Abp.Caching;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Tags.Tags;

[RequiresFeature(SufiTagsFeatures.Enable, SufiTagsFeatures.TagLinks)]
[Authorize(TagsPermissions.TagLinks.Default)]
public class TagLinkAppService : SufiApplicationService, ITagLinkAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly ITagLinkRepository _tagLinkRepository;
    private readonly ITagsPolicyProvider _policyProvider;
    private readonly IDistributedCache<TagLinkCacheItem> _entityTagCache;

    public TagLinkAppService(
        ITagRepository tagRepository,
        ITagLinkRepository tagLinkRepository,
        ITagsPolicyProvider policyProvider,
        IDistributedCache<TagLinkCacheItem> entityTagCache)
    {
        _tagRepository = tagRepository;
        _tagLinkRepository = tagLinkRepository;
        _policyProvider = policyProvider;
        _entityTagCache = entityTagCache;
    }

    [Authorize(TagsPermissions.TagLinks.Assign)]
    public virtual async Task AssignAsync(AssignTagDto input)
    {
        var tag = await _tagRepository.FindAsync(input.TagId);
        if (tag == null)
        {
            throw new BusinessException(TagsErrorCodes.TagNotFound).WithData("TagId", input.TagId);
        }

        var exists = await _tagLinkRepository.ExistsAsync(input.TagId, input.EntityType, input.EntityId, CurrentTenant.Id);
        if (exists)
        {
            return;
        }

        var policy = await _policyProvider.GetAsync();
        var existingLinks = await _tagLinkRepository.GetListByEntityAsync(input.EntityType, input.EntityId, CurrentTenant.Id);
        if (existingLinks.Count >= policy.MaxTagsPerEntity)
        {
            throw new BusinessException(TagsErrorCodes.MaxTagsPerEntityExceeded)
                .WithData("MaxTagsPerEntity", policy.MaxTagsPerEntity);
        }

        var link = new TagLink(GuidGenerator.Create(), input.TagId, input.EntityType, input.EntityId, CurrentTenant.Id);
        await _tagLinkRepository.InsertAsync(link, autoSave: true);

        await _entityTagCache.RemoveAsync(
            TagLinkCacheItem.CreateEntityTagsCacheKey(input.EntityType, input.EntityId),
            considerUow: true);
    }

    [Authorize(TagsPermissions.TagLinks.Unassign)]
    public virtual async Task UnassignAsync(AssignTagDto input)
    {
        var links = await _tagLinkRepository.GetListByEntityAsync(input.EntityType, input.EntityId, CurrentTenant.Id);
        var target = links.FirstOrDefault(x => x.TagId == input.TagId);
        if (target != null)
        {
            await _tagLinkRepository.DeleteAsync(target, autoSave: true);
            await _entityTagCache.RemoveAsync(
                TagLinkCacheItem.CreateEntityTagsCacheKey(input.EntityType, input.EntityId),
                considerUow: true);
        }
    }

    public virtual async Task<List<TagDto>> GetTagsByEntityAsync(EntityTagQueryInput input)
    {
        var cacheKey = TagLinkCacheItem.CreateEntityTagsCacheKey(input.EntityType, input.EntityId);
        var cached = await _entityTagCache.GetOrAddAsync(cacheKey, async () =>
        {
            var links = await _tagLinkRepository.GetListByEntityAsync(input.EntityType, input.EntityId, CurrentTenant.Id);
            if (links.Count == 0)
            {
                return new TagLinkCacheItem();
            }

            var tagIds = links.Select(x => x.TagId).ToHashSet();
            var query = await _tagRepository.GetQueryableAsync();
            var tags = query.Where(x => tagIds.Contains(x.Id)).ToList();
            return new TagLinkCacheItem { Tags = ObjectMapper.Map<List<Tag>, List<TagDto>>(tags) };
        });

        return cached.Tags;
    }

    public virtual async Task<List<TagLinkDto>> GetLinksByTagAsync(Guid tagId)
    {
        var tag = await _tagRepository.FindAsync(tagId);
        if (tag == null)
        {
            throw new BusinessException(TagsErrorCodes.TagNotFound).WithData("TagId", tagId);
        }

        var links = await _tagLinkRepository.GetListByTagAsync(tagId, CurrentTenant.Id);
        return ObjectMapper.Map<List<TagLink>, List<TagLinkDto>>(links);
    }
}
