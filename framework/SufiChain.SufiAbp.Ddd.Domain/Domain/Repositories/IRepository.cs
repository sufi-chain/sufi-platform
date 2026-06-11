namespace SufiChain.SufiAbp.Domain.Repositories;

public interface IRepository<TEntity, TKey> : Volo.Abp.Domain.Repositories.IRepository<TEntity, TKey>
    where TEntity : class, Volo.Abp.Domain.Entities.IEntity<TKey>
{
}
