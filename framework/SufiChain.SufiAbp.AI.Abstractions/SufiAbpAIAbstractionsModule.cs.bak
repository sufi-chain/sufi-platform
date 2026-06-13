using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// SufiAbp AI abstractions module. Owns the neutral, provider-free AI contracts
/// consumed by product modules (<see cref="ISufiAbpAIChatService"/>,
/// <see cref="ISufiAbpAIAudioService"/>, <see cref="ISufiAbpAIRagService"/>,
/// <see cref="ISufiAbpAIWorkspaceCatalog"/>, <see cref="ISufiAbpAIToolRegistry"/>, ...).
/// Null fallback implementations are registered conventionally with
/// <c>TryRegister</c>, so installing a provider module (e.g. AIManagement) replaces
/// them automatically via <c>[Dependency(ReplaceServices = true)]</c>.
/// </summary>
public class SufiAbpAIAbstractionsModule : AbpModule
{
}
