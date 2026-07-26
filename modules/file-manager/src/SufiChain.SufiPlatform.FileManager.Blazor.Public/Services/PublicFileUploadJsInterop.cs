using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

/// <summary>
/// JS interop for public file upload via HTTP multipart (bypasses Blazor Server circuit).
/// </summary>
public class PublicFileUploadJsInterop : IAsyncDisposable
{
    private const string ModuleUrl =
        "./_content/SufiChain.SufiPlatform.FileManager.Blazor.Public/js/public-file-uploader.js?v=public-upload-v1";

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly IConfiguration _configuration;

    public PublicFileUploadJsInterop(IJSRuntime jsRuntime, IConfiguration configuration)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModuleUrl).AsTask());
        _configuration = configuration;
    }

    /// <summary>
    /// API origin (scheme + host + port), or empty for same-origin.
    /// </summary>
    public string GetFileItemsApiBaseUrl()
    {
        var baseUrl = (_configuration["RemoteServices:FileManager:BaseUrl"]
                      ?? _configuration["RemoteServices:Default:BaseUrl"]
                      ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            return "";
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
        {
            return uri.GetLeftPart(UriPartial.Authority);
        }

        return baseUrl;
    }

    public async ValueTask RegisterFileInputChangeAsync(string inputId, object dotNetRef)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("registerFileInputChange", inputId, dotNetRef);
    }

    public async ValueTask TriggerFileInputAsync(string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("triggerFileInput", elementId);
    }

    public async ValueTask UploadFilesFromInputAsync(
        string inputId,
        PublicJsUploadMetadata metadata,
        string? accessToken,
        object dotNetRef)
    {
        var apiBaseUrl = GetFileItemsApiBaseUrl();
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("uploadFilesFromInput", inputId, apiBaseUrl, metadata, accessToken, dotNetRef);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_moduleTask.IsValueCreated)
        {
            return;
        }

        try
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected
        }
    }
}

public class PublicJsUploadMetadata
{
    public string? StructureKey { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? FolderPath { get; set; }
    public Guid? FolderId { get; set; }
    public bool AutoConfirm { get; set; }
    public string? Alt { get; set; }
    /// <summary>When &gt; 0, oversized files are rejected client-side before HTTP upload.</summary>
    public long MaxFileSize { get; set; }
}

public class PublicJsFileInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
}

public class PublicJsUploadResult
{
    public bool Success { get; set; }
    public JsonElement? Data { get; set; }
    public string? Error { get; set; }
}
