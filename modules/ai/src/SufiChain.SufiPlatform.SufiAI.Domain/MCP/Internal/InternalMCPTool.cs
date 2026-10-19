using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>
/// Wrapper for internal ApplicationService methods marked with [SufiAiMcpTool].
/// This is the tool-dispatch point for internal tools (sufi-chain#384): every call needs an
/// <see cref="AgentCallContext"/>, runs in a new DI scope under that tenant and principal, and is
/// permission-checked against the bound user before the method runs.
/// </summary>
public class InternalMCPTool : IMCPTool, IMCPToolAccessRequirement, IMCPToolEffect
{
    private readonly Type _serviceType;
    private readonly MethodInfo _method;
    private readonly IServiceProvider _serviceProvider;
    private readonly MethodParameterBinder _parameterBinder;
    private readonly string? _permissionName;
    private readonly bool _requiresPermission;
    private readonly bool _requiresAuthentication;
    private readonly bool _allowsAnonymous;
    private readonly bool _isReadOnly;
    public string Name { get; }
    public string Description { get; }
    public string ParameterSchema { get; }
    public MCPToolType ToolType => MCPToolType.Internal;
    public string Source { get; }
    
    public InternalMCPTool(
        string name,
        string description,
        string parameterSchema,
        Type serviceType,
        MethodInfo method,
        IServiceProvider serviceProvider,
        string? permissionName = null,
        bool requiresPermission = false,
        bool requiresAuthentication = false,
        bool allowsAnonymous = false,
        bool isReadOnly = false)
    {
        Name = name;
        Description = description;
        ParameterSchema = parameterSchema;
        _serviceType = serviceType;
        _method = method;
        _serviceProvider = serviceProvider;
        _parameterBinder = new MethodParameterBinder();
        Source = serviceType.Name;
        _permissionName = string.IsNullOrWhiteSpace(permissionName) ? null : permissionName;
        _requiresPermission = requiresPermission || _permissionName is not null;
        _requiresAuthentication = requiresAuthentication;
        _allowsAnonymous = allowsAnonymous;
        _isReadOnly = isReadOnly;
    }

    /// <summary>True only when the method is explicitly marked <c>SufiAiMcpTool.ReadOnly</c> (sufi-chain#384 slice 5).</summary>
    public bool IsReadOnly => _isReadOnly;

    /// <summary>Permission the bound user must hold, from <c>SufiAiMcpToolAttribute.PermissionName</c>.</summary>
    public string? PermissionName => _permissionName;

    /// <summary>True when the tool declares a permission requirement.</summary>
    public bool RequiresPermission => _requiresPermission;
    /// <summary>True when the tool needs a signed-in user (service or method <c>[Authorize]</c>).</summary>
    public bool RequiresAuthentication => _requiresAuthentication;
    /// <summary>True when anonymous turns may run this tool (sufi-chain#384 slice 4).</summary>
    public bool AllowsAnonymous => _allowsAnonymous;
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var refusal = AgentCallBinding.Validate(context);
        if (refusal is not null)
        {
            return MCPToolExecutionResult.CreateFailure(refusal);
        }

        var call = context.CallContext!;
        if (call.IsAnonymous && !_allowsAnonymous)
        {
            // sufi-chain#384 slice 4: anonymous turns run only allowlisted tools.
            return MCPToolExecutionResult.CreateFailure(AgentCallErrorCodes.AnonymousNotAllowed);
        }

        if (call.AgentScope == AgentToolScope.ReadOnly && !_isReadOnly)
        {
            // sufi-chain#384 slice 5: a read-only agent can't run a tool not explicitly marked read-only.
            return MCPToolExecutionResult.CreateFailure(AgentCallErrorCodes.AgentScopeReadOnly);
        }
        using var scope = _serviceProvider.CreateScope();
        var scoped = scope.ServiceProvider;
        var currentTenant = scoped.GetService<ICurrentTenant>();
        var principalAccessor = scoped.GetService<ICurrentPrincipalAccessor>();
        if ((currentTenant is null || principalAccessor is null)
            && (!call.IsAnonymous || call.TenantId is not null || _requiresPermission || _requiresAuthentication))
        {
            // Without ABP identity services the call can't be bound, so fail closed.
            return MCPToolExecutionResult.CreateFailure(AgentCallErrorCodes.IdentityServicesMissing);
        }

        using var tenantChange = currentTenant?.Change(call.TenantId);
        using var principalChange = principalAccessor?.Change(call.Principal);

        try
        {
            var permissionRefusal = await CheckPermissionAsync(scoped, call);
            if (permissionRefusal is not null)
            {
                return MCPToolExecutionResult.CreateFailure(permissionRefusal);
            }

            // Resolve the service inside the bound scope, never from the root provider.
            var service = scoped.GetRequiredService(_serviceType);
            
            // Bind parameters
            var boundArgs = _parameterBinder.BindParameters(_method, parameters);
            
            // Replace CancellationToken if present
            var methodParams = _method.GetParameters();
            for (int i = 0; i < methodParams.Length; i++)
            {
                if (methodParams[i].ParameterType == typeof(CancellationToken))
                {
                    boundArgs[i] = cancellationToken;
                }
            }
            
            // Invoke method
            var result = _method.Invoke(service, boundArgs);
            
            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Extract result from Task<T>
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }
                else
                {
                    result = null; // Task (non-generic) returns void
                }
            }
            
            stopwatch.Stop();
            
            return MCPToolExecutionResult.CreateSuccess(result, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            var innerException = ex is TargetInvocationException { InnerException: not null } ? ex.InnerException : ex;
            if (innerException is AbpValidationException validation)
            {
                var errors = validation.ValidationErrors.Take(12)
                    .Select(error => $"{string.Join(", ", error.MemberNames)}: {error.ErrorMessage}");
                return MCPToolExecutionResult.CreateFailure("Invalid tool arguments. " + string.Join("; ", errors));
            }

            if (innerException is Volo.Abp.BusinessException business && !string.IsNullOrWhiteSpace(business.Code))
            {
                // Default business-exception messages omit the code. Preserve the domain
                // failure for tool callers without exposing exception data or stack traces.
                return MCPToolExecutionResult.CreateFailure(business.Code);
            }
            
            return MCPToolExecutionResult.CreateFailure(
                innerException.Message,
                innerException.ToString()
            );
        }
    }

    private async Task<string?> CheckPermissionAsync(IServiceProvider scoped, AgentCallContext call)
    {
        if (!_requiresPermission && !_requiresAuthentication)
        {
            return null;
        }

        if (call.IsAnonymous)
        {
            return AgentCallErrorCodes.AnonymousNotAllowed;
        }

        if (!_requiresPermission)
        {
            return null;
        }

        if (_permissionName is null)
        {
            // RequiresPermission without a name can't be checked; refuse rather than guess.
            return AgentCallErrorCodes.PermissionNotDeclared;
        }

        var checker = scoped.GetService<IPermissionChecker>();
        if (checker is null || !await checker.IsGrantedAsync(call.Principal, _permissionName))
        {
            return AgentCallErrorCodes.PermissionDenied;
        }

        return null;
    }
}
