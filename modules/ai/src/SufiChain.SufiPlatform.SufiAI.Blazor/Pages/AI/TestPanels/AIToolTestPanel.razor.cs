using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class AIToolTestPanel : AIComponentBase
{
    private static readonly TimeSpan CatalogLoadTimeout = TimeSpan.FromSeconds(5);

    private static class ToolChatLoadingKeys
    {
        public const string LoadWorkspaces = "tool-chat-load-workspaces";
        public const string LoadTools = "tool-chat-load-tools";
        public const string SendMessage = "tool-chat-send-message";
    }

    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;

    private ISufiAIChatAppService AIChatAppService => LazyGetRequiredService(ref _aiChatAppService);
    private ISufiAIChatAppService? _aiChatAppService;

    private List<WorkspaceDto> _workspaces = new();
    private List<MCPToolDto> _tools = new();
    private HashSet<string> _selectedToolNames = new(StringComparer.Ordinal);
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private string _messageInput = string.Empty;
    private List<ToolChatMessage> _messages = new();
    private bool _dataLoadStarted;
    private string? _catalogLoadError;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender || _dataLoadStarted || !IsInteractive || IsDisposed)
        {
            return;
        }

        _dataLoadStarted = true;

        // CRITICAL: do not await catalog/workspace I/O inside OnAfterRenderAsync.
        // Awaiting MCP/Redis on the renderer turn freezes the entire Blazor circuit.
        _ = LoadPanelDataAsync();
    }

    private async Task LoadPanelDataAsync()
    {
        try
        {
            await LoadWorkspacesAsync();
            await LoadToolsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load MCP tool test panel data");
            if (!IsDisposed)
            {
                await HandleErrorAsync(ex);
            }
        }
    }

    private async Task LoadToolsAsync()
    {
        if (IsDisposed)
        {
            return;
        }

        _catalogLoadError = null;
        LoadingStates[ToolChatLoadingKeys.LoadTools] = true;
        await InvokeStateHasChangedSafeAsync();

        try
        {
            _tools = await LoadCatalogOffCircuitAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "MCP catalog load failed; showing empty tool list");
            _tools = new List<MCPToolDto>();
            _catalogLoadError = ex.Message;
        }
        finally
        {
            LoadingStates.TryRemove(ToolChatLoadingKeys.LoadTools, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }

    /// <summary>
    /// Runs catalog retrieval on the thread pool with a hard timeout so a stuck Redis/STDIO
    /// path cannot hold the Blazor synchronization context.
    /// </summary>
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

    private void OnToolSelectionChanged(string toolName, bool selected)
    {
        if (selected)
        {
            _selectedToolNames.Add(toolName);
        }
        else
        {
            _selectedToolNames.Remove(toolName);
        }

        ClearChat();
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            // Off-circuit: workspace list also uses ABP UoW/cache and can deadlock the circuit.
            var workspaces = await Task.Run(async () =>
            {
                await using var scope = ServiceScopeFactory.CreateAsyncScope();
                var workspaceApp = scope.ServiceProvider.GetRequiredService<IWorkspaceAppService>();
                var result = await workspaceApp.GetListAsync(new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 100
                });
                return result.Items.Where(w => w.IsActive).ToList();
            }, ComponentCancellationToken);

            _workspaces = workspaces;
        }, ToolChatLoadingKeys.LoadWorkspaces);
    }

    private Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        var workspace = _workspaces.FirstOrDefault(w => w.Id == workspaceId);
        _selectedWorkspaceName = workspace?.Name ?? string.Empty;
        ClearChat();

        return Task.CompletedTask;
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedWorkspaceName) ||
            _selectedToolNames.Count == 0 ||
            string.IsNullOrWhiteSpace(_messageInput))
        {
            return;
        }

        var userMessage = _messageInput;
        _messageInput = string.Empty;
        _messages.Add(new ToolChatMessage { IsUser = true, Content = userMessage });

        await ExecuteWithLoadingAsync(async () =>
        {
            var response = await AIChatAppService.SendMessageWithToolsAsync(new SufiAISendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                AllowedMcpToolNames = _selectedToolNames.OrderBy(name => name, StringComparer.Ordinal).ToList(),
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 1))
                    .Select(message => new SufiAIChatMessageDto
                    {
                        Role = message.IsUser ? "user" : "assistant",
                        Content = message.Content
                    })
                    .ToList()
            });

            _messages.Add(new ToolChatMessage
            {
                IsUser = false,
                Content = response.Message
            });
        }, ToolChatLoadingKeys.SendMessage);
    }

    private void ClearChat()
    {
        _messages.Clear();
        _messageInput = string.Empty;
    }

    private class ToolChatMessage
    {
        public bool IsUser { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
