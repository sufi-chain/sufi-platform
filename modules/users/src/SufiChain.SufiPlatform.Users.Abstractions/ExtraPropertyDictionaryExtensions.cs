using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Users;

public static class ExtraPropertyDictionaryExtensions
{
    public static ExtraPropertyDictionary EnsureNotNull(this ExtraPropertyDictionary? extraProperties)
    {
        return extraProperties ?? new ExtraPropertyDictionary();
    }
}
