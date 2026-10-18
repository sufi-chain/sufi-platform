using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Explicit allowlist entry for a tool that writes and must still be reachable from anonymous turns
/// (for example a self-service onboarding step). Only use it when the tool protects itself: rate limits,
/// one-time codes, or tokens bound to the visitor's own session. Tools without this attribute (and not
/// <c>Public</c> + <c>ReadOnly</c>) are never offered to, or run for, an anonymous caller.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SufiAiMcpPublicWriteAttribute : Attribute
{
}
