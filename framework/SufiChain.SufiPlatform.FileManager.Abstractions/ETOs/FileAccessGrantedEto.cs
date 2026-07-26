using SufiChain.SufiPlatform.EventBus;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Published when access is granted to a file (consumers cache locally instead of calling back).
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileAccessGranted")]
public class FileAccessGrantedEto : SufiIntegrationEto
{
    /// <summary>File id access was granted for.</summary>
    public Guid FileId { get; set; }

    /// <summary>User who received access (null = structure public / anonymous).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Optional access token issued with the grant.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Token expiry when applicable.</summary>
    public DateTime? ExpiresAt { get; set; }
}
