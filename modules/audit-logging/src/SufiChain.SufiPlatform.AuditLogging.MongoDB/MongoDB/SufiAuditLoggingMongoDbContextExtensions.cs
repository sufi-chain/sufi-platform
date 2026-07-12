using Volo.Abp;
using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.AuditLogging.MongoDB;

public static class SufiAuditLoggingMongoDbContextExtensions
{
    public static void ConfigureSufiAuditLogging(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<AuditLog>(b =>
        {
            b.CollectionName = SufiAuditLoggingDbProperties.DbTablePrefix + "AuditLogs";
        });

        builder.Entity<AuditLogExcelFile>(b =>
        {
            b.CollectionName = SufiAuditLoggingDbProperties.DbTablePrefix + "AuditLogExcelFiles";
        });
    }
}
