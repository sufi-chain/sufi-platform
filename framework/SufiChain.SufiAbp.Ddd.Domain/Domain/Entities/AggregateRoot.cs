namespace SufiChain.SufiAbp.Domain.Entities;

public abstract class AggregateRoot<TKey> : Volo.Abp.Domain.Entities.AggregateRoot<TKey>
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TKey id)
        : base(id)
    {
    }
}
