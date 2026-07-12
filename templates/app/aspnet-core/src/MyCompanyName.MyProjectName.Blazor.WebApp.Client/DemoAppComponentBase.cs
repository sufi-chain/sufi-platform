using SufiChain.SufiPlatform.UI.Blazor;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName;

public abstract class DemoAppComponentBase : SufiComponentBase
{
    protected DemoAppComponentBase()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
