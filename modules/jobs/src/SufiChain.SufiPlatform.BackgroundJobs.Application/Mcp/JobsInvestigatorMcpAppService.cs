using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.BackgroundJobs;
using SufiChain.SufiPlatform.BackgroundJobs.Dtos;
using SufiChain.SufiPlatform.BackgroundJobs.Hooshvare;
using SufiChain.SufiPlatform.BackgroundJobs.Permissions;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.BackgroundJobs.Mcp;

[Authorize]
public class JobsInvestigatorMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected IBackgroundJobAppService Jobs { get; }

    public JobsInvestigatorMcpAppService(IBackgroundJobAppService jobs)
    {
        Jobs = jobs;
    }

    [SufiAiMcpTool(JobsInvestigatorHooshvareKeys.Tools.Search,
        "Searches background jobs. Arguments are summarized by property name and never returned raw. Does not delete, retry, or abandon.", ReadOnly = true)]
    [Authorize(BackgroundJobsPermissions.BackgroundJobs.Default)]
    public virtual async Task<object> SearchAsync(string? jobName = null, string? status = null)
    {
        bool? abandoned = status?.Equals("abandoned", StringComparison.OrdinalIgnoreCase) == true ? true
            : status?.Equals("active", StringComparison.OrdinalIgnoreCase) == true ? false
            : null;
        var page = await Jobs.GetListAsync(new GetBackgroundJobListInput
        {
            JobName = jobName,
            IsAbandoned = abandoned,
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(MapList).ToList()
        };
    }

    [SufiAiMcpTool(JobsInvestigatorHooshvareKeys.Tools.Get,
        "Returns one background job with a redacted argument summary. Does not delete, retry, or abandon.", ReadOnly = true)]
    [Authorize(BackgroundJobsPermissions.BackgroundJobs.Default)]
    public virtual async Task<object> GetAsync(Guid id)
    {
        var job = await Jobs.GetAsync(id);
        return new
        {
            job.Id,
            job.JobName,
            job.ApplicationName,
            Priority = job.Priority.ToString(),
            job.TryCount,
            job.CreationTime,
            job.NextTryTime,
            job.LastTryTime,
            job.IsAbandoned,
            JobArgs = SummarizeArgs(job.JobArgs)
        };
    }

    private static object MapList(BackgroundJobListItemDto job) => new
    {
        job.Id,
        job.JobName,
        job.ApplicationName,
        Priority = job.Priority.ToString(),
        job.TryCount,
        job.CreationTime,
        job.NextTryTime,
        job.LastTryTime,
        job.IsAbandoned,
        Status = job.IsAbandoned ? "abandoned" : "active",
        JobArgs = SummarizeArgs(job.JobArgs)
    };

    private static object SummarizeArgs(string? jobArgs)
    {
        if (string.IsNullOrWhiteSpace(jobArgs))
        {
            return new { Type = "empty" };
        }

        var trimmed = jobArgs.Trim();
        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    return new
                    {
                        Type = "object",
                        Properties = document.RootElement.EnumerateObject().Select(property => property.Name).ToList()
                    };
                }
            }
            catch (JsonException)
            {
            }
        }

        return new { Type = "redacted", Length = jobArgs.Length };
    }
}
