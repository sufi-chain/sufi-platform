using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.Repositories;

public class EfCoreEntityRelationRepository : EfCoreRepository<ITagsDbContext, EntityRelation, Guid>, IEntityRelationRepository
{
    public EfCoreEntityRelationRepository(IDbContextProvider<ITagsDbContext> provider) : base(provider) { }

    public override async Task<EntityRelation> UpdateAsync(EntityRelation entity, bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        var context = (DbContext)await GetDbContextAsync();
        if (context.Entry(entity).State != EntityState.Detached)
            return await base.UpdateAsync(entity, autoSave, cancellationToken);

        var head = await (await GetDbSetAsync()).Where(x => x.Id == entity.Id)
            .Select(x => new { x.Version, x.ConcurrencyStamp }).SingleOrDefaultAsync(cancellationToken);
        if (head == null || head.ConcurrencyStamp != entity.ConcurrencyStamp || entity.Version < head.Version)
            throw new AbpDbConcurrencyException("The relation changed before this revision was saved.");

        // Owned revisions have explicit sequence keys. Attaching a detached graph
        // otherwise treats newly appended snapshots as existing unchanged rows.
        context.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
        foreach (var revision in entity.Revisions)
            context.Entry(revision).State = revision.Sequence > head.Version ? EntityState.Added : EntityState.Unchanged;
        if (autoSave)
            await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<EntityRelation?> FindByRequestIdAsync(Guid requestId, Guid? tenantId,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Revisions.Any(revision => revision.RequestId == requestId),
            cancellationToken);
}
