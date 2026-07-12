using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
namespace SufiChain.SufiAbp.Data;

public interface IHasExtraProperties
{
    ExtraPropertyDictionary ExtraProperties { get; }
}
