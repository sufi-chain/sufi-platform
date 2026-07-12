namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class ProviderHealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? AdditionalData { get; set; }
}