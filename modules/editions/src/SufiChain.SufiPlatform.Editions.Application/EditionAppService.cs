using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Editions;

[Authorize(EditionsPermissions.Editions.Default)]
public class EditionAppService : SufiApplicationService, IEditionAppService
{
    protected IEditionRepository EditionRepository { get; }
    protected EditionManager EditionManager { get; }
    protected SufiEditionsOptions EditionsOptions { get; }
    protected IEnumerable<IEditionDtoContributor> DtoContributors { get; }

    public EditionAppService(
        IEditionRepository editionRepository,
        EditionManager editionManager,
        IOptions<SufiEditionsOptions> editionsOptions,
        IEnumerable<IEditionDtoContributor> dtoContributors)
    {
        EditionRepository = editionRepository;
        EditionManager = editionManager;
        EditionsOptions = editionsOptions.Value;
        DtoContributors = dtoContributors;
    }

    public virtual async Task<EditionDto> GetAsync(Guid id)
    {
        return await MapToDtoAsync(await EditionRepository.GetAsync(id));
    }

    public virtual async Task<PagedResultDto<EditionDto>> GetListAsync(GetEditionsInput input)
    {
        var query = await EditionRepository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(x =>
                x.Name.Contains(filter) ||
                x.DisplayName.Contains(filter) ||
                x.Code.Contains(filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var dtos = items.Select(MapToDto).ToList();
        await ContributeAsync(dtos);
        return new PagedResultDto<EditionDto>(totalCount, dtos);
    }

    [Authorize(EditionsPermissions.Editions.Create)]
    public virtual async Task<EditionDto> CreateAsync(EditionCreateDto input)
    {
        var edition = await EditionManager.CreateAsync(input.Name, input.DisplayName, input.Code, input.IsActive);
        await EditionRepository.InsertAsync(edition, autoSave: true);
        return await MapToDtoAsync(edition);
    }

    [Authorize(EditionsPermissions.Editions.Update)]
    public virtual async Task<EditionDto> UpdateAsync(Guid id, EditionUpdateDto input)
    {
        var edition = await EditionRepository.GetAsync(id);
        edition.ConcurrencyStamp = input.ConcurrencyStamp;
        await EditionManager.ChangeNameAsync(edition, input.Name);
        await EditionManager.ChangeCodeAsync(edition, input.Code);
        edition.SetDisplayName(input.DisplayName);
        edition.SetIsActive(input.IsActive);
        await EditionRepository.UpdateAsync(edition, autoSave: true);
        return await MapToDtoAsync(edition);
    }

    [Authorize(EditionsPermissions.Editions.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await EditionRepository.DeleteAsync(id, autoSave: true);
    }

    protected virtual async Task<EditionDto> MapToDtoAsync(Edition edition)
    {
        var dto = MapToDto(edition);
        await ContributeAsync([dto]);
        return dto;
    }

    protected virtual EditionDto MapToDto(Edition edition)
    {
        var storedDisplayName = edition.DisplayName;
        return new EditionDto
        {
            Id = edition.Id,
            Name = edition.Name,
            DisplayName = storedDisplayName,
            LocalizedDisplayName = EditionDisplayNameLocalization.Resolve(
                StringLocalizerFactory,
                EditionsOptions.DisplayNameResources,
                storedDisplayName),
            Code = edition.Code,
            IsActive = edition.IsActive,
            ConcurrencyStamp = edition.ConcurrencyStamp
        };
    }

    protected virtual async Task ContributeAsync(IReadOnlyList<EditionDto> editions)
    {
        foreach (var contributor in DtoContributors)
        {
            await contributor.ContributeAsync(editions);
        }
    }
}
