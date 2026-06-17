using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class AIToolTestPanel : AIManagementComponentBase
{
    private static class ToolChatLoadingKeys
    {
        public const string LoadWorkspaces = "tool-chat-load-workspaces";
        public const string SendMessage = "tool-chat-send-message";
    }

    private ISufiAbpAIChatAppService AIChatAppService => LazyGetRequiredService(ref _aiChatAppService);
    private ISufiAbpAIChatAppService? _aiChatAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private string _messageInput = string.Empty;
    private List<ToolChatMessage> _messages = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadWorkspacesAsync();
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100
            });

            _workspaces = result.Items.Where(w => w.IsActive).ToList();
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
        if (string.IsNullOrWhiteSpace(_selectedWorkspaceName) || string.IsNullOrWhiteSpace(_messageInput))
        {
            return;
        }

        var userMessage = _messageInput;
        _messageInput = string.Empty;
        _messages.Add(new ToolChatMessage { IsUser = true, Content = userMessage });

        await ExecuteWithLoadingAsync(async () =>
        {
            var response = await AIChatAppService.SendMessageWithToolsAsync(new SufiAbpAISendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 1))
                    .Select(message => new SufiAbpAIChatMessageDto
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
