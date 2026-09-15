using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceAssignToTenantModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string Assign = "assign-workspace";
        public const string LoadTenants = "load-tenants";
        public const string LoadAssignments = "load-assignments";
        public const string Deactivate = "deactivate-assignment";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public string? SourceName { get; set; }
    [Parameter] public EventCallback OnAssigned { get; set; }

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private List<AssignableTenantDto> _tenants = new();
    private List<WorkspaceAssignmentDto> _assignments = new();
    private Guid _tenantId;
    private string _name = string.Empty;
    private bool _wasOpen;

    private IEnumerable<AssignableTenantDto> UnassignedTenants =>
        _tenants.Where(tenant => _assignments.All(assignment =>
            !assignment.IsActive || assignment.TenantId != tenant.Id));

    protected override async Task OnParametersSetAsync()
    {
        if (Open && !_wasOpen)
        {
            _tenantId = Guid.Empty;
            _name = SourceName ?? string.Empty;
            await LoadTenantsAsync();
            await LoadAssignmentsAsync();
        }

        _wasOpen = Open;
    }

    private async Task LoadTenantsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            _tenants = await WorkspaceAppService.GetAssignableTenantsAsync();
        }, LoadingKeys.LoadTenants);
    }

    private async Task LoadAssignmentsAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            _assignments = [];
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _assignments = await WorkspaceAppService.GetAssignmentsAsync(WorkspaceId.Value);
        }, LoadingKeys.LoadAssignments);
    }

    private async Task AssignAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        if (_tenantId == Guid.Empty)
        {
            await Message.ErrorAsync(L["TenantRequired"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.AssignToTenantAsync(
                WorkspaceId.Value,
                new AssignWorkspaceToTenantDto
                {
                    TenantId = _tenantId,
                    Name = string.IsNullOrWhiteSpace(_name) ? null : _name.Trim()
                });
            await CloseModal();
            await OnAssigned.InvokeAsync();
        }, LoadingKeys.Assign);
    }

    private async Task DeactivateAssignmentAsync(WorkspaceAssignmentDto assignment)
    {
        var tenantLabel = assignment.TenantName ?? assignment.TenantId?.ToString() ?? string.Empty;
        var confirmed = await Message.ConfirmAsync(
            L["DeactivateAssignmentConfirmation", tenantLabel],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.DeactivateAssignmentAsync(assignment.Id);
            await Message.SuccessAsync(L["AssignmentDeactivatedSuccessfully"]);
            await LoadAssignmentsAsync();
        }, LoadingKeys.Deactivate);
    }

    private Task CloseModal() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }
}