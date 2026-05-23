using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.PermissionManagement;

public interface IResourcePermissionProviderKeyLookupService
{
    public string Name { get; }

    Task<bool> IsAvailableAsync();

    public ILocalizableString DisplayName { get; }

    Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default);

    Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default);
}
