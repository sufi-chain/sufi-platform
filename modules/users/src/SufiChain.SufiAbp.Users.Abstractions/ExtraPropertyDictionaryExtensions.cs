using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Users;

public static class ExtraPropertyDictionaryExtensions
{
    public static ExtraPropertyDictionary ToSufiAbpExtraProperties(this Volo.Abp.Data.ExtraPropertyDictionary? extraProperties)
    {
        return extraProperties == null
            ? new ExtraPropertyDictionary()
            : new ExtraPropertyDictionary(extraProperties);
    }
}
