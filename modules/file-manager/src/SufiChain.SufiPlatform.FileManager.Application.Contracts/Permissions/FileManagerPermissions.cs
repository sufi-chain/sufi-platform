using Volo.Abp.Reflection;

namespace SufiChain.SufiPlatform.FileManager.Permissions;

public static class FileManagerPermissions
{
    public const string GroupName = "SufiFileManager";

    public static class FileItems
    {
        public const string Default = GroupName + ".FileItems";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class FileStructures
    {
        public const string Default = GroupName + ".FileStructures";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
    }

    public static class StorageSettings
    {
        public const string Manage = GroupName + ".StorageSettings";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(FileManagerPermissions));
    }
}