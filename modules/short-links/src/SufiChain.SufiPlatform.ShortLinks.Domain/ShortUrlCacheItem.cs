using System;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlCacheItem
{
    public Guid Id { get; set; }
    
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsActive { get; set; }
}

