namespace SufiChain.SufiPlatform.Identity.Hooshvare;

public static class IdentityAdminAdvisorHooshvareKeys
{
    public const string Key = "SufiIdentity:AdminAdvisor";
    public const string LocalizationResourceName = "SufiIdentity";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string SearchUsers = "identity.search_users";
        public const string GetUser = "identity.get_user";
        public const string GetUserRoles = "identity.get_user_roles";
        public const string ListRoles = "identity.list_roles";
        public const string GetOuTree = "identity.get_ou_tree";
        public const string SearchSecurityLogs = "identity.search_security_logs";
    }

    public static class Context
    {
        public const string UserId = "userId";
        public const string RoleId = "roleId";
        public const string OrganizationUnitId = "organizationUnitId";
        public const string FromUtc = "fromUtc";
        public const string ToUtc = "toUtc";
    }

    public static class Shortcuts
    {
        public const string ExplainUser = "ExplainUser";
        public const string RecentSecurity = "RecentSecurity";
    }
}
