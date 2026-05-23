using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.BackgroundJobs.Controllers;

/// <summary>
/// Controller for background job operations.
/// </summary>
[Area(BackgroundJobsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = BackgroundJobsRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/background-jobs")]
public class BackgroundJobController : SufiAbpControllerBase, IBackgroundJobAppService
{
    private readonly IBackgroundJobAppService _backgroundJobAppService;

    public BackgroundJobController(IBackgroundJobAppService backgroundJobAppService)
    {
        _backgroundJobAppService = backgroundJobAppService;
    }

    /// <summary>
    /// Gets a paged list of background jobs.
    /// </summary>
    [HttpGet]
    public virtual Task<PagedResultDto<BackgroundJobListItemDto>> GetListAsync([FromQuery] GetBackgroundJobListInput input)
    {
        return _backgroundJobAppService.GetListAsync(input);
    }

    /// <summary>
    /// Gets a specific background job by ID.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public virtual Task<BackgroundJobDto> GetAsync(Guid id)
    {
        return _backgroundJobAppService.GetAsync(id);
    }

    /// <summary>
    /// Deletes a background job.
    /// </summary>
    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _backgroundJobAppService.DeleteAsync(id);
    }

    /// <summary>
    /// Retries a background job.
    /// </summary>
    [HttpPost]
    [Route("{id}/retry")]
    public virtual Task RetryAsync(Guid id)
    {
        return _backgroundJobAppService.RetryAsync(id);
    }

    /// <summary>
    /// Abandons a background job.
    /// </summary>
    [HttpPost]
    [Route("{id}/abandon")]
    public virtual Task AbandonAsync(Guid id)
    {
        return _backgroundJobAppService.AbandonAsync(id);
    }
}
