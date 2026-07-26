#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill for <c>ModuleInitializerAttribute</c> on netstandard2.1.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute
{
}
#endif
