using JetBrains.Annotations;

namespace SufiChain.SufiPlatform.TextTemplating.Scriban;

public static class ScribanTemplateDefinitionExtensions
{
    public static TemplateDefinition WithScribanEngine([NotNull] this TemplateDefinition templateDefinition)
    {
        return templateDefinition.WithRenderEngine(ScribanTemplateRenderingEngine.EngineName);
    }
}
