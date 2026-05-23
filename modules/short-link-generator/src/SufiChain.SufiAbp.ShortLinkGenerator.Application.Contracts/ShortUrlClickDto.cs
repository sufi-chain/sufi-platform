using System;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class ShortUrlClickDto
{
    public DateTime ClickedAt { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? Referrer { get; set; }
}

