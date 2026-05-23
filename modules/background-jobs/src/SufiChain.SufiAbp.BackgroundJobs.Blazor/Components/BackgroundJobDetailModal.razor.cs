using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using SufiChain.SufiAbp.BackgroundJobs.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.BackgroundJobs;

namespace SufiChain.SufiAbp.BackgroundJobs.Blazor.Components;

public partial class BackgroundJobDetailModal : BackgroundJobsComponentBase
{

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public BackgroundJobListItemDto? Job { get; set; }

    private async Task OnDialogOpenChanged(bool value)
    {
        // Propagate the change to the parent
        await OpenChanged.InvokeAsync(value);
    }

    private async Task Hide()
    {
        await OpenChanged.InvokeAsync(false);
    }

    private SbColor GetStatusColor(BackgroundJobListItemDto job)
    {
        if (job.IsAbandoned) return SbColor.Danger;
        if (job.TryCount > 3) return SbColor.Warning;
        if (job.NextTryTime.HasValue && job.NextTryTime.Value <= DateTime.Now) return SbColor.Info;
        return SbColor.Success;
    }

    private string GetStatusText(BackgroundJobListItemDto job)
    {
        if (job.IsAbandoned) return L["Abandoned"];
        if (job.TryCount > 3) return L["Retrying"];
        if (job.NextTryTime.HasValue && job.NextTryTime.Value <= DateTime.Now) return L["Pending"];
        return L["Scheduled"];
    }

    private SbColor GetPriorityColor(BackgroundJobPriority priority)
    {
        return priority switch
        {
            BackgroundJobPriority.High => SbColor.Danger,
            BackgroundJobPriority.AboveNormal => SbColor.Warning,
            BackgroundJobPriority.Normal => SbColor.Primary,
            BackgroundJobPriority.BelowNormal => SbColor.Info,
            BackgroundJobPriority.Low => SbColor.Secondary,
            _ => SbColor.Default
        };
    }
}
