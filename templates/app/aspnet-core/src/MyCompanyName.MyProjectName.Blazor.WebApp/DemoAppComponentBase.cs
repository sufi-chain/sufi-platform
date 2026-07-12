using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace MyCompanyName.MyProjectName;

public abstract class DemoAppComponentBase : SufiComponentBase
{
    protected DemoAppComponentBase()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
