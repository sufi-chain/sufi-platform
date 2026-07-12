namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Base class for defining Sufi features.
/// </summary>
public abstract class FeatureDefinitionProvider : Volo.Abp.Features.FeatureDefinitionProvider
{
    /// <inheritdoc />
    public sealed override void Define(Volo.Abp.Features.IFeatureDefinitionContext context)
    {
        Define(new FeatureDefinitionContext(context));
    }

    /// <summary>
    /// Defines Sufi features.
    /// </summary>
    public abstract void Define(IFeatureDefinitionContext context);
}
