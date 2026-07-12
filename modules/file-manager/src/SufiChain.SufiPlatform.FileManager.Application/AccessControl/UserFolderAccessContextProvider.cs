using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.FileManager.AccessControl;

/// <summary>
/// Default <see cref="IUserFolderAccessContextProvider"/>. Builds a
/// <see cref="FolderAccessContext"/> from <see cref="ICurrentUser"/>, resolves role ids
/// via <see cref="IRoleNameToIdResolver"/>, and OU memberships via
/// <see cref="IUserOrganizationUnitProvider"/>.
/// </summary>
public class UserFolderAccessContextProvider : IUserFolderAccessContextProvider, ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly FileManagerOptions _options;
    private readonly IRoleNameToIdResolver _roleNameToIdResolver;
    private readonly IUserOrganizationUnitProvider _organizationUnitProvider;

    public UserFolderAccessContextProvider(
        ICurrentUser currentUser,
        IOptions<FileManagerOptions> options,
        IRoleNameToIdResolver roleNameToIdResolver,
        IUserOrganizationUnitProvider organizationUnitProvider)
    {
        _currentUser = currentUser;
        _options = options.Value;
        _roleNameToIdResolver = roleNameToIdResolver;
        _organizationUnitProvider = organizationUnitProvider;
    }

    public async Task<FolderAccessContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        var roles = _currentUser.Roles ?? System.Array.Empty<string>();
        var roleIds = await _roleNameToIdResolver.ResolveAsync(roles, cancellationToken);

        var organizationUnitIds = _currentUser.Id.HasValue
            ? await _organizationUnitProvider.GetOrganizationUnitIdsAsync(_currentUser.Id.Value, true, cancellationToken)
            : System.Array.Empty<System.Guid>();

        return new FolderAccessContext
        {
            UserId = _currentUser.Id,
            TenantId = _currentUser.TenantId,
            Roles = roles,
            RoleIds = roleIds,
            OrganizationUnitIds = organizationUnitIds,
            AdminRoleName = _options.FolderAdminRoleName
        };
    }
}
