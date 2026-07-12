using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using SufiChain.SufiPlatform.Core;

namespace SufiChain.SufiPlatform.AspNetCore;

public static class SufiApplicationInitializationContextExtensions
{
    public static IApplicationBuilder GetApplicationBuilder(this ApplicationInitializationContext context)
    {
        return Volo.Abp.ApplicationInitializationContextExtensions.GetApplicationBuilder(
            context.AsAbpContext());
    }

    public static IWebHostEnvironment GetEnvironment(this ApplicationInitializationContext context)
    {
        return Volo.Abp.ApplicationInitializationContextExtensions.GetEnvironment(
            context.AsAbpContext());
    }
}
