using Volo.Abp;

namespace SufiChain.SufiPlatform.Features;

[Serializable]
public class FeatureValueInvalidException : BusinessException
{
    public FeatureValueInvalidException(string name)
        : base(FeaturesDomainErrorCodes.FeatureValueInvalid)
    {
        WithData("0", name);
    }
}
