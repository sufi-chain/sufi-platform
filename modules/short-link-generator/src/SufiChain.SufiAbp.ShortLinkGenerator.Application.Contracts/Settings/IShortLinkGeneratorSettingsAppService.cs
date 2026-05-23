using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Application service for managing short-link generator settings.
/// </summary>
public interface IShortLinkGeneratorSettingsAppService : IApplicationService
{
    /// <summary>
    /// Gets the effective short-link generator settings for the current tenant or host scope.
    /// </summary>
    Task<ShortLinkGeneratorSettingsDto> GetAsync();

    /// <summary>
    /// Updates the short-link generator settings for the current tenant or host scope.
    /// </summary>
    Task UpdateAsync(ShortLinkGeneratorSettingsDto input);
}
