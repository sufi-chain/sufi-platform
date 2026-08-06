using System;
using System.Collections.Generic;
using System.Text;
using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiPlatform.Application.Services;

namespace MyCompanyName.MyProjectName
{
    /* Inherit your application services from this class.
     */
    public abstract class DemoAppAppService : SufiApplicationService
    {
        protected DemoAppAppService()
        {
            LocalizationResource = typeof(DemoAppResource);
        }
    }
}
