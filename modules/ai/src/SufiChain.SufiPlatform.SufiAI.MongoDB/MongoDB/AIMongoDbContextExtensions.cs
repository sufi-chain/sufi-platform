using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.MongoDB;

public static class AIMongoDbContextExtensions
{
    public static void ConfigureSufiAI(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Workspace>(b =>
        {
            b.CollectionName = SufiAIDbProperties.DbTablePrefix + "Workspaces";
        });
    }
}
