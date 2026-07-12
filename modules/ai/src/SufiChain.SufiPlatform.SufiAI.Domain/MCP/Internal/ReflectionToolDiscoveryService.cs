using System.Reflection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Attributes;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>
/// Discovers internal MCP tools by scanning ApplicationService methods.
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
                        
                        try
                        {
                            var schema = _schemaGenerator.GenerateSchema(method);
                            
                            var tool = new InternalMCPTool(
                                attribute.Name,
                                attribute.Description,
                                schema,
                                type,
                                method,
                                _serviceProvider
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
        return method.GetCustomAttribute<MCPToolAttribute>() != null ||
               method.GetCustomAttribute<SufiAIToolAttribute>() != null;
    }

    private static ToolAttributeInfo GetToolAttribute(MethodInfo method)
    {
        var mcpToolAttribute = method.GetCustomAttribute<MCPToolAttribute>();
        if (mcpToolAttribute != null)
        {
            return new ToolAttributeInfo(mcpToolAttribute.Name, mcpToolAttribute.Description);
        }

        var sufiAbpToolAttribute = method.GetCustomAttribute<SufiAIToolAttribute>()!;
        return new ToolAttributeInfo(sufiAbpToolAttribute.Name, sufiAbpToolAttribute.Description);
    }

    private sealed record ToolAttributeInfo(string Name, string Description);
}
