namespace SufiChain.SufiPlatform.AuditLogging.Hooshvare;

public static class AuditInvestigatorHooshvareKeys
{
    public const string Key = "SufiAudit:Investigator";
    public const string LocalizationResourceName = "SufiAuditLogging";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string SearchLogs = "audit.search_logs";
        public const string GetLog = "audit.get_log";
        public const string SearchEntityChanges = "audit.search_entity_changes";
        public const string GetEntityChange = "audit.get_entity_change";
    }

    public static class Context
    {
        public const string AuditLogId = "auditLogId";
        public const string EntityType = "entityType";
        public const string EntityId = "entityId";
        public const string UserId = "userId";
        public const string FromUtc = "fromUtc";
        public const string ToUtc = "toUtc";
    }

    public static class Shortcuts
    {
        public const string RecentFailures = "RecentFailures";
        public const string ExplainSelection = "ExplainSelection";
    }
}
