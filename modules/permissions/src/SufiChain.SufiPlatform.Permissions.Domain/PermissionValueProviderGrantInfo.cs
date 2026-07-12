using JetBrains.Annotations;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionValueProviderGrantInfo
{
    public virtual bool IsGranted { get; }

    public virtual string ProviderKey { get; }

    public PermissionValueProviderGrantInfo(bool isGranted, [CanBeNull] string providerKey = null)
    {
        IsGranted = isGranted;
        ProviderKey = providerKey;
    }
}
