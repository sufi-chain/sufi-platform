using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Reads and writes menu labels under the menu permission, in the caller's unit of work.
/// </summary>
public class MenuLabelLocalization : ITransientDependency
{
    private readonly ILocalizationTextSeeder _seeder;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly ISettingProvider _settingProvider;
    private readonly IOptions<SufiDataSeedOptions> _seedOptions;
    private readonly IOptions<AbpLocalizationOptions> _localizationOptions;
    private readonly ICurrentTenant _currentTenant;

    public MenuLabelLocalization(
        ILocalizationTextSeeder seeder,
        IStringLocalizerFactory stringLocalizerFactory,
        ISettingProvider settingProvider,
        IOptions<SufiDataSeedOptions> seedOptions,
        IOptions<AbpLocalizationOptions> localizationOptions,
        ICurrentTenant currentTenant)
    {
        _seeder = seeder;
        _stringLocalizerFactory = stringLocalizerFactory;
        _settingProvider = settingProvider;
        _seedOptions = seedOptions;
        _localizationOptions = localizationOptions;
        _currentTenant = currentTenant;
    }

    public virtual async Task<string> GetDefaultCultureAsync()
    {
        var setting = await _settingProvider.GetOrNullAsync(TenantSeedCultureProvider.DefaultCultureSettingName);
        return SeedCultureHelper.NormalizeCulture(setting)
            ?? SeedCultureHelper.NormalizeCulture(_seedOptions.Value.DefaultCulture)
            ?? "fa";
    }

    public virtual async Task<List<MenuLabelCultureDto>> GetCulturesAsync()
    {
        var defaultCulture = await GetDefaultCultureAsync();
        var languages = _localizationOptions.Value.Languages;
        var rows = new List<MenuLabelCultureDto>();
        if (languages.Count == 0)
        {
            var order = 0;
            foreach (var name in new[] { "fa", "en", "ar", "es" })
            {
                rows.Add(CreateCulture(name, name, defaultCulture, order++));
            }
        }
        else
        {
            var order = 0;
            foreach (var language in languages)
            {
                rows.Add(CreateCulture(language.CultureName, language.DisplayName, defaultCulture, order++));
            }
        }

        return rows
            .GroupBy(x => x.CultureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .ToList();
    }

    public virtual async Task<Dictionary<string, string>> ReadAsync(
        string? storedDisplayName,
        string? plainName,
        string? contextType)
    {
        var cultures = await GetCulturesAsync();
        var defaultCulture = await GetDefaultCultureAsync();
        var loaded = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(storedDisplayName)
            && !BusinessTextEditorStorage.IsPlaceholder(storedDisplayName))
        {
            var resources = ResourcesFor(storedDisplayName, contextType);
            foreach (var culture in cultures)
            {
                var value = await ReadExactAsync(resources, storedDisplayName!, culture.CultureName);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    loaded[culture.CultureName] = value;
                }
            }
        }

        var state = MenuDisplayNamePlanner.BuildEditorState(
            storedDisplayName,
            plainName,
            cultures.Select(x => x.CultureName),
            defaultCulture,
            culture => loaded.TryGetValue(culture, out var value) ? value : null);
        return state.Values;
    }

    public virtual async Task StoreAsync(
        string? existingStored,
        string generatedKey,
        IReadOnlyDictionary<string, string> displayNames,
        Action<string> setDisplayName)
    {
        var key = MenuDisplayNamePlanner.ResolveStoredKey(existingStored, generatedKey);
        var plan = MenuDisplayNamePlanner.PlanWrite(
            displayNames,
            await GetDefaultCultureAsync(),
            MenusConsts.MaxDisplayNameLength);
        if (!plan.Succeeded)
        {
            var code = string.Equals(plan.ErrorMessage, MenuBusinessTexts.LooksLikeKeyMessage, StringComparison.Ordinal)
                ? MenusErrorCodes.DisplayNameLooksLikeKey
                : MenusErrorCodes.DisplayNameRequired;
            throw new BusinessException(code, plan.ErrorMessage);
        }

        setDisplayName(key);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in plan.Upserts)
        {
            values[pair.Key] = pair.Value;
        }

        foreach (var culture in plan.Deletes)
        {
            values[culture] = string.Empty;
        }

        await _seeder.ReplaceKeyValuesAsync(
            MenuLocalizationRegistry.SharedResourceName,
            key,
            values,
            _currentTenant.Id);
    }

    public virtual string Resolve(
        string? storedDisplayName,
        string? contextType,
        string? plainName,
        string? requestedCulture,
        string? defaultCulture)
    {
        var resources = ResourcesFor(storedDisplayName, contextType);
        return MenuDisplayNamePlanner.ResolvePublicLabel(
            storedDisplayName,
            plainName,
            requestedCulture,
            defaultCulture,
            culture => MenuDisplayNamePlanner.LookupCulture(
                _stringLocalizerFactory,
                resources,
                storedDisplayName,
                culture));
    }

    private async Task<string?> ReadExactAsync(IReadOnlyList<string> resources, string key, string culture)
    {
        foreach (var resource in resources)
        {
            var dbValue = await _seeder.FindValueAsync(resource, culture, key, _currentTenant.Id);
            if (MenuDisplayNamePlanner.IsDisplayableText(dbValue, key))
            {
                return dbValue!.Trim();
            }
        }

        foreach (var resourceName in resources)
        {
            if (!_localizationOptions.Value.Resources.TryGetValue(resourceName, out var resource))
            {
                continue;
            }

            foreach (var contributor in resource.Contributors)
            {
                var found = contributor.GetOrNull(culture, key);
                if (found != null && MenuDisplayNamePlanner.IsDisplayableText(found.Value, key))
                {
                    return found.Value.Trim();
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ResourcesFor(string? storedDisplayName, string? contextType)
    {
        string? menuKey = null;
        if (!string.IsNullOrWhiteSpace(storedDisplayName)
            && BusinessLocalizationHelper.TryExtractSeededMenuKey(storedDisplayName, out var extracted))
        {
            menuKey = extracted;
        }

        return MenuLocalizationRegistry.GetReadResourceNames(menuKey, contextType);
    }

    private static MenuLabelCultureDto CreateCulture(string cultureName, string? displayName, string defaultCulture, int sortOrder)
    {
        var normalized = SeedCultureHelper.NormalizeCulture(cultureName) ?? cultureName.Trim().ToLowerInvariant();
        return new MenuLabelCultureDto
        {
            CultureName = normalized,
            DisplayName = Endonym(normalized, displayName),
            IsRtl = IsRightToLeft(normalized),
            IsDefault = string.Equals(normalized, SeedCultureHelper.NormalizeCulture(defaultCulture) ?? "fa", StringComparison.OrdinalIgnoreCase),
            SortOrder = sortOrder
        };
    }

    private static string Endonym(string culture, string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName)
            && !string.Equals(displayName, culture, StringComparison.OrdinalIgnoreCase))
        {
            return displayName;
        }

        return culture switch
        {
            "fa" => "فارسی",
            "en" => "English",
            "ar" => "العربية",
            "es" => "Español",
            _ => string.IsNullOrWhiteSpace(displayName) ? culture : displayName
        };
    }

    private static bool IsRightToLeft(string culture) =>
        culture.StartsWith("fa", StringComparison.OrdinalIgnoreCase)
        || culture.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
        || culture.StartsWith("he", StringComparison.OrdinalIgnoreCase)
        || culture.StartsWith("ur", StringComparison.OrdinalIgnoreCase);
}
