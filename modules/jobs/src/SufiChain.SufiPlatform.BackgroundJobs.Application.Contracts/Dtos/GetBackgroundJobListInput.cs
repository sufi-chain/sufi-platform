using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.BackgroundJobs.Dtos;

/// <summary>
/// Input DTO for getting a paged list of background jobs.
/// </summary>
public class GetBackgroundJobListInput : PagedAndSortedResultRequestDto
{
    public string? JobName { get; set; }
    public string? ApplicationName { get; set; }
    public bool? IsAbandoned { get; set; }
    public BackgroundJobPriority? Priority { get; set; }
}
