using SufiChain.SufiPlatform.SufiAI.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

public class MongoKnowledgeRelationApplicationIntentRepository :
    MongoDbRepository<AIMongoDbContext, KnowledgeRelationApplicationIntent, Guid>, IKnowledgeRelationApplicationIntentRepository
{
    public MongoKnowledgeRelationApplicationIntentRepository(IMongoDbContextProvider<AIMongoDbContext> provider) : base(provider) { }
}
