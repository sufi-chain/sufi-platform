using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Execution;

public class MCPKernelToolRegistrar : IMCPKernelToolRegistrar, ITransientDependency
{
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly ILogger<MCPKernelToolRegistrar> _logger;

    public MCPKernelToolRegistrar(
        IMCPToolRegistry toolRegistry,
        ILogger<MCPKernelToolRegistrar> logger)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    public virtual async Task RegisterToolsAsync(
        Kernel kernel,
        WorkspaceContext context,
        IReadOnlyList<string> allowedToolNames,
        CancellationToken cancellationToken = default)
    {
        if (allowedToolNames.Count == 0)
        {
            return;
        }

        var resolution = await _toolRegistry.ResolveAsync(allowedToolNames, cancellationToken);

        foreach (var tool in resolution.Tools)
        {
            var pluginName = ToKernelPluginName(tool.Name);
            if (kernel.Plugins.Any(plugin => plugin.Name == pluginName))
            {
                continue;
            }

            var function = KernelFunctionFactory.CreateFromMethod(
                async (KernelArguments arguments, CancellationToken ct) =>
                {
                    var parameters = arguments
                        .Where(argument => argument.Value is not null)
                        .ToDictionary(argument => argument.Key, argument => argument.Value);

                    var parameterKeys = string.Join(",", parameters.Keys.OrderBy(key => key, StringComparer.Ordinal));
                    _logger.LogInformation(
                        "Executing MCP tool {ToolName} (Type: {ToolType}, Source: {Source}) in workspace {WorkspaceName}. ParameterKeys={ParameterKeys}",
                        tool.Name,
                        tool.ToolType,
                        tool.Source,
                        context.WorkspaceName,
                        parameterKeys);

                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        var result = await tool.ExecuteAsync(context, parameters, ct);
                        stopwatch.Stop();

                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "MCP tool {ToolName} executed successfully in {ExecutionTimeMs}ms (MeasuredMs={MeasuredMs})",
                                tool.Name,
                                result.ExecutionTimeMs,
                                stopwatch.ElapsedMilliseconds);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "MCP tool {ToolName} execution failed in {ExecutionTimeMs}ms: {ErrorMessage}",
                                tool.Name,
                                result.ExecutionTimeMs > 0 ? result.ExecutionTimeMs : stopwatch.ElapsedMilliseconds,
                                result.ErrorMessage);
                        }

                        return result.Success ? result.Result : result.ErrorMessage;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        _logger.LogError(
                            ex,
                            "Unexpected error executing MCP tool {ToolName} after {ElapsedMilliseconds}ms in workspace {WorkspaceName}",
                            tool.Name,
                            stopwatch.ElapsedMilliseconds,
                            context.WorkspaceName);
                        throw;
                    }
                },
                functionName: ToKernelFunctionName(tool.Name),
                description: tool.Description,
                parameters: CreateParameterMetadata(tool.ParameterSchema));

            kernel.Plugins.AddFromFunctions(pluginName, new[] { function });
        }
    }

    private static List<KernelParameterMetadata> CreateParameterMetadata(string parameterSchema)
    {
        if (string.IsNullOrWhiteSpace(parameterSchema))
        {
            return new List<KernelParameterMetadata>();
        }

        using var document = JsonDocument.Parse(parameterSchema);
        if (!document.RootElement.TryGetProperty("properties", out var properties) ||
            properties.ValueKind != JsonValueKind.Object)
        {
            return new List<KernelParameterMetadata>();
        }

        var required = ReadRequiredParameters(document.RootElement);
        var parameters = new List<KernelParameterMetadata>();

        foreach (var property in properties.EnumerateObject())
        {
            parameters.Add(new KernelParameterMetadata(property.Name)
            {
                Description = ReadString(property.Value, "description"),
                IsRequired = required.Contains(property.Name),
                ParameterType = GetParameterType(property.Value),
                Schema = KernelJsonSchema.Parse(property.Value.GetRawText())
            });
        }

        return parameters;
    }

    private static HashSet<string> ReadRequiredParameters(JsonElement root)
    {
        if (!root.TryGetProperty("required", out var requiredElement) ||
            requiredElement.ValueKind != JsonValueKind.Array)
        {
            return new HashSet<string>();
        }

        return requiredElement
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToHashSet();
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static Type GetParameterType(JsonElement schema)
    {
        var type = ReadString(schema, "type");
        var format = ReadString(schema, "format");

        return type switch
        {
            "boolean" => typeof(bool),
            "integer" => typeof(int),
            "number" => typeof(double),
            "array" => typeof(List<object>),
            "object" => typeof(Dictionary<string, object?>),
            "string" when format == "uuid" => typeof(Guid),
            "string" when format == "date-time" => typeof(DateTime),
            "string" => typeof(string),
            _ => typeof(object)
        };
    }

    private static string ToKernelPluginName(string toolName)
    {
        return "MCP_" + ToKernelFunctionName(toolName);
    }

    private static string ToKernelFunctionName(string toolName)
    {
        return new string(toolName.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
    }
}
