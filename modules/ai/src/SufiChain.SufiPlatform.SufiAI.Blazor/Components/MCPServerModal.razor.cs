using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class MCPServerModal 
{
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
    private string _argumentsJson = string.Empty;

    // sufi-chain#384 slice 6b: TenantLevel (default) or UserScoped.
    private string _bindingMode = "TenantLevel";

    /// <summary>
    /// A user-scoped server receives a per-user token, which STDIO has no channel for. The domain rejects
    /// the combination; the picker hides it so the admin never hits that error.
    /// </summary>
    private bool UserScopedAvailable => _transportType != "STDIO";

    private async Task OnTransportTypeChanged(string value)
    {
        _transportType = value;

        if (!UserScopedAvailable)
        {
            _bindingMode = "TenantLevel";
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnBindingModeChanged(string value)
    {
        _bindingMode = value;
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
            _argumentsJson = Server.ArgumentsJson ?? string.Empty;
            _bindingMode = string.IsNullOrWhiteSpace(Server.BindingMode) ? "TenantLevel" : Server.BindingMode;
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
                    ArgumentsJson = ArgumentsJsonForSave(),
                    BindingMode = EffectiveBindingMode
                };
                
                await MCPServerAppService.CreateAsync(input);
            }
            else
            {
                var input = new UpdateMCPServerDto
                {
                    Name = _name,
                    TransportType = _transportType,
                    Endpoint = _endpoint,
                    Command = _command,
                    ArgumentsJson = ArgumentsJsonForSave(),
                    BindingMode = EffectiveBindingMode
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
        _argumentsJson = string.Empty;
        _bindingMode = "TenantLevel";
    }

    /// <summary>STDIO can never be user-scoped, so the form always saves it as tenant-level.</summary>
    private string EffectiveBindingMode => UserScopedAvailable ? _bindingMode : "TenantLevel";

    private string? ArgumentsJsonForSave() =>
        string.IsNullOrWhiteSpace(_argumentsJson) ? null : _argumentsJson;
}
