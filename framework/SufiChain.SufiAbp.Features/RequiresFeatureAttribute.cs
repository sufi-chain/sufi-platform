namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Requires one or more features to be enabled before a class or method can be used.
/// </summary>
public class RequiresFeatureAttribute : Volo.Abp.Features.RequiresFeatureAttribute
{
    public RequiresFeatureAttribute(params string[] features)
        : base(features)
    {
    }
}
