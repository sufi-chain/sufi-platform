using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.AuditLogging;

public interface IAuditLogInfoToAuditLogConverter
{
    Task<AuditLog> ConvertAsync(AuditLogInfo auditLogInfo);
}
