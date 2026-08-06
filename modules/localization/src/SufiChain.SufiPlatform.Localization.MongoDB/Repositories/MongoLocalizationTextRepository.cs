using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Localization.MongoDB;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Localization.Repositories;

public class MongoLocalizationTextRepository
    : MongoDbRepository<LocalizationMongoDbContext, LocalizationText, Guid>,
      ILocalizationTextRepository
{
    public MongoLocalizationTextRepository(IMongoDbContextProvider<LocalizationMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<LocalizationText?> FindAsync(
        string resourceName,
        string cultureName,
        string key,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        return await query
            .Where(t => t.ResourceName == resourceName && t.CultureName == cultureName && t.Key == key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<LocalizationText>> GetListAsync(
        string resourceName,
        string cultureName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        return await query
            .Where(t => t.ResourceName == resourceName && t.CultureName == cultureName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LocalizationText>> GetListByResourceAsync(
        string resourceName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        return await query
            .Where(t => t.ResourceName == resourceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LocalizationText>> GetPagedListAsync(
        string? resourceName = null,
        string? cultureName = null,
        string? keyFilter = null,
        string? valueFilter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);

        query = ApplyFilter(query, resourceName, cultureName, keyFilter, valueFilter);

        if (!string.IsNullOrWhiteSpace(sorting))
        {
            query = query.OrderBy(sorting);
        }
        else
        {
            query = query.OrderBy(t => t.ResourceName).ThenBy(t => t.CultureName).ThenBy(t => t.Key);
        }

        return await query
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? resourceName = null,
        string? cultureName = null,
        string? keyFilter = null,
        string? valueFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        query = ApplyFilter(query, resourceName, cultureName, keyFilter, valueFilter);
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<List<string>> GetResourceNamesAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        return await query
            .Select(t => t.ResourceName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetCultureNamesAsync(
        string? resourceName = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(resourceName))
        {
            query = query.Where(t => t.ResourceName == resourceName);
        }

        return await query
            .Select(t => t.CultureName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByResourceAsync(
        string resourceName,
        CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        await collection.DeleteManyAsync(
            t => t.ResourceName == resourceName,
            cancellationToken);
    }

    public async Task DeleteByResourceAndCultureAsync(
        string resourceName,
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        await collection.DeleteManyAsync(
            t => t.ResourceName == resourceName && t.CultureName == cultureName,
            cancellationToken);
    }

    private static IQueryable<LocalizationText> ApplyFilter(
        IQueryable<LocalizationText> query,
        string? resourceName,
        string? cultureName,
        string? keyFilter,
        string? valueFilter)
    {
        if (!string.IsNullOrWhiteSpace(resourceName))
        {
            query = query.Where(t => t.ResourceName == resourceName);
        }

        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            query = query.Where(t => t.CultureName == cultureName);
        }

        if (!string.IsNullOrWhiteSpace(keyFilter))
        {
            query = query.Where(t => t.Key.Contains(keyFilter));
        }

        if (!string.IsNullOrWhiteSpace(valueFilter))
        {
            query = query.Where(t => t.Value.Contains(valueFilter));
        }

        return query;
    }
}
