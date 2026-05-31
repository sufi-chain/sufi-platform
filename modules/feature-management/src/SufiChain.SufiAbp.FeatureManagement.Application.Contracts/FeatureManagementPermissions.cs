namespace SufiChain.SufiAbp.FeatureManagement;

public static class FeatureManagementPermissions
{
    public const string GroupName = "FeatureManagement";

    public static class Features
    {
        public const string Default = GroupName + ".Features";
        public const string ManageHostFeatures = Default + ".ManageHostFeatures";
        public const string ManageTenantFeatures = Default + ".ManageTenantFeatures";
    }
}
