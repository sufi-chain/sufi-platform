using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class TestChat : AIComponentBase
{
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private int _activeTab;

    protected override Task OnInitializedAsync()
    {
        PageLayout.Title = L["TestChat"];
        return Task.CompletedTask;
    }
}
