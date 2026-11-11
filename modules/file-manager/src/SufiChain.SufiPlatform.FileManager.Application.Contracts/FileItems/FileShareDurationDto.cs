using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Custom Quick Share expiry. <see cref="RangeAttribute"/> is the server bound.
/// </summary>
public class FileShareDurationDto
{
    [Range(FileShareExpiry.MinMinutes, FileShareExpiry.MaxMinutes)]
    public int DurationMinutes { get; set; }
}
