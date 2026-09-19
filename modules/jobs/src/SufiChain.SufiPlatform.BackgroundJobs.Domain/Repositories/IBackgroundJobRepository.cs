using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using BackgroundJobNameFilter = Volo.Abp.BackgroundJobs.BackgroundJobNameFilter;

namespace SufiChain.SufiPlatform.BackgroundJobs;

public interface IBackgroundJobRepository : IBasicRepository<BackgroundJobRecord, Guid>
{
    Task<List<BackgroundJobRecord>> GetWaitingListAsync([CanBeNull] string applicationName, int maxResultCount, CancellationToken cancellationToken = default);

    Task<List<BackgroundJobRecord>> GetWaitingListAsync(
        [CanBeNull] string applicationName,
        int maxResultCount,
        BackgroundJobNameFilter? jobNameFilter,
        CancellationToken cancellationToken = default);

    Task<int> DeleteAsync(
        [CanBeNull] string applicationName,
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default);
}
