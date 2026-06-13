using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace MyCompanyName.MyProjectName;

public abstract class DemoAppComponentBase : SufiAbpComponentBase
{
    protected DemoAppComponentBase()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
