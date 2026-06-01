namespace SufiChain.Chat.Permissions;

public static class ChatPermissions
{
    public const string GroupName = "Chat";

    public static class Sessions
    {
        public const string Default = GroupName + ".Sessions";
        public const string Create = Default + ".Create";
        public const string Close = Default + ".Close";
        public const string Manage = Default + ".Manage";
    }

    public static class Messages
    {
        public const string Default = GroupName + ".Messages";
        public const string Send = Default + ".Send";
        public const string SendInternal = Default + ".SendInternal";
        public const string Delete = Default + ".Delete";
        public const string ViewInternal = Default + ".ViewInternal";
    }

    public static class Inbox
    {
        public const string Default = GroupName + ".Inbox";
        public const string User = Default + ".User";
        public const string Operator = Default + ".Operator";
        public const string Admin = Default + ".Admin";
        public const string Reply = Default + ".Reply";
        public const string Manage = Default + ".Manage";
    }

    public static class Usage
    {
        public const string Default = GroupName + ".Usage";
        public const string View = Default + ".View";
        public const string ManagePolicies = Default + ".ManagePolicies";
    }

    public static class AiUsage
    {
        public const string Default = GroupName + ".AiUsage";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
        public const string Manage = Default + ".Manage";
    }

    public static class Links
    {
        public const string Default = GroupName + ".Links";
        public const string Manage = Default + ".Manage";
    }
}
