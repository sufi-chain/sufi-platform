using SufiChain.SufiAbp.UI.Blazor;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName;

public abstract class DemoAppComponentBase : SufiAbpComponentBase
{
    protected DemoAppComponentBase()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
