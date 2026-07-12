using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Localization;

public static class LocalizationTextSeederExtensions
{
    public static Task UpsertAsync(
        this ILocalizationTextSeeder seeder,
        DataSeedContext context,
        string resourceName,
        string key,
        IReadOnlyDictionary<string, string> cultureValues,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        return seeder.UpsertAsync(
            resourceName,
            key,
            cultureValues,
            context.TenantId,
            overwriteExisting,
            cancellationToken);
    }

    public static Task UpsertManyAsync(
        this ILocalizationTextSeeder seeder,
        DataSeedContext context,
        string resourceName,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> keysByCulture,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        return seeder.UpsertManyAsync(
            resourceName,
            keysByCulture,
            context.TenantId,
            overwriteExisting,
            cancellationToken);
    }

    public static async Task UpsertStructureTextsAsync(
        this ILocalizationTextSeeder seeder,
        string resourceName,
        string structureKey,
        IReadOnlyDictionary<string, string> displayNameValues,
        IReadOnlyDictionary<string, string>? descriptionValues = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        await seeder.UpsertAsync(
            resourceName,
            BusinessLocalizationKeys.FileStructureDisplayName(structureKey),
            displayNameValues,
            tenantId,
            cancellationToken: cancellationToken);

        if (descriptionValues != null && descriptionValues.Count > 0)
        {
            await seeder.UpsertAsync(
                resourceName,
                BusinessLocalizationKeys.FileStructureDescription(structureKey),
                descriptionValues,
                tenantId,
                cancellationToken: cancellationToken);
        }
    }

    public static Task UpsertStructureTextsAsync(
        this ILocalizationTextSeeder seeder,
        DataSeedContext context,
        string resourceName,
        string structureKey,
        IReadOnlyDictionary<string, string> displayNameValues,
        IReadOnlyDictionary<string, string>? descriptionValues = null,
        CancellationToken cancellationToken = default)
    {
        return seeder.UpsertStructureTextsAsync(
            resourceName,
            structureKey,
            displayNameValues,
            descriptionValues,
            context.TenantId,
            cancellationToken);
    }
}
