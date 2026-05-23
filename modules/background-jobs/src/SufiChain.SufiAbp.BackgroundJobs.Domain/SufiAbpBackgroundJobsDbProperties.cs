using Volo.Abp.Data;

namespace SufiChain.SufiAbp.BackgroundJobs;

public static class SufiAbpBackgroundJobsDbProperties
{
    public static string DbTablePrefix { get; set; } = "BackgroundJobs.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpBackgroundJobs";
}
