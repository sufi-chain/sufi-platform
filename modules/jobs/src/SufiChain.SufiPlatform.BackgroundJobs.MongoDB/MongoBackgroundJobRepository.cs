using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Timing;
using BackgroundJobNameFilter = Volo.Abp.BackgroundJobs.BackgroundJobNameFilter;
using BackgroundJobNameFilterMode = Volo.Abp.BackgroundJobs.BackgroundJobNameFilterMode;

namespace SufiChain.SufiPlatform.BackgroundJobs.MongoDB;

public class MongoBackgroundJobRepository : MongoDbRepository<IBackgroundJobsMongoDbContext, BackgroundJobRecord, Guid>, IBackgroundJobRepository
{
    protected IClock Clock { get; }

    public MongoBackgroundJobRepository(
        IMongoDbContextProvider<IBackgroundJobsMongoDbContext> dbContextProvider,
        IClock clock)
        : base(dbContextProvider)
    {
        Clock = clock;
    }

    public virtual Task<List<BackgroundJobRecord>> GetWaitingListAsync([CanBeNull] string applicationName, int maxResultCount, CancellationToken cancellationToken = default)
    {
        return GetWaitingListAsync(applicationName, maxResultCount, null, cancellationToken);
    }

    public virtual async Task<List<BackgroundJobRecord>> GetWaitingListAsync(
        [CanBeNull] string applicationName,
        int maxResultCount,
        BackgroundJobNameFilter? jobNameFilter,
        CancellationToken cancellationToken = default)
    {
        return await (await GetWaitingListQuery(applicationName, maxResultCount, jobNameFilter, cancellationToken))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<BackgroundJobRecord>> GetWaitingListQuery(
        [CanBeNull] string applicationName,
        int maxResultCount,
        BackgroundJobNameFilter? jobNameFilter = null,
        CancellationToken cancellationToken = default)
    {
        var now = Clock.Now;
        var filter = jobNameFilter ?? BackgroundJobNameFilter.None;
        var jobNames = filter.JobNames.ToList();

        var query = (await GetQueryableAsync(cancellationToken))
            .Where(t => t.ApplicationName == applicationName)
            .Where(t => !t.IsAbandoned && t.CompletionTime == null && t.NextTryTime <= now);

        if (filter.Mode == BackgroundJobNameFilterMode.Include)
        {
            query = query.Where(t => jobNames.Contains(t.JobName));
        }
        else if (filter.Mode == BackgroundJobNameFilterMode.Exclude)
        {
            query = query.Where(t => !jobNames.Contains(t.JobName));
        }

        return query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.TryCount)
            .ThenBy(t => t.NextTryTime)
            .Take(maxResultCount);
    }

    public virtual async Task<int> DeleteAsync(
        [CanBeNull] string applicationName,
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);

        var ids = await (await GetQueryableAsync(token))
            .Where(t => t.ApplicationName == applicationName)
            .Where(t => t.CompletionTime != null && t.CompletionTime < completedBefore)
            .OrderBy(t => t.CompletionTime)
            .Select(t => t.Id)
            .Take(maxResultCount)
            .ToListAsync(token);

        if (ids.Count == 0)
        {
            return 0;
        }

        await DeleteManyAsync(ids, cancellationToken: token);
        return ids.Count;
    }
}
