using Volo.Abp.Reflection;

namespace SufiChain.SufiAbp.Identity;

/// <summary>
/// Permission constants for the SufiAbp Identity module.
/// Follows ABP permission naming conventions.
/// </summary>
public static class IdentityPermissions 
{
    /// <summary>
    /// Permission group name for SufiAbp Identity module.
    /// Uses "SufiAbpIdentity" to integrate with ABP's Identity permission group.
    /// </summary>
    public const string GroupName = "SufiAbpIdentity";

    public static class Roles
    {
        public const string Default = GroupName + ".Roles";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManagePermissions = Default + ".ManagePermissions";
    }

    public static class Users
    {
        public const string Default = GroupName + ".Users";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManagePermissions = Default + ".ManagePermissions";
        public const string ManageRoles = Update + ".ManageRoles";
    }

    public static class UserLookup
    {
        public const string Default = GroupName + ".UserLookup";
    }

    public static class SecurityLogs
    {
        public const string Default = GroupName + ".SecurityLogs";
    }

    public static class OrganizationUnits
    {
        public const string Default = GroupName + ".OrganizationUnits";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageMembers = Default + ".ManageMembers";
        public const string ManageRoles = Default + ".ManageRoles";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(IdentityPermissions));
    }
}
