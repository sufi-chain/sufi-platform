using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class MCPServerModal 
{
    [Inject] private IMCPServerAppService MCPServerAppService { get; set; } = default!;
    
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MCPServerDto? Server { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    
    private static class LoadingKeys
    {
        public const string Save = nameof(Save);
    }
    
    private string _name = string.Empty;
    private string _key = string.Empty;
    private string _transportType = "STDIO";
    private string? _endpoint;
    private string? _command;
    private string? _argumentsJson;
    
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
            _key = Server.Key;
            _transportType = Server.TransportType;
            _endpoint = Server.Endpoint;
            _command = Server.Command;
            _argumentsJson = Server.ArgumentsJson;
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
        
        if (Server == null && string.IsNullOrWhiteSpace(_key))
        {
            await Message.ErrorAsync(L["ServerKeyIsRequired"]);
            return;
        }
        
        await ExecuteWithLoadingAsync(async () =>
        {
            if (Server == null)
            {
                var input = new CreateMCPServerDto
                {
                    Name = _name,
                    Key = _key,
                    TransportType = _transportType,
                    Endpoint = _endpoint,
                    Command = _command,
                    ArgumentsJson = _argumentsJson
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
                    ArgumentsJson = _argumentsJson
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
        _key = string.Empty;
        _transportType = "STDIO";
        _endpoint = null;
        _command = null;
        _argumentsJson = null;
    }
}
