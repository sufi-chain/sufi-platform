using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Localization.Dtos;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Localization;

public interface ILocalizationResourceAppService : IApplicationService
{
    /// <summary>
    /// Gets a localization resource by ID
    /// </summary>
    Task<LocalizationResourceDto> GetAsync(Guid id);

    /// <summary>
    /// Gets paginated list of localization resources
    /// </summary>
    Task<PagedResultDto<LocalizationResourceDto>> GetListAsync(GetLocalizationResourcesInput input);

    /// <summary>
    /// Gets summary of all resources (including from ABP configuration)
    /// </summary>
    Task<List<LocalizationResourceSummaryDto>> GetSummaryListAsync();

    /// <summary>
    /// Creates a localization resource
    /// </summary>
    Task<LocalizationResourceDto> CreateAsync(CreateUpdateLocalizationResourceDto input);

    /// <summary>
    /// Updates a localization resource
    /// </summary>
    Task<LocalizationResourceDto> UpdateAsync(Guid id, CreateUpdateLocalizationResourceDto input);

    /// <summary>
    /// Deletes a localization resource
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Enables or disables a resource
    /// </summary>
    Task SetEnabledAsync(Guid id, bool isEnabled);
}
