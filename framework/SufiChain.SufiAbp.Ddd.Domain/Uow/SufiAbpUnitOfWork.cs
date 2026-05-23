namespace SufiChain.SufiAbp.Uow;

public class SufiAbpUnitOfWork : IUnitOfWork
{
    protected Volo.Abp.Uow.IUnitOfWork InnerUnitOfWork { get; }

    public SufiAbpUnitOfWork(Volo.Abp.Uow.IUnitOfWork innerUnitOfWork)
    {
        InnerUnitOfWork = innerUnitOfWork;
    }

    public virtual Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        return InnerUnitOfWork.CompleteAsync(cancellationToken);
    }

    public virtual void Dispose()
    {
        InnerUnitOfWork.Dispose();
    }
}
