using System.Collections.Generic;
using Volo.Abp;

namespace SufiChain.SufiAbp.PermissionManagement;

public class MultiplePermissionValueProviderGrantInfo
{
    public Dictionary<string, PermissionValueProviderGrantInfo> Result { get; set; }

    public MultiplePermissionValueProviderGrantInfo()
    {
        Result = new Dictionary<string, PermissionValueProviderGrantInfo>();
    }

    public MultiplePermissionValueProviderGrantInfo(string[] names, bool isGranted = false)
    {
        Check.NotNull(names, nameof(names));

        Result = new Dictionary<string, PermissionValueProviderGrantInfo>();
        foreach (var name in names)
        {
            Result.Add(name, new PermissionValueProviderGrantInfo(isGranted));
        }
    }
}
