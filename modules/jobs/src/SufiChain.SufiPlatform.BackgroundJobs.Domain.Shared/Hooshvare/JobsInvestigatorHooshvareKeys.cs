namespace SufiChain.SufiPlatform.BackgroundJobs.Hooshvare;

public static class JobsInvestigatorHooshvareKeys
{
    public const string Key = "SufiJobs:Investigator";
    public const string LocalizationResourceName = "SufiBackgroundJobs";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string Search = "jobs.search";
        public const string Get = "jobs.get";
    }

    public static class Context
    {
        public const string JobId = "jobId";
        public const string JobName = "jobName";
        public const string Status = "status";
    }

    public static class Shortcuts
    {
        public const string AbandonedJobs = "AbandonedJobs";
        public const string ExplainSelection = "ExplainSelection";
    }
}
