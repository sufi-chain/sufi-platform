using System;
using System.Collections.Generic;
using Volo.Abp.Collections;

namespace SufiChain.SufiPlatform.TextTemplating;

public class SufiTextTemplatingOptions
{
    public ITypeList<ITemplateDefinitionProvider> DefinitionProviders { get; }
    public ITypeList<ITemplateContentContributor> ContentContributors { get; }
    public IDictionary<string, Type> RenderingEngines { get; }

    public string? DefaultRenderingEngine { get; set; }

    public HashSet<string> DeletedTemplates { get; }

    public SufiTextTemplatingOptions()
    {
        DefinitionProviders = new TypeList<ITemplateDefinitionProvider>();
        ContentContributors = new TypeList<ITemplateContentContributor>();
        RenderingEngines = new Dictionary<string, Type>();
        DeletedTemplates = new HashSet<string>();
    }
}
