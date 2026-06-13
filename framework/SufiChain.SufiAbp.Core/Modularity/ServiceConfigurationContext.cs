using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiAbp.Modularity;

public class ServiceConfigurationContext
{
    private readonly Volo.Abp.Modularity.ServiceConfigurationContext _innerContext;

    public ServiceConfigurationContext(Volo.Abp.Modularity.ServiceConfigurationContext innerContext)
    {
        _innerContext = innerContext;
    }

    public IServiceCollection Services => _innerContext.Services;

    public Volo.Abp.Modularity.ServiceConfigurationContext AsAbpContext()
    {
        return _innerContext;
    }
}
