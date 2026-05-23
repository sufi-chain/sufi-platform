using JetBrains.Annotations;
using Volo.Abp;

namespace SufiChain.SufiAbp.FeatureManagement;

[Serializable]
public class FeatureNameValueWithGrantedProvider : NameValue
{
    public FeatureValueProviderInfo Provider { get; set; }

    public FeatureNameValueWithGrantedProvider([NotNull] string name, string value)
    {
        Check.NotNull(name, nameof(name));

        Name = name;
        Value = value;
    }
}
