using Volo.Abp.MongoDB;
using Volo.Abp;

namespace SufiChain.SufiPlatform.AuditLogging.MongoDB;

public static class SufiAuditLoggingMongoDbContextExtensions
{
    public static void ConfigureAuditLogging(
        this IMongoModelBuilder builder)
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
