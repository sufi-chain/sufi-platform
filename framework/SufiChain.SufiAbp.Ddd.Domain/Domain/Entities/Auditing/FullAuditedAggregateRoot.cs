namespace SufiChain.SufiAbp.Domain.Entities.Auditing;

public abstract class FullAuditedAggregateRoot<TKey> : Volo.Abp.Domain.Entities.Auditing.FullAuditedAggregateRoot<TKey>
{
    protected FullAuditedAggregateRoot()
    {
    }

    protected FullAuditedAggregateRoot(TKey id)
        : base(id)
    {
    }
}
