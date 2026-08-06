using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Caching;

public interface IDistributedCache<TCacheItem> : Volo.Abp.Caching.IDistributedCache<TCacheItem>
    where TCacheItem : class
{
}
