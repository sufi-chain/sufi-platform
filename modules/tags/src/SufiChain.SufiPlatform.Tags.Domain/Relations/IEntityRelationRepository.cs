using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Tags.Relations;

public interface IEntityRelationRepository : IRepository<EntityRelation, Guid>
{
    Task<EntityRelation?> FindByRequestIdAsync(Guid requestId, Guid? tenantId,
        CancellationToken cancellationToken = default);
}
