using JetBrains.Annotations;
using Volo.Abp;

namespace SufiChain.SufiAbp.PermissionManagement;

public class PermissionValueProviderInfo
{
    public string Name { get; }

    public string Key { get; }

    public PermissionValueProviderInfo([NotNull] string name, [NotNull] string key)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(key, nameof(key));

        Name = name;
        Key = key;
    }
}
