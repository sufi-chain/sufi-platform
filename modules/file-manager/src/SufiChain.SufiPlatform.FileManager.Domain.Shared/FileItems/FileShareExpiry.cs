namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Quick Share custom expiry. One minute through seven days.
/// </summary>
public static class FileShareExpiry
{
    public const int MinMinutes = 1;

    public const int MaxMinutes = 10080;

    public static bool IsInRange(int durationMinutes)
    {
        return durationMinutes >= MinMinutes && durationMinutes <= MaxMinutes;
    }
}
