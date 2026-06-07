namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Base class for defining SufiAbp features.
/// </summary>
public abstract class FeatureDefinitionProvider : Volo.Abp.Features.FeatureDefinitionProvider
{
    /// <inheritdoc />
    public sealed override void Define(Volo.Abp.Features.IFeatureDefinitionContext context)
    {
        Define(new FeatureDefinitionContext(context));
    }

    /// <summary>
    /// Defines SufiAbp features.
    /// </summary>
    public abstract void Define(IFeatureDefinitionContext context);
}
