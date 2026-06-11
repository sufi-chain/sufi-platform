namespace SufiChain.SufiAbp.Caching;

public class CacheNameAttribute : Volo.Abp.Caching.CacheNameAttribute
{
    public CacheNameAttribute(string name)
        : base(name)
    {
    }
}
