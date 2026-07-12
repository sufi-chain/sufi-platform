using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiPlatform.ShortLinks;

public class CreateShortUrlDto : ExtensibleObject
{
    [Required]
    [StringLength(ShortLinksConsts.ShortUrl.MaxDestinationUrlLength)]
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(ShortLinksConsts.ShortUrl.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    [StringLength(ShortLinksConsts.ShortUrl.MaxCreatedByModuleLength)]
    public string? CreatedByModule { get; set; }

    public CreateShortUrlDto()
        : base(setDefaultsForExtraProperties: false)
    {
    }
}
