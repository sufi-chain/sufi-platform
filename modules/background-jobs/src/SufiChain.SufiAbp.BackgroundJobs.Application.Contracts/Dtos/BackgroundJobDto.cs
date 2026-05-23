using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.BackgroundJobs;

namespace SufiChain.SufiAbp.BackgroundJobs.Dtos;

/// <summary>
/// Full background job details DTO.
/// </summary>
public class BackgroundJobDto : SufiAbpEntityDto<Guid>
{
    public string? JobName { get; set; }
    public string? ApplicationName { get; set; }
    public string? JobArgs { get; set; }
    public BackgroundJobPriority Priority { get; set; }
    public short TryCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime NextTryTime { get; set; }
    public DateTime? LastTryTime { get; set; }
    public bool IsAbandoned { get; set; }
}

/// <summary>
/// Background job list item DTO for grid display.
/// </summary>
public class BackgroundJobListItemDto : SufiAbpEntityDto<Guid>
{
    public string? JobName { get; set; }
    public string? ApplicationName { get; set; }
    public string? JobArgs { get; set; }
    public BackgroundJobPriority Priority { get; set; }
    public short TryCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? NextTryTime { get; set; }
    public DateTime? LastTryTime { get; set; }
    public bool IsAbandoned { get; set; }
}
