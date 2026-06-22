using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AI.MCP.Servers;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AI.Blazor.Pages.AI;

public partial class MCPServers
{
    [Inject] private IMCPServerAppService MCPServerAppService { get; set; } = default!;
    
    private static class LoadingKeys
    {
        public const string LoadServers = nameof(LoadServers);
        public const string LoadWorkspaces = nameof(LoadWorkspaces);
        public const string ToggleServer = nameof(ToggleServer);
        public const string DeleteServer = nameof(DeleteServer);
        public const string TestConnection = nameof(TestConnection);
    }
    
    private List<WorkspaceDto> _workspaces = new();
    private List<MCPServerDto> _servers = new();
    private Guid? _selectedWorkspaceId;
    private bool _modalOpen;
    private MCPServerDto? _editingServer;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadWorkspacesAsync();
        
        if (_workspaces.Count > 0)
        {
            _selectedWorkspaceId = _workspaces[0].Id;
            await LoadServersAsync();
        }
    }
    
    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            // TODO: Implement proper lazy loading with pagination
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100
            });
            _workspaces = result.Items.Where(w => w.IsActive).ToList();
        }, LoadingKeys.LoadWorkspaces);
    }
    
    private async Task LoadServersAsync()
    {
        if (!_selectedWorkspaceId.HasValue)
            return;
        
        await ExecuteWithLoadingAsync(async () =>
        {
            _servers = await MCPServerAppService.GetByWorkspaceAsync(_selectedWorkspaceId.Value);
        }, LoadingKeys.LoadServers);
    }
    
    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        await LoadServersAsync();
    }
    
    private void OpenCreateModal()
    {
        _editingServer = null;
        _modalOpen = true;
    }
    
    private void OpenEditModal(MCPServerDto server)
    {
        _editingServer = server;
        _modalOpen = true;
    }
    
    private async Task OnServerSavedAsync()
    {
        _modalOpen = false;
        await LoadServersAsync();
    }
    
    private async Task ToggleServerAsync(MCPServerDto server)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            if (server.IsEnabled)
            {
                await MCPServerAppService.DisableAsync(server.Id);
            }
            else
            {
                await MCPServerAppService.EnableAsync(server.Id);
            }
            
            await LoadServersAsync();
        }, LoadingKeys.ToggleServer);
    }

    private async Task TestConnectionAsync(MCPServerDto server)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var success = await MCPServerAppService.TestConnectionAsync(server.Id);
            
            if (success)
            {
                await Message.SuccessAsync(L["ConnectionTestSuccessful"]);
            }
            else
            {
                await Message.ErrorAsync(L["ConnectionTestFailed"]);
            }
            
            await LoadServersAsync();
        }, $"{LoadingKeys.TestConnection}-{server.Id}");
    }
    
    private async Task DeleteServerAsync(MCPServerDto server)
    {
        var confirmed = await Message.ConfirmAsync(
            L["DeleteConfirmationMessage", server.Name]
        );
        
        if (!confirmed)
            return;
        
        await ExecuteWithLoadingAsync(async () =>
        {
            await MCPServerAppService.DeleteAsync(server.Id);
            await LoadServersAsync();
        }, LoadingKeys.DeleteServer);
    }
}
