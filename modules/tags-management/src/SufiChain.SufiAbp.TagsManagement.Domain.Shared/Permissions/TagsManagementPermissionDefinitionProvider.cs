using SufiChain.SufiAbp.TagsManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.TagsManagement.Permissions;

public class TagsManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(TagsManagementPermissions.GroupName, L("Permission:TagsManagement"));

        var tags = group.AddPermission(TagsManagementPermissions.Tags.Default, L("Permission:TagsManagement.Tags"));
        tags.AddChild(TagsManagementPermissions.Tags.Create, L("Permission:TagsManagement.Tags"));
        tags.AddChild(TagsManagementPermissions.Tags.Update, L("Permission:TagsManagement.Tags"));
        tags.AddChild(TagsManagementPermissions.Tags.Delete, L("Permission:TagsManagement.Tags"));

        var tagLinks = group.AddPermission(TagsManagementPermissions.TagLinks.Default, L("Permission:TagsManagement.TagLinks"));
        tagLinks.AddChild(TagsManagementPermissions.TagLinks.Assign, L("Permission:TagsManagement.TagLinks"));
        tagLinks.AddChild(TagsManagementPermissions.TagLinks.Unassign, L("Permission:TagsManagement.TagLinks"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpTagsManagementResource>(name);
    }
}
