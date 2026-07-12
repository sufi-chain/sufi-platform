using System.Globalization;
using Microsoft.Extensions.Options;
using Volo.Abp.Localization;
using SufiLanguageInfo = SufiChain.SufiPlatform.UI.Localization.LanguageInfo;

namespace SufiChain.SufiPlatform.UI.Services.Localization;

/// <summary>
/// Default Sufi language provider backed by <see cref="AbpLocalizationOptions"/>.
/// Hosts configure languages once via <c>Configure&lt;AbpLocalizationOptions&gt;</c>.
/// </summary>
public class DefaultLanguageProvider : UI.Localization.ILanguageProvider
{
    private static readonly HashSet<string> RtlCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar", "ar-SA", "ar-AE", "ar-BH", "ar-DZ", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB",
        "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SY", "ar-TN", "ar-YE",
        "he", "he-IL",
        "fa", "fa-IR",
        "ur", "ur-PK",
        "ps", "ps-AF",
        "dv", "dv-MV",
        "ku", "ku-Arab"
    };

    private readonly AbpLocalizationOptions _options;

    public DefaultLanguageProvider(IOptions<AbpLocalizationOptions> options)
    {
        _options = options.Value;
    }

    public Task<IReadOnlyList<SufiLanguageInfo>> GetLanguagesAsync()
    {
        IReadOnlyList<SufiLanguageInfo> languages;

        if (_options.Languages.Count > 0)
        {
            languages = _options.Languages
                .Select(language => new SufiLanguageInfo
                {
                    CultureName = language.CultureName,
                    UiCultureName = language.UiCultureName,
                    DisplayName = language.DisplayName,
                    IsRtl = IsRightToLeft(language.CultureName)
                })
                .ToList();
        }
        else
        {
            var culture = CultureInfo.CurrentUICulture;
            languages = new List<SufiLanguageInfo>
            {
                new(culture.Name, culture.Name, culture.DisplayName, IsRightToLeft(culture.Name))
            };
        }

        return Task.FromResult(languages);
    }

    private static bool IsRightToLeft(string cultureName)
    {
        return RtlCultures.Contains(cultureName) ||
               cultureName.Contains('-') && RtlCultures.Contains(cultureName.Split('-')[0]);
    }
}
