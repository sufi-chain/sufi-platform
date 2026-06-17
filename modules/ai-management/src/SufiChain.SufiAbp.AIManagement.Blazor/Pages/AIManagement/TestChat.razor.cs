using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class TestChat : AIManagementComponentBase
{
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private int _activeTab;

    protected override Task OnInitializedAsync()
    {
        PageLayout.Title = L["TestChat"];
        return Task.CompletedTask;
    }
}
