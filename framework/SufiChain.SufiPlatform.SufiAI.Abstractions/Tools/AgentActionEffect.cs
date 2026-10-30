namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// What a tool or action does to data (sufi-chain#312, agent foundations tool/action model).
/// Registered tools are <see cref="Read"/> or <see cref="Write"/>. <see cref="Unspecified"/> is only the
/// attribute's "not set" value: C# attribute arguments cannot be a nullable enum, and an omitted
/// declaration resolves to <see cref="Write"/> unless the tool is marked read-only.
/// </summary>
public enum AgentActionEffect
{
    /// <summary>Not declared. Resolves to <see cref="Read"/> when the tool is read-only, otherwise <see cref="Write"/>.</summary>
    Unspecified = 0,

    /// <summary>The tool can change data, send a message, or call something whose effect is not declared.</summary>
    Write = 1,

    /// <summary>The tool only reads data. It must be declared; unmarked tools resolve to <see cref="Write"/>.</summary>
    Read = 2
}
