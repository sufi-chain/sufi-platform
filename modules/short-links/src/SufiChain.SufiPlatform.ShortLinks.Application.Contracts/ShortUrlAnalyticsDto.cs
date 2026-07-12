using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlAnalyticsDto
{
    public Guid Id { get; set; }
    
    public string ShortCode { get; set; } = string.Empty;
    
    public int ClickCount { get; set; }
    
    public DateTime? LastAccessedAt { get; set; }
    
    public List<ShortUrlClickDto> RecentClicks { get; set; } = new();
}

