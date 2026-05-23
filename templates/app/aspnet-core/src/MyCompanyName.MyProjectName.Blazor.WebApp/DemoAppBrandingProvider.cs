using Microsoft.Extensions.Localization;
using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace MyCompanyName.MyProjectName;

[Dependency(ReplaceServices = true)]
public class DemoAppBrandingProvider : DefaultBrandingProvider
{
    private readonly IStringLocalizer<DemoAppResource> _localizer;

    public DemoAppBrandingProvider(IStringLocalizer<DemoAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
