using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp.MultiTenancy;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.MultiTenancy;

/// <summary>
/// Extension methods for registering the tenant switch endpoint middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    private const string SwitchTenantPath = "/Account/SwitchTenant";

    /// <summary>
    /// Registers a lightweight <c>/Account/SwitchTenant</c> endpoint that sets the tenant cookie
    /// and redirects, so Blazor UI tenant switching works without needing an MVC controller.
    /// Place this early in the pipeline (before routing/authorization).
    /// If the host already has an MVC controller handling this route (e.g. <c>SufiAccountController</c>),
    /// this middleware acts as a fallback — the controller will take priority via MVC routing.
    /// </summary>
    public static IApplicationBuilder UseSpTenantSwitch(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals(SwitchTenantPath, StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(context.Request.Method))
            {
                await HandleTenantSwitchAsync(context);
                return; // short-circuit
            }

            await next(context);
        });
    }

    private static async Task HandleTenantSwitchAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SufiChain.SufiPlatform.UI.TenantSwitch");

        var options = context.RequestServices.GetRequiredService<IOptions<TenantSwitchOptions>>().Value;
        var cookieName = options.TenantCookieName;

        var tenantId = context.Request.Query["tenantId"].FirstOrDefault();
        var tenantName = context.Request.Query["tenantName"].FirstOrDefault();
        var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault() ?? "/";

        // Validate returnUrl is local (starts with "/" but not "//")
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//"))
        {
            returnUrl = "/";
        }

        if (!string.IsNullOrEmpty(cookieName))
        {
            string value;

            if (!string.IsNullOrEmpty(tenantId))
            {
                // Tenant ID provided directly (GUID) — use as-is
                value = tenantId;
            }
            else if (!string.IsNullOrEmpty(tenantName))
            {
                value = await ResolveTenantIdAsync(context, tenantName, logger) ?? tenantName;
            }
            else
            {
                // No tenant — clear cookie (switch to host)
                value = string.Empty;
            }

            logger.LogInformation(
                "SwitchTenant: setting cookie '{CookieName}' = '{Value}', redirecting to '{ReturnUrl}'",
                cookieName, value, returnUrl);

            context.Response.Cookies.Append(cookieName, value, new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = false,
                Secure = context.Request.IsHttps
            });
        }
        else
        {
            logger.LogWarning("SwitchTenant: TenantCookieName is not configured; no cookie set.");
        }

        context.Response.Redirect(returnUrl);
    }

    /// <summary>
    /// Resolves a tenant name to a GUID string via the Sufi tenant lookup service.
    /// Returns <c>null</c> if the store is not available or the tenant is not found.
    /// </summary>
    private static async Task<string?> ResolveTenantIdAsync(
        HttpContext context,
        string tenantName,
        ILogger logger)
    {
        var tenantStore = context.RequestServices.GetService<ITenantStore>();
        if (tenantStore != null)
        {
            try
            {
                var normalizedName = tenantName.ToUpperInvariant();
                var tenantConfig = await tenantStore.FindAsync(normalizedName);
                if (tenantConfig?.Id != null)
                {
                    logger.LogInformation(
                        "SwitchTenant: resolved tenant name '{TenantName}' → ID '{TenantId}' via ITenantStore",
                        tenantName, tenantConfig.Id);
                    return tenantConfig.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "SwitchTenant: error resolving tenant name '{TenantName}' via ITenantStore.",
                    tenantName);
            }
        }

        var tenantLookupService = context.RequestServices.GetService<ITenantLookupService>();
        if (tenantLookupService == null)
        {
            logger.LogDebug(
                "SwitchTenant: ITenantLookupService is not available; storing tenant name as-is. " +
                "Ensure a tenant-management module is registered for tenant lookup.");
            return null;
        }

        try
        {
            var tenants = await tenantLookupService.GetListAsync(tenantName, 0, 10);
            var tenant = tenants.Items.FirstOrDefault(item =>
                string.Equals(item.Name, tenantName, StringComparison.OrdinalIgnoreCase));

            if (tenant != null)
            {
                logger.LogInformation(
                    "SwitchTenant: resolved tenant name '{TenantName}' → ID '{TenantId}'",
                    tenantName, tenant.Id);
                return tenant.Id.ToString();
            }

            logger.LogWarning(
                "SwitchTenant: tenant '{TenantName}' not found; storing name as-is.",
                tenantName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "SwitchTenant: error resolving tenant name '{TenantName}' via ITenantLookupService; storing name as-is.",
                tenantName);
            return null;
        }
    }
}
