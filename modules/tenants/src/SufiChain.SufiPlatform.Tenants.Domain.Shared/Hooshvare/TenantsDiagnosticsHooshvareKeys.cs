namespace SufiChain.SufiPlatform.Tenants.Hooshvare;

public static class TenantsDiagnosticsHooshvareKeys
{
    public const string Key = "SufiTenants:Diagnostics";
    public const string LocalizationResourceName = "SufiTenants";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string List = "tenants.list";
        public const string Get = "tenants.get";
        public const string GetSummary = "tenants.get_summary";
    }

    public static class Context
    {
        public const string TenantId = "tenantId";
    }

    public static class Shortcuts
    {
        public const string SummarizeSelection = "SummarizeSelection";
        public const string ListTenants = "ListTenants";
    }
}
