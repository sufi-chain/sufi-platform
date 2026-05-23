using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Aspects;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace SufiChain.SufiAbp.AspNetCore.Auditing;

/// <summary>
/// Fixes audit logging for Blazor Server and other non-HTTP app service calls.
/// ABP's AuditingHelper.ShouldSaveAudit uses method.DeclaringType, which for interface methods
/// returns the interface (e.g. ILocalizationTextAppService). Interfaces don't implement
/// IAuditingEnabled — only the implementation class does — so ShouldSaveAudit returns false
/// and no audit logs are created. This interceptor also checks the target implementation type.
/// </summary>
public class SufiAbpAuditingInterceptor : AuditingInterceptor, ITransientDependency
{
    public SufiAbpAuditingInterceptor(IServiceScopeFactory serviceScopeFactory)
        : base(serviceScopeFactory)
    {
    }

    protected override bool ShouldIntercept(
        IAbpMethodInvocation invocation,
        AbpAuditingOptions options,
        IAuditingHelper auditingHelper)
    {
        if (base.ShouldIntercept(invocation, options, auditingHelper))
        {
            return true;
        }

        // Base returned false — often because method.DeclaringType is the interface.
        // Also check the target implementation type (e.g. LocalizationTextAppService).
        var targetType = invocation.TargetObject?.GetType();
        if (targetType == null)
        {
            return false;
        }

        if (AuditingInterceptorRegistrar.ShouldAuditTypeByDefaultOrNull(
                targetType,
                ignoreIntegrationServiceAttribute: options.IsEnabledForIntegrationServices) == true)
        {
            return true;
        }

        return false;
    }
}
