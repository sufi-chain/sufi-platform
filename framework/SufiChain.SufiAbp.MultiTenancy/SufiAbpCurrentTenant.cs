using SufiChain.SufiAbp.DependencyInjection;

namespace SufiChain.SufiAbp.MultiTenancy;

public class SufiAbpCurrentTenant : ICurrentTenant, ITransientDependency
{
    private readonly Volo.Abp.MultiTenancy.CurrentTenant _currentTenant;

    public SufiAbpCurrentTenant(Volo.Abp.MultiTenancy.CurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public virtual bool IsAvailable => _currentTenant.IsAvailable;

    public virtual Guid? Id => _currentTenant.Id;

    public virtual string? Name => _currentTenant.Name;

    public virtual IDisposable Change(Guid? id, string? name = null)
    {
        return _currentTenant.Change(id, name);
    }
}
