using Volo.Abp.Reflection;

namespace SufiChain.SufiPlatform.BackgroundJobs.Permissions;

/// <summary>
/// Permission constants for the Background Jobs module.
/// Follows ABP permission naming conventions.
/// </summary>
public static class BackgroundJobsPermissions
{
    public const string GroupName = "SufiBackgroundJobs";

    public static class BackgroundJobs
    {
        public const string Default = GroupName + ".BackgroundJobs";
        public const string Delete = Default + ".Delete";
        public const string Retry = Default + ".Retry";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(BackgroundJobsPermissions));
    }
}
