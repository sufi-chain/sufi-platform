using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.ShortLinks;

public class UpdateShortUrlDto
{
    [Required]
    [StringLength(ShortLinksConsts.ShortUrl.MaxDestinationUrlLength)]
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(ShortLinksConsts.ShortUrl.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
}

