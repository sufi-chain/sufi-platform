using Volo.Abp.Reflection;

namespace SufiChain.SufiAbp.LocalizationManagement.Permissions;

public static class LocalizationManagementPermissions
{
    public const string GroupName = "LocalizationManagement";

    public static class Texts
    {
        public const string Default = GroupName + ".Texts";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Import = Default + ".Import";
        public const string Export = Default + ".Export";
    }

    public static class Resources
    {
        public const string Default = GroupName + ".Resources";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(LocalizationManagementPermissions));
    }
}
