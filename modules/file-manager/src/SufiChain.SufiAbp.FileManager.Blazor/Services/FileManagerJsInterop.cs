using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.Blazor.Services;

public class FileManagerJsInterop : IAsyncDisposable
{
    /// <summary>
    /// Path segment for the file-items API. Must match FileItemController route
    /// (api/sabp/file-manager/file-items).
    /// </summary>
    public const string FileItemsApiPath = "api/sabp/file-manager/file-items";

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly IConfiguration _configuration;

    public FileManagerJsInterop(IJSRuntime jsRuntime, IConfiguration configuration)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SufiChain.SufiAbp.FileManager.Blazor/js/sufi-file-manager.js?v=file-manager-upload-v1").AsTask());
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the origin (scheme + host + port) for the file-items HTTP API, or empty string for same-origin.
    /// Uses RemoteServices:FileManager:BaseUrl or RemoteServices:Default:BaseUrl from configuration.
    /// The file-items path is never appended here; JS builds the full upload URL once from this origin
    /// plus a single path constant, so the path cannot be duplicated.
    /// </summary>
    public string GetFileItemsApiBaseUrl()
    {
        var baseUrl = (_configuration["RemoteServices:FileManager:BaseUrl"]
                      ?? _configuration["RemoteServices:Default:BaseUrl"]
                      ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return "";
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
            return uri.GetLeftPart(UriPartial.Authority);
        return baseUrl;
    }

    /// <summary>
    /// Upload a file directly via JavaScript (bypasses SignalR for better performance with large files).
    /// This method sends the file directly to the API endpoint without going through Blazor's SignalR circuit.
    /// API base URL is read from configuration (RemoteServices:FileManager:BaseUrl) and combined with the
    /// file-items path from FileItemController.
    /// </summary>
    /// <param name="file">JavaScript File object reference</param>
    /// <param name="metadata">Upload metadata (structure key, entity info, etc.)</param>
    /// <param name="accessToken">Optional Bearer token for authentication. If null, falls back to cookie auth.</param>
    /// <param name="progressCallback">Optional .NET reference for progress updates (must implement OnUploadProgress method)</param>
    /// <returns>Upload result with success status and data or error message</returns>
    public async ValueTask<JsUploadResult> UploadFileAsync(
        IJSObjectReference file,
        JsUploadMetadata metadata,
        string? accessToken = null,
        DotNetObjectReference<object>? progressCallback = null)
    {
        var apiBaseUrl = GetFileItemsApiBaseUrl();
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<JsUploadResult>("uploadFile", file, apiBaseUrl, metadata, accessToken, progressCallback);
    }

    public async ValueTask TriggerFileInput(string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("triggerFileInput", elementId);
    }

    /// <summary>
    /// Register change handler on file input. When user selects files, JS calls .NET OnFileInputChange;
    /// C# then calls UploadFilesFromInputAsync so uploads go via HTTP (bypass SignalR, avoid circuit timeout).
    /// </summary>
    public async ValueTask RegisterFileInputChangeAsync(string inputId, object dotNetRef)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("registerFileInputChange", inputId, dotNetRef);
    }

    /// <summary>
    /// Upload files from a file input via HTTP (bypasses SignalR). Call after OnFileInputChange from JS.
    /// API base URL is read from configuration and combined with the file-items path from FileItemController.
    /// </summary>
    public async ValueTask UploadFilesFromInputAsync(
        string inputId,
        JsUploadMetadata metadata,
        string? accessToken,
        object dotNetRef)
    {
        var apiBaseUrl = GetFileItemsApiBaseUrl();
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("uploadFilesFromInput", inputId, apiBaseUrl, metadata, accessToken, dotNetRef);
    }

    public async ValueTask InitializeDragDrop(string dropZoneId, DotNetObjectReference<object> dotNetReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeDragDrop", dropZoneId, dotNetReference);
    }

    public async ValueTask CreateImagePreview(string imageData, string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("createImagePreview", imageData, elementId);
    }

    public async ValueTask OpenLightbox(string imageUrl, string title)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("openLightbox", imageUrl, title);
    }

    public async ValueTask DownloadFile(string fileName, string contentBase64)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("downloadFile", fileName, contentBase64);
    }

    public async ValueTask<bool> CopyToClipboard(string text)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>("copyToClipboard", text);
    }

    public async ValueTask InitializeImageZoom(string imageElementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeImageZoom", imageElementId);
    }

    public async ValueTask InitializeVideoPlayer(string videoElementId, DotNetObjectReference<object> dotNetReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeVideoPlayer", videoElementId, dotNetReference);
    }

    public async ValueTask<string> FormatFileSize(long bytes)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("formatFileSize", bytes);
    }

    public async ValueTask<ImageValidationResult> ValidateImageDimensions(
        IJSObjectReference file,
        int? minWidth,
        int? minHeight,
        int? maxWidth,
        int? maxHeight)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<ImageValidationResult>(
            "validateImageDimensions",
            file, minWidth, minHeight, maxWidth, maxHeight);
    }

    public async ValueTask<string> GetImageDataUrl(IJSObjectReference file)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("getImageDataUrl", file);
    }

    public async ValueTask ScrollToElement(string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToElement", elementId);
    }

    public async ValueTask ShowToast(string message, ToastType type = ToastType.Info, int duration = 3000)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("showToast", message, type.ToString().ToLower(), duration);
    }

    public async ValueTask InitializeSortable(string containerId, DotNetObjectReference<object> dotNetReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeSortable", containerId, dotNetReference);
    }

    public async ValueTask Dispose(string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dispose", elementId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, nothing to dispose - this is expected
                // when the user navigates away or the page is reloaded
            }
        }
    }
}

public class ImageValidationResult
{
    public bool Valid { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Metadata for JavaScript file upload
/// </summary>
public class JsUploadMetadata
{
    public string? StructureKey { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing. Takes precedence over FolderId.
    /// </summary>
    public string? FolderPath { get; set; }
    /// <summary>
    /// Target folder ID (used when FolderPath is not set)
    /// </summary>
    public Guid? FolderId { get; set; }
    public bool AutoConfirm { get; set; }
    public string? Alt { get; set; }
}

/// <summary>
/// File info passed from JS when user selects files (name and size only).
/// </summary>
public class JsFileInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [System.Text.Json.Serialization.JsonPropertyName("size")]
    public long Size { get; set; }
}

/// <summary>
/// Result from JavaScript file upload
/// </summary>
public class JsUploadResult
{
    public bool Success { get; set; }
    public JsonElement? Data { get; set; }
    public string? Error { get; set; }
}

