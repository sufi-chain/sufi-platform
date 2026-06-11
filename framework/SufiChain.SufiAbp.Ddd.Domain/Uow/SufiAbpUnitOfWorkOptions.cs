namespace SufiChain.SufiAbp.Uow;

public class SufiAbpUnitOfWorkDefaultOptions : Volo.Abp.Uow.AbpUnitOfWorkDefaultOptions
{
}

public enum SufiAbpUnitOfWorkTransactionBehavior
{
    Auto = Volo.Abp.Uow.UnitOfWorkTransactionBehavior.Auto,
    Enabled = Volo.Abp.Uow.UnitOfWorkTransactionBehavior.Enabled,
    Disabled = Volo.Abp.Uow.UnitOfWorkTransactionBehavior.Disabled
}

public static class SufiAbpUnitOfWorkTransactionBehaviorExtensions
{
    public static Volo.Abp.Uow.UnitOfWorkTransactionBehavior ToAbpUnitOfWorkTransactionBehavior(
        this SufiAbpUnitOfWorkTransactionBehavior behavior)
    {
        return (Volo.Abp.Uow.UnitOfWorkTransactionBehavior)behavior;
    }
}
