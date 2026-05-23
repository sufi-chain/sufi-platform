using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class UpdateShortUrlDto
{
    [Required]
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDestinationUrlLength)]
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
}

