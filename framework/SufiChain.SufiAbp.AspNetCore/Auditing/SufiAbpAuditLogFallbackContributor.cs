using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AspNetCore.Auditing;

/// <summary>
/// Fallback contributor that populates HttpMethod, Url, and ClientIpAddress when they remain
/// empty after the built-in contributors. This occurs for direct in-process app service calls
/// (e.g., from Blazor Server components) where there is no HTTP request context.
/// </summary>
public class SufiAbpAuditLogFallbackContributor : AuditLogContributor, ITransientDependency
{
    public override void PostContribute(AuditLogContributionContext context)
    {
        var auditInfo = context.AuditInfo;

        // Only fill when values are still empty
        var needsUrl = string.IsNullOrEmpty(auditInfo.Url);
        var needsMethod = string.IsNullOrEmpty(auditInfo.HttpMethod);
        var needsIp = string.IsNullOrEmpty(auditInfo.ClientIpAddress);

        if (!needsUrl && !needsMethod && !needsIp)
        {
            return;
        }

        var firstAction = auditInfo.Actions.FirstOrDefault();

        if (needsUrl && firstAction != null)
        {
            auditInfo.Url = firstAction.ServiceName + "." + firstAction.MethodName;
        }

        if (needsMethod && firstAction != null)
        {
            auditInfo.HttpMethod = InferHttpMethod(firstAction.MethodName);
        }

        if (needsIp)
        {
            var clientInfoProvider = context.ServiceProvider.GetRequiredService<IWebClientInfoProvider>();
            var ip = clientInfoProvider.ClientIpAddress;
            if (!string.IsNullOrEmpty(ip))
            {
                auditInfo.ClientIpAddress = ip;
            }
        }
    }

    private static string InferHttpMethod(string methodName)
    {
        if (string.IsNullOrEmpty(methodName))
        {
            return "Invoke";
        }

        var name = methodName.ToUpperInvariant();
        if (name.StartsWith("DELETE"))
            return "DELETE";
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
