using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.LocalizationManagement.Caching;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;
using SufiChain.SufiAbp.LocalizationManagement.ExternalStore;
using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Permissions;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Localization;
using Volo.Abp.Localization.External;
using LocalizationResourceEntity = SufiChain.SufiAbp.LocalizationManagement.Entities.LocalizationResource;

namespace SufiChain.SufiAbp.LocalizationManagement;

[Authorize(LocalizationManagementPermissions.Resources.Default)]
public class LocalizationResourceAppService : ApplicationService, ILocalizationResourceAppService
{
    private readonly ILocalizationResourceRepository _resourceRepository;
    private readonly ILocalizationTextRepository _textRepository;
    private readonly AbpLocalizationOptions _localizationOptions;
    private readonly LocalizationTextCacheService _cacheService;
    private readonly IExternalLocalizationStore _externalStore;

    public LocalizationResourceAppService(
        ILocalizationResourceRepository resourceRepository,
        ILocalizationTextRepository textRepository,
        Microsoft.Extensions.Options.IOptions<AbpLocalizationOptions> localizationOptions,
        LocalizationTextCacheService cacheService,
        IExternalLocalizationStore externalStore)
    {
        _resourceRepository = resourceRepository;
        _textRepository = textRepository;
        _localizationOptions = localizationOptions.Value;
        _cacheService = cacheService;
        _externalStore = externalStore;
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }

    public async Task<LocalizationResourceDto> GetAsync(Guid id)
    {
        var resource = await _resourceRepository.GetAsync(id);
        return ObjectMapper.Map<LocalizationResourceEntity, LocalizationResourceDto>(resource);
    }

    public async Task<PagedResultDto<LocalizationResourceDto>> GetListAsync(GetLocalizationResourcesInput input)
    {
        var resources = await _resourceRepository.GetPagedListAsync(
            input.Filter,
            input.IsEnabled,
            0,
            int.MaxValue,
            input.Sorting);

        resources = resources
            .Where(r => !SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(r.ResourceName))
            .ToList();

        var items = resources
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<LocalizationResourceDto>(
            resources.Count,
            ObjectMapper.Map<List<LocalizationResourceEntity>, List<LocalizationResourceDto>>(items));
    }

    public async Task<List<LocalizationResourceSummaryDto>> GetSummaryListAsync()
    {
        var result = new List<LocalizationResourceSummaryDto>();

        // Get configured resources from ABP
        foreach (var (resourceName, resource) in _localizationOptions.Resources)
        {
            if (SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(resourceName))
            {
                continue;
            }

            var dbResource = await _resourceRepository.FindByNameAsync(resourceName);
            var cultures = await _textRepository.GetCultureNamesAsync(resourceName);
            var textCount = await _textRepository.GetCountAsync(resourceName: resourceName);

            result.Add(new LocalizationResourceSummaryDto
            {
                ResourceName = resourceName,
                DisplayName = dbResource?.DisplayName ?? resourceName,
                IsEnabled = dbResource?.IsEnabled ?? true,
                TextCount = (int)textCount,
                SupportedCultures = cultures
            });
        }

        // Get database-only resources
        var dbResources = await _resourceRepository.GetEnabledListAsync();

        foreach (var dbResource in dbResources.Where(r =>
                     !_localizationOptions.Resources.ContainsKey(r.ResourceName) &&
                     !SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(r.ResourceName)))
        {
            var cultures = await _textRepository.GetCultureNamesAsync(dbResource.ResourceName);
            var textCount = await _textRepository.GetCountAsync(resourceName: dbResource.ResourceName);

            result.Add(new LocalizationResourceSummaryDto
            {
                ResourceName = dbResource.ResourceName,
                DisplayName = dbResource.DisplayName ?? dbResource.ResourceName,
                IsEnabled = dbResource.IsEnabled,
                TextCount = (int)textCount,
                SupportedCultures = cultures
            });
        }

        return result.OrderBy(r => r.ResourceName).ToList();
    }

    [Authorize(LocalizationManagementPermissions.Resources.Create)]
    public async Task<LocalizationResourceDto> CreateAsync(CreateUpdateLocalizationResourceDto input)
    {
        if (await _resourceRepository.ExistsAsync(input.ResourceName))
        {
            throw new BusinessException("SufiChain.SufiAbp.LocalizationManagement:010001")
                .WithData("ResourceName", input.ResourceName);
        }

        var resource = new LocalizationResourceEntity(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.ResourceName,
            input.DefaultCulture,
            input.DisplayName);

        if (!input.IsEnabled)
        {
            resource.Disable();
        }

        foreach (var baseName in input.BaseResourceNames)
        {
            resource.AddBaseResource(baseName);
        }

        await _resourceRepository.InsertAsync(resource);
        return ObjectMapper.Map<LocalizationResourceEntity, LocalizationResourceDto>(resource);
    }

    [Authorize(LocalizationManagementPermissions.Resources.Update)]
    public async Task<LocalizationResourceDto> UpdateAsync(Guid id, CreateUpdateLocalizationResourceDto input)
    {
        var resource = await _resourceRepository.GetAsync(id);

        resource.SetResourceName(input.ResourceName);
        resource.SetDefaultCulture(input.DefaultCulture);
        resource.SetDisplayName(input.DisplayName);

        if (input.IsEnabled)
        {
            resource.Enable();
        }
        else
        {
            resource.Disable();
        }

        // Update base resources
        resource.BaseResourceNames.Clear();
        foreach (var baseName in input.BaseResourceNames)
        {
            resource.AddBaseResource(baseName);
        }

        await _resourceRepository.UpdateAsync(resource);
        ClearExternalStoreCache(resource.ResourceName);
        return ObjectMapper.Map<LocalizationResourceEntity, LocalizationResourceDto>(resource);
    }

    [Authorize(LocalizationManagementPermissions.Resources.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var resource = await _resourceRepository.GetAsync(id);

        // Delete all texts for this resource
        await _textRepository.DeleteByResourceAsync(resource.ResourceName);

        // Invalidate all cached cultures for this resource
        await _cacheService.InvalidateResourceAsync(resource.ResourceName);
        ClearExternalStoreCache(resource.ResourceName);

        await _resourceRepository.DeleteAsync(id);
    }

    public async Task SetEnabledAsync(Guid id, bool isEnabled)
    {
        var resource = await _resourceRepository.GetAsync(id);

        if (isEnabled)
        {
            resource.Enable();
        }
        else
        {
            resource.Disable();
        }

        await _resourceRepository.UpdateAsync(resource);
        ClearExternalStoreCache(resource.ResourceName);
    }

    private void ClearExternalStoreCache(string resourceName)
    {
        if (_externalStore is DatabaseExternalLocalizationStore dbStore)
        {
            dbStore.ClearCache(resourceName);
        }
    }
}
