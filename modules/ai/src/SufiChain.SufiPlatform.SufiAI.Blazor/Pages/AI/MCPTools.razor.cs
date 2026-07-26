using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class MCPTools
{
    private static readonly TimeSpan CatalogLoadTimeout = TimeSpan.FromSeconds(8);

    private static class LoadingKeys
    {
        public const string LoadTools = nameof(LoadTools);
        public const string LoadSchema = nameof(LoadSchema);
    }

    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;

    private List<MCPToolDto> _tools = new();
    private bool _schemaDialogOpen;
    private MCPToolDto? _selectedTool;
    private bool _toolsLoadStarted;
    private string? _catalogLoadError;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender || _toolsLoadStarted || !IsInteractive || IsDisposed)
        {
            return;
        }

        _toolsLoadStarted = true;
        // Do not await — keep the renderer turn free.
        _ = LoadToolsAsync();
    }

    private async Task LoadToolsAsync()
    {
        if (IsDisposed)
        {
            return;
        }

        _catalogLoadError = null;
        LoadingStates[LoadingKeys.LoadTools] = true;
        await InvokeStateHasChangedSafeAsync();

        try
        {
            _tools = await LoadCatalogOffCircuitAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "MCP tools page catalog load failed");
            _tools = new List<MCPToolDto>();
            _catalogLoadError = ex.Message;
        }
        finally
        {
            LoadingStates.TryRemove(LoadingKeys.LoadTools, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }

    private async Task RefreshToolsAsync()
    {
        LoadingStates[LoadingKeys.LoadTools] = true;
        await InvokeStateHasChangedSafeAsync();

        try
        {
            await Task.Run(async () =>
            {
                await using var scope = ServiceScopeFactory.CreateAsyncScope();
                var app = scope.ServiceProvider.GetRequiredService<IMCPToolAppService>();
                await app.RefreshToolRegistryAsync();
            }, ComponentCancellationToken);

            _tools = await LoadCatalogOffCircuitAsync();
            _catalogLoadError = null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "MCP tools refresh failed");
            _catalogLoadError = ex.Message;
        }
        finally
        {
            LoadingStates.TryRemove(LoadingKeys.LoadTools, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }

    private async Task<List<MCPToolDto>> LoadCatalogOffCircuitAsync()
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ComponentCancellationToken);
        timeoutCts.CancelAfter(CatalogLoadTimeout);

        try
        {
            return await Task.Run(async () =>
            {
                await using var scope = ServiceScopeFactory.CreateAsyncScope();
                var catalog = scope.ServiceProvider.GetRequiredService<IMCPToolAppService>();
                return await catalog.GetCatalogAsync();
            }, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ComponentCancellationToken.IsCancellationRequested)
        {
            _catalogLoadError = "Catalog load timed out";
            Logger.LogWarning("MCP catalog load timed out after {Timeout}s", CatalogLoadTimeout.TotalSeconds);
            return new List<MCPToolDto>();
        }
    }

    private async Task ViewToolSchemaAsync(MCPToolDto tool)
    {
        _selectedTool = tool;
        _schemaDialogOpen = true;

        LoadingStates[LoadingKeys.LoadSchema] = true;
        await InvokeStateHasChangedSafeAsync();

        try
        {
            var details = await Task.Run(async () =>
            {
                await using var scope = ServiceScopeFactory.CreateAsyncScope();
                var catalog = scope.ServiceProvider.GetRequiredService<IMCPToolAppService>();
                return await catalog.GetAsync(tool.Name);
            }, ComponentCancellationToken);

            if (details != null)
            {
                _selectedTool = details;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load schema for tool {ToolName}", tool.Name);
        }
        finally
        {
            LoadingStates.TryRemove(LoadingKeys.LoadSchema, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }
}
