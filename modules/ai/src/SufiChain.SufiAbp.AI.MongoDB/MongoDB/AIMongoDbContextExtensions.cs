using Volo.Abp;
using Volo.Abp.MongoDB;
using SufiChain.SufiAbp.AI.Workspaces;

namespace SufiChain.SufiAbp.AI.MongoDB;

public static class AIMongoDbContextExtensions
{
    public static void ConfigureSufiAI(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Workspace>(b =>
        {
            b.CollectionName = AIDbProperties.DbTablePrefix + "Workspaces";
        });
    }
}
