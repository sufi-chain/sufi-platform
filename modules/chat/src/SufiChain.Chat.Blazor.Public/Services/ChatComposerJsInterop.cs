using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SufiChain.Chat.Blazor.Public.Services;

public sealed class ChatComposerGeolocationResult
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }
}

public sealed class ChatComposerVoiceRecordingResult
{
    public string Base64 { get; set; } = string.Empty;

    public string MimeType { get; set; } = "audio/webm";

    public long Size { get; set; }
}

public sealed class BrowserFilePayload
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Base64 { get; set; } = string.Empty;
}

public class ChatComposerJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ChatComposerJsInterop> _logger;
    private IJSObjectReference? _module;
    private bool _disposed;

    public ChatComposerJsInterop(IJSRuntime jsRuntime, ILogger<ChatComposerJsInterop> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<BrowserFilePayload[]> ReadInputFilesAsync(ElementReference element)
    {
        _logger.LogDebug("[ChatComposer] ReadInputFilesAsync invoked");

        var module = await TryEnsureModuleAsync();
        if (module == null)
        {
            _logger.LogWarning("[ChatComposer] ReadInputFilesAsync aborted — JS module not available");
            return Array.Empty<BrowserFilePayload>();
        }

        var files = await module.InvokeAsync<BrowserFilePayload[]>("readInputFiles", element);
        _logger.LogDebug("[ChatComposer] ReadInputFilesAsync returned {Count} file(s)", files.Length);
        return files;
    }

    public async Task<ChatComposerGeolocationResult> GetGeolocationAsync()
    {
        _logger.LogDebug("[ChatComposer] GetGeolocationAsync invoked");

        var module = await TryEnsureModuleAsync();
        if (module == null)
        {
            _logger.LogWarning("[ChatComposer] GetGeolocationAsync aborted — JS module not available");
            throw new InvalidOperationException("JavaScript module not available. Component may not be interactive yet.");
        }

        var result = await module.InvokeAsync<ChatComposerGeolocationResult>("getGeolocation");
        _logger.LogDebug("[ChatComposer] GetGeolocationAsync returned lat={Lat} lng={Lng}", result.Latitude, result.Longitude);
        return result;
    }

    public async Task StartVoiceRecordingAsync()
    {
        _logger.LogDebug("[ChatComposer] StartVoiceRecordingAsync invoked");

        var module = await TryEnsureModuleAsync();
        if (module == null)
        {
            _logger.LogWarning("[ChatComposer] StartVoiceRecordingAsync aborted — JS module not available");
            throw new InvalidOperationException("JavaScript module not available. Component may not be interactive yet.");
        }

        await module.InvokeVoidAsync("startVoiceRecording");
        _logger.LogDebug("[ChatComposer] StartVoiceRecordingAsync — JS startVoiceRecording completed");
    }

    public async Task<ChatComposerVoiceRecordingResult> StopVoiceRecordingAsync()
    {
        _logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync invoked");

        var module = await TryEnsureModuleAsync();
        if (module == null)
        {
            _logger.LogWarning("[ChatComposer] StopVoiceRecordingAsync aborted — JS module not available");
            throw new InvalidOperationException("JavaScript module not available. Component may not be interactive yet.");
        }

        var result = await module.InvokeAsync<ChatComposerVoiceRecordingResult>("stopVoiceRecording");
        _logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync returned size={Size} mime={Mime}", result.Size, result.MimeType);
        return result;
    }

    public async Task PositionOverlayPopoverAsync(
        ElementReference anchor,
        ElementReference popover,
        ElementReference shell,
        string mode)
    {
        var module = await TryEnsureModuleAsync();
        if (module == null)
        {
            return;
        }

        await module.InvokeVoidAsync("positionOverlayPopover", anchor, popover, shell, mode);
    }

    public async Task RegisterPopoverClickAwayAsync<T>(ElementReference anchor, DotNetObjectReference<T> dotNetRef)
        where T : class
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("SufiBlazor.clickAway.register", anchor, dotNetRef, "OnPopoverClickAway");
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected
        }
        catch (InvalidOperationException)
        {
            // Static rendering
        }
    }

    public async Task UnregisterPopoverClickAwayAsync(ElementReference anchor)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("SufiBlazor.clickAway.unregister", anchor);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected
        }
        catch (InvalidOperationException)
        {
            // Static rendering
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_module != null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, cleanup not needed
            }
            catch (TaskCanceledException)
            {
                // Cancellation during disposal is fine
            }
            catch (InvalidOperationException)
            {
                // Static rendering, cleanup not needed
            }
        }
    }

    // Static web asset served by the RCL at _content/{PackageId}/js/chat-composer.js
    // (matches the FileManager module convention of placing JS under wwwroot/js/).
    // Bump the version suffix whenever chat-composer.js changes so browsers do not
    // serve a stale cached ES module.
    private const string ModuleUrl =
        "./_content/SufiChain.Chat.Blazor.Public/js/chat-composer.js?v=chat-composer-v6";

    private async Task<IJSObjectReference?> TryEnsureModuleAsync()
    {
        if (_module != null)
        {
            return _module;
        }

        try
        {
            _logger.LogDebug("[ChatComposer] Importing JS module from {ModuleUrl}", ModuleUrl);
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModuleUrl);
            _logger.LogDebug("[ChatComposer] JS module imported successfully");
            return _module;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued"))
        {
            // Component is in static prerender phase, JS not available yet. Retry on next call.
            _logger.LogDebug("[ChatComposer] JS module import skipped — static prerender phase (will retry)");
            return null;
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected; nothing to do.
            _logger.LogWarning("[ChatComposer] JS module import aborted — circuit disconnected");
            return null;
        }
        catch (Exception ex)
        {
            // Surface real import failures (e.g. 404 on the static asset) instead of hiding them.
            _logger.LogError(ex, "[ChatComposer] JS module import FAILED from {ModuleUrl}", ModuleUrl);
            throw;
        }
    }
}
