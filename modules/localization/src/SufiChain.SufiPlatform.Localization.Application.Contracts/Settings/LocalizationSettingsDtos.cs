using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Localization.Settings;

public class LocalizationSettingsDto
{
    public string DefaultCulture { get; set; } = "fa";
}

public interface ILocalizationSettingsAppService : IApplicationService
{
    Task<LocalizationSettingsDto> GetAsync();
    Task UpdateAsync(LocalizationSettingsDto input);
}
