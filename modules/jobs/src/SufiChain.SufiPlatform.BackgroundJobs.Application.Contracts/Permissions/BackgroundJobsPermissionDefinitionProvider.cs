using SufiChain.SufiPlatform.BackgroundJobs.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.BackgroundJobs.Permissions;

/// <summary>
/// Defines permissions for the Background Jobs module.
/// </summary>
public class BackgroundJobsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var backgroundJobsGroup = context.AddGroup(
            BackgroundJobsPermissions.GroupName,
            L("Permission:BackgroundJobs"));

        var backgroundJobsPermission = backgroundJobsGroup.AddPermission(
            BackgroundJobsPermissions.BackgroundJobs.Default,
            L("Permission:BackgroundJobs.Management"));

        backgroundJobsPermission.AddChild(
            BackgroundJobsPermissions.BackgroundJobs.Delete,
            L("Permission:BackgroundJobs.Delete"));

        backgroundJobsPermission.AddChild(
            BackgroundJobsPermissions.BackgroundJobs.Retry,
            L("Permission:BackgroundJobs.Retry"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiBackgroundJobsResource>(name);
    }
}
