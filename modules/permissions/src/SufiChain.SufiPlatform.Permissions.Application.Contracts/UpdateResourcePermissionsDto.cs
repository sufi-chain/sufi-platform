using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class UpdateResourcePermissionsDto
{
    public string ProviderName { get; set; }

    public string ProviderKey { get; set; }

    public List<string> Permissions { get; set; }
}
