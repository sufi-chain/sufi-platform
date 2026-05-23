using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.RAG;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class RAG : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string Search = "search";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private IRAGAppService RAGAppService => LazyGetRequiredService(ref _ragAppService);
    private IRAGAppService? _ragAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private string _query = string.Empty;
    private string _maxResultsText = "10";
    private List<DocumentChunkDto> _searchResults = new();
    private bool _hasSearched = false;

    protected override async Task OnInitializedAsync()
    {
        SetupPageLayout();
        await LoadWorkspacesAsync();
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["RAG:Search"];
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100
            });
            _workspaces = result.Items.Where(w => w.IsActive).ToList();
            StateHasChanged();
        }, LoadingKeys.LoadWorkspaces);
    }

    private Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        return Task.CompletedTask;
    }

    private async Task SearchAsync()
    {
        if (!_selectedWorkspaceId.HasValue || string.IsNullOrWhiteSpace(_query))
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
            var input = new SearchDocumentsInput
            {
                WorkspaceName = workspace.Name,
                Query = _query,
                MaxResults = int.TryParse(_maxResultsText, out var max) ? max : 10
            };

            _searchResults = await RAGAppService.SearchDocumentsAsync(input);
            _hasSearched = true;
            StateHasChanged();
        }, LoadingKeys.Search);
    }

    private void ClearSearch()
    {
        _query = string.Empty;
        _searchResults.Clear();
        _hasSearched = false;
        _maxResultsText = "10";
    }
}
