using Volo.Abp.Data;

namespace SufiChain.SufiAbp.AuditLogging;

public static class SufiAbpAuditLoggingDbProperties
{
    public static string DbTablePrefix { get; set; } = "AuditLogging.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpAuditLogging";
}
