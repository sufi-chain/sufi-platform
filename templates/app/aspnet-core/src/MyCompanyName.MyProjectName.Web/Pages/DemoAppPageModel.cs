using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace MyCompanyName.MyProjectName.Web.Pages
{
    public abstract class DemoAppPageModel : AbpPageModel
    {
        protected DemoAppPageModel()
        {
            LocalizationResourceType = typeof(DemoAppResource);
        }
    }
}