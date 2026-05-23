using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.LocalizationManagement.Repositories;

public class EfCoreLocalizationTextRepository
    : EfCoreRepository<ISufiAbpLocalizationManagementDbContext, LocalizationText, Guid>,
      ILocalizationTextRepository
{
    public EfCoreLocalizationTextRepository(IDbContextProvider<ISufiAbpLocalizationManagementDbContext> dbContextProvider)
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
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(t => t.ResourceName == resourceName && t.CultureName == cultureName && t.Key == key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<LocalizationText>> GetListAsync(
        string resourceName,
        string cultureName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(t => t.ResourceName == resourceName && t.CultureName == cultureName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LocalizationText>> GetListByResourceAsync(
        string resourceName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
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
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet.AsQueryable(), resourceName, cultureName, keyFilter, valueFilter);

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
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet.AsQueryable(), resourceName, cultureName, keyFilter, valueFilter);
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<List<string>> GetResourceNamesAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Select(t => t.ResourceName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetCultureNamesAsync(
        string? resourceName = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

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
        var dbSet = await GetDbSetAsync();
        await dbSet
            .Where(t => t.ResourceName == resourceName)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByResourceAndCultureAsync(
        string resourceName,
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        await dbSet
            .Where(t => t.ResourceName == resourceName && t.CultureName == cultureName)
            .ExecuteDeleteAsync(cancellationToken);
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
