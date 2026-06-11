namespace SufiChain.SufiAbp.Caching;

public interface IDistributedCache<TCacheItem> : Volo.Abp.Caching.IDistributedCache<TCacheItem>
    where TCacheItem : class
{
}
