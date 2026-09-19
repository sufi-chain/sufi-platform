using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiBlazor.Components;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class Workspaces : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string DeleteWorkspace = "delete-workspace";
        public const string ConvertWorkspace = "convert-workspace";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected new ICurrentTenant CurrentTenant { get; set; } = default!;
    

    private SbDataGrid<WorkspaceDto>? _gridRef;
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private Guid? _editingWorkspaceId;
    private bool _showCloneModal;
    private Guid? _cloneWorkspaceId;
    private string? _cloneSourceName;
    private bool _showAssignModal;
    private Guid? _assignWorkspaceId;
    private string? _assignSourceName;

    private bool IsHost => CurrentTenant.Id == null;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadWorkspaces);
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Workspaces"];
    }

    private async Task<SbDataResponse<WorkspaceDto>> LoadWorkspacesDataAsync(SbDataRequest request)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize,
            Sorting = "CreationTime DESC"
        };

        var result = await WorkspaceAppService.GetListAsync(input);
        _totalCount = result.TotalCount;
        return new SbDataResponse<WorkspaceDto>(result.Items, result.TotalCount);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
    }

    private async Task OnPageSizeChangedAsync(int pageSize)
    {
        _pageSize = pageSize;
        _pageIndex = 0;
    }

    private void OpenCreateModal()
    {
        _showCreateModal = true;
    }

    private void OpenEditModal(WorkspaceDto workspace)
    {
        _editingWorkspaceId = workspace.Id;
        _showEditModal = true;
    }

    private void OpenCloneModal(WorkspaceDto workspace)
    {
        _cloneWorkspaceId = workspace.Id;
        _cloneSourceName = workspace.Name;
        _showCloneModal = true;
    }

    private void OpenAssignModal(WorkspaceDto workspace)
    {
        _assignWorkspaceId = workspace.Id;
        _assignSourceName = workspace.Name;
        _showAssignModal = true;
    }

    private void OpenModelConfigurations(WorkspaceDto workspace)
    {
        Navigation.NavigateTo($"/panel/admin/ai/workspaces/{workspace.Id}/model-configurations");
    }

    private async Task OnWorkspaceCreatedAsync()
    {
        _showCreateModal = false;
        await Message.SuccessAsync(L["WorkspaceCreatedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceUpdatedAsync()
    {
        _showEditModal = false;
        await Message.SuccessAsync(L["WorkspaceUpdatedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceConvertedAsync()
    {
        _showEditModal = false;
        await Message.SuccessAsync(L["WorkspaceConvertedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceClonedAsync()
    {
        _showCloneModal = false;
        await Message.SuccessAsync(L["WorkspaceClonedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceAssignedAsync()
    {
        _showAssignModal = false;
        await Message.SuccessAsync(L["WorkspaceAssignedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
    }

    private async Task ConvertToCustomAsync(WorkspaceDto workspace)
    {
        var confirmed = await Message.ConfirmAsync(
            L["ConvertToCustomConfirmation", workspace.Name],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.ConvertToCustomAsync(workspace.Id);
            await Message.SuccessAsync(L["WorkspaceConvertedSuccessfully"]);
            await (_gridRef?.RefreshDataAsync() ?? Task.CompletedTask);
        }, LoadingKeys.ConvertWorkspace);
    }

    private async Task DeleteWorkspaceAsync(WorkspaceDto workspace)
    {
        var confirmed = await Message.ConfirmAsync(
            L["WorkspaceDeleteConfirmationMessage", workspace.Name],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.DeleteAsync(workspace.Id);
            await Message.SuccessAsync(L["WorkspaceDeletedSuccessfully"]);
            await (_gridRef?.RefreshDataAsync() ?? Task.CompletedTask);
        }, LoadingKeys.DeleteWorkspace);
    }

    private SbColor GetProviderColor(AIProviderType provider)
    {
        return provider switch
        {
            AIProviderType.OpenAI => SbColor.Primary,
            _ => SbColor.Default
        };
    }
}
