using SufiChain.SufiPlatform.ShortLinks.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.ShortLinks.Permissions;

public class ShortLinksPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ShortLinksGroup = context.AddGroup(
            ShortLinksPermissions.GroupName,
            L("Permission:SufiShortLinks"));

        var shortLinksPermission = ShortLinksGroup.AddPermission(
            ShortLinksPermissions.ShortLinks.Default,
            L("Permission:SufiShortLinks"));

        shortLinksPermission.AddChild(
            ShortLinksPermissions.ShortLinks.Create,
            L("Permission:Create"));

        shortLinksPermission.AddChild(
            ShortLinksPermissions.ShortLinks.Edit,
            L("Permission:Edit"));

        shortLinksPermission.AddChild(
            ShortLinksPermissions.ShortLinks.Delete,
            L("Permission:Delete"));

        shortLinksPermission.AddChild(
            ShortLinksPermissions.ShortLinks.ViewAnalytics,
            L("Permission:ViewAnalytics"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiShortLinksResource>(name);
    }
}