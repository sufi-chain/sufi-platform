using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Localization.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Localization.Repositories;

public class EfCoreLocalizationResourceRepository
    : EfCoreRepository<ISufiLocalizationDbContext, LocalizationResource, Guid>,
      ILocalizationResourceRepository
{
    public EfCoreLocalizationResourceRepository(IDbContextProvider<ISufiLocalizationDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<LocalizationResource?> FindByNameAsync(
        string resourceName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(r => r.ResourceName == resourceName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<LocalizationResource>> GetEnabledListAsync(
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(r => r.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetResourceNamesAsync(
        bool enabledOnly = true,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (enabledOnly)
        {
            query = query.Where(r => r.IsEnabled);
        }

        return await query
            .Select(r => r.ResourceName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LocalizationResource>> GetPagedListAsync(
        string? filter = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet.AsQueryable(), filter, isEnabled);

        if (!string.IsNullOrWhiteSpace(sorting))
        {
            query = query.OrderBy(sorting);
        }
        else
        {
            query = query.OrderBy(r => r.ResourceName);
        }

        return await query
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? filter = null,
        bool? isEnabled = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet.AsQueryable(), filter, isEnabled);
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string resourceName,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(r => r.ResourceName == resourceName, cancellationToken);
    }

    private static IQueryable<LocalizationResource> ApplyFilter(
        IQueryable<LocalizationResource> query,
        string? filter,
        bool? isEnabled)
    {
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(r =>
                r.ResourceName.Contains(filter) ||
                (r.DisplayName != null && r.DisplayName.Contains(filter)));
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(r => r.IsEnabled == isEnabled.Value);
        }

        return query;
    }
}
