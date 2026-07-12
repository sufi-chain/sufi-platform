namespace SufiChain.SufiPlatform.Tags.Permissions;

public static class TagsPermissions
{
    public const string GroupName = "SufiTags";

    public static class Tags
    {
        public const string Default = GroupName + ".Tags";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class TagLinks
    {
        public const string Default = GroupName + ".TagLinks";
        public const string Assign = Default + ".Assign";
        public const string Unassign = Default + ".Unassign";
    }
}