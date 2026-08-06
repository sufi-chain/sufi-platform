namespace SufiChain.SufiPlatform.Features;

public static class FeaturesPermissions
{
    public const string GroupName = "SufiFeatures";

    public static class Features
    {
        public const string Default = GroupName + ".Features";
        public const string ManageHostFeatures = Default + ".ManageHostFeatures";
        public const string ManageTenantFeatures = Default + ".ManageTenantFeatures";
    }
}
