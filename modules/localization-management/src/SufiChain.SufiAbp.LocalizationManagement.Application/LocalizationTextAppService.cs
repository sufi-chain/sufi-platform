using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Caching;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Permissions;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.LocalizationManagement;

[Authorize(LocalizationManagementPermissions.Texts.Default)]
public class LocalizationTextAppService : SufiAbpApplicationService, ILocalizationTextAppService
{
    private readonly ILocalizationTextRepository _textRepository;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly AbpLocalizationOptions _localizationOptions;
    private readonly LocalizationTextCacheService _cacheService;

    public LocalizationTextAppService(
        ILocalizationTextRepository textRepository,
        IStringLocalizerFactory stringLocalizerFactory,
        Microsoft.Extensions.Options.IOptions<AbpLocalizationOptions> localizationOptions,
        LocalizationTextCacheService cacheService)
    {
        _textRepository = textRepository;
        _stringLocalizerFactory = stringLocalizerFactory;
        _localizationOptions = localizationOptions.Value;
        _cacheService = cacheService;
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }

    public async Task<LocalizationTextDto> GetAsync(Guid id)
    {
        var text = await _textRepository.GetAsync(id);
        return ObjectMapper.Map<LocalizationText, LocalizationTextDto>(text);
    }

    public async Task<PagedResultDto<LocalizationTextDto>> GetListAsync(GetLocalizationTextsInput input)
    {
        var skipCount = Math.Clamp(input.SkipCount, 0, int.MaxValue);
        var hasResourceName = !string.IsNullOrWhiteSpace(input.ResourceName);

        if (hasResourceName &&
            SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(input.ResourceName!))
        {
            return new PagedResultDto<LocalizationTextDto>(0, new List<LocalizationTextDto>());
        }

        var items = await _textRepository.GetPagedListAsync(
            input.ResourceName,
            input.CultureName,
            input.KeyFilter,
            input.ValueFilter,
            hasResourceName ? skipCount : 0,
            hasResourceName ? input.MaxResultCount : int.MaxValue,
            input.Sorting);

        if (!hasResourceName)
        {
            items = items
                .Where(item => !SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(item.ResourceName))
                .ToList();
        }

        var totalCount = !hasResourceName
            ? items.Count
            : await _textRepository.GetCountAsync(
                input.ResourceName,
                input.CultureName,
                input.KeyFilter,
                input.ValueFilter);

        if (!hasResourceName)
        {
            items = items
                .Skip(skipCount)
                .Take(input.MaxResultCount)
                .ToList();
        }

        return new PagedResultDto<LocalizationTextDto>(
            totalCount,
            ObjectMapper.Map<List<LocalizationText>, List<LocalizationTextDto>>(items));
    }

    public async Task<List<LocalizationTextWithBaseValueDto>> GetAllForResourceAsync(string resourceName, string cultureName)
    {
        if (SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(resourceName))
        {
            return new List<LocalizationTextWithBaseValueDto>();
        }

        var result = new List<LocalizationTextWithBaseValueDto>();

        // Get database overrides
        var dbTexts = await _textRepository.GetListAsync(resourceName, cultureName);
        var dbTextDict = dbTexts.ToDictionary(t => t.Key, t => t);

        // Try to get base values from ABP localization
        var baseTexts = GetBaseTextsFromResource(resourceName, cultureName);

        // Merge base texts with database overrides
        foreach (var baseText in baseTexts)
        {
            var dto = new LocalizationTextWithBaseValueDto
            {
                ResourceName = resourceName,
                CultureName = cultureName,
                Key = baseText.Key,
                BaseValue = baseText.Value
            };

            if (dbTextDict.TryGetValue(baseText.Key, out var dbText))
            {
                dto.Id = dbText.Id;
                dto.Value = dbText.Value;
                dto.IsOverride = true;
                dto.CreationTime = dbText.CreationTime;
                dto.LastModificationTime = dbText.LastModificationTime;
            }
            else
            {
                dto.Value = baseText.Value;
                dto.IsOverride = false;
            }

            result.Add(dto);
        }

        // Add any database entries that don't exist in base
        foreach (var dbText in dbTexts.Where(t => !baseTexts.ContainsKey(t.Key)))
        {
            result.Add(new LocalizationTextWithBaseValueDto
            {
                Id = dbText.Id,
                ResourceName = dbText.ResourceName,
                CultureName = dbText.CultureName,
                Key = dbText.Key,
                Value = dbText.Value,
                BaseValue = null,
                IsOverride = true,
                CreationTime = dbText.CreationTime,
                LastModificationTime = dbText.LastModificationTime
            });
        }

        return result.OrderBy(t => t.Key).ToList();
    }

    [Authorize(LocalizationManagementPermissions.Texts.Create)]
    public async Task<LocalizationTextDto> CreateOrUpdateAsync(CreateUpdateLocalizationTextDto input)
    {
        var existing = await _textRepository.FindAsync(input.ResourceName, input.CultureName, input.Key);

        if (existing != null)
        {
            await CheckPolicyAsync(LocalizationManagementPermissions.Texts.Update);
            existing.UpdateValue(input.Value);
            await _textRepository.UpdateAsync(existing);
            await _cacheService.InvalidateAsync(input.ResourceName, input.CultureName);
            return ObjectMapper.Map<LocalizationText, LocalizationTextDto>(existing);
        }

        var text = new LocalizationText(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.ResourceName,
            input.CultureName,
            input.Key,
            input.Value);

        await _textRepository.InsertAsync(text);
        await _cacheService.InvalidateAsync(input.ResourceName, input.CultureName);
        return ObjectMapper.Map<LocalizationText, LocalizationTextDto>(text);
    }

