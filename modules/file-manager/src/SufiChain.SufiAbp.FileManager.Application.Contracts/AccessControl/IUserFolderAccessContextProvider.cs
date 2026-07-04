using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.FileFolders;

namespace SufiChain.SufiAbp.FileManager.AccessControl;

/// <summary>
/// Builds the <see cref="FolderAccessContext"/> for the current user, including
/// role ids and organization-unit memberships that are not directly available from
/// <c>ICurrentUser</c>. The file-manager module ships a default implementation that
/// resolves role ids from role names and returns an empty OU set when no Identity
/// integration is registered. Hosts integrating the SufiChain.SufiAbp.Identity module
/// can replace this to provide real OU membership resolution.
/// </summary>
public interface IUserFolderAccessContextProvider
{
    /// <summary>
    /// Builds the access context for the currently authenticated user.
    /// </summary>
    Task<FolderAccessContext> GetContextAsync(CancellationToken cancellationToken = default);
}
