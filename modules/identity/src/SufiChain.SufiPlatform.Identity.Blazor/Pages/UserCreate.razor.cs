using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Identity.Blazor.Pages;

public partial class UserCreate : IdentityComponentBase
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["CreateUser"];
    }
}
