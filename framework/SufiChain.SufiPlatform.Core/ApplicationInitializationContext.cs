using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiPlatform.Core;

public class ApplicationInitializationContext
{
    private readonly Volo.Abp.ApplicationInitializationContext _innerContext;

    public ApplicationInitializationContext(Volo.Abp.ApplicationInitializationContext innerContext)
    {
        _innerContext = innerContext;
    }

    public IServiceProvider ServiceProvider => _innerContext.ServiceProvider;

    public Volo.Abp.ApplicationInitializationContext AsAbpContext()
    {
        return _innerContext;
    }
}
