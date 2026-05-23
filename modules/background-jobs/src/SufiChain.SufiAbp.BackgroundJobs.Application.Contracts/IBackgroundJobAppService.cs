using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.BackgroundJobs;

/// <summary>
/// Application service for managing background jobs.
/// </summary>
[RemoteService(Name = BackgroundJobsRemoteServiceConsts.RemoteServiceName)]
public interface IBackgroundJobAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of background jobs.
    /// </summary>
    Task<PagedResultDto<BackgroundJobListItemDto>> GetListAsync(GetBackgroundJobListInput input);

    /// <summary>
    /// Gets a specific background job by ID.
    /// </summary>
    Task<BackgroundJobDto> GetAsync(Guid id);

    /// <summary>
    /// Deletes a background job.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Retries a background job by resetting its try count and scheduling it for immediate execution.
    /// </summary>
    Task RetryAsync(Guid id);

    /// <summary>
    /// Abandons a background job by marking it as abandoned.
    /// </summary>
    Task AbandonAsync(Guid id);
}
