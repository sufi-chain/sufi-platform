using System.Collections.Generic;

namespace SufiChain.SufiAbp.PermissionManagement;

public class GetResourcePermissionWithProviderListResultDto
{
    public List<ResourcePermissionWithProdiverGrantInfoDto> Permissions { get; set; }
}
