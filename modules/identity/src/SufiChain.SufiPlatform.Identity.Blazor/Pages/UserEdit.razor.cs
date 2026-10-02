using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Identity.Blazor.Pages;

public partial class UserEdit : IdentityComponentBase
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Parameter]
    public Guid UserId { get; set; }

    protected override void OnInitialized()
    {
        PageLayout.Title = L["EditUser"];
    }
}
