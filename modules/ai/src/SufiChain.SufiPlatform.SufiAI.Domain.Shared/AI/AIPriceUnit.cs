namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Unit of a stored route price. The amount is what an admin types:
/// dollars per million tokens, per minute, per hour, per million characters, per image, or per request.
/// </summary>
public enum AIPriceUnit
{
    PerMillionTokens = 0,
    PerMinute = 1,
    PerHour = 2,
    PerMillionCharacters = 3,
    PerImage = 4,
    PerRequest = 5
}
