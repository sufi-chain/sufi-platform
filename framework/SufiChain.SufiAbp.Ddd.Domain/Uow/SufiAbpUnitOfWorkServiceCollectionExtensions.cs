using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiAbp.Uow;

public static class SufiAbpUnitOfWorkServiceCollectionExtensions
{
    public static IServiceCollection AddAlwaysDisableUnitOfWorkTransaction(this IServiceCollection services)
    {
        return Volo.Abp.Uow.UnitOfWorkCollectionExtensions.AddAlwaysDisableUnitOfWorkTransaction(services);
    }
}
