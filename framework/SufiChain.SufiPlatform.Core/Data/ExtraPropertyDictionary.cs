using Volo.Abp.Data;
namespace SufiChain.SufiPlatform.Data;

public class ExtraPropertyDictionary : Volo.Abp.Data.ExtraPropertyDictionary
{
    public ExtraPropertyDictionary()
    {
    }

    public ExtraPropertyDictionary(IDictionary<string, object?> dictionary)
        : base(dictionary)
    {
    }
}
