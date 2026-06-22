using System.Text.Json;
using Microsoft.SemanticKernel;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.MCP.Execution;

public class MCPKernelToolRegistrar : IMCPKernelToolRegistrar, ITransientDependency
{
    private readonly IMCPToolRegistry _toolRegistry;

    public MCPKernelToolRegistrar(IMCPToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public virtual async Task RegisterToolsAsync(
        Kernel kernel,
        string workspaceName,
        WorkspaceContext context,
        CancellationToken cancellationToken = default)
    {
        var tools = await _toolRegistry.GetToolsForWorkspaceAsync(workspaceName, cancellationToken);
        foreach (var tool in tools)
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

                    var result = await tool.ExecuteAsync(context, parameters, ct);
                    return result.Success ? result.Result : result.ErrorMessage;
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
