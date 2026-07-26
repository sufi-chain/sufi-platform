using Volo.Abp.Reflection;

namespace SufiChain.SufiPlatform.Editions;

public static class EditionsPermissions
{
    public const string GroupName = "SufiEditions";

    public static class Editions
    {
        public const string Default = GroupName + ".Editions";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageFeatures = Default + ".ManageFeatures";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(EditionsPermissions));
    }
}
