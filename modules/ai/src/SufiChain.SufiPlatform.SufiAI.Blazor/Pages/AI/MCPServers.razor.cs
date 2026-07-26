using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class MCPServers
{
    [Inject] private IMCPServerAppService MCPServerAppService { get; set; } = default!;
    
    private static class LoadingKeys
    {
        public const string LoadServers = nameof(LoadServers);
        public const string ToggleServer = nameof(ToggleServer);
        public const string DeleteServer = nameof(DeleteServer);
        public const string TestConnection = nameof(TestConnection);
    }
    
    private List<MCPServerDto> _servers = new();
    private bool _modalOpen;
    private MCPServerDto? _editingServer;
    private bool _serversLoadStarted;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender || _serversLoadStarted || !IsInteractive)
        {
            return;
        }

        _serversLoadStarted = true;
        await LoadServersAsync();
    }
    
    private async Task LoadServersAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            _servers = await MCPServerAppService.GetListAsync();
        }, LoadingKeys.LoadServers);
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
