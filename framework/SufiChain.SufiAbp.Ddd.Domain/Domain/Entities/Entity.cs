namespace SufiChain.SufiAbp.Domain.Entities;

public abstract class Entity<TKey> : Volo.Abp.Domain.Entities.Entity<TKey>
{
    protected Entity()
    {
    }

    protected Entity(TKey id)
        : base(id)
    {
    }
}
