using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Localization;

/// <summary>
/// Idempotent upsert API for seeding business-tier localization text into the database.
/// </summary>
public interface ILocalizationTextSeeder
{
    Task UpsertAsync(
        string resourceName,
        string key,
        IReadOnlyDictionary<string, string> cultureValues,
        Guid? tenantId = null,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default);

    Task UpsertManyAsync(
        string resourceName,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> keysByCulture,
        Guid? tenantId = null,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default);
}
