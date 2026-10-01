namespace SufiChain.SufiPlatform.Caching;

public interface ICacheCatalog
{
    IReadOnlyList<CacheCatalogItem> GetAll();

    bool Contains(string cacheName);
}
