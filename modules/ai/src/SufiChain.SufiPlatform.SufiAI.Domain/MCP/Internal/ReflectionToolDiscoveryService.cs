using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>
/// Discovers internal MCP tools by scanning ApplicationService methods marked with <see cref="SufiAiMcpToolAttribute"/>.
/// </summary>
public class ReflectionToolDiscoveryService : IInternalToolDiscoveryService, ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReflectionToolDiscoveryService> _logger;
    private readonly JsonSchemaGenerator _schemaGenerator;
    private List<IMCPTool>? _cachedTools;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public ReflectionToolDiscoveryService(
        IServiceProvider serviceProvider,
        ILogger<ReflectionToolDiscoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _schemaGenerator = new JsonSchemaGenerator();
    }
    
    public async Task<List<IMCPTool>> DiscoverToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedTools != null)
            return _cachedTools;
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedTools != null)
                return _cachedTools;
            
            _cachedTools = await ScanForToolsAsync(cancellationToken);
            return _cachedTools;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cachedTools = null;
            _cachedTools = await ScanForToolsAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    private Task<List<IMCPTool>> ScanForToolsAsync(CancellationToken cancellationToken)
    {
        var tools = new List<IMCPTool>();
        
        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && 
                       (a.FullName?.Contains("SufiChain") == true || 
                        a.FullName?.Contains("Application") == true));
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                               (typeof(IApplicationService).IsAssignableFrom(t) ||
                                typeof(ISufiAITool).IsAssignableFrom(t) ||
                                t.Name.EndsWith("AppService")));
                
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(HasToolAttribute);
                    
                    foreach (var method in methods)
                    {
                        var attribute = GetToolAttribute(method);
                        var declared = method.GetCustomAttribute<SufiAiMcpToolAttribute>()!;
                        var access = GetAccessRequirement(type, method, attribute.PermissionName);

                        if (!declared.TryGetEffect(out var effect, out var refusal))
                        {
                            _logger.LogWarning(
                                "Not registering MCP tool {ToolName} from {ServiceType}.{MethodName}: {Reason}",
                                attribute.Name,
                                type.Name,
                                method.Name,
                                refusal);
                            continue;
                        }
                        
                        try
                        {
                            var schema = _schemaGenerator.GenerateSchema(method);
                            
                            var tool = new InternalMCPTool(
                                attribute.Name,
                                attribute.Description,
                                schema,
                                type,
                                method,
                                _serviceProvider,
                                access.PermissionName,
                                attribute.RequiresPermission,
                                access.RequiresAuthentication,
                                AllowsAnonymous(method),
                                effect == AgentActionEffect.Read,
                                effect
                            );
                            
                            tools.Add(tool);
                            
                            _logger.LogInformation(
                                "Discovered MCP tool: {ToolName} from {ServiceType}.{MethodName}",
                                attribute.Name,
                                type.Name,
                                method.Name
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Failed to register MCP tool {ToolName} from {ServiceType}.{MethodName}",
                                attribute.Name,
                                type.Name,
                                method.Name
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan assembly {AssemblyName} for MCP tools", assembly.FullName);
            }
        }
        
        _logger.LogInformation("Discovered {ToolCount} internal MCP tools", tools.Count);
        
        return Task.FromResult(tools);
    }

    private static bool HasToolAttribute(MethodInfo method)
    {
        return method.GetCustomAttribute<SufiAiMcpToolAttribute>() != null;
    }

    private static ToolAttributeInfo GetToolAttribute(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<SufiAiMcpToolAttribute>()!;
        return new ToolAttributeInfo(attribute.Name, attribute.Description, attribute.PermissionName, attribute.RequiresPermission);
    }

    /// <summary>
    /// sufi-chain#384 slice 3: fold <c>[Authorize]</c> on the service or method into the tool's access
    /// requirement. An explicit <c>PermissionName</c> wins; otherwise an <c>[Authorize(policy)]</c> policy is
    /// used as the permission (ABP permissions are policies). <c>[AllowAnonymous]</c> on the method clears it.
    /// </summary>
    public static (string? PermissionName, bool RequiresAuthentication) GetAccessRequirement(
        Type serviceType,
        MethodInfo method,
        string? declaredPermission)
    {
        if (method.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) is not null)
        {
            return (declaredPermission, false);
        }

        var authorize = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Concat(serviceType.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
            .ToList();
        if (authorize.Count == 0)
        {
            return (declaredPermission, false);
        }

        var policy = authorize.Select(a => a.Policy).FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
        return (string.IsNullOrWhiteSpace(declaredPermission) ? policy : declaredPermission, true);
    }

    /// <summary>
    /// sufi-chain#384 slice 4: anonymous allowlist = public and <see cref="AgentActionEffect.Read"/>,
    /// or <c>[SufiAiMcpPublicWrite]</c>. A conflicting effect is not allowlisted.
    /// </summary>
    public static bool AllowsAnonymous(MethodInfo method)
    {
        var tool = method.GetCustomAttribute<SufiAiMcpToolAttribute>();
        if (method.GetCustomAttribute<SufiAiMcpPublicWriteAttribute>() is not null)
        {
            return true;
        }

        return tool is { Public: true }
            && tool.TryGetEffect(out var effect, out _)
            && effect == AgentActionEffect.Read;
    }

    /// <summary>sufi-chain#312: read-only only when the resolved effect is Read. Unmarked counts as write.</summary>
    public static bool IsReadOnly(MethodInfo method) =>
        method.GetCustomAttribute<SufiAiMcpToolAttribute>() is { } tool
        && tool.TryGetEffect(out var effect, out _)
        && effect == AgentActionEffect.Read;

    private sealed record ToolAttributeInfo(string Name, string Description, string? PermissionName, bool RequiresPermission);
}
