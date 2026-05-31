using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.TagsManagement.Features;
using SufiChain.SufiAbp.TagsManagement.Permissions;
using SufiChain.SufiAbp.TagsManagement.Settings;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Features;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

[RequiresFeature(TagsManagementFeatures.Names.Enable)]
[Authorize(TagsManagementPermissions.TagLinks.Default)]
public class TagLinkAppService : ApplicationService, ITagLinkAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly ITagLinkRepository _tagLinkRepository;
    private readonly ITagsManagementPolicyProvider _policyProvider;

    public TagLinkAppService(
        ITagRepository tagRepository,
        ITagLinkRepository tagLinkRepository,
        ITagsManagementPolicyProvider policyProvider)
    {
        _tagRepository = tagRepository;
        _tagLinkRepository = tagLinkRepository;
        _policyProvider = policyProvider;
    }

    [Authorize(TagsManagementPermissions.TagLinks.Assign)]
    public virtual async Task AssignAsync(AssignTagDto input)
    {
        var tag = await _tagRepository.FindAsync(input.TagId);
        if (tag == null)
        {
            throw new BusinessException(TagsManagementErrorCodes.TagNotFound).WithData("TagId", input.TagId);
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
            throw new BusinessException(TagsManagementErrorCodes.MaxTagsPerEntityExceeded)
                .WithData("MaxTagsPerEntity", policy.MaxTagsPerEntity);
        }

        var link = new TagLink(GuidGenerator.Create(), input.TagId, input.EntityType, input.EntityId, CurrentTenant.Id);
        await _tagLinkRepository.InsertAsync(link, autoSave: true);
    }

    [Authorize(TagsManagementPermissions.TagLinks.Unassign)]
    public virtual async Task UnassignAsync(AssignTagDto input)
    {
        var links = await _tagLinkRepository.GetListByEntityAsync(input.EntityType, input.EntityId, CurrentTenant.Id);
        var target = links.FirstOrDefault(x => x.TagId == input.TagId);
        if (target != null)
        {
            await _tagLinkRepository.DeleteAsync(target, autoSave: true);
        }
    }

    public virtual async Task<List<TagDto>> GetTagsByEntityAsync(EntityTagQueryInput input)
    {
        var links = await _tagLinkRepository.GetListByEntityAsync(input.EntityType, input.EntityId, CurrentTenant.Id);
        if (links.Count == 0)
        {
            return new List<TagDto>();
        }

        var tagIds = links.Select(x => x.TagId).ToHashSet();
        var query = await _tagRepository.GetQueryableAsync();
        var tags = query.Where(x => tagIds.Contains(x.Id)).ToList();
        return ObjectMapper.Map<List<Tag>, List<TagDto>>(tags);
    }
}
