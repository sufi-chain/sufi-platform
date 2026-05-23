using System;
using System.Collections.Generic;
using System.Text;
using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.Application.Services;

namespace MyCompanyName.MyProjectName
{
    /* Inherit your application services from this class.
     */
    public abstract class DemoAppAppService : ApplicationService
    {
        protected DemoAppAppService()
        {
            LocalizationResource = typeof(DemoAppResource);
        }
    }
}