    [Authorize(LocalizationManagementPermissions.Texts.Update)]
    public async Task<LocalizationTextDto> UpdateValueAsync(Guid id, UpdateLocalizationTextValueDto input)
    {
        var text = await _textRepository.GetAsync(id);
        text.UpdateValue(input.Value);
        await _textRepository.UpdateAsync(text);
        await _cacheService.InvalidateAsync(text.ResourceName, text.CultureName);
        return ObjectMapper.Map<LocalizationText, LocalizationTextDto>(text);
    }

    [Authorize(LocalizationManagementPermissions.Texts.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var text = await _textRepository.GetAsync(id);
        await _textRepository.DeleteAsync(text);
        await _cacheService.InvalidateAsync(text.ResourceName, text.CultureName);
    }

    [Authorize(LocalizationManagementPermissions.Texts.Delete)]
    public async Task DeleteByKeyAsync(string resourceName, string cultureName, string key)
    {
        var text = await _textRepository.FindAsync(resourceName, cultureName, key);
        if (text != null)
        {
            await _textRepository.DeleteAsync(text);
            await _cacheService.InvalidateAsync(resourceName, cultureName);
        }
    }

    public async Task<List<string>> GetResourceNamesAsync()
    {
        // Combine configured resources with database resources
        var configuredResources = _localizationOptions.Resources.Keys.ToList();
        var dbResources = await _textRepository.GetResourceNamesAsync();

        return configuredResources
            .Union(dbResources)
            .Where(resourceName => !SufiAbpLocalizationResourceNameMap.IsReplacedAbpResource(resourceName))
            .Distinct()
            .OrderBy(r => r)
            .ToList();
    }

    public async Task<List<string>> GetCultureNamesAsync(string? resourceName = null)
    {
        // Get cultures from database
        var dbCultures = await _textRepository.GetCultureNamesAsync(resourceName);

        // Common cultures to always include
        var commonCultures = new[] { "en", "fa", "ar" };

        return commonCultures.Union(dbCultures).Distinct().OrderBy(c => c).ToList();
    }

    [Authorize(LocalizationManagementPermissions.Texts.Import)]
    public async Task<ImportResultDto> ImportAsync(ImportLocalizationTextsDto input)
    {
        var result = new ImportResultDto();

        try
        {
            // Parse JSON in ABP format: { "culture": "en", "texts": { "Key": "Value" } }
            var jsonDoc = JsonDocument.Parse(input.JsonContent);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("texts", out var textsElement))
            {
                result.Errors.Add("Invalid JSON format. Expected 'texts' property.");
                return result;
            }

            foreach (var prop in textsElement.EnumerateObject())
            {
                result.TotalKeys++;

                try
                {
                    var key = prop.Name;
                    var value = prop.Value.GetString();

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var existing = await _textRepository.FindAsync(input.ResourceName, input.CultureName, key);

                    if (existing != null)
                    {
                        if (input.OverwriteExisting)
                        {
                            existing.UpdateValue(value);
                            await _textRepository.UpdateAsync(existing);
                            result.UpdatedCount++;
                        }
                        else
                        {
                            result.SkippedCount++;
                        }
                    }
                    else
                    {
                        var text = new LocalizationText(
                            GuidGenerator.Create(),
                            CurrentTenant.Id,
                            input.ResourceName,
                            input.CultureName,
                            key,
                            value);
                        await _textRepository.InsertAsync(text);
                        result.ImportedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error importing key '{prop.Name}': {ex.Message}");
                }
            }

            // Invalidate cache after import
            await _cacheService.InvalidateAsync(input.ResourceName, input.CultureName);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Invalid JSON: {ex.Message}");
        }

        return result;
    }

    [Authorize(LocalizationManagementPermissions.Texts.Export)]
    public async Task<string> ExportAsync(ExportLocalizationTextsDto input)
    {
        var texts = await _textRepository.GetListAsync(input.ResourceName, input.CultureName);

        var exportData = new Dictionary<string, object>
        {
            ["culture"] = input.CultureName,
            ["texts"] = texts.ToDictionary(t => t.Key, t => t.Value)
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public async Task<PagedResultDto<LocalizationTextWithBaseValueDto>> GetMergedListAsync(GetMergedLocalizationTextsInput input)
    {
        var skipCount = Math.Clamp(input.SkipCount, 0, int.MaxValue);

        var allMerged = await GetAllForResourceAsync(input.ResourceName, input.CultureName);

        // Apply filters
        var filtered = allMerged.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(input.KeyFilter))
        {
            filtered = filtered.Where(t => t.Key.Contains(input.KeyFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (input.OnlyOverridden)
        {
            filtered = filtered.Where(t => t.IsOverride);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        var pagedItems = filteredList
            .Skip(skipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<LocalizationTextWithBaseValueDto>(totalCount, pagedItems);
    }

    private Dictionary<string, string> GetBaseTextsFromResource(string resourceName, string cultureName)
    {
        var result = new Dictionary<string, string>();

        var localizer = _stringLocalizerFactory.CreateByResourceNameOrNull(resourceName);
        if (localizer == null)
        {
            return result;
        }

        try
        {
            using (CultureHelper.Use(cultureName))
            {
                var allStrings = localizer.GetAllStrings(
                    includeParentCultures: true,
                    includeBaseLocalizers: false,
                    includeDynamicContributors: false);

                foreach (var localizedString in allStrings)
                {
                    if (!localizedString.ResourceNotFound)
                    {
                        result[localizedString.Name] = localizedString.Value;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors when getting base texts
        }

        return result;
    }
}
