using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.AuditLogging;

public static class SufiAuditLoggingDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiAuditLogging.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAuditLogging";
}
