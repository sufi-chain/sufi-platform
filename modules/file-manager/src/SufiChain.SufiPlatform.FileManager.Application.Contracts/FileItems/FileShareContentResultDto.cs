namespace SufiChain.SufiPlatform.FileManager.FileItems;

public enum FileShareLinkStatus
{
    Valid = 0,
    Invalid = 1,
    Expired = 2,
    Revoked = 3
}

/// <summary>Result of resolving a Quick Share token (sufi-chain#436).</summary>
public class FileShareContentResultDto
{
    public FileShareLinkStatus Status { get; set; } = FileShareLinkStatus.Invalid;

    public FileContentDto? Content { get; set; }
}