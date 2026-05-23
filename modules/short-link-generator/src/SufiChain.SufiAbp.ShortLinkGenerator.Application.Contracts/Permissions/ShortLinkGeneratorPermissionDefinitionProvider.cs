using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Permissions;

public class ShortLinkGeneratorPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ShortLinkGeneratorGroup = context.AddGroup(
            ShortLinkGeneratorPermissions.GroupName,
            L("Permission:ShortLinkGenerator"));

        var shortLinksPermission = ShortLinkGeneratorGroup.AddPermission(
            ShortLinkGeneratorPermissions.ShortLinks.Default,
            L("Permission:ShortLinks"));

        shortLinksPermission.AddChild(
            ShortLinkGeneratorPermissions.ShortLinks.Create,
            L("Permission:Create"));

        shortLinksPermission.AddChild(
            ShortLinkGeneratorPermissions.ShortLinks.Edit,
            L("Permission:Edit"));

        shortLinksPermission.AddChild(
            ShortLinkGeneratorPermissions.ShortLinks.Delete,
            L("Permission:Delete"));

        shortLinksPermission.AddChild(
            ShortLinkGeneratorPermissions.ShortLinks.ViewAnalytics,
            L("Permission:ViewAnalytics"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpShortLinkGeneratorResource>(name);
    }
}

