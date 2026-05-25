using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.TagsManagement.Permissions;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

[Authorize(TagsManagementPermissions.Tags.Default)]
public class TagAppService : ApplicationService, ITagAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly TagManager _tagManager;

    public TagAppService(ITagRepository tagRepository, TagManager tagManager)
    {
        _tagRepository = tagRepository;
        _tagManager = tagManager;
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

    [Authorize(TagsManagementPermissions.Tags.Create)]
    public virtual async Task<TagDto> CreateAsync(CreateTagDto input)
    {
        var tag = await _tagManager.CreateAsync(input.Name, input.Scope, input.Color, CurrentTenant.Id);
        await _tagRepository.InsertAsync(tag, autoSave: true);
        return ObjectMapper.Map<Tag, TagDto>(tag);
    }

    [Authorize(TagsManagementPermissions.Tags.Update)]
    public virtual async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto input)
    {
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
}
