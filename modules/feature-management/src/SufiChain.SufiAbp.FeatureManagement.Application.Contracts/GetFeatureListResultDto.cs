using System.Collections.Generic;

namespace SufiChain.SufiAbp.FeatureManagement;

public class GetFeatureListResultDto
{
    public List<FeatureGroupDto> Groups { get; set; }
}
