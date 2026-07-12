using System.Collections.Generic;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Permissions;

public class MultipleResourcePermissionValueProviderGrantInfo
{
    public Dictionary<string, ResourcePermissionValueProviderGrantInfo> Result { get; }

    public MultipleResourcePermissionValueProviderGrantInfo()
    {
        Result = new Dictionary<string, ResourcePermissionValueProviderGrantInfo>();
    }

    public MultipleResourcePermissionValueProviderGrantInfo(string[] names)
    {
        Check.NotNull(names, nameof(names));

        Result = new Dictionary<string, ResourcePermissionValueProviderGrantInfo>();

        foreach (var name in names)
        {
            Result.Add(name, ResourcePermissionValueProviderGrantInfo.NonGranted);
        }
    }
}
