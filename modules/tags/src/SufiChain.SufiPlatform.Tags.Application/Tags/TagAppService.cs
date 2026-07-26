using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Tags.Caching;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Settings;
using Volo.Abp;
using Volo.Abp.Caching;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Tags.Tags;

[RequiresFeature(SufiTagsFeatures.Enable, SufiTagsFeatures.Tags)]
[Authorize(TagsPermissions.Tags.Default)]
public class TagAppService : SufiApplicationService, ITagAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly TagManager _tagManager;
    private readonly ITagsPolicyProvider _policyProvider;
    private readonly IDistributedCache<TagCacheItem> _tagCache;

    public TagAppService(
        ITagRepository tagRepository,
        TagManager tagManager,
        ITagsPolicyProvider policyProvider,
        IDistributedCache<TagCacheItem> tagCache)
    {
        _tagRepository = tagRepository;
        _tagManager = tagManager;
        _policyProvider = policyProvider;
        _tagCache = tagCache;
    }

    public virtual async Task<TagDto> GetAsync(Guid id)
    {
        var tag = await _tagRepository.GetAsync(id);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    public virtual async Task<PagedResultDto<TagDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _tagRepository.GetQueryableAsync();
        var totalCount = query.LongCount();
        var items = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        return new PagedResultDto<TagDto>(totalCount, ObjectMapper.Map<List<Tag>, List<TagDto>>(items));
    }

    public virtual async Task<ListResultDto<TagDto>> GetListByScopeAsync(string scope)
    {
        var cacheKey = TagCacheItem.CreateScopeListCacheKey(scope);
        var cached = await _tagCache.GetOrAddAsync(cacheKey, async () =>
        {
            var items = await _tagRepository.GetListByScopeAsync(scope, CurrentTenant.Id);
            return new TagCacheItem { Tags = ObjectMapper.Map<List<Tag>, List<TagDto>>(items) };
        });

        return new ListResultDto<TagDto>(cached.Tags);
    }

    public virtual async Task<ListResultDto<TagDto>> SearchAsync(SearchTagsInput input)
    {
        var items = await _tagRepository.SearchAsync(
            input.Scope,
            input.Filter,
            CurrentTenant.Id,
            input.SkipCount,
            input.MaxResultCount);

        return new ListResultDto<TagDto>(ObjectMapper.Map<List<Tag>, List<TagDto>>(items));
    }

    [Authorize(TagsPermissions.Tags.Create)]
    public virtual async Task<TagDto> CreateAsync(CreateTagDto input)
    {
        await CheckTagNameLengthAsync(input.Name);

        var tag = await _tagManager.CreateAsync(input.Name, input.Scope, input.Color, CurrentTenant.Id);
        await _tagRepository.InsertAsync(tag, autoSave: true);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    [Authorize(TagsPermissions.Tags.Update)]
    public virtual async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto input)
    {
        await CheckTagNameLengthAsync(input.Name);

        var tag = await _tagRepository.GetAsync(id);
        var previousScope = tag.Scope;
        tag.SetName(input.Name);
        tag.SetScope(input.Scope);
        tag.SetColor(input.Color);
        await _tagRepository.UpdateAsync(tag, autoSave: true);

        // Scope change: invalidate both old and new scope list caches.
        await _tagCache.RemoveAsync(TagCacheItem.CreateScopeListCacheKey(previousScope), considerUow: true);
        if (!string.Equals(previousScope, input.Scope, StringComparison.Ordinal))
        {
            await _tagCache.RemoveAsync(TagCacheItem.CreateScopeListCacheKey(input.Scope), considerUow: true);
        }

        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    [Authorize(TagsPermissions.Tags.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _tagRepository.DeleteAsync(id, autoSave: true);
    }

    protected virtual async Task CheckTagNameLengthAsync(string name)
    {
        var policy = await _policyProvider.GetAsync();
        if (name.Length > policy.MaxTagNameLength)
        {
            throw new BusinessException(TagsErrorCodes.MaxTagNameLengthExceeded)
                .WithData("MaxTagNameLength", policy.MaxTagNameLength);
        }
    }
}
