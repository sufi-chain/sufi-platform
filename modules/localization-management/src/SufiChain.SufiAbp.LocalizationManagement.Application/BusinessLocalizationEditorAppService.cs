using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;
using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Permissions;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.LocalizationManagement;

[Authorize(LocalizationManagementPermissions.Texts.Default)]
public class BusinessLocalizationEditorAppService : SufiAbpApplicationService, IBusinessLocalizationEditorAppService
{
    private readonly ILocalizationTextRepository _textRepository;
    private readonly ILocalizationTextSeeder _localizationTextSeeder;
    private readonly AbpLocalizationOptions _localizationOptions;
    private readonly SufiAbpDataSeedOptions _dataSeedOptions;

    public BusinessLocalizationEditorAppService(
        ILocalizationTextRepository textRepository,
        ILocalizationTextSeeder localizationTextSeeder,
        IOptions<AbpLocalizationOptions> localizationOptions,
        IOptions<SufiAbpDataSeedOptions> dataSeedOptions)
    {
        _textRepository = textRepository;
        _localizationTextSeeder = localizationTextSeeder;
        _localizationOptions = localizationOptions.Value;
        _dataSeedOptions = dataSeedOptions.Value;
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }

    public virtual async Task<BusinessLocalizationKeyValuesDto> GetKeyValuesAsync(GetBusinessLocalizationKeyValuesInput input)
    {
        var cultures = await GetCultureNamesAsync();
        var texts = await _textRepository.GetListByResourceAsync(input.ResourceName);
        var valuesByCulture = texts
            .Where(x => string.Equals(x.Key, input.Key, StringComparison.Ordinal))
            .ToDictionary(x => x.CultureName, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var culture in cultures)
        {
            values[culture] = valuesByCulture.TryGetValue(culture, out var value) ? value : string.Empty;
        }

        return new BusinessLocalizationKeyValuesDto
        {
            ResourceName = input.ResourceName,
            Key = input.Key,
            Values = values,
            IsBusinessKey = BusinessLocalizationHelper.IsBusinessLocalizationKey(input.Key)
        };
    }

    [Authorize(LocalizationManagementPermissions.Texts.Create)]
    public virtual async Task SaveKeyValuesAsync(SaveBusinessLocalizationKeyValuesInput input)
    {
        if (input.Values.Count == 0)
        {
            return;
        }

        var existingCultures = (await _textRepository.GetListByResourceAsync(input.ResourceName))
            .Where(x => string.Equals(x.Key, input.Key, StringComparison.Ordinal))
            .Select(x => x.CultureName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (existingCultures.Count > 0)
        {
            await CheckPolicyAsync(LocalizationManagementPermissions.Texts.Update);
        }

        await _localizationTextSeeder.UpsertAsync(
            input.ResourceName,
            input.Key,
            input.Values,
            CurrentTenant.Id,
            overwriteExisting: true);
    }

    public virtual Task<List<BusinessLocalizationCultureDto>> GetEditorCulturesAsync()
    {
        var defaultCulture = SeedCultureHelper.NormalizeCulture(_dataSeedOptions.DefaultCulture) ?? "fa";
        var cultures = GetConfiguredCultures();
        var result = cultures.Select(culture => new BusinessLocalizationCultureDto
        {
            CultureName = culture.CultureName,
            DisplayName = culture.DisplayName,
            IsRtl = IsRightToLeft(culture.CultureName),
            IsDefault = string.Equals(culture.CultureName, defaultCulture, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        return Task.FromResult(result);
    }

    protected virtual async Task<List<string>> GetCultureNamesAsync()
    {
        var configured = GetConfiguredCultures()
            .Select(x => x.CultureName)
            .ToList();

        if (configured.Count > 0)
        {
            return configured;
        }

        var dbCultures = await _textRepository.GetCultureNamesAsync();
        if (dbCultures.Count > 0)
        {
            return dbCultures;
        }

        return _dataSeedOptions.SupportedCultures?.ToList() ?? new List<string> { "fa", "en", "ar", "es" };
    }

    protected virtual IReadOnlyList<LanguageInfo> GetConfiguredCultures()
    {
        IReadOnlyList<LanguageInfo> cultures;

        if (_localizationOptions.Languages.Count > 0)
        {
            cultures = _localizationOptions.Languages;
        }
        else
        {
            cultures = (_dataSeedOptions.SupportedCultures ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new LanguageInfo(x, x, x))
                .ToList();
        }

        return cultures
            .GroupBy(x => x.CultureName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static bool IsRightToLeft(string cultureName)
    {
        return cultureName.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith("fa", StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith("he", StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith("ur", StringComparison.OrdinalIgnoreCase);
    }
}
