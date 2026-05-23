using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class TestChat : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string SendMessage = "send-message";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private string _messageInput = string.Empty;
    private List<ChatMessage> _messages = new();
    private bool _useStreaming = true;

    protected override async Task OnInitializedAsync()
    {
        SetupPageLayout();
        await LoadWorkspacesAsync();
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["TestChat"];
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
        }, LoadingKeys.LoadWorkspaces);
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
        // Add placeholder for assistant message
        var assistantMessage = new ChatMessage
        {
            IsUser = false,
            Content = string.Empty,
            Timestamp = DateTime.Now
        };
        _messages.Add(assistantMessage);
        StateHasChanged();

        await ExecuteWithLoadingAsync(async () =>
        {
            var startTime = DateTime.Now;
            int? totalTokens = null;

            var input = new SendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 2))
                    .Where(m => m.IsUser || !string.IsNullOrEmpty(m.Content))
                    .Select(m => new ChatMessageDto
                    {
                        Role = m.IsUser ? "user" : "assistant",
                        Content = m.Content
                    })
                    .ToList(),
                Stream = true
            };

            try
            {
                await foreach (var chunk in AIAppService.StreamChatMessageAsync(input))
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
        }, LoadingKeys.SendMessage);
    }

    private async Task SendMessageNonStreamingAsync(string userMessage)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var startTime = DateTime.Now;
            
            var input = new SendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = userMessage,
                ConversationHistory = _messages
                    .Take(Math.Max(0, _messages.Count - 1))
                    .Where(m => m.IsUser || !string.IsNullOrEmpty(m.Content))
                    .Select(m => new ChatMessageDto
                    {
                        Role = m.IsUser ? "user" : "assistant",
                        Content = m.Content
                    })
                    .ToList(),
                Stream = false
            };

            var response = await AIAppService.SendChatMessageAsync(input);
            
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
        }, LoadingKeys.SendMessage);
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
