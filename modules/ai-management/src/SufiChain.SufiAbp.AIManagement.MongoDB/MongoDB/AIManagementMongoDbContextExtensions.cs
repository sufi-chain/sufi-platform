using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.MongoDB;

public static class AIManagementMongoDbContextExtensions
{
    public static void ConfigureSufiAbpAIManagement(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Workspace>(b =>
        {
            b.CollectionName = AIManagementDbProperties.DbTablePrefix + "Workspaces";
        });
    }
}
