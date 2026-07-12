using SufiChain.SufiPlatform.Tags.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Tags.Permissions;

public class TagsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(TagsPermissions.GroupName, L("Permission:SufiTags"));

        var tags = group.AddPermission(TagsPermissions.Tags.Default, L("Permission:SufiTags.Tags"));
        tags.AddChild(TagsPermissions.Tags.Create, L("Permission:SufiTags.Tags"));
        tags.AddChild(TagsPermissions.Tags.Update, L("Permission:SufiTags.Tags"));
        tags.AddChild(TagsPermissions.Tags.Delete, L("Permission:SufiTags.Tags"));

        var tagLinks = group.AddPermission(TagsPermissions.TagLinks.Default, L("Permission:SufiTags.TagLinks"));
        tagLinks.AddChild(TagsPermissions.TagLinks.Assign, L("Permission:SufiTags.TagLinks"));
        tagLinks.AddChild(TagsPermissions.TagLinks.Unassign, L("Permission:SufiTags.TagLinks"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiTagsResource>(name);
    }
}