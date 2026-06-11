using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Resolves the Semantic Kernel <see cref="Kernel"/> for the default workspace.
/// <para>
/// Framework/advanced use only: this contract exposes the
/// <c>Microsoft.SemanticKernel</c> SDK surface. Product modules must not consume it;
/// they use <see cref="ISufiAbpAIChatService"/> and the other SufiAbp DTO-based
/// contracts instead.
/// </para>
/// </summary>
public interface IKernelAccessor
{
    /// <summary>
    /// The kernel, or <c>null</c> when no provider is configured.
    /// </summary>
    Kernel? Kernel { get; }
}

/// <summary>
/// Resolves the Semantic Kernel <see cref="Kernel"/> for the workspace identified by
/// <typeparamref name="TWorkSpace"/>. Framework/advanced use only — see
/// <see cref="IKernelAccessor"/>.
/// </summary>
public interface IKernelAccessor<TWorkSpace> : IKernelAccessor
    where TWorkSpace : class
{
}
