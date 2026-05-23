using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Uow;

public class SufiAbpUnitOfWorkManager : IUnitOfWorkManager, ITransientDependency
{
    protected Volo.Abp.Uow.IUnitOfWorkManager InnerUnitOfWorkManager { get; }

    public SufiAbpUnitOfWorkManager(Volo.Abp.Uow.IUnitOfWorkManager innerUnitOfWorkManager)
    {
        InnerUnitOfWorkManager = innerUnitOfWorkManager;
    }

    public virtual bool HasActiveUnitOfWork =>
        InnerUnitOfWorkManager.Current is { IsDisposed: false };

    public virtual IUnitOfWork Begin(bool requiresNew = false, bool isTransactional = false)
    {
        return new SufiAbpUnitOfWork(
            InnerUnitOfWorkManager.Begin(
                new Volo.Abp.Uow.AbpUnitOfWorkOptions(isTransactional),
                requiresNew));
    }
}
