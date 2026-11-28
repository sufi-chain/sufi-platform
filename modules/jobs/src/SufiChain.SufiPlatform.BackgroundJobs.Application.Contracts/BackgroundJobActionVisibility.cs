namespace SufiChain.SufiPlatform.BackgroundJobs;

/// <summary>
/// Which row actions a background-jobs page may show. View stays available to anyone who can open the page.
/// </summary>
public static class BackgroundJobActionVisibility
{
    public static bool ShowRetry(bool canRetry, bool isAbandoned) => canRetry && isAbandoned;

    public static bool ShowAbandon(bool canRetry, bool isAbandoned) => canRetry && !isAbandoned;

    public static bool ShowDelete(bool canDelete) => canDelete;
}
