using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class GetResourcePermissionListResultDto
{
    public List<ResourcePermissionGrantInfoDto> Permissions { get; set; }
}
