using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class Workspaces : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string DeleteWorkspace = "delete-workspace";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private SbDataGrid<WorkspaceDto>? _gridRef;
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private Guid? _editingWorkspaceId;
    private bool _showModelConfigurationsModal;
    private Guid? _modelConfigurationsWorkspaceId;
    private string? _modelConfigurationsWorkspaceName;
    private bool _showMCPToolsModal;
    private Guid? _mcpToolsWorkspaceId;

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

    private void OpenModelConfigurationsModal(WorkspaceDto workspace)
    {
        _modelConfigurationsWorkspaceId = workspace.Id;
        _modelConfigurationsWorkspaceName = workspace.Name;
        _showModelConfigurationsModal = true;
    }

    private void OpenMCPToolsModal(WorkspaceDto workspace)
    {
        _mcpToolsWorkspaceId = workspace.Id;
        _showMCPToolsModal = true;
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

    private async Task OnMCPToolsUpdatedAsync()
    {
        _showMCPToolsModal = false;
        await Message.SuccessAsync(L["MCPToolsUpdatedSuccessfully"]);
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadWorkspaces);
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
