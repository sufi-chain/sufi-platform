using SufiChain.SufiPlatform.BackgroundJobs.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.BackgroundJobs.Blazor;

/// <summary>
/// Base class for Blazor components in the Background Jobs module.
/// Provides module localization via BackgroundJobsResource.
/// </summary>
public abstract class BackgroundJobsComponentBase : SufiComponentBase
{
    protected BackgroundJobsComponentBase()
    {
        LocalizationResource = typeof(SufiBackgroundJobsResource);
    }
}
