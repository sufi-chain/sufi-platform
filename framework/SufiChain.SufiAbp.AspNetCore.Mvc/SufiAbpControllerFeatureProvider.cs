using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.AspNetCore.Mvc;

public class SufiAbpControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private static readonly HashSet<string> ReplacedAbpControllerNames = new(StringComparer.Ordinal)
    {
        "Volo.Abp.AspNetCore.Mvc.ApiExploring.AbpApiDefinitionController",
        "Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations.AbpApplicationConfigurationController",
        "Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations.AbpApplicationLocalizationController"
    };

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        for (var i = feature.Controllers.Count - 1; i >= 0; i--)
        {
            var controllerType = feature.Controllers[i];
            if (controllerType.FullName != null && ReplacedAbpControllerNames.Contains(controllerType.FullName))
            {
                feature.Controllers.RemoveAt(i);
            }
        }
    }
}
