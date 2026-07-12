using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Application service for managing short-link generator settings.
/// </summary>
public interface IShortLinksSettingsAppService : IApplicationService
{
    /// <summary>
    /// Gets the effective short-link generator settings for the current tenant or host scope.
    /// </summary>
    Task<ShortLinksSettingsDto> GetAsync();

    /// <summary>
    /// Updates the short-link generator settings for the current tenant or host scope.
    /// </summary>
    Task UpdateAsync(ShortLinksSettingsDto input);
}
