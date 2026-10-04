using System.Globalization;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Reads the provider <c>TimeoutSeconds</c> setting. A missing or invalid value uses <paramref name="defaultSeconds"/>.
/// </summary>
public static class OtpRequestTimeout
{
    public static TimeSpan FromSettings(IReadOnlyDictionary<string, string> settings, int defaultSeconds = 30)
    {
        if (settings.TryGetValue("TimeoutSeconds", out var raw) &&
            int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds) &&
            seconds > 0)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        return TimeSpan.FromSeconds(defaultSeconds);
    }
}
