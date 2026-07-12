using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.TagsManagement.Features;
using SufiChain.SufiAbp.TagsManagement.Permissions;
using SufiChain.SufiAbp.TagsManagement.Settings;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

[RequiresFeature(SufiAbpTagsManagementFeatures.Enable, SufiAbpTagsManagementFeatures.Tags)]
[Authorize(TagsManagementPermissions.Tags.Default)]
public class TagAppService : SufiAbpApplicationService, ITagAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly TagManager _tagManager;
    private readonly ITagsManagementPolicyProvider _policyProvider;

    public TagAppService(
        ITagRepository tagRepository,
        TagManager tagManager,
        ITagsManagementPolicyProvider policyProvider)
    {
        _tagRepository = tagRepository;
        _tagManager = tagManager;
        _policyProvider = policyProvider;
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
        var items = await _tagRepository.GetListByScopeAsync(scope, CurrentTenant.Id);
        return new ListResultDto<TagDto>(ObjectMapper.Map<List<Tag>, List<TagDto>>(items));
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

    [Authorize(TagsManagementPermissions.Tags.Create)]
    public virtual async Task<TagDto> CreateAsync(CreateTagDto input)
    {
        await CheckTagNameLengthAsync(input.Name);

        var tag = await _tagManager.CreateAsync(input.Name, input.Scope, input.Color, CurrentTenant.Id);
        await _tagRepository.InsertAsync(tag, autoSave: true);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    [Authorize(TagsManagementPermissions.Tags.Update)]
    public virtual async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto input)
    {
        await CheckTagNameLengthAsync(input.Name);

        var tag = await _tagRepository.GetAsync(id);
        tag.SetName(input.Name);
        tag.SetScope(input.Scope);
        tag.SetColor(input.Color);
        await _tagRepository.UpdateAsync(tag, autoSave: true);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    [Authorize(TagsManagementPermissions.Tags.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _tagRepository.DeleteAsync(id, autoSave: true);
    }

    protected virtual async Task CheckTagNameLengthAsync(string name)
    {
        var policy = await _policyProvider.GetAsync();
        if (name.Length > policy.MaxTagNameLength)
        {
            throw new BusinessException(TagsManagementErrorCodes.MaxTagNameLengthExceeded)
                .WithData("MaxTagNameLength", policy.MaxTagNameLength);
        }
    }
}
