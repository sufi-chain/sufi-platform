namespace SufiChain.SufiAbp.ShortLinkGenerator.Permissions;

public static class ShortLinkGeneratorPermissions
{
    public const string GroupName = "ShortLinkGenerator";
    
    public static class ShortLinks
    {
        public const string Default = GroupName + ".ShortLinks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewAnalytics = Default + ".ViewAnalytics";
    }
}

