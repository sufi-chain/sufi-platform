using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity.Blazor.Components;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Identity.Blazor.Pages;

public partial class UserManagement : IdentityComponentBase
{

    private const string PermissionProviderName = "U"; // User provider

    private const string CreateUserRoute = "/panel/admin/identity/users/new";

    private static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
        public const string DeleteUser = "delete-user";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IIdentityUserAppService UserAppService => LazyGetRequiredService(ref _userAppService);
    private IIdentityUserAppService? _userAppService;

    private SbDataGrid<IdentityUserDto>? _gridRef;
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    // Permission management
    private PermissionsModal? _permissionManagementModal;
    private bool _hasManagePermissionsPermission;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            _hasManagePermissionsPermission = await IsGrantedAsync(IdentityPermissions.Users.ManagePermissions);
            await RefreshGridAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Users"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private async Task<SbDataResponse<IdentityUserDto>> LoadUsersDataAsync(SbDataRequest request)
    {
        var result = await UserAppService.GetListAsync(new GetIdentityUsersInput
        {
            Filter = request.GetFilterValue("UserName", "Email", "PhoneNumber") ?? _filter,
            Sorting = BuildUserSorting(request),
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        _totalCount = result.TotalCount;
        return new SbDataResponse<IdentityUserDto>(result.Items, result.TotalCount);
    }

    private static readonly HashSet<string> UserSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "UserName",
        "Email",
        "PhoneNumber",
        "IsActive",
        "Name",
        "Surname"
    };

    private static string? BuildUserSorting(SbDataRequest request)
    {
        if (request.Sorts.Count == 0)
        {
            return null;
        }

        var parts = new List<string>();
        foreach (var sort in request.Sorts)
        {
            if (string.IsNullOrWhiteSpace(sort.Field) || !UserSortFields.Contains(sort.Field))
            {
                continue;
            }

            parts.Add(sort.Direction == SbSortDirection.Descending ? sort.Field + " DESC" : sort.Field);
        }

        return parts.Count == 0 ? null : string.Join(",", parts);
    }

    private Task RefreshGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadUsers);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshGridAsync();
    }

    private void GoToCreateUser()
    {
        NavigationManager.NavigateTo(CreateUserRoute);
    }

    private void GoToEditUser(IdentityUserDto user)
    {
        NavigationManager.NavigateTo($"/panel/admin/identity/users/{user.Id}");
    }

    private async Task DeleteUserAsync(IdentityUserDto user)
    {
        if (!await Message.ConfirmAsync(L["DeleteUserConfirmation", user.UserName!]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await UserAppService.DeleteAsync(user.Id);
            await Notify.SuccessAsync(L["UserDeletedSuccessfully"]);
            await RefreshGridAsync();
        }, LoadingKeys.DeleteUser);
    }

    private async Task ShowPermissionModalAsync(IdentityUserDto user)
    {
        if (_permissionManagementModal != null)
        {
            await _permissionManagementModal.OpenAsync(
                PermissionProviderName,
                user.Id.ToString(),
                user.UserName
            );
        }
    }

    private static IReadOnlyDictionary<string, string> BuildHooshvareContext()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal);
    }
}
