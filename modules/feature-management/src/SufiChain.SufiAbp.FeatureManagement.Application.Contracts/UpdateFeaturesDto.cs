using System.Collections.Generic;

namespace SufiChain.SufiAbp.FeatureManagement;

public class UpdateFeaturesDto
{
    public List<UpdateFeatureDto> Features { get; set; }
}
