using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.RAG;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class IndexingStatus : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string LoadSources = "load-sources";
        public const string StartIndexing = "start-indexing";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private IRAGAppService RAGAppService => LazyGetRequiredService(ref _ragAppService);
    private IRAGAppService? _ragAppService;

    private SbDataGrid<DocumentSourceDto>? _gridRef;
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private List<DocumentSourceDto> _documentSources = new();

    protected override async Task OnInitializedAsync()
    {
        SetupPageLayout();
        await LoadWorkspacesAsync();
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["IndexingStatus"];
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            // TODO: Implement proper lazy loading with pagination
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100
            });
            _workspaces = result.Items.Where(w => w.IsActive).ToList();
            StateHasChanged();
        }, LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        if (_selectedWorkspaceId.HasValue)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                if (_gridRef != null)
                {
                    await _gridRef.RefreshDataAsync();
                }
            });
        }
    }

    private async Task<SbDataResponse<DocumentSourceDto>> LoadDocumentSourcesDataAsync(SbDataRequest request)
    {
        if (!_selectedWorkspaceId.HasValue)
        {
            return new SbDataResponse<DocumentSourceDto>(new List<DocumentSourceDto>(), 0);
        }

        _documentSources = await RAGAppService.GetDocumentSourcesAsync();
        _totalCount = _documentSources.Count;
        
        var pagedSources = _documentSources
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new SbDataResponse<DocumentSourceDto>(pagedSources, _totalCount);
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

    private async Task RefreshStatusAsync()
    {
        if (_selectedWorkspaceId.HasValue)
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                if (_gridRef != null)
                {
                    await _gridRef.RefreshDataAsync();
                }
            });
        }
    }

    private async Task StartIndexingAsync(DocumentSourceDto source)
    {
        if (!_selectedWorkspaceId.HasValue)
        {
            return;
        }

        var workspace = _workspaces.FirstOrDefault(w => w.Id == _selectedWorkspaceId.Value);
        if (workspace == null)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await RAGAppService.StartIndexingAsync(workspace.Name, source.Name);
            await Message.Success(L["IndexingStartedSuccessfully"]);
            if (_gridRef != null)
            {
                await _gridRef.RefreshDataAsync();
            }
        });
    }

    private SbColor GetStatusColor(IndexingStatusType status)
    {
        return status switch
        {
            IndexingStatusType.Pending => SbColor.Default,
            IndexingStatusType.Indexing => SbColor.Info,
            IndexingStatusType.Complete => SbColor.Success,
            IndexingStatusType.Failed => SbColor.Danger,
            _ => SbColor.Default
        };
    }

    private string GetStatusText(IndexingStatusType status)
    {
        return status switch
        {
            IndexingStatusType.Pending => L["Pending"],
            IndexingStatusType.Indexing => L["Indexing"],
            IndexingStatusType.Complete => L["Complete"],
            IndexingStatusType.Failed => L["Failed"],
            _ => L["Unknown"]
        };
    }
}
