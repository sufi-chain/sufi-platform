using System.Collections.Generic;

namespace SufiChain.SufiAbp.TextTemplating;

public interface ITemplateDefinitionContext
{
    IReadOnlyList<TemplateDefinition> GetAll();

    TemplateDefinition? GetOrNull(string name);

    void Add(params TemplateDefinition[] definitions);
}
