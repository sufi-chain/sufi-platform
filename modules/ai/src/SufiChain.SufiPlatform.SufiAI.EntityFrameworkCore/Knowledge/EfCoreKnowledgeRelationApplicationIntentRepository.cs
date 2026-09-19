using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

public class EfCoreKnowledgeRelationApplicationIntentRepository :
    EfCoreRepository<IAIDbContext, KnowledgeRelationApplicationIntent, Guid>, IKnowledgeRelationApplicationIntentRepository
{
    public EfCoreKnowledgeRelationApplicationIntentRepository(IDbContextProvider<IAIDbContext> provider) : base(provider) { }
}
