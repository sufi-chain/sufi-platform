using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// SSE (Server-Sent Events) transport client for MCP servers.
/// </summary>
public class SSETransportClient : IMCPTransportClient
{
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SSETransportClient> _logger;
    private bool _isConnected;
    
    public MCPTransportType TransportType => MCPTransportType.SSE;
    public bool IsConnected => _isConnected;
    
    public SSETransportClient(
        string endpoint,
        HttpClient httpClient,
        ILogger<SSETransportClient> logger)
    {
        _endpoint = endpoint;
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test connection
            var response = await _httpClient.GetAsync($"{_endpoint}/health", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            _isConnected = true;
            _logger.LogInformation("Connected to MCP server via SSE: {Endpoint}", _endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server: {Endpoint}", _endpoint);
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Endpoint", _endpoint)
                .WithData("Error", ex.Message);
        }
    }
    
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = false;
        return Task.CompletedTask;
    }
    
    public async Task<List<MCPServerToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoint}/tools");
        ApplyBearerToken(request, bearerToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(content);
        
        var tools = new List<MCPServerToolDefinition>();
        
        if (doc.RootElement.TryGetProperty("tools", out var toolsArray))
        {
            foreach (var tool in toolsArray.EnumerateArray())
            {
                tools.Add(new MCPServerToolDefinition
                {
                    Name = tool.GetProperty("name").GetString() ?? "",
                    Description = tool.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    ParameterSchema = tool.TryGetProperty("inputSchema", out var schema) 
                        ? schema.GetRawText() 
                        : "{}"
                });
            }
        }
        
        return tools;
    }
    
    public async Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        var request = new
        {
            name = toolName,
            arguments = parameters
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/tools/call")
            {
                Content = content
            };
            ApplyBearerToken(httpRequest, bearerToken);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<object>(responseContent);
            
            return new MCPServerToolResult
            {
                Success = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new MCPServerToolResult
            {
                Success = false,
                ErrorMessage = McpSanitizedError.FromException(ex, requestId: null).ToClientMessage()
            };
        }
    }

    /// <summary>
    /// sufi-chain#384 slice 6b: attaches the short-lived per-user token for a <c>UserScoped</c> server.
    /// A tenant-level server passes null and keeps sending unauthenticated requests exactly as before.
    /// </summary>
    private static void ApplyBearerToken(HttpRequestMessage request, string? bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            return;
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
    }
    
    public void Dispose()
    {
        // HttpClient is managed externally
    }
}
