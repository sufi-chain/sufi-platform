namespace SufiChain.SufiAbp.Uow;

public interface IUnitOfWork : IDisposable
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
