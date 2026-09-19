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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Internal;

/// <summary>
/// Wrapper for internal ApplicationService methods marked with [SufiAiMcpTool].
/// </summary>
public class InternalMCPTool : IMCPTool
{
    private readonly Type _serviceType;
    private readonly MethodInfo _method;
    private readonly IServiceProvider _serviceProvider;
    private readonly MethodParameterBinder _parameterBinder;
    
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
        IServiceProvider serviceProvider)
    {
        Name = name;
        Description = description;
        ParameterSchema = parameterSchema;
        _serviceType = serviceType;
        _method = method;
        _serviceProvider = serviceProvider;
        _parameterBinder = new MethodParameterBinder();
        Source = serviceType.Name;
    }
    
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Resolve service from DI
            var service = _serviceProvider.GetRequiredService(_serviceType);
            
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
}
