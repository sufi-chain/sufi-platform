using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.AspNetCore.Auditing;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Auditing;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AspNetCore;

[DependsOn(typeof(AbpAspNetCoreModule))]
public class SufiAbpAspNetCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Replace default AuditingInterceptor so app service calls are audited even when
        // invoked via interface (method.DeclaringType would be the interface, which doesn't
        // implement IAuditingEnabled — SufiAbpAuditingInterceptor also checks the target type).
        context.Services.Replace(
            ServiceDescriptor.Transient<AuditingInterceptor, SufiAbpAuditingInterceptor>());

        context.Services.Replace(
            ServiceDescriptor.Transient<Volo.Abp.AspNetCore.Auditing.AbpAuditingMiddleware, SufiAbpAuditingMiddleware>());

        // HTTP method filter: default blacklist GET/HEAD. Hosts can override via Configure<SufiAbpAuditingHttpMethodFilterOptions>.
        Configure<SufiAbpAuditingHttpMethodFilterOptions>(_ => { });

        // Exclude Blazor Server SignalR circuit from auditing to avoid OperationCanceledException
        // when the circuit is torn down during tenant switch (forceLoad) or page navigation.
        Configure<AbpAspNetCoreAuditingOptions>(options =>
        {
            options.IgnoredUrls.AddIfNotContains("/_blazor");
        });

        Configure<AbpAuditingOptions>(options =>
        {
            options.Contributors.Add(new SufiAbpAuditLogFallbackContributor());

            // Force logging of Blazor Server direct in-process calls (e.g. delete, copy, move),
            // but only when the action is NOT a blacklisted HTTP method (e.g. GET).
            // Otherwise we'd bypass the GET filter and log read operations.
            var methodFilter = new System.Lazy<SufiAbpAuditingHttpMethodFilterOptions>(() =>
                context.Services.BuildServiceProvider().GetRequiredService<IOptions<SufiAbpAuditingHttpMethodFilterOptions>>().Value);
            options.AlwaysLogSelectors.Add(auditInfo =>
            {
                if (auditInfo.Actions?.Count == 0)
                    return Task.FromResult(false);
                var hasAuditableAction = auditInfo.Actions!.Any(a =>
                    a.ServiceName != null &&
                    (a.ServiceName.EndsWith("AppService") || a.ServiceName.EndsWith("Controller")));
                if (!hasAuditableAction)
                    return Task.FromResult(false);
                // Respect blacklist: don't force-save when the effective method is blacklisted
                var filter = methodFilter.Value;
                var effectiveMethod = InferHttpMethodFromActions(auditInfo) ?? auditInfo.HttpMethod ?? "Invoke";
                if (filter.WhitelistedHttpMethods is { Count: > 0 } whitelist)
                {
                    if (!whitelist.Any(m => string.Equals(m, effectiveMethod, StringComparison.OrdinalIgnoreCase)))
                        return Task.FromResult(false);
                }
                else if (filter.BlacklistedHttpMethods.Any(m =>
                    string.Equals(m, effectiveMethod, StringComparison.OrdinalIgnoreCase)))
                {
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            });
        });
    }

    private static string? InferHttpMethodFromActions(Volo.Abp.Auditing.AuditLogInfo auditInfo)
    {
        var first = auditInfo.Actions?.FirstOrDefault();
        if (first?.MethodName == null) return null;
        var name = first.MethodName.ToUpperInvariant();
        if (name.StartsWith("DELETE")) return "DELETE";
        if (name.StartsWith("CREATE") || name.StartsWith("ADD") || name.StartsWith("UPDATE") ||
            name.StartsWith("REMOVE") || name.StartsWith("PASTE") || name.StartsWith("CUT") ||
            name.StartsWith("COPY") || name.StartsWith("MOVE") || name.StartsWith("UPLOAD"))
            return "POST";
        if (name.StartsWith("GET") || name.StartsWith("FIND") || name.StartsWith("LIST") ||
            name.StartsWith("SEARCH") || name.StartsWith("DOWNLOAD"))
            return "GET";
        return "Invoke";
    }
}
