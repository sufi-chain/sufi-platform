using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Components;

public partial class AddGroupMembersDialog : ChatComponentBase, IDisposable
{
    private const int SearchDebounceMilliseconds = 300;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public Guid? SessionId { get; set; }

    [Parameter]
    public IReadOnlyCollection<Guid> ExistingMemberUserIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public EventCallback OnMembersAdded { get; set; }

    [Inject]
    protected IChatContactAppService ContactAppService { get; set; } = default!;

    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    protected string FilterText { get; set; } = string.Empty;

    protected IReadOnlyList<ChatContactDto> Contacts { get; set; } = Array.Empty<ChatContactDto>();

    protected HashSet<Guid> SelectedUserIds { get; } = new();

    protected bool IsLoadingContacts { get; set; }

    protected bool CanSearch =>
        !string.IsNullOrWhiteSpace(FilterText) &&
        FilterText.Trim().Length >= ChatContactSearchConsts.MinFilterLength;

    private CancellationTokenSource? _searchDebounceCts;

    protected async Task OnOpenChangedAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);

        if (open)
        {
            FilterText = string.Empty;
            Contacts = Array.Empty<ChatContactDto>();
            SelectedUserIds.Clear();
            IsLoadingContacts = false;
            return;
        }

        CancelPendingSearch();
    }

    protected async Task OnFilterTextChangedAsync()
    {
        CancelPendingSearch();

        if (!CanSearch)
        {
            Contacts = Array.Empty<ChatContactDto>();
            IsLoadingContacts = false;
            return;
        }

        _searchDebounceCts = new CancellationTokenSource();
        var cancellationToken = _searchDebounceCts.Token;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, cancellationToken);
            await SearchAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected async Task SearchAsync()
    {
        if (!CanSearch)
        {
            Contacts = Array.Empty<ChatContactDto>();
            return;
        }

        IsLoadingContacts = true;
        try
        {
            var result = await ContactAppService.SearchAsync(new SearchChatContactsInput
            {
                Filter = FilterText.Trim(),
                MaxResultCount = 20,
                SkipCount = 0
            });

            Contacts = result.Items
                .Where(contact => !ExistingMemberUserIds.Contains(contact.Id))
                .ToList();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsLoadingContacts = false;
        }
    }

    protected Task ToggleContactAsync(ChatContactDto contact)
    {
        if (!SelectedUserIds.Add(contact.Id))
        {
            SelectedUserIds.Remove(contact.Id);
        }

        return Task.CompletedTask;
    }

    protected bool IsSelected(ChatContactDto contact) => SelectedUserIds.Contains(contact.Id);

    protected async Task AddMembersAsync()
    {
        if (!SessionId.HasValue || SelectedUserIds.Count == 0)
        {
            return;
        }

        try
        {
            foreach (var userId in SelectedUserIds)
            {
                var contact = Contacts.FirstOrDefault(item => item.Id == userId);
                await SessionAppService.AddParticipantAsync(SessionId.Value, new AddChatParticipantInput
                {
                    UserId = userId,
                    ParticipantKind = ChatMessageSenderKind.Visitor,
                    DisplayName = contact?.DisplayName
                });
            }

            await OnMembersAdded.InvokeAsync();
            await OnOpenChangedAsync(false);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected async Task CloseAsync()
    {
        await OnOpenChangedAsync(false);
    }

    public void Dispose()
    {
        CancelPendingSearch();
    }

    private void CancelPendingSearch()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = null;
    }
}
