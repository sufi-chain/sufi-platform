using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Blazor.Components;

public partial class NewDirectMessageDialog : ChatComponentBase
{
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

    protected async Task OnOpenChangedAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);

        if (open)
        {
            await SearchAsync();
        }
    }

    protected async Task SearchAsync()
    {
        IsLoading = true;
        try
        {
            var result = await ContactAppService.SearchAsync(new SearchChatContactsInput
            {
                Filter = string.IsNullOrWhiteSpace(FilterText) ? null : FilterText,
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
}
