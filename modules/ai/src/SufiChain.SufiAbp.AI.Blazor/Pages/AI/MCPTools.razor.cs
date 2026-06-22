using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI.MCP.Tools;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AI.Blazor.Pages.AI;

public partial class MCPTools
{
    private static class LoadingKeys
    {
        public const string LoadTools = nameof(LoadTools);
        public const string LoadWorkspaces = nameof(LoadWorkspaces);
    }
    
    private List<WorkspaceDto> _workspaces = new();
    private List<MCPToolDto> _tools = new();
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private bool _schemaDialogOpen;
    private MCPToolDto? _selectedTool;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadWorkspacesAsync();
        
        if (_workspaces.Count > 0)
        {
            _selectedWorkspaceId = _workspaces[0].Id;
            _selectedWorkspaceName = _workspaces[0].Name;
            await LoadToolsAsync();
        }
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
        }, LoadingKeys.LoadWorkspaces);
    }
    
    private async Task LoadToolsAsync()
    {
        if (string.IsNullOrEmpty(_selectedWorkspaceName))
            return;
        
        await ExecuteWithLoadingAsync(async () =>
        {
            _tools = await MCPToolAppService.GetToolsForWorkspaceAsync(_selectedWorkspaceName);
        }, LoadingKeys.LoadTools);
    }
    
    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        
        var workspace = _workspaces.FirstOrDefault(w => w.Id == workspaceId);
        _selectedWorkspaceName = workspace?.Name ?? string.Empty;
        
        await LoadToolsAsync();
    }
    
    private async Task RefreshToolsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await MCPToolAppService.RefreshToolRegistryAsync();
            await LoadToolsAsync();
        }, LoadingKeys.LoadTools);
    }
    
    private void ViewToolSchema(MCPToolDto tool)
    {
        _selectedTool = tool;
        _schemaDialogOpen = true;
    }
}
