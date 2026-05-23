using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.PermissionManagement;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Localization;

namespace SufiChain.SufiAbp.PermissionManagement.OpenIddict;

public class ApplicationResourcePermissionProviderKeyLookupService : IResourcePermissionProviderKeyLookupService, ITransientDependency
{
    public string Name => ClientResourcePermissionValueProvider.ProviderName;

    public ILocalizableString DisplayName { get; }

    protected IApplicationFinder ApplicationFinder { get; }

    protected ICurrentTenant CurrentTenant { get; }

    public ApplicationResourcePermissionProviderKeyLookupService(
        IApplicationFinder applicationFinder,
        ICurrentTenant currentTenant)
    {
        ApplicationFinder = applicationFinder;
        CurrentTenant = currentTenant;
        DisplayName = LocalizableString.Create<SufiAbpOpenIddictResource>(nameof(ApplicationResourcePermissionProviderKeyLookupService));
    }

    public virtual Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(CurrentTenant.Id == null);
    }

    public virtual async Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string filter = null, int page = 1, CancellationToken cancellationToken = default)
    {
        var applications = await ApplicationFinder.SearchAsync(filter, page);
        return applications.Select(x => new ResourcePermissionProviderKeyInfo(x.ClientId, x.ClientId)).ToList();
    }

    public virtual Task<List<ResourcePermissionProviderKeyInfo>> SearchAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        // Keys are ClientIds
        return Task.FromResult(keys.Select(x => new ResourcePermissionProviderKeyInfo(x, x)).ToList());
    }
}
