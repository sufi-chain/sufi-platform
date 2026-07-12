namespace SufiChain.SufiPlatform.ShortLinks.Permissions;

public static class ShortLinksPermissions
{
    public const string GroupName = "SufiShortLinks";
    
    public static class ShortLinks
    {
        public const string Default = GroupName + ".ShortLinks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAnalytics = Default + ".ViewAnalytics";
    }
}