using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.UI.Bundling;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.FileManager;

/// <summary>
/// JavaScript interop for the image editor (Cropper.js + Canvas filters).
/// All editing happens in browser; no backend processing.
/// Loads Cropper via bundle paths from IComponentBundleManager when available.
/// </summary>
public class ImageEditorInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IComponentBundleManager? _bundleManager;
    private IJSObjectReference? _module;
    private bool _disposed;

    public ImageEditorInterop(IJSRuntime jsRuntime, IComponentBundleManager? bundleManager = null)
    {
        _jsRuntime = jsRuntime;
        _bundleManager = bundleManager;
    }

    private async Task<IJSObjectReference> EnsureModuleAsync()
    {
        if (_module == null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SufiChain.SufiPlatform.FileManager.Blazor/js/filemanager-image-editor.js");
        }
        return _module;
    }

    public async Task<string?> InitAsync(ElementReference imageElement, object? options = null)
    {
        var module = await EnsureModuleAsync();
        var opts = new Dictionary<string, object?>
        {
            ["autoCrop"] = false
        };
        if (options != null)
        {
            var aspectRatioProp = options.GetType().GetProperty("aspectRatio");
            if (aspectRatioProp != null)
            {
                var value = aspectRatioProp.GetValue(options);
                // Skip NaN/Infinity - they cannot be serialized to JSON
                if (value is double d && !double.IsNaN(d) && !double.IsInfinity(d))
                    opts["aspectRatio"] = value;
            }
        }
        if (_bundleManager != null)
        {
            var scriptFiles = await _bundleManager.GetScriptBundleFilesAsync(FileManagerBundles.Cropper);
            var styleFiles = await _bundleManager.GetStyleBundleFilesAsync(FileManagerBundles.Cropper);
            if (scriptFiles.Count > 0) opts["scriptUrl"] = scriptFiles[0];
            if (styleFiles.Count > 0) opts["styleUrl"] = styleFiles[0];
        }
        return await module.InvokeAsync<string?>("initCropper", imageElement, opts);
    }

    public async Task<string> GetCroppedDataUrlAsync(string editorId, string format, double quality)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<string>("getCroppedDataUrl", editorId, format, quality);
    }

    public async Task RotateAsync(string editorId, double deg)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("rotate", editorId, deg);
    }

    public async Task FlipXAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("flipX", editorId);
    }

    public async Task FlipYAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("flipY", editorId);
    }

    public async Task SetAspectRatioAsync(string editorId, double ratio)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("setAspectRatio", editorId, ratio);
    }

    public async Task ResizeAsync(string editorId, int width, int height)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("resize", editorId, width, height);
    }

    public async Task ApplyFilterAsync(string editorId, string name, object value)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("applyFilter", editorId, name, value);
    }

    public async Task<bool> UndoAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("undo", editorId);
    }

    public async Task<bool> RedoAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("redo", editorId);
    }

    public async Task<bool> CanUndoAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("canUndo", editorId);
    }

    public async Task<bool> CanRedoAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("canRedo", editorId);
    }

    public async Task ResetAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("reset", editorId);
    }

    public async Task ZoomAsync(string editorId, double ratio)
    {
        var module = await EnsureModuleAsync();
        await module.InvokeVoidAsync("zoom", editorId, ratio);
    }

    public async Task<bool> ToggleCropBoxAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("toggleCropBox", editorId);
    }

    public async Task<bool> IsCropBoxVisibleAsync(string editorId)
    {
        var module = await EnsureModuleAsync();
        return await module.InvokeAsync<bool>("isCropBoxVisible", editorId);
    }

    public async Task DestroyAsync(string editorId)
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("destroy", editorId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_module == null) return;
        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit already disconnected; JS interop is no longer available. Best-effort cleanup.
        }
        catch (TaskCanceledException)
        {
            // Disposal during shutdown/cancellation.
        }
        finally
        {
            _module = null;
        }
    }
}
