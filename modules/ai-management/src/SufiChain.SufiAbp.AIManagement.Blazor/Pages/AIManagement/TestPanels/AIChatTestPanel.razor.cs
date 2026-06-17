using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class AIChatTestPanel : AIManagementComponentBase
{
    private static class ChatLoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string SendMessage = "send-message";
    }

    
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private ISufiAbpAIChatAppService AIChatAppService => LazyGetRequiredService(ref _aiChatAppService);
    private ISufiAbpAIChatAppService? _aiChatAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private string _messageInput = string.Empty;
    private List<ChatMessage> _messages = new();
    private bool _useStreaming = false;

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
            StateHasChanged();
        }, ChatLoadingKeys.LoadWorkspaces);
    }

    private Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        
        var workspace = _workspaces.FirstOrDefault(w => w.Id == workspaceId);
        _selectedWorkspaceName = workspace?.Name ?? string.Empty;
        
        return Task.CompletedTask;
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedWorkspaceName) || string.IsNullOrWhiteSpace(_messageInput))
        {
            return;
        }

         // Validate workspace exists
         if (!_workspaces.Any(w => w.Name == _selectedWorkspaceName))
         {
             await HandleErrorAsync(new InvalidOperationException($"Workspace '{_selectedWorkspaceName}' not found. Please select a valid workspace."));
             return;
         }

        var userMessage = _messageInput;
        _messageInput = string.Empty;

        // Add user message
        _messages.Add(new ChatMessage
        {
            IsUser = true,
            Content = userMessage,
            Timestamp = DateTime.Now
        });
        StateHasChanged();

        if (_useStreaming)
        {
            await SendMessageStreamingAsync(userMessage);
        }
        else
        {
            await SendMessageNonStreamingAsync(userMessage);
        }
    }

    private async Task SendMessageStreamingAsync(string userMessage)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var startTime = DateTime.Now;
            int? totalTokens = null;

             // Add placeholder for assistant message inside try block
             var assistantMessage = new ChatMessage
             {
                 IsUser = false,
                 Content = string.Empty,
                 Timestamp = DateTime.Now
             };
             _messages.Add(assistantMessage);

            var input = new SufiAbpAISendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 2))
                    .Where(m => m.IsUser || !string.IsNullOrEmpty(m.Content))
                    .Select(m => new SufiAbpAIChatMessageDto
                    {
                        Role = m.IsUser ? "user" : "assistant",
                        Content = m.Content
                    })
                    .ToList()
            };

            try
            {
                await foreach (var chunk in AIChatAppService.StreamMessageAsync(input))
                {
                    assistantMessage.Content += chunk.Message;
                    totalTokens = chunk.TokensUsed;
                    StateHasChanged();
                }
                
                var latency = (DateTime.Now - startTime).TotalMilliseconds;

                // Update metadata after streaming completes
                assistantMessage.Metadata = new Dictionary<string, object>
                {
                    ["tokens"] = totalTokens,
                    ["latency"] = (int)latency
                };
            }
            catch (Exception ex)
            {
                // Display error in the assistant message
                assistantMessage.Content = $"❌ Error: {ex.Message}";
                assistantMessage.Metadata = new Dictionary<string, object>
                {
                    ["error"] = true,
                    ["errorMessage"] = ex.Message
                };
                
                // Re-throw to let ExecuteWithLoadingAsync handle it
                throw;
            }
            
            StateHasChanged();
        }, ChatLoadingKeys.SendMessage);
    }

    private async Task SendMessageNonStreamingAsync(string userMessage)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var startTime = DateTime.Now;
            
            var input = new SufiAbpAISendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 1))
                    .Where(m => m.IsUser || !string.IsNullOrEmpty(m.Content))
                    .Select(m => new SufiAbpAIChatMessageDto
                    {
                        Role = m.IsUser ? "user" : "assistant",
                        Content = m.Content
                    })
                    .ToList()
            };

            var response = await AIChatAppService.SendMessageAsync(input);
            
            var latency = (DateTime.Now - startTime).TotalMilliseconds;

            // Add assistant response
            _messages.Add(new ChatMessage
            {
                IsUser = false,
                Content = response.Message,
                Timestamp = DateTime.Now,
                Metadata = new Dictionary<string, object>
                {
                    ["tokens"] = response.TokensUsed,
                    ["latency"] = (int)latency
                }
            });
            
            StateHasChanged();
        }, ChatLoadingKeys.SendMessage);
    }

    private void ClearChat()
    {
        _messages.Clear();
        _messageInput = string.Empty;
    }

    private class ChatMessage
    {
        public bool IsUser { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
