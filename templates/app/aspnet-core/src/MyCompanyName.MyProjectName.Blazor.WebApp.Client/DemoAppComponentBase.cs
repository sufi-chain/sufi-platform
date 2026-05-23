using SufiChain.SufiAbp.UI.Blazor;
using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.AspNetCore.Components;

namespace MyCompanyName.MyProjectName;

public abstract class DemoAppComponentBase : SufiAbpComponentBase
{
    protected DemoAppComponentBase()
    {
        LocalizationResource = typeof(DemoAppResource);
    }
}
