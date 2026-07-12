using Microsoft.AspNetCore.Authentication;
using OpenIddict.Validation.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderSufiOpenIddictMiddlewareExtension
{
    public static IApplicationBuilder UseAbpOpenIddictValidation(this IApplicationBuilder app, string scheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
    {
        return app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                var result = await context.AuthenticateAsync(scheme);
                if (result.Succeeded && result.Principal != null)
                {
                    context.User = result.Principal;
                }
            }

            await next();
        });
    }
}
