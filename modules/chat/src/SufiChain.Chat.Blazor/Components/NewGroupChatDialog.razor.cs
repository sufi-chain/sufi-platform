using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Components;

public partial class NewGroupChatDialog : ChatComponentBase
{
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public EventCallback<Guid> OnCreated { get; set; }

    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    protected CreateGroupChatSessionInput Input { get; set; } = new();

    protected async Task OnOpenChangedAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);

        if (open)
        {
            Input = new CreateGroupChatSessionInput();
        }
    }

    protected async Task CreateAsync()
    {
        try
        {
            var session = await SessionAppService.CreateGroupSessionAsync(Input);
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
