using Volo.Abp;
using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

public static class SufiAbpAuditLoggingMongoDbContextExtensions
{
    public static void ConfigureSufiAbpAuditLogging(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<AuditLog>(b =>
        {
            b.CollectionName = SufiAbpAuditLoggingDbProperties.DbTablePrefix + "AuditLogs";
        });

        builder.Entity<AuditLogExcelFile>(b =>
        {
            b.CollectionName = SufiAbpAuditLoggingDbProperties.DbTablePrefix + "AuditLogExcelFiles";
        });
    }
}
