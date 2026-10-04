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

    /// <summary>
    /// Returns the stored value for a resource/culture/key, or null when missing.
    /// Uses the ambient unit of work so seed-time inserts are visible before commit.
    /// </summary>
    /// <summary>
    /// Writes every culture in <paramref name="cultureValues"/>. A blank value deletes that culture.
    /// Unlike <see cref="UpsertAsync"/>, an empty value is not skipped.
    /// </summary>
    Task ReplaceKeyValuesAsync(
        string resourceName,
        string key,
        IReadOnlyDictionary<string, string> cultureValues,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    Task<string?> FindValueAsync(
        string resourceName,
        string cultureName,
        string key,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);
}
