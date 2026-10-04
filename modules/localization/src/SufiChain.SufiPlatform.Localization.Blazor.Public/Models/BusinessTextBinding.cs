namespace SufiChain.SufiPlatform.Localization.Blazor.Public.Models;

/// <summary>
/// Non-string binding for a business text. Razor treats this as an expression, so a missing
/// <c>@</c> binds the field instead of the literal name of that field.
/// </summary>
public sealed record BusinessTextBinding(string? ResourceName, string? Key, string? LiteralValue = null);
