using SufiChain.SufiAbp.FileManager.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.FileManager.Permissions;

public class FileManagerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var FileManagerGroup = context.AddGroup(FileManagerPermissions.GroupName, L("Permission:FileManager"));

        var mediaItems = FileManagerGroup.AddPermission(FileManagerPermissions.FileItems.Default, L("Permission:MediaItems"));
        mediaItems.AddChild(FileManagerPermissions.FileItems.Create, L("Permission:Create"));
        mediaItems.AddChild(FileManagerPermissions.FileItems.Update, L("Permission:Update"));
        mediaItems.AddChild(FileManagerPermissions.FileItems.Delete, L("Permission:Delete"));

        var mediaStructures = FileManagerGroup.AddPermission(FileManagerPermissions.FileStructures.Default, L("Permission:MediaStructures"));
        mediaStructures.AddChild(FileManagerPermissions.FileStructures.Create, L("Permission:Create"));
        mediaStructures.AddChild(FileManagerPermissions.FileStructures.Update, L("Permission:Update"));
        mediaStructures.AddChild(FileManagerPermissions.FileStructures.Delete, L("Permission:Delete"));

        FileManagerGroup.AddPermission(FileManagerPermissions.Settings.Default, L("Permission:FileManagerSettings"));
        FileManagerGroup.AddPermission(FileManagerPermissions.StorageSettings.Manage, L("Permission:FileManagerStorageSettings"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpFileManagerResource>(name);
    }
}
