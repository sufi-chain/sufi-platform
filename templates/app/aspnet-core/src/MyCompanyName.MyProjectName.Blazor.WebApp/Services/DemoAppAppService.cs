using Volo.Abp.Application.Services;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName.Services;

/// <summary>
/// Inherit your application services from this class.
/// </summary>
public abstract class DemoAppAppService : ApplicationService
{
    protected DemoAppAppService()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
