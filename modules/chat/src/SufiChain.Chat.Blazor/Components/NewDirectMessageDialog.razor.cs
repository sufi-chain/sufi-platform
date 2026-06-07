using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Blazor.Components;

public partial class NewDirectMessageDialog : ChatComponentBase, IDisposable
{
    private const int SearchDebounceMilliseconds = 300;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public EventCallback<Guid> OnCreated { get; set; }

    [Inject]
    protected IChatContactAppService ContactAppService { get; set; } = default!;

    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    protected string FilterText { get; set; } = string.Empty;

    protected IReadOnlyList<ChatContactDto> Contacts { get; set; } = Array.Empty<ChatContactDto>();

    protected bool IsLoading { get; set; }

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
            IsLoading = false;
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
            IsLoading = false;
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

        IsLoading = true;
        try
        {
            var result = await ContactAppService.SearchAsync(new SearchChatContactsInput
            {
                Filter = FilterText.Trim(),
                MaxResultCount = 20,
                SkipCount = 0
            });

            Contacts = result.Items;
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

    protected async Task SelectContactAsync(ChatContactDto contact)
    {
        try
        {
            var session = await SessionAppService.GetOrCreateDirectSessionAsync(new GetOrCreateDirectSessionInput
            {
                OtherUserId = contact.Id
            });

            await OnCreated.InvokeAsync(session.Id);
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
