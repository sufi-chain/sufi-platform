using System.Text.Json;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// JSON-RPC messages for the MCP transports. Streamable HTTP (2025-03-26) and the legacy
/// HTTP+SSE transport (2024-11-05) both speak this shape: <c>initialize</c>,
/// <c>notifications/initialized</c>, <c>tools/list</c>, and <c>tools/call</c>.
/// </summary>
internal static class McpJsonRpc
{
    public const string RequestedProtocolVersion = "2025-03-26";
    public const string SessionHeader = "Mcp-Session-Id";
    public const string ProtocolHeader = "MCP-Protocol-Version";

    public static string CreateRequest(int id, string method, object? parameters)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            method,
            @params = parameters ?? new { }
        });
    }

    public static string CreateNotification(string method)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method
        });
    }

    public static bool TryParseMessage(string? json, out RpcMessage message)
    {
        message = default;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            int? id = root.TryGetProperty("id", out var idElement) ? ReadId(idElement) : null;
            if (root.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
            {
                message = new RpcMessage
                {
                    Id = id,
                    Error = ReadError(error)
                };
                return true;
            }

            if (!root.TryGetProperty("result", out var result))
            {
                return false;
            }

            message = new RpcMessage
            {
                Id = id,
                HasResult = true,
                Result = result.Clone()
            };
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static List<MCPServerToolDefinition> ReadTools(JsonElement tools)
    {
        var definitions = new List<MCPServerToolDefinition>();
        if (tools.ValueKind != JsonValueKind.Array)
        {
            return definitions;
        }

        foreach (var tool in tools.EnumerateArray())
        {
            if (tool.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var name = tool.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                ? nameElement.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            definitions.Add(new MCPServerToolDefinition
            {
                Name = name,
                Description = tool.TryGetProperty("description", out var description) && description.ValueKind == JsonValueKind.String
                    ? description.GetString() ?? string.Empty
                    : string.Empty,
                ParameterSchema = tool.TryGetProperty("inputSchema", out var schema)
                    ? schema.GetRawText()
                    : "{}"
            });
        }

        return definitions;
    }

    public static async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static string FormatHttpFailure(HttpResponseMessage response, string? body)
    {
        var status = (int)response.StatusCode;
        var reason = response.ReasonPhrase;
        var uri = response.RequestMessage?.RequestUri?.ToString() ?? "the MCP server";
        var snippet = Snippet(body);
        var message = string.IsNullOrWhiteSpace(snippet)
            ? $"HTTP {status} {reason} from {uri}"
            : $"HTTP {status} {reason} from {uri}: {snippet}";
        return McpSanitizedError.Sanitize(message);
    }

    public static string Snippet(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var snippet = body.Trim().ReplaceLineEndings(" ");
        return snippet.Length <= 300 ? snippet : snippet[..300];
    }

    private static string ReadError(JsonElement error)
    {
        if (error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(message.GetString()))
        {
            return message.GetString()!;
        }

        var raw = error.ValueKind == JsonValueKind.String ? error.GetString() : error.GetRawText();
        return string.IsNullOrWhiteSpace(raw) ? "MCP server returned an error." : raw;
    }

    private static int? ReadId(JsonElement id)
    {
        if (id.ValueKind == JsonValueKind.Number && id.TryGetInt32(out var number))
        {
            return number;
        }

        if (id.ValueKind == JsonValueKind.String && int.TryParse(id.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }
}

internal readonly struct RpcMessage
{
    public int? Id { get; init; }

    public bool HasResult { get; init; }

    public JsonElement Result { get; init; }

    public string? Error { get; init; }
}
