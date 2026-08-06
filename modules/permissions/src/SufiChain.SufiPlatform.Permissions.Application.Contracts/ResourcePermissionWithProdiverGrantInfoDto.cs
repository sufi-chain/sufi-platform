using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class ResourcePermissionWithProdiverGrantInfoDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public List<string> Providers { get; set; }

    public bool IsGranted { get; set; }
}
