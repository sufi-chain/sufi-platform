using System.Reflection;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Caching;

/// <summary>
/// Cache item types already loaded from Sufi Platform and ABP. The backoffice panel lists this catalog.
/// </summary>
public class CacheCatalog : ICacheCatalog, ISingletonDependency
{
    private readonly Lazy<IReadOnlyList<CacheCatalogItem>> _items = new(Discover);

    public IReadOnlyList<CacheCatalogItem> GetAll()
    {
        return _items.Value;
    }

    public bool Contains(string cacheName)
    {
        return _items.Value.Any(item => string.Equals(item.CacheName, cacheName, StringComparison.Ordinal));
    }

    private static IReadOnlyList<CacheCatalogItem> Discover()
    {
        var items = new List<CacheCatalogItem>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (assembly.IsDynamic ||
                name == null ||
                (!name.StartsWith("SufiChain.", StringComparison.Ordinal) &&
                 !name.StartsWith("Volo.Abp.", StringComparison.Ordinal)))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (type is not { IsClass: true, IsAbstract: false })
                {
                    continue;
                }

                var named = type.GetCustomAttributes(typeof(CacheNameAttribute), inherit: true).Length > 0 ||
                            type.Name.EndsWith("CacheItem", StringComparison.Ordinal);
                if (!named || string.IsNullOrWhiteSpace(type.FullName))
                {
                    continue;
                }

                items.Add(new CacheCatalogItem
                {
                    CacheName = CacheNameAttribute.GetCacheName(type),
                    ItemType = type.FullName
                });
            }
        }

        return items
            .GroupBy(item => item.CacheName, StringComparer.Ordinal)
            .Select(group => new CacheCatalogItem
            {
                CacheName = group.Key,
                ItemType = string.Join(", ", group.Select(item => item.ItemType).Distinct(StringComparer.Ordinal).OrderBy(type => type, StringComparer.Ordinal))
            })
            .OrderBy(item => item.CacheName, StringComparer.Ordinal)
            .ToList();
    }
}
