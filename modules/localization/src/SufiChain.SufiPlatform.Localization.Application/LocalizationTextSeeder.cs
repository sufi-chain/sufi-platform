using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Caching;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Localization;

public class LocalizationTextSeeder : ILocalizationTextSeeder, ITransientDependency
{
    protected ILocalizationTextRepository TextRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected LocalizationTextCacheService CacheService { get; }

    public LocalizationTextSeeder(
        ILocalizationTextRepository textRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        LocalizationTextCacheService cacheService)
    {
        TextRepository = textRepository;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
        CacheService = cacheService;
    }

    public virtual async Task UpsertAsync(
        string resourceName,
        string key,
        IReadOnlyDictionary<string, string> cultureValues,
        Guid? tenantId = null,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (cultureValues.Count == 0)
        {
            return;
        }

        using (CurrentTenant.Change(tenantId))
        {
            foreach (var (cultureName, value) in cultureValues)
            {
                if (string.IsNullOrWhiteSpace(cultureName) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var normalizedCulture = SeedCultureHelper.NormalizeCulture(cultureName)!;
                await UpsertSingleAsync(resourceName, normalizedCulture, key, value, overwriteExisting, cancellationToken);
            }
        }
    }

    public virtual async Task UpsertManyAsync(
        string resourceName,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> keysByCulture,
        Guid? tenantId = null,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        if (keysByCulture.Count == 0)
        {
            return;
        }

        using (CurrentTenant.Change(tenantId))
        {
            foreach (var (cultureName, keys) in keysByCulture)
            {
                if (string.IsNullOrWhiteSpace(cultureName) || keys.Count == 0)
                {
                    continue;
                }

                var normalizedCulture = SeedCultureHelper.NormalizeCulture(cultureName)!;
                foreach (var (key, value) in keys)
                {
                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    await UpsertSingleAsync(resourceName, normalizedCulture, key, value, overwriteExisting, cancellationToken);
                }
            }
        }
    }

    protected virtual async Task UpsertSingleAsync(
        string resourceName,
        string cultureName,
        string key,
        string value,
        bool overwriteExisting,
        CancellationToken cancellationToken)
    {
        var existing = await TextRepository.FindAsync(resourceName, cultureName, key, cancellationToken: cancellationToken);
        if (existing != null)
        {
            if (!overwriteExisting)
            {
                return;
            }

            existing.UpdateValue(value);
            await TextRepository.UpdateAsync(existing, autoSave: true, cancellationToken: cancellationToken);
            await CacheService.InvalidateAsync(resourceName, cultureName);
            return;
        }

        var text = new LocalizationText(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            resourceName,
            cultureName,
            key,
            value);

        await TextRepository.InsertAsync(text, autoSave: true, cancellationToken: cancellationToken);
        await CacheService.InvalidateAsync(resourceName, cultureName);
    }
}
