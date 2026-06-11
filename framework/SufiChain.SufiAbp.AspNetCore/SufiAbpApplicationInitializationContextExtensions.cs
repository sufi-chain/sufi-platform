using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using SufiChain.SufiAbp.Core;

namespace SufiChain.SufiAbp.AspNetCore;

public static class SufiAbpApplicationInitializationContextExtensions
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
