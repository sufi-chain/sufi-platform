using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;

namespace SufiChain.Chat.Blazor.Components;

/// <summary>
/// Modal dialog for creating or editing a tenant assistant mapping row.
/// </summary>
public partial class ChatAssistantMappingEditDialog : ChatComponentBase
{
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public bool IsCreate { get; set; }

    [Parameter]
    public ChatAssistantMappingDto? Mapping { get; set; }

    [Parameter]
    public IReadOnlyList<ChatAiWorkspaceOptionDto> WorkspaceOptions { get; set; } = Array.Empty<ChatAiWorkspaceOptionDto>();

    [Parameter]
    public IReadOnlyList<string> ExistingKeys { get; set; } = Array.Empty<string>();

    [Parameter]
    public EventCallback<ChatAssistantMappingDto> OnSaved { get; set; }

    protected ChatAssistantMappingDto _editModel { get; set; } = new();

    protected string DialogTitle =>
        IsCreate ? L["AssistantMappings:CreateTitle"] : L["AssistantMappings:EditTitle"];

    protected bool CanSave =>
        !string.IsNullOrWhiteSpace(_editModel.Key) &&
        !string.IsNullOrWhiteSpace(_editModel.DisplayName) &&
        !string.IsNullOrWhiteSpace(_editModel.WorkspaceName);

    protected bool IsSelectedWorkspaceHealthy =>
        WorkspaceOptions.Any(option =>
            option.Name.Equals(_editModel.WorkspaceName, StringComparison.OrdinalIgnoreCase) &&
            option.IsHealthy);

    private bool _wasOpen;

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            ResetForm();
        }

        _wasOpen = Open;
    }

    protected virtual void ResetForm()
    {
        _editModel = Mapping == null
            ? new ChatAssistantMappingDto
            {
                IsEnabled = true,
                IsPublic = true
            }
            : new ChatAssistantMappingDto
            {
                Key = Mapping.Key,
                DisplayName = Mapping.DisplayName,
                WorkspaceName = Mapping.WorkspaceName,
                IsEnabled = Mapping.IsEnabled,
                IsPublic = Mapping.IsPublic,
                IsWorkspaceHealthy = Mapping.IsWorkspaceHealthy
            };
    }

    protected virtual async Task OnOpenChangedAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    protected virtual async Task SaveAsync()
    {
        if (!CanSave)
        {
            return;
        }

        var key = _editModel.Key.Trim().ToLowerInvariant();
        if (!ChatAssistantMappings.IsValidKey(key))
        {
            await Message.ErrorAsync(L["AssistantMappings:KeyInvalid"]);
            return;
        }

        if (ExistingKeys.Any(existing =>
                existing.Equals(key, StringComparison.OrdinalIgnoreCase)))
        {
            await Message.ErrorAsync(L["AssistantMappings:KeyDuplicate", key]);
            return;
        }

        var workspace = WorkspaceOptions.FirstOrDefault(option =>
            option.Name.Equals(_editModel.WorkspaceName, StringComparison.OrdinalIgnoreCase));

        var saved = new ChatAssistantMappingDto
        {
            Key = key,
            DisplayName = _editModel.DisplayName.Trim(),
            WorkspaceName = _editModel.WorkspaceName.Trim(),
            IsEnabled = _editModel.IsEnabled,
            IsPublic = _editModel.IsPublic,
            IsWorkspaceHealthy = workspace?.IsHealthy == true
        };

        await OnSaved.InvokeAsync(saved);
        await OnOpenChangedAsync(false);
    }

    protected virtual Task CloseAsync()
    {
        return OnOpenChangedAsync(false);
    }
}
