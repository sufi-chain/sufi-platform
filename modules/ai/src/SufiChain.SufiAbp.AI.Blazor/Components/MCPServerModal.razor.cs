using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AI.MCP.Servers;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.AI.Blazor.Components;

public partial class MCPServerModal 
{
    [Inject] private IMCPServerAppService MCPServerAppService { get; set; } = default!;
    
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MCPServerDto? Server { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    
    private static class LoadingKeys
    {
        public const string Save = nameof(Save);
    }
    
    private string _name = string.Empty;
    private string _transportType = "STDIO";
    private string? _endpoint;
    private string? _command;
    private string? _argumentsJson;
    private string? _metadataJson;
    
    private async Task OnTransportTypeChanged(string value)
    {
        _transportType = value;
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnParametersSet()
    {
        if (Server != null)
        {
            _name = Server.Name;
            _transportType = Server.TransportType;
            _endpoint = Server.Endpoint;
            _command = Server.Command;
            _argumentsJson = Server.ArgumentsJson;
            _metadataJson = Server.MetadataJson;
        }
        else
        {
            ResetForm();
        }
    }
    
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            await Message.ErrorAsync(L["NameIsRequired"]);
            return;
        }
        
        if (!WorkspaceId.HasValue && Server == null)
        {
            await Message.ErrorAsync(L["WorkspaceIsRequired"]);
            return;
        }
        
        await ExecuteWithLoadingAsync(async () =>
        {
            if (Server == null)
            {
                var input = new CreateMCPServerDto
                {
                    Name = _name,
                    WorkspaceId = WorkspaceId!.Value,
                    TransportType = _transportType,
                    Endpoint = _endpoint,
                    Command = _command,
                    ArgumentsJson = _argumentsJson,
                    MetadataJson = _metadataJson
                };
                
                await MCPServerAppService.CreateAsync(input);
            }
            else
            {
                var input = new UpdateMCPServerDto
                {
                    Name = _name,
                    Endpoint = _endpoint,
                    Command = _command,
                    ArgumentsJson = _argumentsJson,
                    MetadataJson = _metadataJson
                };
                
                await MCPServerAppService.UpdateAsync(Server.Id, input);
            }
            
            await OnSaved.InvokeAsync();
            await CloseAsync();
        });
    }
    
    private async Task CancelAsync()
    {
        await CloseAsync();
    }
    
    private async Task CloseAsync()
    {
        await SetOpenAsync(false);
        ResetForm();
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }
    
    private void ResetForm()
    {
        _name = string.Empty;
        _transportType = "STDIO";
        _endpoint = null;
        _command = null;
        _argumentsJson = null;
        _metadataJson = null;
    }
}
