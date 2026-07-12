using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class GetPermissionListResultDto
{
    public string EntityDisplayName { get; set; }

    public List<PermissionGroupDto> Groups { get; set; }
}
