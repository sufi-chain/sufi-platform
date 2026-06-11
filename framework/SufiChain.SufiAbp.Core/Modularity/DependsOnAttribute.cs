namespace SufiChain.SufiAbp.Modularity;

public class DependsOnAttribute : Volo.Abp.Modularity.DependsOnAttribute
{
    public DependsOnAttribute(params Type[] dependedModuleTypes)
        : base(dependedModuleTypes)
    {
    }
}
