using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionGroupDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }
    
    public string DisplayNameKey { get; set; }
    
    public string DisplayNameResource { get; set; }

    public List<PermissionGrantInfoDto> Permissions { get; set; }
}
