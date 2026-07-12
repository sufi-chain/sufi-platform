using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.Auditing;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.AspNetCore.Auditing;

/// <summary>
/// Extends ABP's auditing middleware with configurable HTTP method blacklist/whitelist.
/// By default, GET and HEAD are blacklisted. Hosts can configure
/// <see cref="SufiAuditingHttpMethodFilterOptions"/> to customize.
/// </summary>
public class SufiAuditingMiddleware : AbpAuditingMiddleware, ITransientDependency
{
    private readonly IAuditingManager _auditingManager;
    private readonly SufiAuditingHttpMethodFilterOptions _methodFilterOptions;

    public SufiAuditingMiddleware(
        IAuditingManager auditingManager,
        ICurrentUser currentUser,
        IOptions<Volo.Abp.Auditing.AbpAuditingOptions> auditingOptions,
        IOptions<AbpAspNetCoreAuditingOptions> aspNetCoreAuditingOptions,
        IOptions<SufiAuditingHttpMethodFilterOptions> methodFilterOptions,
        IUnitOfWorkManager unitOfWorkManager)
        : base(auditingManager, currentUser, auditingOptions, aspNetCoreAuditingOptions, unitOfWorkManager)
    {
        _auditingManager = auditingManager;
        _methodFilterOptions = methodFilterOptions.Value;
    }

    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (await ShouldSkipAsync(context, next) || !AuditingOptions.IsEnabled || IsIgnoredUrlInternal(context))
        {
            await next(context);
            return;
        }

        var hasError = false;
        using (var saveHandle = _auditingManager.BeginScope())
        {
            Debug.Assert(_auditingManager.Current != null);

            try
            {
                await next(context);

                if (_auditingManager.Current.Log.Exceptions.Any())
                {
                    hasError = true;
                }
            }
            catch (Exception ex)
            {
                hasError = true;

                if (!_auditingManager.Current.Log.Exceptions.Contains(ex))
                {
                    _auditingManager.Current.Log.Exceptions.Add(ex);
                }

                throw;
            }
            finally
            {
                if (await ShouldWriteAuditLogAsync(_auditingManager.Current!.Log, context, hasError))
                {
                    if (UnitOfWorkManager.Current != null)
                    {
                        try
                        {
                            await UnitOfWorkManager.Current.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            if (!_auditingManager.Current.Log.Exceptions.Contains(ex))
                            {
                                _auditingManager.Current.Log.Exceptions.Add(ex);
                            }
                        }
                    }

                    await saveHandle.SaveAsync();
                }
            }
        }
    }

    private async Task<bool> ShouldWriteAuditLogAsync(AuditLogInfo auditLogInfo, HttpContext httpContext, bool hasError)
    {
        var method = httpContext.Request.Method;

        // Whitelist: when set, only these methods are audited
        if (_methodFilterOptions.WhitelistedHttpMethods is { Count: > 0 } whitelist)
        {
            if (!whitelist.Any(m => string.Equals(m, method, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }
        else
        {
            // Blacklist: exclude these methods
            if (_methodFilterOptions.BlacklistedHttpMethods.Any(m =>
                    string.Equals(m, method, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return await BaseShouldWriteAuditLogAsync(auditLogInfo, httpContext, hasError);
    }

    private async Task<bool> BaseShouldWriteAuditLogAsync(AuditLogInfo auditLogInfo, HttpContext httpContext, bool hasError)
    {
        foreach (var selector in AuditingOptions.AlwaysLogSelectors)
        {
            if (await selector(auditLogInfo))
            {
                return true;
            }
        }

        if (AuditingOptions.AlwaysLogOnException && hasError)
        {
            return true;
        }

        if (!AuditingOptions.IsEnabledForAnonymousUsers && !CurrentUser.IsAuthenticated)
        {
            return false;
        }

        if (!AuditingOptions.IsEnabledForGetRequests &&
            (string.Equals(httpContext.Request.Method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(httpContext.Request.Method, HttpMethods.Head, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private bool IsIgnoredUrlInternal(HttpContext context)
    {
        if (context.Request.Path.Value == null)
        {
            return false;
        }

        if (!AuditingOptions.IsEnabledForIntegrationServices &&
            context.Request.Path.Value.StartsWith($"/{Volo.Abp.AspNetCore.AbpAspNetCoreConsts.DefaultIntegrationServiceApiPrefix}/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (AspNetCoreAuditingOptions.IgnoredUrls.Any(x => context.Request.Path.Value.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }
}
