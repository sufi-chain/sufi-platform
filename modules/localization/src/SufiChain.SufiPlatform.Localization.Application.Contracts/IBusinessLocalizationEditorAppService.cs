using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Localization.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Localization;

/// <summary>
/// Embedded business localization editor API for multi-culture key editing.
/// </summary>
public interface IBusinessLocalizationEditorAppService : IApplicationService
{
    /// <summary>
    /// Gets all culture values for a single business localization key.
    /// </summary>
    Task<BusinessLocalizationKeyValuesDto> GetKeyValuesAsync(GetBusinessLocalizationKeyValuesInput input);

    /// <summary>
    /// Upserts all culture values for a single business localization key.
    /// </summary>
    Task SaveKeyValuesAsync(SaveBusinessLocalizationKeyValuesInput input);

    /// <summary>
    /// Gets cultures available for the embedded editor UI.
    /// </summary>
    Task<List<BusinessLocalizationCultureDto>> GetEditorCulturesAsync();
}
