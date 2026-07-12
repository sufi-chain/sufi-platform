using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Sufi AI abstractions module. Owns the neutral, provider-free AI contracts
/// consumed by product modules (<see cref="ISufiAIChatService"/>,
/// <see cref="ISufiAIAudioService"/>, <see cref="ISufiAIRagService"/>,
/// <see cref="ISufiAIWorkspaceCatalog"/>, <see cref="ISufiAIToolRegistry"/>, ...).
/// Null fallback implementations are registered conventionally with
/// <c>TryRegister</c>, so installing a provider module (e.g. AI) replaces
/// them automatically via <c>[Dependency(ReplaceServices = true)]</c>.
/// </summary>
public class SufiAIAbstractionsModule : AbpModule
{
}
