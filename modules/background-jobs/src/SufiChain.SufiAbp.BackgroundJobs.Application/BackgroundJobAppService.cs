using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using SufiChain.SufiAbp.BackgroundJobs.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.BackgroundJobs;

/// <summary>
/// Application service for managing background jobs.
/// </summary>
[Authorize(BackgroundJobsPermissions.BackgroundJobs.Default)]
public class BackgroundJobAppService : ApplicationService, IBackgroundJobAppService
{
    private readonly IBackgroundJobRepository _backgroundJobRepository;

    public BackgroundJobAppService(IBackgroundJobRepository backgroundJobRepository)
    {
        _backgroundJobRepository = backgroundJobRepository;
    }

    public virtual async Task<PagedResultDto<BackgroundJobListItemDto>> GetListAsync(GetBackgroundJobListInput input)
    {
        // Get all jobs and filter in memory since IBackgroundJobRepository has limited query methods
        var allJobs = await _backgroundJobRepository.GetListAsync();
        
        // Apply filters
        var query = allJobs.AsQueryable();
        
        if (!string.IsNullOrEmpty(input.JobName))
        {
            query = query.Where(x => x.JobName != null && x.JobName.Contains(input.JobName, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(input.ApplicationName))
        {
            query = query.Where(x => x.ApplicationName != null && x.ApplicationName.Contains(input.ApplicationName, StringComparison.OrdinalIgnoreCase));
        }
        
        if (input.IsAbandoned.HasValue)
        {
            query = query.Where(x => x.IsAbandoned == input.IsAbandoned.Value);
        }
        
        if (input.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == input.Priority.Value);
        }
        
        // Apply sorting
        query = !string.IsNullOrEmpty(input.Sorting) 
            ? ApplySorting(query, input.Sorting)
            : query.OrderByDescending(x => x.CreationTime);
        
        var totalCount = query.Count();
        
        // Apply paging
        var items = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();
        
        return new PagedResultDto<BackgroundJobListItemDto>(
            totalCount,
            ObjectMapper.Map<List<BackgroundJobRecord>, List<BackgroundJobListItemDto>>(items)
        );
    }

    public virtual async Task<BackgroundJobDto> GetAsync(Guid id)
    {
        var job = await _backgroundJobRepository.GetAsync(id);
        return ObjectMapper.Map<BackgroundJobRecord, BackgroundJobDto>(job);
    }

    [Authorize(BackgroundJobsPermissions.BackgroundJobs.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _backgroundJobRepository.DeleteAsync(id);
    }

    [Authorize(BackgroundJobsPermissions.BackgroundJobs.Retry)]
    public virtual async Task RetryAsync(Guid id)
    {
        var job = await _backgroundJobRepository.GetAsync(id);
        job.TryCount = 0;
        job.IsAbandoned = false;
        job.NextTryTime = Clock.Now;
        await _backgroundJobRepository.UpdateAsync(job);
    }

    [Authorize(BackgroundJobsPermissions.BackgroundJobs.Retry)]
    public virtual async Task AbandonAsync(Guid id)
    {
        var job = await _backgroundJobRepository.GetAsync(id);
        job.IsAbandoned = true;
        await _backgroundJobRepository.UpdateAsync(job);
    }

    private static IQueryable<BackgroundJobRecord> ApplySorting(IQueryable<BackgroundJobRecord> query, string sorting)
    {
        var sortParts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sortField = sortParts[0];
        var isDescending = sortParts.Length > 1 && sortParts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
        
        return sortField.ToLower() switch
        {
            "jobname" => isDescending ? query.OrderByDescending(x => x.JobName) : query.OrderBy(x => x.JobName),
            "applicationname" => isDescending ? query.OrderByDescending(x => x.ApplicationName) : query.OrderBy(x => x.ApplicationName),
            "priority" => isDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
            "trycount" => isDescending ? query.OrderByDescending(x => x.TryCount) : query.OrderBy(x => x.TryCount),
            "creationtime" => isDescending ? query.OrderByDescending(x => x.CreationTime) : query.OrderBy(x => x.CreationTime),
            "nexttrytime" => isDescending ? query.OrderByDescending(x => x.NextTryTime) : query.OrderBy(x => x.NextTryTime),
            "isabandoned" => isDescending ? query.OrderByDescending(x => x.IsAbandoned) : query.OrderBy(x => x.IsAbandoned),
            _ => query.OrderByDescending(x => x.CreationTime)
        };
    }
}
