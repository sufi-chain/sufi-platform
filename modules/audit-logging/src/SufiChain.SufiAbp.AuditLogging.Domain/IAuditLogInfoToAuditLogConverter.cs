using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.AuditLogging;

public interface IAuditLogInfoToAuditLogConverter
{
    Task<AuditLog> ConvertAsync(AuditLogInfo auditLogInfo);
}
