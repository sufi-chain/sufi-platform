using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlDto : ExtensibleAuditedEntityDto<Guid>
{
    public string ShortCode { get; set; } = string.Empty;
    
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    public int ClickCount { get; set; }
    
    public DateTime? LastAccessedAt { get; set; }
    
    public bool IsActive { get; set; }
    
    public string CreatedByModule { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string FullShortUrl { get; set; } = string.Empty;

}
