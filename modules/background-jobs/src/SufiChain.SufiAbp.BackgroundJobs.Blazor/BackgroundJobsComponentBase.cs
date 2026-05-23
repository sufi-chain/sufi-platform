using SufiChain.SufiAbp.BackgroundJobs.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.BackgroundJobs.Blazor;

/// <summary>
/// Base class for Blazor components in the Background Jobs module.
/// Provides module localization via BackgroundJobsResource.
/// </summary>
public abstract class BackgroundJobsComponentBase : SufiAbpComponentBase
{
    protected BackgroundJobsComponentBase()
    {
        LocalizationResource = typeof(SufiAbpBackgroundJobsResource);
    }
}
