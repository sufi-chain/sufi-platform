using System.Globalization;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.UI.Localization;

namespace SufiChain.SufiAbp.UI.Services.Localization;

/// <summary>
/// Default SufiAbp language provider backed by configured localization options.
/// </summary>
public class DefaultLanguageProvider : ILanguageProvider
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

    private readonly SufiAbpLocalizationOptions _options;

    public DefaultLanguageProvider(IOptions<SufiAbpLocalizationOptions> options)
    {
        _options = options.Value;
    }

    public Task<IReadOnlyList<LanguageInfo>> GetLanguagesAsync()
    {
        var languages = _options.Languages.Count > 0
            ? _options.Languages
            : new List<LanguageInfo>
            {
                new(CultureInfo.CurrentUICulture.Name, CultureInfo.CurrentUICulture.Name, CultureInfo.CurrentUICulture.DisplayName)
            };

        return Task.FromResult<IReadOnlyList<LanguageInfo>>(
            languages.Select(language => new LanguageInfo
            {
                CultureName = language.CultureName,
                UiCultureName = language.UiCultureName,
                DisplayName = language.DisplayName,
                IsRtl = IsRightToLeft(language.CultureName)
            }).ToList());
    }

    private static bool IsRightToLeft(string cultureName)
    {
        return RtlCultures.Contains(cultureName) ||
               cultureName.Contains('-') && RtlCultures.Contains(cultureName.Split('-')[0]);
    }
}
