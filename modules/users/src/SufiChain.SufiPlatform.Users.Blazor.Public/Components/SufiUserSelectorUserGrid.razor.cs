using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Users;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Users.Blazor.Public.Components;

public partial class SufiUserSelectorUserGrid : UsersPublicComponentBase
{
    private SbDataGrid<UserLookupDto>? _gridRef;

    [Parameter, EditorRequired]
    public Func<SbDataRequest, Task<SbDataResponse<UserLookupDto>>> ItemsProvider { get; set; } = default!;

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public int PageIndex { get; set; }

    [Parameter]
    public EventCallback<int> PageIndexChanged { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    [Parameter]
    public long TotalCount { get; set; }

    [Parameter]
    public SbSelectionMode SelectionMode { get; set; } = SbSelectionMode.SingleRow;

    [Parameter]
    public IReadOnlySet<string> SelectedKeys { get; set; } = new HashSet<string>();

    [Parameter]
    public EventCallback<IReadOnlySet<string>> SelectedKeysChanged { get; set; }

    [Parameter]
    public EventCallback<UserLookupDto> OnRowClicked { get; set; }

    public Task RefreshDataAsync()
    {
        return _gridRef?.RefreshDataAsync() ?? Task.CompletedTask;
    }
}
