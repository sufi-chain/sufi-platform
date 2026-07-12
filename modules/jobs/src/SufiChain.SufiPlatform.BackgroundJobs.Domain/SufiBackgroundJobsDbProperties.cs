using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.BackgroundJobs;

public static class SufiBackgroundJobsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiBackgroundJobs.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiBackgroundJobs";
}
