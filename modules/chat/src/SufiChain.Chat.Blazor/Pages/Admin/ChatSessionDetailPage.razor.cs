using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Transcripts;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Sessions.Default)]
public partial class ChatSessionDetailPage : ChatComponentBase
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    [Inject]
    protected IChatMessageAppService MessageAppService { get; set; } = default!;

    [Inject]
    protected IConversationLinkAppService LinkAppService { get; set; } = default!;

    [Inject]
    protected IChatTranscriptExporter TranscriptExporter { get; set; } = default!;

    protected ChatSessionDto? Session { get; set; }

    protected List<ChatMessageDto> Messages { get; set; } = new();

    protected List<ConversationLinkDto> Links { get; set; } = new();

    protected string? TranscriptText { get; set; }

    protected bool IsLoading { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

    protected virtual async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            Session = await SessionAppService.GetAsync(Id);
            var messages = await MessageAppService.GetListAsync(new GetChatMessageListInput
            {
                SessionId = Id,
                MaxResultCount = 100,
                SkipCount = 0,
                IncludeInternal = true,
                Sorting = "CreationTime"
            });
            Messages = messages.Items.ToList();
            Links = await LinkAppService.GetBySessionAsync(Id);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task ExportTranscriptAsync()
    {
        try
        {
            TranscriptText = await TranscriptExporter.ExportAsPlainTextAsync(Id, new ChatTranscriptExportOptions
            {
                IncludeInternalMessages = true,
                IncludeLinks = true
            });
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }
}
