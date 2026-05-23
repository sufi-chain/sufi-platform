namespace SufiChain.SufiAbp.Uow;

public interface IUnitOfWorkManager
{
    bool HasActiveUnitOfWork { get; }

    IUnitOfWork Begin(bool requiresNew = false, bool isTransactional = false);
}
