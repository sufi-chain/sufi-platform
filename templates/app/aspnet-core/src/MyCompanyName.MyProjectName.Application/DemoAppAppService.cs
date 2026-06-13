using System;
using System.Collections.Generic;
using System.Text;
using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiAbp.Application.Services;

namespace MyCompanyName.MyProjectName
{
    /* Inherit your application services from this class.
     */
    public abstract class DemoAppAppService : SufiAbpApplicationService
    {
        protected DemoAppAppService()
        {
            LocalizationResource = typeof(DemoAppResource);
        }
    }
}
