using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Localization.Dtos;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Localization;

public interface ILocalizationTextAppService : IApplicationService
{
    /// <summary>
    /// Gets a localization text by ID
    /// </summary>
    Task<LocalizationTextDto> GetAsync(Guid id);

    /// <summary>
    /// Gets paginated list of localization texts
    /// </summary>
    Task<PagedResultDto<LocalizationTextDto>> GetListAsync(GetLocalizationTextsInput input);

    /// <summary>
    /// Gets all texts for a specific resource and culture, including base values
    /// </summary>
    Task<List<LocalizationTextWithBaseValueDto>> GetAllForResourceAsync(string resourceName, string cultureName);

    /// <summary>
    /// Gets paginated merged list of texts for a specific resource and culture.
    /// Merges base JSON values with database overrides.
    /// </summary>
    Task<PagedResultDto<LocalizationTextWithBaseValueDto>> GetMergedListAsync(GetMergedLocalizationTextsInput input);

    /// <summary>
    /// Creates or updates a localization text
    /// </summary>
    Task<LocalizationTextDto> CreateOrUpdateAsync(CreateUpdateLocalizationTextDto input);

    /// <summary>
    /// Updates only the value of an existing text
    /// </summary>
    Task<LocalizationTextDto> UpdateValueAsync(Guid id, UpdateLocalizationTextValueDto input);

    /// <summary>
    /// Deletes a localization text
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Deletes a localization text by key
    /// </summary>
    Task DeleteByKeyAsync(string resourceName, string cultureName, string key);

    /// <summary>
    /// Gets available resource names
    /// </summary>
    Task<List<string>> GetResourceNamesAsync();

    /// <summary>
    /// Gets available culture names
    /// </summary>
    Task<List<string>> GetCultureNamesAsync(string? resourceName = null);

    /// <summary>
    /// Imports translations from JSON
    /// </summary>
    Task<ImportResultDto> ImportAsync(ImportLocalizationTextsDto input);

    /// <summary>
    /// Exports translations to JSON format
    /// </summary>
    Task<string> ExportAsync(ExportLocalizationTextsDto input);
}
