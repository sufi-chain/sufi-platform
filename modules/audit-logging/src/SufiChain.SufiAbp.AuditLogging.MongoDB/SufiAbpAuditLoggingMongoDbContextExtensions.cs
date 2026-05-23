using Volo.Abp.MongoDB;
using Volo.Abp;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

public static class SufiAbpAuditLoggingMongoDbContextExtensions
{
    public static void ConfigureAuditLogging(
        this IMongoModelBuilder builder)
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
