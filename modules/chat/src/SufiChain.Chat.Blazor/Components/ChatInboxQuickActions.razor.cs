using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;

namespace SufiChain.Chat.Blazor.Components;

/// <summary>
/// Inbox quick actions (new DM, group, AI chat) for the messenger context rail and mobile list header.
/// </summary>
public partial class ChatInboxQuickActions : ChatComponentBase
{
    [Parameter]
    public bool CanCreateDirectMessages { get; set; }

    [Parameter]
    public bool CanCreateGroups { get; set; }

    [Parameter]
    public bool ShowStartAiChat { get; set; }

    [Parameter]
    public string? AssistantUnavailableMessageKey { get; set; }

    [Parameter]
    public EventCallback OnNewDirectMessage { get; set; }

    [Parameter]
    public EventCallback OnNewGroup { get; set; }

    [Parameter]
    public EventCallback OnStartAiChat { get; set; }

    [Parameter]
    public EventCallback<string?> OnStartAssistantChat { get; set; }

    [Parameter]
    public IReadOnlyList<ChatAssistantPickerOptionDto> AssistantOptions { get; set; } = Array.Empty<ChatAssistantPickerOptionDto>();

    [Parameter]
    public string? Class { get; set; }

    protected virtual Task StartAssistantAsync(string? assistantKey)
    {
        if (OnStartAssistantChat.HasDelegate)
        {
            return OnStartAssistantChat.InvokeAsync(assistantKey);
        }

        return OnStartAiChat.InvokeAsync();
    }
}
