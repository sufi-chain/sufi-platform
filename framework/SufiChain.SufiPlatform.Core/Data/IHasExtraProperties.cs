using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
namespace SufiChain.SufiPlatform.Data;

public interface IHasExtraProperties
{
    ExtraPropertyDictionary ExtraProperties { get; }
}
