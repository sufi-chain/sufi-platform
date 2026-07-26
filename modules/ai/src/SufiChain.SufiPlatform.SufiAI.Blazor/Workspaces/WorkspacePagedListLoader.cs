using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Workspaces;

/// <summary>
/// Loads all active workspaces for admin dropdowns by paging through the workspace list API.
/// </summary>
public static class WorkspacePagedListLoader
{
    /// <summary>Page size when loading workspaces for dropdown filters.</summary>
    private const int PageSize = 50;

    public static async Task<List<WorkspaceDto>> LoadAllActiveAsync(IWorkspaceAppService workspaceAppService)
    {
        var workspaces = new List<WorkspaceDto>();
        var skipCount = 0;
        long totalCount;

        // Dropdowns load active workspaces in pages until TotalCount is reached.
        do
        {
            var result = await workspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                SkipCount = skipCount,
                MaxResultCount = PageSize
            });

            workspaces.AddRange(result.Items.Where(w => w.IsActive));
            skipCount += PageSize;
            totalCount = result.TotalCount;
        } while (skipCount < totalCount);

        return workspaces;
    }
}
