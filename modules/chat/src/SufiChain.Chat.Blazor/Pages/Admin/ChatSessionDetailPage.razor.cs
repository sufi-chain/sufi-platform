using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Transcripts;
using SufiChain.SufiBlazor.Components;

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

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected ChatSessionDto? Session { get; set; }

    protected List<ChatMessageDto> Messages { get; set; } = new();

    protected List<ConversationLinkDto> Links { get; set; } = new();

    protected string? TranscriptText { get; set; }

    protected bool IsLoading { get; set; }

    protected bool IsExporting { get; set; }

    protected bool IsClosing { get; set; }

    protected bool CanClose { get; set; }

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
            CanClose = await AuthorizationService.IsGrantedAsync(ChatPermissions.Sessions.Close);
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
        IsExporting = true;
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
        finally
        {
            IsExporting = false;
        }
    }

    protected virtual async Task CloseSessionAsync()
    {
        if (Session == null || Session.Status != ChatSessionStatus.Open)
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(L["CloseSession:Confirm"]);
        if (!confirmed)
        {
            return;
        }

        IsClosing = true;
        try
        {
            await SessionAppService.CloseAsync(Id);
            await Message.SuccessAsync(L["CloseSession:Success"]);
            await LoadAsync();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsClosing = false;
        }
    }

    protected virtual void BackToList()
    {
        NavigationManager.NavigateTo("/admin/chat/sessions");
    }

    protected static SbColor GetStatusChipColor(ChatSessionStatus status) => status switch
    {
        ChatSessionStatus.Open => SbColor.Success,
        ChatSessionStatus.Closed => SbColor.Muted,
        _ => SbColor.Default
    };

    protected static SbColor GetAccessModeChipColor(AccessMode mode) => mode switch
    {
        AccessMode.PublicAnonymous => SbColor.Warning,
        AccessMode.PublicAuthenticated => SbColor.Info,
        AccessMode.Internal => SbColor.Primary,
        _ => SbColor.Default
    };

    protected static string GetParticipantIcon(ChatMessageSenderKind kind) => kind switch
    {
        ChatMessageSenderKind.Visitor => "user",
        ChatMessageSenderKind.Operator => "shield",
        ChatMessageSenderKind.Assistant => "sparkles",
        ChatMessageSenderKind.System => "settings",
        _ => "user"
    };
}
